using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Environments;
using Distance = Mars.Numerics.Distance;
using Mars.Common.Core.Random;


namespace LaserTagBox.Model.Mind.Examples
{
    public class Example1 : AbstractPlayerMind
    {
        private enum AiState
        {
            ExploreGoal,
            MovingToGoal,
            OnTheHill,
            InTheDitch,
            ExploreDitch
        }

        private PlayerMindLayer _mindLayer;

        private AiState _currentState = AiState.ExploreGoal;

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
                case AiState.InTheDitch:
                    HandleInTheDitchState();
                    break;
                case AiState.ExploreDitch:
                    HandleExploreDitch();
                    break;
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
            //var checkTeammate = teammates.First().Energy;

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
            /*else if(Body.Energy==checkTeammate+20 || Body.Energy==checkTeammate+10)
            {
                _goal = teammates.First().Position;
                _movedPreviousTick = Body.GoTo(_goal);

                _currentState = AIState.MovingToGoal;
            }*/
            else
            {
                _goal = nearestFreeHill;
                _movedPreviousTick = Body.GoTo(_goal);

                _currentState = AiState.MovingToGoal;
            }
        }

        private readonly CircularBuffer<Position> _previousGoals = new CircularBuffer<Position>(2);
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

        private int _ticksInTheDitch;

        private void HandleInTheDitchState()
        {
            var enemies = Body.ExploreEnemies1();
            if (enemies?.Count > 0)
            {
                var enemy = enemies.First();
                Body.Tag5(enemy.Position);
            }
            else
            {
                ++_ticksInTheDitch;
            }

            if (Body.RemainingShots == 0)
            {
                Body.Reload3();
            }

            if (_ticksInTheDitch >= 5 || Body.WasTaggedLastTick)
            {
                _currentState = AiState.ExploreDitch; //ExploreGoal will be replaced with ExploreDitch
            }
            else
            {
                _currentState = AiState.InTheDitch;
            }
        }

        private void HandleExploreDitch()
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

            var ditches = Body.ExploreDitches1();
            var teammates = Body.ExploreTeam();
            //var checkTeammate = teammates.First().Energy;

            var nearestFreeDitch = ditches?.Where(x =>
                {
                    var isOccupiedByAnyTeammate = teammates.Any(y =>
                        Distance.Manhattan(x.PositionArray, y.Position.PositionArray) == 0);
                    var isSameHill = _previousGoals.Contains(x);

                    return !isOccupiedByAnyTeammate && !isSameHill;
                })
                .OrderBy(x => Body.GetDistance(x))
                .FirstOrDefault();

            if (nearestFreeDitch == null)
            {
                // random walk
                var myX = Body.Position.X;
                var myY = Body.Position.Y;

                var x = RandomHelper.Random.NextDouble(Math.Max(0, myX - 4), Math.Min(myX + 4, _mindLayer.Width - 1));
                var y = RandomHelper.Random.NextDouble(Math.Max(0, myY - 4), Math.Min(myY + 4, _mindLayer.Height - 1));

                Body.GoTo(Position.CreatePosition(x, y));

                _currentState = AiState.ExploreDitch; // self transition
            }
            else
            {
                _goal = nearestFreeDitch;
                _movedPreviousTick = Body.GoTo(_goal);

                _currentState = AiState.MovingToGoal;
            }
        }

        private void DoPassiveStrategy()
        {
        }
    }
}


public class CircularBuffer<T> : IEnumerable<T>
{
    readonly int size;
    readonly object locker;

    int count;
    int head;
    int rear;
    T[] values;

    public CircularBuffer(int max)
    {
        this.size = max;
        locker = new object();
        count = 0;
        head = 0;
        rear = 0;
        values = new T[size];
    }

    static int Incr(int index, int size)
    {
        return (index + 1) % size;
    }

    private void UnsafeEnsureQueueNotEmpty()
    {
        if (count == 0)
            throw new Exception("Empty queue");
    }

    public int Size
    {
        get { return size; }
    }

    public object SyncRoot
    {
        get { return locker; }
    }

    #region Count

    public int Count
    {
        get { return UnsafeCount; }
    }

    public int SafeCount
    {
        get
        {
            lock (locker)
            {
                return UnsafeCount;
            }
        }
    }

    public int UnsafeCount
    {
        get { return count; }
    }

    #endregion

    #region Enqueue

    public void Enqueue(T obj)
    {
        UnsafeEnqueue(obj);
    }

    public void SafeEnqueue(T obj)
    {
        lock (locker)
        {
            UnsafeEnqueue(obj);
        }
    }

    public void UnsafeEnqueue(T obj)
    {
        values[rear] = obj;

        if (Count == Size)
            head = Incr(head, Size);
        rear = Incr(rear, Size);
        count = Math.Min(count + 1, Size);
    }

    #endregion

    #region Dequeue

    public T Dequeue()
    {
        return UnsafeDequeue();
    }

    public T SafeDequeue()
    {
        lock (locker)
        {
            return UnsafeDequeue();
        }
    }

    public T UnsafeDequeue()
    {
        UnsafeEnsureQueueNotEmpty();

        T res = values[head];
        values[head] = default(T);
        head = Incr(head, Size);
        count--;

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
        lock (locker)
        {
            return UnsafePeek();
        }
    }

    public T UnsafePeek()
    {
        UnsafeEnsureQueueNotEmpty();

        return values[head];
    }

    #endregion


    #region GetEnumerator

    public IEnumerator<T> GetEnumerator()
    {
        return UnsafeGetEnumerator();
    }

    public IEnumerator<T> SafeGetEnumerator()
    {
        lock (locker)
        {
            List<T> res = new List<T>(count);
            var enumerator = UnsafeGetEnumerator();
            while (enumerator.MoveNext())
                res.Add(enumerator.Current);
            return res.GetEnumerator();
        }
    }

    public IEnumerator<T> UnsafeGetEnumerator()
    {
        int index = head;
        for (int i = 0; i < count; i++)
        {
            yield return values[index];
            index = Incr(index, size);
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    #endregion
}