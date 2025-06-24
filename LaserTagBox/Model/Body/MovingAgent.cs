using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using LaserTagBox.Model.Spots;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Body;

/// <summary>
///     Encapsulates the movement properties and capabilities of a LaserTag agent 
/// </summary>
public abstract class MovingAgent : IAgent<PlayerBodyLayer>, IPositionable
{
    #region Fields
    private bool _initialized;
    private int _memberId;
    private int _xSpawn;
    private int _ySpawn;
    private Color _color;
    #endregion
    
    #region Properties
    public Guid ID { get; set; }
    
    protected PlayerBodyLayer Battleground;
    
    private OccupiableSpot CurrentSpot { get; set; }

    [PropertyDescription(Name = "memberId")]
    public int MemberId
    {
        get => _memberId;
        set
        {
            if (_initialized) throw new NotSupportedException();
            _memberId = value;
        }
    }

    [PropertyDescription(Name = "xSpawn")]
    public int XSpawn
    {
        get => _xSpawn;
        set
        {
            if (_initialized) throw new NotSupportedException();
            _xSpawn = value;
        }
    }

    [PropertyDescription(Name = "ySpawn")]
    public int YSpawn
    {
        get => _ySpawn;
        set
        {
            if (_initialized) throw new NotSupportedException();
            _ySpawn = value;
        }
    }

    [PropertyDescription(Name = "team")]
    public Color Color
    {
        get => _color;
        set
        {
            if (_initialized) throw new NotSupportedException();
            _color = value;
        }
    }
    
    public Stance Stance { get; protected set; } = Stance.Standing;
    protected double MovementDelayCounter { get; set; }
    protected bool HasMoved { get; set; }
    public Position Position { get; set; }
    public double XCor => Position.X;
    public double YCor => Position.Y;
    #endregion

    #region Initialization
    public void Init(PlayerBodyLayer layer)
    {
        if (_initialized) throw new NotSupportedException();
        _initialized = true;

        Battleground = layer;
        Position = Position.CreatePosition(XSpawn, YSpawn);
        InsertIntoEnv();

        _sGoal = new Tuple<double, double, int, int, int, double, double>(XSpawn, YSpawn, G, Rhs, 0, 0.0, 0.0);
    }
    #endregion

    #region Tick
    public virtual void Tick()
    {
        CurrentSpot = FindCurrentSpot();
    }
    #endregion

    #region Methods
    private OccupiableSpot FindCurrentSpot()
    {
        return (OccupiableSpot) Battleground.SpotEnv.Entities.FirstOrDefault(spot =>
            spot.Position.Equals(Position));
    }
    #endregion

    //	*********************** movement attributes ***********************
    // values: standing, kneeling, lying

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

    protected double MovementDelayPenalty => Stance switch
    {
        Stance.Standing => 0,
        Stance.Kneeling => 2,
        Stance.Lying => 3,
        _ => throw new ArgumentOutOfRangeException()
    };

    //	*********************** pathfinding attributes ***********************
    private const int G = 1000;
    private const int Rhs = 1000;
    private double _km = 0.0;

    // tuple values: (<x>, <y>, <g>, <rhs>, <cost> <key1>, <key2>)
    private Tuple<double, double, int, int, int, double, double> _sStart;
    private Tuple<double, double, int, int, int, double, double> _sGoal;
    private Tuple<double, double, int, int, int, double, double> _sLast;

    private List<Tuple<double, double, int, int, int, double, double>> _routeList = new();

    private List<Tuple<double, double, int, int, int, double, double>> _expandQueue = new();

