using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;
using Mars.Numerics;


namespace LaserTagBox.Model.Mind.Examples;

public class Example4 : AbstractPlayerMind
{
    private PlayerMindLayer _mindLayer;

    private AiState _currentState = AiState.ExploreGoal;

    private enum AiState
    {
        ExploreGoal,
        MovingToGoal,
        OnTheHill
    }

    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
    }

    public override void Tick()
    {
        if (Body.Energy > 50)
        {
            DoActiveStrategy();
        }
        else
        {
            // TODO: passive strategy
            // DoPassiveStrategy();
            DoActiveStrategy();
        }
    }

    private void DoActiveStrategy()
    {
        switch (_currentState)
        {
            case AiState.ExploreGoal:
                HandleExploreGoalState();
                break;
            case AiState.MovingToGoal:
                HandleMovingToGoalState();
                break;
            case AiState.OnTheHill:
                HandleOnTheHillState();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Position _goal;
    private bool _movedPreviousTick = true;

    private void HandleExploreGoalState()
    {
        var enemies = Body.ExploreEnemies1();
        if (enemies?.Count > 0)
        {
            var enemy = enemies.First();
            Body.Tag5(enemy.Position);
        }

        if (Body.RemainingShots == 0)
        {
            Body.Reload3();
        }

        var hills = Body.ExploreHills1();
        var teammates = Body.ExploreTeam();

        var nearestFreeHill = hills?.Where(x =>
            {
                var isOccupiedByAnyTeammate = teammates.Any(y =>
                    Distance.Manhattan(x.PositionArray, y.Position.PositionArray) == 0);
                var isSameHill = _previousGoals.Contains(x);

                return !isOccupiedByAnyTeammate && !isSameHill;
            })
            .OrderBy(x => Body.GetDistance(x))
            .FirstOrDefault();

        if (nearestFreeHill == null)
        {
            // random walk
            var myX = Body.Position.X;
            var myY = Body.Position.Y;

            var x = RandomHelper.Random.NextDouble(Math.Max(0, myX - 4), Math.Min(myX + 4, _mindLayer.Width - 1));
            var y = RandomHelper.Random.NextDouble(Math.Max(0, myY - 4), Math.Min(myY + 4, _mindLayer.Height - 1));

            Body.GoTo(Position.CreatePosition(x, y));

            _currentState = AiState.ExploreGoal; // self transition
        }
        else
        {
            _goal = nearestFreeHill;
            _movedPreviousTick = Body.GoTo(_goal);

            _currentState = AiState.MovingToGoal;
        }
    }

    private readonly CircularBuffer<Position> _previousGoals = new(2);
    private int _ticksOnTheHill;

    private void HandleMovingToGoalState()
    {
        var enemies = Body.ExploreEnemies1();
        if (enemies?.Count > 0)
        {
            var enemy = enemies.First();
            Body.Tag5(enemy.Position);
        }

        if (Body.RemainingShots == 0)
        {
            Body.Reload3();
        }

        var teammates = Body.ExploreTeam();
        var isGoalOccupied =
            teammates?.Any(x => Distance.Manhattan(x.Position.PositionArray, _goal.PositionArray) == 0) ?? false;
        var isGoalReached = Body.GetDistance(_goal) == 0;

        if (!isGoalReached && !isGoalOccupied)
        {
            _movedPreviousTick = Body.GoTo(_goal);
            if (!_movedPreviousTick)
            {
                _goal = null;
                _currentState = AiState.ExploreGoal;
            }
            else
            {
                // continue move towards the goal
                _currentState = AiState.MovingToGoal;
            }
        }
        else if (isGoalReached)
        {
            _currentState = AiState.OnTheHill;

            _previousGoals.Enqueue(_goal);
            _goal = null;
            _ticksOnTheHill = 0;
        }
        else if (isGoalOccupied)
        {
            _currentState = AiState.ExploreGoal;
            _goal = null;
        }
    }

    private void HandleOnTheHillState()
    {
        var enemies = Body.ExploreEnemies1();
        if (enemies?.Count > 0)
        {
            var enemy = enemies.First();
            Body.Tag5(enemy.Position);
        }
        else
        {
            ++_ticksOnTheHill;
        }

        if (Body.RemainingShots == 0)
        {
            Body.Reload3();
        }

        if (_ticksOnTheHill >= 7 || Body.WasTaggedLastTick)
        {
            _currentState = AiState.ExploreGoal;
        }
        else
        {
            _currentState = AiState.OnTheHill;
        }
    }

    private void DoPassiveStrategy()
    {
    }

    private class CircularBuffer<T> : IEnumerable<T>
    {
        private int _count;
        private int _head;
        private int _rear;
        private readonly T[] _values;

        public CircularBuffer(int max)
        {
            Size = max;
            SyncRoot = new object();
            _count = 0;
            _head = 0;
            _rear = 0;
            _values = new T[Size];
        }

        static int Incr(int index, int size)
        {
            return (index + 1) % size;
        }

        private void UnsafeEnsureQueueNotEmpty()
        {
            if (_count == 0)
                throw new Exception("Empty queue");
        }

        private int Size { get; }

        private object SyncRoot { get; }

        #region Count

        private int Count => UnsafeCount;

        public int SafeCount
        {
            get
            {
                lock (SyncRoot)
                {
                    return UnsafeCount;
                }
            }
        }

        private int UnsafeCount => _count;

        #endregion

        #region Enqueue

        public void Enqueue(T obj)
        {
            UnsafeEnqueue(obj);
        }

        public void SafeEnqueue(T obj)
        {
            lock (SyncRoot)
            {
                UnsafeEnqueue(obj);
            }
        }

        private void UnsafeEnqueue(T obj)
        {
            _values[_rear] = obj;

            if (Count == Size)
                _head = Incr(_head, Size);
            _rear = Incr(_rear, Size);
            _count = Math.Min(_count + 1, Size);
        }

        #endregion

        #region Dequeue

        public T Dequeue()
        {
            return UnsafeDequeue();
        }

        public T SafeDequeue()
        {
            lock (SyncRoot)
            {
                return UnsafeDequeue();
            }
        }

        private T UnsafeDequeue()
        {
            UnsafeEnsureQueueNotEmpty();

            var res = _values[_head];
            _values[_head] = default(T);
            _head = Incr(_head, Size);
            _count--;

            return res;
        }

        #endregion

        #region Peek

        public T Peek()
        {
            return UnsafePeek();
        }

        public T SafePeek()
        {
            lock (SyncRoot)
            {
                return UnsafePeek();
            }
        }

        private T UnsafePeek()
        {
            UnsafeEnsureQueueNotEmpty();

            return _values[_head];
        }

        #endregion


        #region GetEnumerator

        public IEnumerator<T> GetEnumerator()
        {
            return UnsafeGetEnumerator();
        }

        public IEnumerator<T> SafeGetEnumerator()
        {
            lock (SyncRoot)
            {
                var res = new List<T>(_count);
                var enumerator = UnsafeGetEnumerator();
                while (enumerator.MoveNext())
                    res.Add(enumerator.Current);
                return res.GetEnumerator();
            }
        }

        private IEnumerator<T> UnsafeGetEnumerator()
        {
            var index = _head;
            for (var i = 0; i < _count; i++)
            {
                yield return _values[index];
                index = Incr(index, Size);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}