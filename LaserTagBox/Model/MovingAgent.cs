using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model
{
    public abstract class MovingAgent : IAgent<PlayerBodyLayer>, IPositionable
    {
        public void Init(PlayerBodyLayer layer)
        {
            if (_initialized) throw new NotSupportedException();
            _initialized = true;

            Battleground = layer;
        }

        public virtual void Tick()
        {
            CurrentSpot = FindCurrentSpot();
        }

        private OccupiableSpot FindCurrentSpot()
        {
            return (OccupiableSpot) Battleground.SpotEnv.Entities.FirstOrDefault(spot => spot.Position.Equals(Position));
        }

        public Guid ID { get; set; }
        private bool _initialized;
        protected PlayerBodyLayer Battleground;

        //	*********************** movement attributes ***********************
        // values: standing, kneeling, lying
        public Stance Stance { get; protected set; } = Stance.Standing;
        private OccupiableSpot CurrentSpot { get; set; }

        //	*********************** exploration attributes ***********************
        private double VisualRangePenalty =>
            CurrentSpot switch
            {
                null => 0,
                Hill _ => 3,
                Ditch _ => -3,
                _ => 0
            };

        public double VisualRange =>
            VisualRangePenalty + Stance switch
            {
                Stance.Standing => 10,
                Stance.Kneeling => 8,
                Stance.Lying => 5,
                _ => throw new ArgumentOutOfRangeException()
            };

        private double VisibilityRangePenalty =>
            CurrentSpot switch
            {
                null => 0,
                Hill _ => 3,
                Ditch _ => -3,
                _ => 0
            };

        public double VisibilityRange =>
            VisibilityRangePenalty + Stance switch
            {
                Stance.Standing => 10,
                Stance.Kneeling => 8,
                Stance.Lying => 5,
                _ => throw new ArgumentOutOfRangeException()
            };

        public double MovementDelay =>
            Stance switch
            {
                Stance.Standing => 0,
                Stance.Kneeling => 2,
                Stance.Lying => 3,
                _ => throw new ArgumentOutOfRangeException()
            };

        public bool hasMoved { get; set; }

        public Position Position { get; set; }

        private double Xcor => Position.X;
        private double Ycor => Position.Y;

        //	*********************** pathfinding attributes ***********************
        public int g = 1000;
        public int rhs = 1000;
        public double km = 0.0;

        // tuple values: (<x>, <y>, <g>, <rhs>, <cost> <key1>, <key2>)
        public Tuple<double, double, int, int, int, double, double> sStart;
        public Tuple<double, double, int, int, int, double, double> sGoal;
        public Tuple<double, double, int, int, int, double, double> sLast;

        public List<Tuple<double, double, int, int, int, double, double>> routeList =
            new List<Tuple<double, double, int, int, int, double, double>>();

        public List<Tuple<double, double, int, int, int, double, double>> expandQueue =
            new List<Tuple<double, double, int, int, int, double, double>>();

        public bool pathCalculated = false;


        // USER METHOD: gets distance between agent&& #(x, y), if possible
        // return: distance|| -1.0 if distance can! be determined
        // private double getDistance(double x, double y)
        // {
        //     var position = Position.CreatePosition(x, y);
        //     var isInList = false;
        //     foreach (var fighter in enemiesFromTeam)
        //     {
        //         if (fighter.Position.Equals(position))
        //             isInList = true;
        //     }
        //
        //     if (!isInList) return -1.0;
        //
        //     var anyHills = hills.Any(a => a.Equals(position));
        //     var anyBarriers = barriers.Any(a => a.Equals(position));
        //     var anyDitches = ditches.Any(a => a.Equals(position));
        //     if (hasBeeline(x, y) || anyHills || anyBarriers || anyDitches)
        //     {
        //         return Math.Max(Math.Abs(Position.X - x), Math.Abs(Position.Y - y));
        //     }
        //
        //     return -1.0;
        // }
        // **************************************************************************************** 
// *********************************** MOVEMENT BEGIN *************************************
// ****************************************************************************************

        // USER METHOD: main method for pathfinding&& movement algorithm
        // implementation of modified D* Lite Algorithm for Agent pathfinding, movement,&& path readjustment in case of unforeseen obstacles
        // xGoal: x-coordinate of grid cell Guest wants to move to
        // yGoal: y-coordinate of grid cell Guest wants to move to
        // return: boolean states if step was successfully taken
        private bool goTo(double inXGoal, double inYGoal)
        {
            var xGoal = Math.Floor(inXGoal);
            var yGoal = Math.Floor(inYGoal);
            if ((int) Battleground[xGoal, yGoal] == 1)
            {
                hasMoved = true;
                return false;
            }

            if ((MovementDelay > 0) || hasMoved || ((xGoal == Position.X) && (yGoal == Position.Y)))
            {
                return false;
            }

            if (xGoal != sGoal.Item1 || yGoal != sGoal.Item2)
            {
                pathCalculated = false;
            }

            if (!pathCalculated)
            {
                initPathfinding(xGoal, yGoal);
            }

            if (!pathCalculated)
            {
                return false;
            }

            sStart = findNextCell();
            var neighsInRouteList = findNeighsInRouteList();
            var neighsWithChangedCost = scanCostChanges(neighsInRouteList);
            if (neighsWithChangedCost.Count != 0)
            {
                // calculation of completely new path
                var newExpandQueue = new List<Tuple<double, double, int, int, int, double, double>>();
                expandQueue = newExpandQueue;
                var newRouteList = new List<Tuple<double, double, int, int, int, double, double>>();
                routeList = newRouteList;
                var keyInit = Math.Max(Math.Abs(Xcor - xGoal), Math.Abs(Ycor - yGoal));
                sGoal = new Tuple<double, double, int, int, int, double, double>(xGoal, yGoal, g, 0,
                    Battleground.GetintValue(xGoal, yGoal), keyInit, 0.0);
                expandQueue.Add(sGoal);
                km = 0.0;
                sStart = new Tuple<double, double, int, int, int, double, double>(Xcor, Ycor, g, rhs,
                    (int) Battleground[Position.X, Position.Y], 1000.0, 1000.0);
                sLast = sStart;
                computeShortestPath(sStart, km);
                sStart = findNextCell();
            }

            var position = Position.CreatePosition(sStart.Item1, sStart.Item2);
            if (Battleground.GetintValue(sStart.Item1, sStart.Item2) == 2)
            {
                var hill = Battleground.NearestHill(position);
                if (!hill.Free)
                {
                    pathCalculated = false;
                    return false;
                }
            }
            else if (Battleground.GetintValue(sStart.Item1, sStart.Item2) == 3)
            {
                var ditch = Battleground.NearestDitch(position);
                if (!ditch.Free)
                {
                    pathCalculated = false;
                    return false;
                }
            }

            moveMe(sStart.Item1, sStart.Item2);
            if ((Xcor == sGoal.Item1) && (Ycor == sGoal.Item2))
            {
                pathCalculated = false;
            }

            return true;
        }

        // computes initial route from (xcor, ycor) to (<xGoal>, <yGoal>)
        // xGoal: x-coordinate of grid cell Guest wants to move to
        // yGoal: y-coordinate of grid cell Guest wants to move to
        private void initPathfinding(double xGoal, double yGoal)
        {
            var costSStart = (int) Battleground[Position.X, Position.Y];
            sStart = new Tuple<double, double, int, int, int, double, double>(Xcor, Ycor, g, rhs, costSStart, 1000.0,
                1000.0);
            sLast = sStart;
            var keyInit = Math.Max(Math.Abs(Xcor - xGoal), Math.Abs(Ycor - yGoal));
            var costSGoal = Battleground.GetintValue(xGoal, yGoal);
            sGoal = new Tuple<double, double, int, int, int, double, double>(xGoal, yGoal, g, 0, costSGoal, keyInit,
                0.0);
            expandQueue = new List<Tuple<double, double, int, int, int, double, double>>();
            routeList = new List<Tuple<double, double, int, int, int, double, double>>();
            expandQueue.Add(sGoal);
            pathCalculated = true;
            computeShortestPath(sStart, km);
        }

        // computes shortest path from current position to goal
        // sStart: tuple representing grid cell from which Guest starts moving towards goal
        // km: key modifier value
        private void computeShortestPath(Tuple<double, double, int, int, int, double,
            double> sStart, double km)
        {
            Tuple<double, double, int, int, int, double, double> u = getTopKey();
            while ((u.Item6 != 0.0) && ((u.Item6 < calcKey1(sStart, km)) || (sStart.Item4 > sStart.Item3)))
            {
                var kOld1 = u.Item6;
                var kNew1 = calcKey1(u, km);
                var kNew2 = calcKey2(u);
                if (kOld1 < kNew1)
                {
                    var v = new Tuple<double, double, int, int, int, double, double>(u.Item1, u.Item2, u.Item3, u.Item4,
                        u.Item5, kNew1, kNew2);
                    updateQueue(u, v);
                }
                else if (u.Item3 > u.Item4)
                {
                    var uNew = new Tuple<double, double, int, int, int, double, double>(u.Item1, u.Item2, u.Item4,
                        u.Item4, u.Item5, u.Item6, u.Item7);
                    routeList.Add(uNew);
                    var NeighList = Neigh(uNew);
                    removeFromQueue(u);

                    foreach (var s in NeighList)
                    {
                        if ((s.Item1 != sGoal.Item1) || (s.Item2 != sGoal.Item2))
                        {
                            var newRHS = RHS(s, uNew);
                            var sNew = new Tuple<double, double, int, int, int, double, double>(s.Item1, s.Item2,
                                s.Item3, newRHS, s.Item5, s.Item6, s.Item7);
                            updateVertex(sNew, km);
                        }
                    }
                }
                else
                {
                    var gOld = u.Item3;
                    var uNew = new Tuple<double, double, int, int, int, double, double>(u.Item1, u.Item2, g, u.Item4,
                        u.Item5, u.Item6, u.Item7);
                    updateQueue(u, uNew);
                    var neighList = Neigh(uNew);
                    neighList.Add(uNew);
                    foreach (var s in neighList)
                    {
                        var updated = false;
                        if (s.Item4 == (u.Item5 + gOld))
                        {
                            if ((s.Item1 != sGoal.Item1) || (s.Item2 != sGoal.Item2))
                            {
                                var neighListTemp = Neigh(s);
                                var newRHS = getMinRHS(s, neighListTemp);
                                var newS = new Tuple<double, double, int, int, int, double, double>(s.Item1, s.Item2,
                                    s.Item3, newRHS, s.Item5, s.Item6, s.Item7);
                                updateVertex(newS, km);
                                updated = true;
                            }
                        }

                        if (!updated)
                        {
                            updateVertex(s, km);
                        }
                    }
                }

                u = getTopKey();
                if ((u.Item1 == -1.0) && (u.Item2 == -1.0))
                {
                    return;
                }
            }
        }

        // determines which cell an Guest should move to next
        // return: tuple representing grid cell that Guest needs to move to
        private Tuple<double, double, int, int, int, double, double> findNextCell()
        {
            var nextCellCandidates = findNeighsInRouteList();
            var minNeigh = nextCellCandidates[0];
            foreach (var neigh in nextCellCandidates)
            {
                if ((cost(neigh, sStart) + neigh.Item3) < (cost(minNeigh, sStart) + minNeigh.Item3))
                {
                    minNeigh = neigh;
                }
                else if ((cost(neigh, sStart) + neigh.Item3) == (cost(minNeigh, sStart) + minNeigh.Item3))
                {
                    if ((Xcor == neigh.Item1) || (Ycor == neigh.Item2))
                    {
                        minNeigh = neigh;
                    }
                }
            }

            return minNeigh;
        }

        // creates a list of tuples representing grid cells that are neighbors of sStart&& that are in routeList
        // return: list of tuples
        private List<Tuple<double, double, int, int, int, double, double>> findNeighsInRouteList()
        {
            var nextCellCandidates = new List<Tuple<double, double, int, int, int, double, double>>();
            foreach (var s in routeList)
            {
                if (heur(sStart, s) == 1.0)
                {
                    nextCellCandidates.Add(s);
                }
            }

            return nextCellCandidates;
        }

        // determines for all elements of List if their cost is ! equal to 1.
        // List: neighbors of sStart
        // return: list with neighbors of sStart that are in routeList&& whose cost is ! equal to 1
        private List<Tuple<double, double, int, int, int, double, double>> scanCostChanges(
            List<Tuple<double, double, int, int, int, double, double>> neighsinRouteList)
        {
            int currCostNeigh;
            var neighsWithChangedCost = new List<Tuple<double, double, int, int, int, double, double>>();
            foreach (var currNeigh in neighsinRouteList)
            {
                currCostNeigh = cost(currNeigh, sStart);
                if (currCostNeigh != currNeigh.Item5)
                {
                    neighsWithChangedCost.Add(currNeigh);
                }
            }

            return neighsWithChangedCost;
        }

        // obtains the next cell that should be examined during pathfinding
        // return: tuple with most favorable key1&& key2
        private Tuple<double, double, int, int, int, double, double> getTopKey()
        {
            if (expandQueue.Count == 0)
            {
                pathCalculated = false;
                return new Tuple<double, double, int, int, int, double, double>(-1.0, -1.0, 0, 0, 0, 0.0, 0.0);
            }

            var minKey1 = expandQueue[0].Item6;
            var minKey2 = expandQueue[0].Item7;
            foreach (var currElem in expandQueue)
            {
                if (currElem.Item6 < minKey1)
                {
                    minKey1 = currElem.Item6;
                    minKey2 = currElem.Item7;
                }
                else if ((currElem.Item6 == minKey1) && (currElem.Item7 < minKey2))
                {
                    minKey2 = currElem.Item7;
                }
            }

            Tuple<double, double, int, int, int, double, double> minKeyTuple = null;
            for (var i = 0; i < expandQueue.Count; i++)
            {
                if ((expandQueue[i].Item6 == minKey1) && (expandQueue[i].Item7 == minKey2))
                {
                    minKeyTuple = expandQueue[i];
                    i = expandQueue.Count;
                }
            }

            return minKeyTuple;
        }

        // calculates the first part of the key of a grid cell (using heuristic)
        // s: tuple for which key1 is to be calculated
        // km: key modifier value
        // return: key1 value of s
        private double calcKey1(Tuple<double, double, int, int, int, double, double> s, double km)
        {
            return Math.Min(s.Item3, s.Item4) + heur(sStart, s) + km;
        }

        // heuristic: takes the larger value of the x-range&& y-range between two grid cells
        // s1: tuple representing the first grid cell to be used in heuristic calculation
        // s2: tuple representing the second grid cell to be used in heuristic calculation
        // return: result of heuristic calculation
        private double heur(Tuple<double, double, int, int, int, double, double> s1,
            Tuple<double, double, int, int, int, double, double> s2)
        {
            return Math.Max(Math.Abs(s1.Item1 - s2.Item1), Math.Abs(s1.Item2 - s2.Item2));
        }

        // calculates the second part of the key of a grid cell
        // s: tuple for which key2 is to be calculated
        // return: key2 value of s
        private double calcKey2(Tuple<double, double, int, int, int, double, double> s)
        {
            return Math.Min(s.Item3, s.Item4);
        }

        // updates expandQueue by calling removeFromQueue method&& adding a new tuple
        // u: tuple to be removed from expandQueue
        // newU: tuple to be added to expandQueue
        private void updateQueue(Tuple<double, double, int, int, int, double, double> u,
            Tuple<double, double, int, int, int, double, double> newU)
        {
            removeFromQueue(u);
            expandQueue.Add(newU);
        }

        // updates routeList by creating a copy&& adding s&& all entries from old routeList unequal to s to it
        // s: new tuple to be added to routeList to replace old version of s
        private void updateRouteList(Tuple<double, double, int, int, int, double, double> s)
        {
            var newRouteList = new List<Tuple<double, double, int, int, int, double, double>>();
            foreach (var entry in routeList)
            {
                if ((entry.Item1 != s.Item1) || (entry.Item2 != s.Item2))
                {
                    newRouteList.Add(entry);
                }

                if ((entry.Item1 == s.Item1) && (entry.Item2 == s.Item2))
                {
                    newRouteList.Add(s);
                }
            }

            routeList = newRouteList;
        }

        // removes a tuple from expandQueue
        // u: tuple to be removed from expandQueue
        private void removeFromQueue(Tuple<double, double, int, int, int, double, double> u)
        {
            var newExpandQueue = new List<Tuple<double, double, int, int, int, double, double>>();
            foreach (var s in expandQueue)
            {
                if ((s.Item1 != u.Item1) || ((s.Item2 != u.Item2)))
                {
                    newExpandQueue.Add(s);
                }
            }

            expandQueue = newExpandQueue;
        }

        // finds all grid cells neighboring s (and whether they have representative tuples in routeList|| expandQueue)
        // s: tuple representing grid cell of which neighbors are to be found
        // return: list containing tuples representing neighbors of s
        private List<Tuple<double, double, int, int, int, double, double>> Neigh(
            Tuple<double, double, int, int, int, double, double> s)
        {
            var NeighList = new List<Tuple<double, double, int, int, int, double, double>>();
            double x;
            double y;
            for (x = s.Item1 - 1.0; x <= s.Item1 + 1.0; x++)
            {
                for (y = s.Item2 - 1.0; y <= s.Item2 + 1.0; y++)
                {
                    if ((x != s.Item1) || (y != s.Item2))
                    {
                        var found = false;
                        foreach (var r in routeList)
                        {
                            if ((r.Item1 == x) && (r.Item2 == y))
                            {
                                NeighList.Add(r);
                                found = true;
                            }
                        }

                        foreach (var e in expandQueue)
                        {
                            if ((e.Item1 == x) && (e.Item2 == y))
                            {
                                NeighList.Add(e);
                                found = true;
                            }
                        }

                        if (!found)
                        {
                            var neigh = new Tuple<double, double, int, int, int, double, double>(x, y, g, rhs, 0,
                                1000.0, 1000.0);
                            var costNeigh = cost(neigh, s);
                            if (costNeigh != 1000)
                            {
                                var newNeigh =
                                    new Tuple<double, double, int, int, int, double, double>(x, y, g, rhs, costNeigh,
                                        1000.0, 1000.0);
                                NeighList.Add(newNeigh);
                            }
                        }
                    }
                }
            }

            return NeighList;
        }

        // calculates the rhs value of tuple based on a!her tuple
        // s: tuple representing grid cell neighboring u
        // u: tuple representing grid cell currently being examined
        // return: updated rhs value
        private int RHS(Tuple<double, double, int, int, int, double, double> s,
            Tuple<double, double, int, int, int, double, double> u)
        {
            if ((s.Item1 == sStart.Item1) && (s.Item2 == sStart.Item2))
            {
                return 0;
            }

            return Math.Min(s.Item4, (cost(u, s) + u.Item3));
        }

        // updates tuple u by adding it to|| removing it from expandQueue|| changing its information
        // u: tuple to be updated
        // km: key modifier value
        private void updateVertex(Tuple<double, double, int, int, int, double, double> u, double km)
        {
            var isInQueue = inQueue(u);
            var newKey1 = calcKey1(u, km);
            var newKey2 = calcKey2(u);
            if ((u.Item3 != u.Item4) && isInQueue)
            {
                var uNew = new Tuple<double, double, int, int, int, double, double>(u.Item1, u.Item2, u.Item3, u.Item4,
                    u.Item5, newKey1, newKey2);
                updateQueue(u, uNew);
            }
            else if ((u.Item3 != u.Item4) && (!isInQueue))
            {
                var uNew = new Tuple<double, double, int, int, int, double, double>(u.Item1, u.Item2, u.Item3, u.Item4,
                    u.Item5, newKey1, newKey2);
                expandQueue.Add(uNew);
            }
            else if ((u.Item3 == u.Item4) && (isInQueue))
            {
                routeList.Add(u);
                removeFromQueue(u);
            }
        }

        // checks if a tuple is in expandQueue
        // u: tuple to be checked
        // return: boolean
        private bool inQueue(Tuple<double, double, int, int, int, double, double> u)
        {
            foreach (var element in expandQueue)
            {
                if ((element.Item1 == u.Item1) && (element.Item2 == u.Item2))
                {
                    return true;
                }
            }

            return false;
        }

        // checks if a tuple is in routeList
        // u: tuple to be checked
        // return: boolean
        private bool inRouteList(Tuple<double, double, int, int, int, double, double> u)
        {
            foreach (var element in routeList)
            {
                if ((element.Item1 == u.Item1) && (element.Item2 == u.Item2))
                {
                    return true;
                }
            }

            return false;
        }

        // computes cost of moving from s to u&& checks numeric value of s on grid layer
        // s: tuple representing the first grid cell to be used in cost calculation
        // u: tuple representing the second grid cell to be used in cost calculation
        // return: cost value on s
        private int cost(Tuple<double, double, int, int, int, double, double> s,
            Tuple<double, double, int, int, int, double, double> u)
        {
            var dist = heur(s, u);
            if (dist == 1.0)
            {
                if (Battleground[s.Item1, s.Item2] == 1)
                {
                    return 1000;
                }
                else if ((Battleground[s.Item1, s.Item2] == 2) || (Battleground[s.Item1, s.Item2] == 3))
                {
                    return 100;
                }
                else
                {
                    return 1;
                }
            }
            else if (dist < 1.0)
            {
                return 0;
            }
            else
            {
                Console.WriteLine("s&& u are ! neighbors!");
            }

            return 1;
        }

        // obtains minimum rhs-value of all neighbors of s
        // s: tuple representing grid cell whose neighbors minimum rhs value is to be determined
        // neighListTemp: list of tuples representing grid cells that neighbor s
        // return: smallest rhs values among neighbors of s
        private int getMinRHS(Tuple<double, double, int, int, int, double, double> s, List<
            Tuple<double, double, int, int, int, double, double>> neighListTemp)
        {
            var minRHS = neighListTemp[0].Item3 + cost(neighListTemp[0], s);
            foreach (var neigh in neighListTemp)
            {
                var compRHS = neigh.Item3 + cost(neigh, s);
                if (compRHS < minRHS)
                {
                    minRHS = compRHS;
                }
            }

            return minRHS;
        }

        // prints a list of tuples
        // list: list of tuples to be printed
        private void printQueue(List<Tuple<double, double, int, int, int, double, double>> list)
        {
            foreach (var tuple in list)
            {
                Console.WriteLine(tuple);
            }
        }

        // moves agent, if possible, onto the grid cell with coordinates (x, y)
        private void moveMe(double x, double y)
        {
            if (CurrentSpot != null)
                CurrentSpot.Free = true;

            Position = MoveToPosition(Position.CreatePosition(x, y));

            var newSpot = FindCurrentSpot();
            if(newSpot != null)
                newSpot.Free = false;
            
            hasMoved = true;
        }

        protected abstract Position MoveToPosition(Position position);
    }
}