    private bool _pathCalculated;

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
    protected bool TryMoveTo(Position goal) //TODO return type provides information about result
    {
        var xGoal = Math.Floor(goal.X);
        var yGoal = Math.Floor(goal.Y);
        
        if (!Battleground.IsInRaster(goal))
        {
            return false;
        }
        
        if ((int) Battleground[xGoal, yGoal] == 1 || (int) Battleground[xGoal, yGoal] == 4 || (int) Battleground[xGoal, yGoal] == 5)
        {
            HasMoved = true;
            return false;
        }

        if (MovementDelayCounter > 0 || HasMoved || (xGoal == Position.X && yGoal == Position.Y))
        {
            return false;
        }

        MovementDelayCounter = MovementDelayPenalty;

        if (xGoal != _sGoal.Item1 || yGoal != _sGoal.Item2)
        {
            _pathCalculated = false;
        }

        if (!_pathCalculated)
        {
            InitPathfinding(xGoal, yGoal);
        }

        if (!_pathCalculated)
        {
            return false;
        }

        _sStart = FindNextCell();
        var neighsInRouteList = FindNeighsInRouteList();
        var neighsWithChangedCost = ScanCostChanges(neighsInRouteList);
        if (neighsWithChangedCost.Count != 0)
        {
            // calculation of completely new path
            var newExpandQueue = new List<Tuple<double, double, int, int, int, double, double>>();
            _expandQueue = newExpandQueue;
            var newRouteList = new List<Tuple<double, double, int, int, int, double, double>>();
            _routeList = newRouteList;
            var keyInit = Math.Max(Math.Abs(XCor - xGoal), Math.Abs(YCor - yGoal));
            _sGoal = new Tuple<double, double, int, int, int, double, double>(xGoal, yGoal, G, 0,
                Battleground.GetIntValue(xGoal, yGoal), keyInit, 0.0);
            _expandQueue.Add(_sGoal);
            _km = 0.0;
            _sStart = new Tuple<double, double, int, int, int, double, double>(XCor, YCor, G, Rhs,
                (int) Battleground[Position.X, Position.Y], 1000.0, 1000.0);
            _sLast = _sStart;
            ComputeShortestPath(_sStart, _km);
            _sStart = FindNextCell();
        }

        var position = Position.CreatePosition(_sStart.Item1, _sStart.Item2);
        if (Battleground.GetIntValue(_sStart.Item1, _sStart.Item2) == 2)
        {
            var hill = Battleground.NearestHill(position);
            if (!hill.Free)
            {
                _pathCalculated = false;
                return false;
            }
        }
        else if (Battleground.GetIntValue(_sStart.Item1, _sStart.Item2) == 3)
        {
            var ditch = Battleground.NearestDitch(position);
            if (!ditch.Free)
            {
                _pathCalculated = false;
                return false;
            }
        }

        MoveMe(_sStart.Item1, _sStart.Item2);
        if (XCor == _sGoal.Item1 && YCor == _sGoal.Item2)
        {
            _pathCalculated = false;
        }

        return true;
    }

    // computes initial route from (xcor, ycor) to (<xGoal>, <yGoal>)
    // xGoal: x-coordinate of grid cell Guest wants to move to
    // yGoal: y-coordinate of grid cell Guest wants to move to
    private void InitPathfinding(double xGoal, double yGoal)
    {
        var costSStart = (int) Battleground[Position.X, Position.Y];
        _sStart = new Tuple<double, double, int, int, int, double, double>(XCor, YCor, G, Rhs, costSStart, 1000.0,
            1000.0);
        _sLast = _sStart;
        var keyInit = Math.Max(Math.Abs(XCor - xGoal), Math.Abs(YCor - yGoal));
        var costSGoal = Battleground.GetIntValue(xGoal, yGoal);
        _sGoal = new Tuple<double, double, int, int, int, double, double>(xGoal, yGoal, G, 0, costSGoal, keyInit,
            0.0);
        _expandQueue = new List<Tuple<double, double, int, int, int, double, double>>();
        _routeList = new List<Tuple<double, double, int, int, int, double, double>>();
        _expandQueue.Add(_sGoal);
        _pathCalculated = true;
        ComputeShortestPath(_sStart, _km);
    }

    // computes shortest path from current position to goal
    // sStart: tuple representing grid cell from which Guest starts moving towards goal
    // km: key modifier value
    private void ComputeShortestPath(Tuple<double, double, int, int, int, double,
        double> sStart, double km)
    {
        var u = GetTopKey();
        while (u.Item6 != 0.0 && (u.Item6 < CalcKey1(sStart, km) || sStart.Item4 > sStart.Item3))
        {
            var kOld1 = u.Item6;
            var kNew1 = CalcKey1(u, km);
            var kNew2 = CalcKey2(u);
            if (kOld1 < kNew1)
            {
                var v = new Tuple<double, double, int, int, int, double, double>(u.Item1, u.Item2, u.Item3, u.Item4,
                    u.Item5, kNew1, kNew2);
                UpdateQueue(u, v);
            }
            else if (u.Item3 > u.Item4)
            {
                var uNew = new Tuple<double, double, int, int, int, double, double>(u.Item1, u.Item2, u.Item4,
                    u.Item4, u.Item5, u.Item6, u.Item7);
                _routeList.Add(uNew);
                var neighbors = Neigh(uNew);
                RemoveFromQueue(u);

                foreach (var s in neighbors)
                {
                    if (s.Item1 != _sGoal.Item1 || s.Item2 != _sGoal.Item2)
                    {
                        var newRhs = RHS(s, uNew);
                        var sNew = new Tuple<double, double, int, int, int, double, double>(s.Item1, s.Item2,
                            s.Item3, newRhs, s.Item5, s.Item6, s.Item7);
                        UpdateVertex(sNew, km);
                    }
                }
            }
            else
            {
                var gOld = u.Item3;
                var uNew = new Tuple<double, double, int, int, int, double, double>(u.Item1, u.Item2, G, u.Item4,
                    u.Item5, u.Item6, u.Item7);
                UpdateQueue(u, uNew);
                var neighList = Neigh(uNew);
                neighList.Add(uNew);
                foreach (var s in neighList)
                {
                    var updated = false;
                    if (s.Item4 == (u.Item5 + gOld))
                    {
                        if (s.Item1 != _sGoal.Item1 || s.Item2 != _sGoal.Item2)
                        {
                            var neighListTemp = Neigh(s);
                            var newRHS = GetMinRhs(s, neighListTemp);
                            var newS = new Tuple<double, double, int, int, int, double, double>(s.Item1, s.Item2,
                                s.Item3, newRHS, s.Item5, s.Item6, s.Item7);
                            UpdateVertex(newS, km);
                            updated = true;
                        }
                    }

                    if (!updated)
                    {
                        UpdateVertex(s, km);
                    }
                }
            }

            u = GetTopKey();
            if (u.Item1 == -1.0 && u.Item2 == -1.0)
            {
                return;
            }
        }
    }

