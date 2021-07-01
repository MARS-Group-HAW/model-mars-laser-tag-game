using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Mars.Common;
using Mars.Components.Common;
using Mars.Components.Layers;
using Mars.Interfaces.Data;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using ServiceStack.Text;

namespace LaserTagBox.Model.Mind {
    public class SensedMap {
        public readonly int Xsize = 50;
        public readonly int Ysize = 50;


        private List<Position> _barrierList;

        // private int[][] EnvMap { get; set; }
        private Dictionary<Position, SensedObjects> EnvMap;

        private List<Position> newExploredAreaList;

        public SensedObjects get(Position pos) {
            EnvMap.TryGetValue(pos, out var outObj);
            return outObj;
        }

        public IEnumerable<Position> Explore(Position pos, int distance) {
            var posDictionary = new Dictionary<Position, double>();
            for (int x = ((int) Math.Round(pos.X)) - distance; x < (pos.X + distance); x++) {
                for (int y = (int) Math.Round(pos.Y) - distance; y < (pos.Y + distance); y++) {
                    var dist = new Distance().SquareEuclidean(pos.PositionArray, new Position(x, y).PositionArray);
                    var dist2 = new Distance().Euclidean(pos.PositionArray, new Position(x, y).PositionArray);
                    if (IsValidPosition(new Position(x, y))) {
                        posDictionary.Add(new Position(x, y), dist2);
                    }
                }
            }

            var count = posDictionary.Count;
            var calcedDistance = new Distance().Euclidean(pos.PositionArray,
                new Position(pos.X, pos.Y + distance).PositionArray);
            return posDictionary.Where(pair => (pair.Value <= calcedDistance)).Select(pair => pair.Key);
        }

        public SensedMap() {
            newExploredAreaList = new List<Position>();
            _barrierList = new List<Position>();
            EnvMap = new Dictionary<Position, SensedObjects>();
        }

        public List<Position> getNewExploredAreas() {
            var outList = new List<Position>(newExploredAreaList);
            newExploredAreaList.Clear();
            return outList;
        }

        public enum SensedObjects {
            Unknown,
            Error,
            Explored,
            Barrier,
            CircleTest,
            Hill,
            Ditch
        }

        public SensedObjects this[Position pos] {
            get => this[(int) Math.Round(pos.X), (int) Math.Round(pos.Y)];
            set => this[(int) Math.Round(pos.X), (int) Math.Round(pos.Y)] = value;
        }

        public SensedObjects this[int x, int y] {
            get {
                EnvMap.TryGetValue(new Position(x, y), out var outObj);
                return outObj;
            }

            set {
                if (x < 0 || y < 0) {
                    return;
                }

                if (value == SensedObjects.Barrier) {
                    EnvMap.Remove(new Position(x, y));
                }

                if (EnvMap.TryAdd(new Position(x, y), value) && value == SensedObjects.Explored) {
                    
                    newExploredAreaList.Add(new Position(x, y));
                }
            }
        }


        public void addBarriers(IEnumerable<Position> barrierList) {
            _barrierList.AddRange(barrierList);
            foreach (var barrier in barrierList) {
                this[barrier] = SensedObjects.Barrier;
            }
        }

        public bool IsValidPosition(Position newPos) {
            return newPos.X >= 0 && newPos.Y >= 0 && newPos.X <= 51 && newPos.Y <= 51;
        }

        public void printMap() {
            for (int y = 0; y < 50; y++) {
                for (int x = 0; x < 50; x++) {
                    Console.Write((int) this[x, y] + " ");
                }

                Console.Write("\n");
            }
        }

        public void senseEnvironment(Position pos, int viewingDistance) {
            var list = CalcCircleHorn(pos, viewingDistance + 1); //Weil halt ist so
            foreach (var circlePos in list) {
                OwnHasBeeline(pos.X, pos.Y, circlePos.X, circlePos.Y, true);
                // this[(int) circlePos.X,(int) circlePos.Y] = SensedObjects.Explored;
            }
        }


        public void senseEnvironment(Position pos, List<Position> BarrierList) {
            var others = Explore(pos, 4);
            BarrierList.AddRange(others);
            foreach (var bPos in BarrierList) {
                OwnHasBeeline(pos.X, pos.Y, bPos.X, bPos.Y, true);
                // this[(int) circlePos.X,(int) circlePos.Y] = SensedObjects.Explored;
            }
        }

