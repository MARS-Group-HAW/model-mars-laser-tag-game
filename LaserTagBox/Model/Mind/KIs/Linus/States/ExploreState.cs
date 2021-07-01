using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Mars.Common;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;

namespace LaserTagBox.Model.Mind.States {
    public class ExploreState {
        private IOurPlayerMind _mind;
        private SensedMap _map;
        private bool _reachedInitPos;
        private static Position _goal;

        private Position _leaderGoal;

        private Position _lastLeaderGoal;

        public ExploreState(IOurPlayerMind mind, SensedMap map) {
            _mind = mind;
            _map = map;
            _reachedInitPos = false;
            _lastLeaderGoal = new Position(25, 25);
            if (_mind.IsLeader()) {
                // _goal = new Position(_mind.Body.Position.X, _mind.Body.Position.Y);
                _goal = _mind.getBody().Position;
                _leaderGoal = _goal;
            }
        }

        public List<Position> getDifferentDirections() {
            var outList = new List<Position>();
            for (int i = 0; i < 360; i = i + 45) {
                var newPos = PositionHelper.CalculateDiscretePositionByBearing(_mind.getBody().Position.X,
                    _mind.getBody().Position.Y, i, 10);

                outList.Add(newPos);
            }

            return outList;
        }

        private Position SelectNextGoalByRandomCircleCalc() {
            Position newPos = null;
            while (newPos == null || !_map.IsValidPosition(newPos)) {
                var bearing = RandomHelper.Random.Next(360);
                // newPos = PositionHelper.CalculateDerivedPosition(Body.Position, EXPLORE_RANGE, bearing);
                newPos = PositionHelper.CalculateDiscretePositionByBearing(_mind.getBody().Position.X,
                    _mind.getBody().Position.Y,
                    bearing, 10);
                // var testPos = PositionHelper.CalculatePosition(Body.Position.X, Body.Position.Y, Body.Position.X + 2,
                //     Body.Position.Y + 2, 1);
                // var envMap = EnvMap.Explore(new Position(), EXPLORE_RANGE / 2, -1);
                // var knownObjs = 0;
                // foreach (var node in envMap) {
                //     if (EnvMap[node.Node.NodePosition] != (double) SensedMap.SensedObjects.Unknown) {
                //         knownObjs++;
                //     }
                // }
                // if(knownObjs )
                //TODO kommt noch
            }

            return newPos;
        }

        private Position SelectNextGoalByCompleteRandom() {
            Position outPos = null;
            while (outPos == null || _map.get(outPos) == SensedMap.SensedObjects.Barrier) {
                outPos = new Position(RandomHelper.Random.Next(_map.Xsize), RandomHelper.Random.Next(_map.Ysize));
            }

            return outPos;
        }

        private Position SelectNextGoalByCoverage() {
            var rndPos = _mind.getBody().Position;

            var possibleSpots = new Dictionary<Position, double>();
            double lowestCoverage = 100;
            Position bestSpot = rndPos;

            var posList = getDifferentDirections();
            foreach (var pos in posList) {
                if (_map.IsValidPosition(pos)) {
                    var coverage = getExploreCoverage(pos);
                    possibleSpots.Add(pos, coverage);
                    if (coverage < lowestCoverage) {
                        lowestCoverage = coverage;
                        bestSpot = pos;
                    }
                }
            }

            var spotList = possibleSpots.ToList();
            spotList.Sort((pairA, pairB) => pairA.Value.CompareTo(pairB.Value));

            //rndPos = SelectNextGoalByRandomCircleCalc();
            //possibleSpots.Add(rndPos, getExploreCoverage(rndPos));
            return spotList.FirstOrDefault().Key;
        }

        private double getExploreCoverage(Position pos) {
            var coverageMap = _map.Explore(pos, 10);
            double coveragePercentage = 0;
            int exploredSum = 0;
            int totalField = 0;
            foreach (var covpos in coverageMap) {
                if (_map.IsValidPosition(covpos)) {
                    totalField++;
                    var mapObj = _map.get(covpos);
                    if (mapObj != SensedMap.SensedObjects.Unknown) {
                        exploredSum++;
                    }
                    else {
                        //Console.Write("unknwon");
                    }
                }
            }

            if (totalField > 0) {
                coveragePercentage = (double) exploredSum / (double) totalField;
            }

            return coveragePercentage;
        }

        private Position SelectNextGoal() {
            return SelectNextGoalByRandomCircleCalc();
        }


        public void tick() {
            bool waitForTeammembers = false;
            if (_mind.IsLeader()) {
                _goal = _mind.getBody().Position; //If we are newly assigned leader
                if (!_reachedInitPos) {
                    var teamList = _mind.getBody().ExploreTeam();
                    int missing = teamList.Count;
                    foreach (var member in teamList) {
                        if (member.Position.Equals(_mind.getBody().Position)) {
                            missing--;
                        }
                    }

                    if (missing == 0) {
                        //everyone is at initPos
                        _reachedInitPos = true;
                        Console.WriteLine("Everyone reached InitPos at: " + _goal);
                    }
                }
                else {
                    if (_leaderGoal == null || _mind.getBody().GetDistance(_leaderGoal) < 1) {
                        // _goal = SelectNextGoal();
                        foreach (var teamMem in _mind.getBody().ExploreTeam()) {
                            if (_mind.getBody().GetDistance(teamMem.Position) > 2) {
                                if (_leaderGoal != null) {
                                    waitForTeammembers = true;
                                    Console.WriteLine("Diff detected");
                                }
                            }
                        }

                        if (!waitForTeammembers) {
                            var tempLeader = _leaderGoal;
                            _leaderGoal = SelectNextGoalByCoverage();
                            if (_lastLeaderGoal.Equals(_leaderGoal)) {
                                _leaderGoal = SelectNextGoalByRandomCircleCalc();
                                _lastLeaderGoal = _leaderGoal;
                            }
                            else {
                                _lastLeaderGoal = tempLeader;
                            }


                            Console.WriteLine("got next goal@ " + _leaderGoal);
                        }
                    }
                }
            }

            bool moveResult = false;

            //while (!moveResult) {
            if (_mind.IsLeader()) {
                moveResult = _mind.getBody().GoTo(_leaderGoal);
            }
            else {
                moveResult = _mind.getBody().GoTo(_goal);
            }


            if (!moveResult) {
                if (_mind.IsLeader()) {
                    Console.WriteLine("something went wrong while moving." + _goal + " " + _leaderGoal +
                                      " setting new Goal");
                    // _goal = SelectNextGoal();
                    // _leaderGoal = SelectNextGoalByCoverage();
                    // moveResult = _mind.getBody().GoTo(_leaderGoal);
                    if (!moveResult && !waitForTeammembers) {
                        _leaderGoal = SelectNextGoalByRandomCircleCalc();
                        _mind.getBody().GoTo(_leaderGoal);
                    }
                }
            }
            // }
        }
    }
}