    // determines which cell an Guest should move to next
    // return: tuple representing grid cell that Guest needs to move to
    private Tuple<double, double, int, int, int, double, double> FindNextCell()
    {
        var nextCellCandidates = FindNeighsInRouteList();
        var minNeigh = nextCellCandidates[0];
        foreach (var neigh in nextCellCandidates)
        {
            if ((Cost(neigh, _sStart) + neigh.Item3) < (Cost(minNeigh, _sStart) + minNeigh.Item3))
            {
                minNeigh = neigh;
            }
            else if (Cost(neigh, _sStart) + neigh.Item3 == Cost(minNeigh, _sStart) + minNeigh.Item3)
            {
                if (XCor == neigh.Item1 || YCor == neigh.Item2)
                {
                    minNeigh = neigh;
                }
            }
        }

        return minNeigh;
    }

    // creates a list of tuples representing grid cells that are neighbors of sStart&& that are in routeList
    // return: list of tuples
    private List<Tuple<double, double, int, int, int, double, double>> FindNeighsInRouteList()
    {
        var nextCellCandidates = new List<Tuple<double, double, int, int, int, double, double>>();
        foreach (var s in _routeList)
        {
            if (Heur(_sStart, s) == 1.0)
            {
                nextCellCandidates.Add(s);
            }
        }

        return nextCellCandidates;
    }

    // determines for all elements of List if their cost is ! equal to 1.
    // List: neighbors of sStart
    // return: list with neighbors of sStart that are in routeList&& whose cost is ! equal to 1
    private List<Tuple<double, double, int, int, int, double, double>> ScanCostChanges(
        List<Tuple<double, double, int, int, int, double, double>> neighborsInRouteList)
    {
        var neighsWithChangedCost = new List<Tuple<double, double, int, int, int, double, double>>();
        foreach (var currNeigh in neighborsInRouteList)
        {
            var currentCostNeighbor = Cost(currNeigh, _sStart);
            if (currentCostNeighbor != currNeigh.Item5)
            {
                neighsWithChangedCost.Add(currNeigh);
            }
        }

        return neighsWithChangedCost;
    }

    // obtains the next cell that should be examined during pathfinding
    // return: tuple with most favorable key1&& key2
    private Tuple<double, double, int, int, int, double, double> GetTopKey()
    {
        if (_expandQueue.Count == 0)
        {
            _pathCalculated = false;
            return new Tuple<double, double, int, int, int, double, double>(-1.0, -1.0, 0, 0, 0, 0.0, 0.0);
        }

        var minKey1 = _expandQueue[0].Item6;
        var minKey2 = _expandQueue[0].Item7;
        foreach (var currElem in _expandQueue)
        {
            if (currElem.Item6 < minKey1)
            {
                minKey1 = currElem.Item6;
                minKey2 = currElem.Item7;
            }
            else if (currElem.Item6 == minKey1 && currElem.Item7 < minKey2)
            {
                minKey2 = currElem.Item7;
            }
        }

        Tuple<double, double, int, int, int, double, double> minKeyTuple = null;
        for (var i = 0; i < _expandQueue.Count; i++)
        {
            if (_expandQueue[i].Item6 == minKey1 && _expandQueue[i].Item7 == minKey2)
            {
                minKeyTuple = _expandQueue[i];
                i = _expandQueue.Count;
            }
        }

        return minKeyTuple;
    }