        public List<Position> CalcCircleHorn(Position pos, int radius) {
            var outList = new List<Position>();
            int d = -radius;
            int x = radius;
            int y = 0;
            while (!(y > x)) {
                outList.Add(new Position(x + (int) pos.X, y + (int) pos.Y));
                outList.Add(new Position(y + (int) pos.X, x + (int) pos.Y));
                outList.Add(new Position(y + (int) pos.X, -x + (int) pos.Y));
                outList.Add(new Position(x + (int) pos.X, -y + (int) pos.Y));
                outList.Add(new Position(-x + (int) pos.X, -y + (int) pos.Y));
                outList.Add(new Position(-y + (int) pos.X, -x + (int) pos.Y));
                outList.Add(new Position(-y + (int) pos.X, x + (int) pos.Y));
                outList.Add(new Position(-x + (int) pos.X, y + (int) pos.Y));
                //--------
                // this[x + (int) pos.X, y + (int) pos.Y] = SensedObjects.CircleTest;
                // this[y + (int) pos.Y, x + (int) pos.X] = SensedObjects.CircleTest;
                // this[y + (int) pos.Y, -x + (int) pos.X] = SensedObjects.CircleTest;
                // this[x + (int) pos.X, -y + (int) pos.Y] = SensedObjects.CircleTest;
                // this[-x + (int) pos.X, -y + (int) pos.Y] = SensedObjects.CircleTest;
                // this[-y + (int) pos.Y, -x + (int) pos.X] = SensedObjects.CircleTest;
                // this[-y + (int) pos.Y, x + (int) pos.X] = SensedObjects.CircleTest;
                // this[-x + (int) pos.X, y + (int) pos.Y] = SensedObjects.CircleTest;
                d = d + (2 * y) + 1;
                y = y + 1;
                if (d > 0) {
                    x = x - 1;
                    d = d - (2 * x);
                }
            }

            return outList;
        }


        public int OwnHasBeeline(Position source, Position target) {
            return OwnHasBeeline(source.X, source.Y, target.X, target.Y, false);
        }


        //TODO distanz mit angeben. Als abbruchbedingungen
        public int OwnHasBeeline(double x1, double y1, double x2, double y2, bool drawMap) {
            var x = (int) x1;
            var y = (int) y1;
            var newX2 = (int) x2;
            var newY2 = (int) y2;
            var w = newX2 - x;
            var h = newY2 - y;
            var dx1 = 0;
            var dy1 = 0;
            var dx2 = 0;
            var dy2 = 0;
            double numberOfKnownPosition = 0;
            double numberOfAllPosition = 0;
            if (w < 0) {
                dx1 = -1;
            }
            else if (w > 0) {
                dx1 = 1;
            }

            if (h < 0) {
                dy1 = -1;
            }
            else if (h > 0) {
                dy1 = 1;
            }

            if (w < 0) {
                dx2 = -1;
            }
            else if (w > 0) {
                dx2 = 1;
            }

            var longest = Math.Abs(w);
            var shortest = Math.Abs(h);
            if (!(longest > shortest)) {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) {
                    dy2 = -1;
                }
                else if (h > 0) {
                    dy2 = 1;
                }

                dx2 = 0;
            }

            var numerator = longest / 2;
            for (var i = 0; i < longest; i++) {
                if (newX2 == x && newY2 == y) {
                    return 0;
                }

                var currPos = this[x, y];
                if (currPos == SensedObjects.Barrier || currPos == SensedObjects.Hill) {
                    // Console.WriteLine("Dont shoot man");
                    return 0;
                }
                else {
                    this[x, y] = SensedObjects.Explored;
                }

                if (currPos != SensedObjects.Unknown) {
                    numberOfKnownPosition++;
                }

                numberOfAllPosition++;


                numerator = numerator + shortest;
                if (!(numerator < longest)) {
                    numerator = numerator - longest;
                    x = x + dx1;
                    y = y + dy1;
                }
                else {
                    x = x + dx2;
                    y = y + dy2;
                }
            }

            return Math.Max((int) ((numberOfKnownPosition / numberOfAllPosition) * 100), 0);
        }
    }
}