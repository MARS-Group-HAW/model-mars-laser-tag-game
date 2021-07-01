using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using Mars.Components.Common;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;

namespace LaserTagBox.Model.Mind
{
    /// <summary>
    ///     manages the behaviour for the whole team
    /// </summary>
    public class Team
    {
        private const int MinDistance = 15;
        private const CombatStrategy DefaultCombatStrategy = CombatStrategy.Explorer;
        private readonly Distance _distance = new();
        private readonly int _mapDimensionX;
        private readonly int _mapDimensionY;
        private readonly List<Position> _obstacles = new();
        private CombatStrategy _combatStrategy = DefaultCombatStrategy;
        private List<ExtendedEnemySnapshot> _enemies = new();
        private List<FriendSnapshot> _friends = new();
        private long _lastTick;
        private Position _movementTarget;
        private Position _regroupingTarget;
        private bool _regroupingTick;

        public Team()
        {
            _mapDimensionX = 50;
            _mapDimensionY = 50;
            _movementTarget = Position.CreatePosition(0, 0);
            _movementTarget = GetRandomPosition();
        }

        /// <summary>
        ///     selects an enemy from the list of visible enemies
        /// </summary>
        /// <param name="currentTick">current sim tick</param>
        /// <returns>position of enemy target</returns>
        public Position GetEnemyTarget(long currentTick)
        {
            return _enemies.OrderBy(o => o.Snapshot.Stance).FirstOrDefault(o => o.Tick >= currentTick - 1).Snapshot
                .Position;
        }

        /// <summary>
        ///     resets the combat strategy, should be called after a fight ended
        /// </summary>
        public void ResetCombatStrategy()
        {
            _combatStrategy = DefaultCombatStrategy;
        }

        /// <summary>
        ///     get the combat strategy for an agent
        /// </summary>
        /// <returns>combat strategy</returns>
        public CombatStrategy GetCombatStrategy()
        {
            // switch between explorer & shooter
            _combatStrategy = _combatStrategy switch
            {
                CombatStrategy.Explorer => CombatStrategy.Shooter,
                CombatStrategy.Shooter => CombatStrategy.Explorer,
                _ => _combatStrategy
            };
            return _combatStrategy;
        }

        /// <summary>
        ///     to be called, if the goto-method failed
        /// </summary>
        public void SetMovementTargetFailed()
        {
            // gather all players on one spot
            if (GetMaxTeamDistance() > 0)
            {
                _movementTarget = _friends.First().Position;
                return;
            }

            _movementTarget = GetRandomPosition();
        }

        public void AddObstacles(IEnumerable<Position> obstacles)
        {
            if (obstacles == null) return;

            var newObstacles = obstacles
                .Where(o => !_obstacles.Contains(o))
                .ToList();
            _obstacles.AddRange(newObstacles);
        }

        /// <summary>
        ///     get the movement target for an agent
        /// </summary>
        /// <param name="currentTick">current sim tick</param>
        /// <returns>position of movement target</returns>
        public Position GetMovementTarget(long currentTick, Position currentPos)
        {
            // move all players onto one spot at start
            if (currentTick == 1)
            {
                _movementTarget = _friends.OrderBy(f => f.Id).First().Position;
                return _movementTarget;
            }

            if (_obstacles.Contains(currentPos))
            {
                _movementTarget = GetRandomPosition();
                return _movementTarget;
            }

            var maxTeamDistance = GetMaxTeamDistance();
            if (_lastTick != currentTick)
            {
                _lastTick = currentTick;
                _regroupingTick = false;

                if (maxTeamDistance > 0)
                {
                    _regroupingTick = true;
                    _regroupingTarget = _friends.First(f => !_obstacles.Contains(f.Position)).Position;
                    return _regroupingTarget;
                }
            }

            if (_regroupingTick)
                return _regroupingTarget;

            var lastKnownEnemy = LastEnemySeen();
            var maxTargetDistance = GetMaxDistanceToTarget();

            // if no enemy was seen at all or withing the last 5 ticks: select a new target
            // only if the position wasn't already updated in the last 5 ticks
            // and the team members are all on the target tile
            if ((lastKnownEnemy == 0 || currentTick - lastKnownEnemy >= 5)
                && maxTargetDistance < 1
                && maxTeamDistance < 1
                || _obstacles.Any(o => o.Equals(_movementTarget)))
                _movementTarget = GetRandomPosition();

            return _movementTarget;
        }

        /// <summary>
        ///     updates the friend snapshots
        /// </summary>
        /// <param name="friends">list of friend snapshots</param>
        public void UpdateFriends(List<FriendSnapshot> friends)
        {
            _friends = friends ?? new List<FriendSnapshot>();
        }

        /// <summary>
        ///     updates the list of seen enemies
        /// </summary>
        /// <param name="snapshots">snapshot list</param>
        /// <param name="currentTick">current tick of sim</param>
        public void UpdateSeenEnemies(IEnumerable<EnemySnapshot> snapshots, long currentTick)
        {
            // new snapshots with new positions
            var newSnapshots = snapshots.Select(s => new ExtendedEnemySnapshot
            {
                Snapshot = s,
                Tick = currentTick
            }).ToList();

            // snapshots that were not updated
            var oldSnapshots = _enemies
                .Where(e => !newSnapshots.Contains(e))
                .ToList();

            // update list
            newSnapshots.AddRange(oldSnapshots);
            _enemies = newSnapshots;
        }

        /// <summary>
        ///     calculate the max distance to the target from all team members
        /// </summary>
        /// <returns>distance in m as int</returns>
        private int GetMaxDistanceToTarget()
        {
            if (!_friends.Any())
                return 0;
            
            return (int) _friends
                .Select(f => _distance.Manhattan(f.Position.PositionArray, _movementTarget.PositionArray))
                .Max();
        }

        /// <summary>
        ///     calculate the max distance between two players of the team
        /// </summary>
        /// <returns>distance in m as int</returns>
        private int GetMaxTeamDistance()
        {
            if (!_friends.Any())
                return 0;
            
            return (int) _friends.Select(o =>
                    _friends.Select(i => _distance.Manhattan(o.Position.PositionArray, i.Position.PositionArray))
                        .Max())
                .Max();
        }

        /// <summary>
        ///     calculates the ticks since an enemy was last seen
        /// </summary>
        /// <returns>ticks</returns>
        private long LastEnemySeen()
        {
            var enemy = _enemies.OrderByDescending(o => o.Tick).FirstOrDefault();
            return enemy.Equals(default(ExtendedEnemySnapshot)) ? 0 : enemy.Tick;
        }

        /// <summary>
        ///     creates a position with the given min distance to from the current movement target to the new position
        /// </summary>
        /// <returns>position</returns>
        private Position GetRandomPosition()
        {
            double distance = 0;
            var target = Position.CreatePosition(0, 0);
            while (distance < MinDistance)
            {
                target = Position.CreatePosition(RandomHelper.Random.Next(_mapDimensionX),
                    RandomHelper.Random.Next(_mapDimensionY));
                if (_obstacles.Contains(target)) continue;
                distance = _distance.Manhattan(_movementTarget.PositionArray, target.PositionArray);
            }

            return target;
        }
    }
}