    // calculates the first part of the key of a grid cell (using heuristic)
    // s: tuple for which key1 is to be calculated
    // km: key modifier value
    // return: key1 value of s
    private double CalcKey1(Tuple<double, double, int, int, int, double, double> s, double km)
    {
        return Math.Min(s.Item3, s.Item4) + Heur(_sStart, s) + km;
    }

    // heuristic: takes the larger value of the x-range&& y-range between two grid cells
    // s1: tuple representing the first grid cell to be used in heuristic calculation
    // s2: tuple representing the second grid cell to be used in heuristic calculation
    // return: result of heuristic calculation
    private double Heur(Tuple<double, double, int, int, int, double, double> s1,
        Tuple<double, double, int, int, int, double, double> s2)
    {
        return Math.Max(Math.Abs(s1.Item1 - s2.Item1), Math.Abs(s1.Item2 - s2.Item2));
    }

    // calculates the second part of the key of a grid cell
    // s: tuple for which key2 is to be calculated
    // return: key2 value of s
    private double CalcKey2(Tuple<double, double, int, int, int, double, double> s)
    {
        return Math.Min(s.Item3, s.Item4);
    }

    // updates expandQueue by calling removeFromQueue method&& adding a new tuple
    // u: tuple to be removed from expandQueue
    // newU: tuple to be added to expandQueue
    private void UpdateQueue(Tuple<double, double, int, int, int, double, double> u,
        Tuple<double, double, int, int, int, double, double> newU)
    {
        RemoveFromQueue(u);
        _expandQueue.Add(newU);
    }

    // updates routeList by creating a copy&& adding s&& all entries from old routeList unequal to s to it
    // s: new tuple to be added to routeList to replace old version of s
    private void UpdateRouteList(Tuple<double, double, int, int, int, double, double> s)
    {
        var newRouteList = new List<Tuple<double, double, int, int, int, double, double>>();
        foreach (var entry in _routeList)
        {
            if (entry.Item1 != s.Item1 || entry.Item2 != s.Item2)
            {
                newRouteList.Add(entry);
            }

            if (entry.Item1 == s.Item1 && entry.Item2 == s.Item2)
            {
                newRouteList.Add(s);
            }
        }

        _routeList = newRouteList;
    }

    // removes a tuple from expandQueue
    // u: tuple to be removed from expandQueue
    private void RemoveFromQueue(Tuple<double, double, int, int, int, double, double> u)
    {
        var newExpandQueue = new List<Tuple<double, double, int, int, int, double, double>>();
        foreach (var s in _expandQueue)
        {
            if (s.Item1 != u.Item1 || (s.Item2 != u.Item2))
            {
                newExpandQueue.Add(s);
            }
        }

        _expandQueue = newExpandQueue;
    }

    // finds all grid cells neighboring s (and whether they have representative tuples in routeList|| expandQueue)
    // s: tuple representing grid cell of which neighbors are to be found
    // return: list containing tuples representing neighbors of s
    private List<Tuple<double, double, int, int, int, double, double>> Neigh(
        Tuple<double, double, int, int, int, double, double> s)
    {
        var neighbors = new List<Tuple<double, double, int, int, int, double, double>>();
        for (var x = s.Item1 - 1.0; x <= s.Item1 + 1.0; x++)
        {
            for (var y = s.Item2 - 1.0; y <= s.Item2 + 1.0; y++)
            {
                if (x != s.Item1 || y != s.Item2)
                {
                    var found = false;
                    foreach (var r in _routeList)
                    {
                        if (r.Item1 == x && r.Item2 == y)
                        {
                            neighbors.Add(r);
                            found = true;
                        }
                    }

                    foreach (var e in _expandQueue)
                    {
                        if (e.Item1 == x && e.Item2 == y)
                        {
                            neighbors.Add(e);
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        var neighbor = new Tuple<double, double, int, int, int, double, double>(x, y, G, Rhs, 0,
                            1000.0, 1000.0);
                        var costNeighbor = Cost(neighbor, s);
                        if (costNeighbor != 1000)
                        {
                            var newNeighbor =
                                new Tuple<double, double, int, int, int, double, double>(x, y, G, Rhs, costNeighbor,
                                    1000.0, 1000.0);
                            neighbors.Add(newNeighbor);
                        }
                    }
                }
            }
        }

        return neighbors;
    }

    // calculates the rhs value of tuple based on a!her tuple
    // s: tuple representing grid cell neighboring u
    // u: tuple representing grid cell currently being examined
    // return: updated rhs value
    private int RHS(Tuple<double, double, int, int, int, double, double> s,
        Tuple<double, double, int, int, int, double, double> u)
    {
        if (s.Item1 == _sStart.Item1 && s.Item2 == _sStart.Item2)
        {
            return 0;
        }

        return Math.Min(s.Item4, Cost(u, s) + u.Item3);
    }

    // updates tuple u by adding it to|| removing it from expandQueue|| changing its information
    // u: tuple to be updated
    // km: key modifier value
    private void UpdateVertex(Tuple<double, double, int, int, int, double, double> u, double km)
    {
        var isInQueue = InQueue(u);
        var newKey1 = CalcKey1(u, km);
        var newKey2 = CalcKey2(u);
        if (u.Item3 != u.Item4 && isInQueue)
        {
            var uNew = new Tuple<double, double, int, int, int, double, double>(u.Item1, u.Item2, u.Item3, u.Item4,
                u.Item5, newKey1, newKey2);
            UpdateQueue(u, uNew);
        }
        else if (u.Item3 != u.Item4 && !isInQueue)
        {
            var uNew = new Tuple<double, double, int, int, int, double, double>(u.Item1, u.Item2, u.Item3, u.Item4,
                u.Item5, newKey1, newKey2);
            _expandQueue.Add(uNew);
        }
        else if (u.Item3 == u.Item4 && isInQueue)
        {
            _routeList.Add(u);
            RemoveFromQueue(u);
        }
    }

    // checks if a tuple is in expandQueue
    // u: tuple to be checked
    // return: boolean
    private bool InQueue(Tuple<double, double, int, int, int, double, double> u)
    {
        foreach (var element in _expandQueue)
        {
            if (element.Item1 == u.Item1 && element.Item2 == u.Item2)
            {
                return true;
            }
        }

        return false;
    }

    // checks if a tuple is in routeList
    // u: tuple to be checked
    // return: boolean
    private bool InRouteList(Tuple<double, double, int, int, int, double, double> u)
    {
        foreach (var element in _routeList)
        {
            if (element.Item1 == u.Item1 && element.Item2 == u.Item2)
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
    private int Cost(Tuple<double, double, int, int, int, double, double> s,
        Tuple<double, double, int, int, int, double, double> u)
    {
        var dist = Heur(s, u);
        if (dist == 1.0)
        {
            if (Battleground[s.Item1, s.Item2] == 1 || Battleground[s.Item1, s.Item2] == 4 ||
                Battleground[s.Item1, s.Item2] == 5)
            {
                return 1000;
            }
            if (Battleground[s.Item1, s.Item2] == 2 || Battleground[s.Item1, s.Item2] == 3)
            {
                return 100;
            }
            return 1;
        }
        if (dist < 1.0)
        {
            return 0;
        }
        Console.WriteLine("s&& u are ! neighbors!");
        return 1;
    }

    // obtains minimum rhs-value of all neighbors of s
    // s: tuple representing grid cell whose neighbors minimum rhs value is to be determined
    // neighListTemp: list of tuples representing grid cells that neighbor s
    // return: smallest rhs values among neighbors of s
    private int GetMinRhs(Tuple<double, double, int, int, int, double, double> s, List<
        Tuple<double, double, int, int, int, double, double>> neighListTemp)
    {
        var minRhs = neighListTemp[0].Item3 + Cost(neighListTemp[0], s);
        foreach (var neigh in neighListTemp)
        {
            var compRhs = neigh.Item3 + Cost(neigh, s);
            if (compRhs < minRhs)
            {
                minRhs = compRhs;
            }
        }

        return minRhs;
    }

    // prints a list of tuples
    // list: list of tuples to be printed
    private void PrintQueue(List<Tuple<double, double, int, int, int, double, double>> list)
    {
        foreach (var tuple in list)
        {
            Console.WriteLine(tuple);
        }
    }

    // moves agent, if possible, onto the grid cell with coordinates (x, y)
    protected void MoveMe(double x, double y)
    {
        if (CurrentSpot != null)
            CurrentSpot.Free = true;

        Position = MoveToPosition(Position.CreatePosition(x, y));

        var newSpot = FindCurrentSpot();
        if (newSpot != null)
            newSpot.Free = false;

        HasMoved = true;
    }
    
    /// <summary>
    ///    Resets the pathfinding state of the agent, clearing all relevant data structures.
    /// </summary>
    protected void ResetPathfinding()
    {
        _sStart = null;
        _sLast = null;
        _routeList.Clear();
        _expandQueue.Clear();
        _pathCalculated = false;
        _km = 0.0;
    }


    protected abstract void InsertIntoEnv();
    protected abstract Position MoveToPosition(Position position);
}