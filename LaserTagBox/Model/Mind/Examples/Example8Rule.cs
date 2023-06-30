using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LaserTagBox.Model.Shared;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;
using MongoDB.Driver;
using ServiceStack;

namespace LaserTagBox.Model.Mind;

public class Example8Rule : AbstractPlayerMind
{
    #region Fields

    private enum GamePhase
    {
        Exploring,
        Fight,
        Retreat,
        Camping
    }

    private PlayerMindLayer _mindLayer;
    private Position _goal;
    private EnemySnapshot _target;
    private IPlayerBody _leader;
    private bool _isLeader;
    private GamePhase _gamePhase;
    private int _retreatThreshold = 50;
    private int _backToCombatThreshold = 80;
    private int _retreatDistance = 5;

    //static
    private static List<IPlayerBody> _teammates = new();
    private static Dictionary<int, EnemySnapshot> _sharedEnemies = new();
    private static Dictionary<Position, bool> _sharedHills = new();
    private static HashSet<Position> _sharedDitches = new();
    private static HashSet<Position> _sharedBarriers = new();
    private static EnemySnapshot _sharedTarget = new();

    private static Mutex _mutexTeammates = new Mutex();
    private static Mutex _mutexEnemies = new Mutex();
    private static Mutex _mutexHills = new Mutex();
    private static Mutex _mutexDitches = new Mutex();
    private static Mutex _mutexBarriers = new Mutex();
    private static Mutex _mutexTarget = new Mutex();


    #endregion

    #region Init

    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
        _teammates.Add(Body);
    }

    #endregion

    #region Tick

    public override void Tick()
    {
        if (Body.ActionPoints < 10)
        {
            return;
        }

        UpdateTeamMates();

        switch (_gamePhase)
        {
            case GamePhase.Exploring:
                CheckAmmo3();
                LookForEnemies8();
                LookForHills1();
                LookForDitches1();
                SetGoal();
                // LookForBarriers1();
                MoveToGoal();
                break;
            case GamePhase.Fight:
                CheckAmmo3();
                LookForEnemies8();
                break;
            case GamePhase.Retreat:
                CheckAmmo3();
                LookForEnemies8();
                Retreat1();
                MoveToGoal();
                break;
            case GamePhase.Camping:
                CheckAmmo3();
                LookForEnemies8();
                Camping();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // always reload if we still have ActionPoints left
        if (Body.ActionPoints >= 3)
        {
            Body.Reload3();
        }
    }

    #endregion

    #region Methods

    private void Camping()
    {
        if (Body.Energy > _backToCombatThreshold)
        {
            _gamePhase = GamePhase.Exploring;
        }
    }

    private void CheckAmmo3()
    {
        if (Body.ActionPoints < 3) return;
        if (Body.RemainingShots == 0) Body.Reload3();
    }

    private void MoveToGoal()
    {
        if (_gamePhase == GamePhase.Retreat)
        {
            ChangeStance2(Stance.Standing);
        }

        if (_goal != null)
        {
            if (Body.GetDistance(_goal) < 1)
            {
                // goal reached
                _mutexEnemies.WaitOne();
                if (_sharedEnemies.Any(enemy => Equals(enemy.Value.Position, _goal)))
                {
                    // remove from _sharedEnemies
                    _sharedEnemies.Remove(_sharedEnemies.First(enemy => Equals(enemy.Value.Position, _goal)).Key);
                }

                _mutexEnemies.ReleaseMutex();
                _mutexHills.WaitOne();
                if (_sharedHills.Any(hill => Equals(hill.Key, _goal)))
                {
                    // mark hill as visited
                    _sharedHills[_sharedHills.First(hill => Equals(hill.Key, _goal)).Key] = true;
                }

                _mutexHills.ReleaseMutex();
                _mutexDitches.WaitOne();
                if (_sharedDitches.Any(ditch => Equals(ditch, _goal)))
                {
                    _gamePhase = GamePhase.Camping;
                    Camping();
                }

                _mutexDitches.ReleaseMutex();
            }
            else
            {
                if (!Body.GoTo(_goal))
                {
                    SetGoal();
                }
            }
        }

        if (_gamePhase == GamePhase.Retreat)
        {
            ChangeStance2(Stance.Lying);
        }
    }

    private void ChangeStance2(Stance newStance)
    {
        if (Body.Stance == newStance || Body.ActionPoints < 2) return;
        Body.ChangeStance2(newStance);
    }

    private void LookForDitches1()
    {
        if (Body.ActionPoints < 1) return;

        var seenDitches = Body.ExploreDitches1();

        if (seenDitches.Any())
        {
            _mutexDitches.WaitOne();
            foreach (var ditch in seenDitches)
            {
                _sharedDitches.Add(ditch);
            }

            _mutexDitches.ReleaseMutex();
        }
    }

    private void LookForHills1()
    {
        if (Body.ActionPoints < 1) return;

        var seenHills = Body.ExploreHills1();

        if (seenHills.Any())
        {
            _mutexHills.WaitOne();
            foreach (var hill in seenHills)
            {
                _sharedHills.TryAdd(hill, false);
            }

            _mutexHills.ReleaseMutex();
        }
    }

    private void LookForBarriers1()
    {
        if (Body.ActionPoints < 1) return;

        var seenBarriers = Body.ExploreBarriers1();

        if (seenBarriers.Any())
        {
            _mutexBarriers.WaitOne();
            foreach (var barrier in seenBarriers)
            {
                _sharedBarriers.Add(barrier);
            }

            _mutexBarriers.ReleaseMutex();
        }
    }

    private void LookForEnemies8()
    {
        var seenEnemies = Body.ExploreEnemies1();

        if (seenEnemies.Any())
        {
            if (_gamePhase == GamePhase.Exploring)
            {
                _gamePhase = GamePhase.Fight;
            }

            _mutexEnemies.WaitOne();
            foreach (var enemy in seenEnemies)
            {
                if (!_sharedEnemies.TryAdd(enemy.MemberId, enemy))
                {
                    _sharedEnemies[enemy.MemberId] = enemy;
                }
            }

            _mutexEnemies.ReleaseMutex();

            SetTarget(seenEnemies);

            // no need for beeline, since it is already checked in ExploreEnemies.

            if (Body.ActionPoints >= 7)
            {
                ChangeCombatStance2();
            }

            if (Body.ActionPoints >= 5)
            {
                //Body.HasBeeline1(_target.Position);
                Body.Tag5(_target.Position);
            }
        }
        else
        {
            if (_isLeader)
            {
                _mutexTarget.WaitOne();
                _sharedTarget = new EnemySnapshot();
                _mutexTarget.ReleaseMutex();
            }
            else
            {
                SetGoal();
            }

            if (_gamePhase == GamePhase.Fight)
            {
                _gamePhase = GamePhase.Exploring;
                if (Body.Stance != Stance.Standing)
                {
                    Body.ChangeStance2(Stance.Standing);
                }
            }
        }

        if (Body.Energy < _retreatThreshold && Body.WasTaggedLastTick)
        {
            _gamePhase = GamePhase.Retreat;
            if (Body.Stance != Stance.Standing)
            {
                Body.ChangeStance2(Stance.Standing);
            }

            Retreat1();
        }
    }

    private void ChangeCombatStance2()
    {
        if (Body.ActionPoints < 2) return;

        var distance = Body.GetDistance(_target.Position);

        switch (Body.Stance)
        {
            case Stance.Standing:
                if (Body.VisualRange - 5 >= distance)
                {
                    Body.ChangeStance2(Stance.Lying);
                }
                else if (Body.VisualRange - 2 >= distance)
                {
                    Body.ChangeStance2(Stance.Kneeling);
                }

                break;
            case Stance.Kneeling:
                if (Body.VisualRange - 5 >= distance)
                {
                    Body.ChangeStance2(Stance.Lying);
                }

                break;
            case Stance.Lying:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetTarget(List<EnemySnapshot> seenEnemies)
    {
        _mutexTarget.WaitOne();
        if (_isLeader)
        {
            // set the closest enemy as a target.
            _target = seenEnemies.MinBy(enemy => Body.GetDistance(enemy.Position));
            _sharedTarget = _target;
        }
        else if (_sharedTarget.Position != null)
        {
            if (seenEnemies.Any() && seenEnemies.Exists(enemy => enemy.MemberId == _sharedTarget.MemberId))
            {
                _sharedTarget = seenEnemies.First(enemy => enemy.MemberId == _sharedTarget.MemberId);
                _target = _sharedTarget;
            }
            else
            {
                // set the closest enemy as a target.
                _target = seenEnemies.MinBy(enemy => Body.GetDistance(enemy.Position));
            }
        }
        else
        {
            // set the closest enemy as a target.
            _target = seenEnemies.MinBy(enemy => Body.GetDistance(enemy.Position));
            _sharedTarget = _target;
        }

        _mutexTarget.ReleaseMutex();
    }

    /// <summary>
    /// Chooses a team leader based on its body-id. Only team-members that are alive will be considered a leader.
    /// If the leader-body is equal to it's own, _isLeader will be set to true.
    /// </summary>
    private void UpdateTeamMates()
    {
        _mutexTeammates.WaitOne();
        var toBeRemoved = _teammates.Where(mate => !mate.Alive).ToList();

        if (toBeRemoved.Any())
        {
            foreach (var mate in toBeRemoved)
            {
                _teammates.Remove(mate);
            }
        }

        if (_teammates.Count <= 1)
        {
            _leader = Body;
        }
        else
        {
            _leader = _teammates.Where(body => body.Alive).OrderBy(body => body.GetId()).First();
        }

        _isLeader = Equals(_leader, Body);
        _mutexTeammates.ReleaseMutex();
    }

    /// <summary>
    /// Checks for beeline and then shoots at target.
    /// </summary>
    private void TagTarget6()
    {
        if (Body.ActionPoints < 6) return;

        if (_target.Position != null)
        {
            if (Body.HasBeeline1(_target.Position))
            {
                Body.Tag5(_target.Position);
            }
        }
    }

    private void SetGoal()
    {
        if (_isLeader)
        {
            if (_gamePhase == GamePhase.Exploring)
            {
                _goal = GetLastKnownEnemy() ?? GetNextUnvisitedHill() ?? GetRandomCenterPosition();
            }
        }
        else
        {
            if (Body.GetDistance(_leader.Position) < 5)
            {
                _goal = GetLastKnownEnemy() ?? GetNextUnvisitedHill() ?? GetRandomPosition();
            }
            else
            {
                _mutexTarget.WaitOne();
                _goal = _sharedTarget.Position ?? _leader.Position;
                _mutexTarget.ReleaseMutex();
            }
        }
    }

    private Position GetRandomPosition()
    {
        var newX = RandomHelper.Random.Next(_mindLayer.Width);
        var newY = RandomHelper.Random.Next(_mindLayer.Height);
        return Position.CreatePosition(newX, newY);
    }

    private Position GetRandomCenterPosition()
    {
        var boundary = 5;
        
        var centerWidth = _mindLayer.Width / 2;
        var centerHeight = _mindLayer.Height / 2;

        var minX = centerWidth - boundary;
        var minY = centerHeight - boundary;

        var maxX = centerWidth + boundary;
        var maxY = centerHeight + boundary;

        var newX = RandomHelper.Random.Next(minX, maxX);
        var newY = RandomHelper.Random.Next(minY, maxY);
        
        return Position.CreatePosition(newX, newY);
    }

    private Position GetLastKnownEnemy()
    {
        _mutexTarget.WaitOne();
        if (_sharedTarget.Position != null)
        {
            var enemyPos = _sharedTarget.Position;
            _mutexTarget.ReleaseMutex();
            return enemyPos;
        }
        _mutexTarget.ReleaseMutex();
        _mutexEnemies.WaitOne();
        if (_sharedEnemies.Any())
        {
            var enemyPos = _sharedEnemies.MinBy(enemy => Body.GetDistance(enemy.Value.Position)).Value.Position;
            _mutexEnemies.ReleaseMutex();
            return enemyPos;
        }
        _mutexEnemies.ReleaseMutex();
        
        return null;
    }

    private Position GetNextUnvisitedHill()
    {
        _mutexHills.WaitOne();
        if (_sharedHills.Any(hill => !hill.Value))
        {
            var hillPos = _sharedHills.Where(hill => !hill.Value)
                .MinBy(hill => Body.GetDistance(hill.Key)).Key;
            _mutexHills.ReleaseMutex();
            return hillPos;
        }
        _mutexHills.ReleaseMutex();
        return null;
    }

    private void Retreat1()
    {
        if (Body.ActionPoints < 1) return;
        
        var enemies = Body.ExploreEnemies1();

        if (enemies.Any())
        {
            _goal = GetRetreatDitch(enemies.First().Position) ?? GetRetreatPosition(enemies.First().Position);
        }

        if (Body.Energy > _backToCombatThreshold)
        {
            _gamePhase = GamePhase.Exploring;
            SetGoal();
        }
    }

    private Position GetRetreatDitch(Position enemyPos)
    {
        Position ditchPos;
        _mutexDitches.WaitOne();
        if (Body.Position.X >= enemyPos.X)
        {
            // X => ours
            ditchPos = Body.Position.Y >= enemyPos.Y ?
                // X > ours , Y > ours
                _sharedDitches.Where(ditch => ditch.X >= Body.Position.X && ditch.Y >= Body.Position.Y).MinBy(ditch => Body.GetDistance(ditch)) :
                // X > ours, y < ours
                _sharedDitches.Where(ditch => ditch.X >= Body.Position.X && ditch.Y < Body.Position.Y).MinBy(ditch => Body.GetDistance(ditch));
        }
        else
        {
            // X < ours
            ditchPos = Body.Position.Y >= enemyPos.Y ?
                // X < ours , Y > ours
                _sharedDitches.Where(ditch => ditch.X < Body.Position.X && ditch.Y >= Body.Position.Y).MinBy(ditch => Body.GetDistance(ditch)) :
                // X < ours, y < ours
                _sharedDitches.Where(ditch => ditch.X < Body.Position.X && ditch.Y < Body.Position.Y).MinBy(ditch => Body.GetDistance(ditch));
        }
        _mutexDitches.ReleaseMutex();
        return ditchPos;
    }

    private Position GetRetreatPosition(Position enemyPos)
    {
        var newX = Body.Position.X;
        var newY = Body.Position.Y;
        
        
        if (Body.Position.X >= enemyPos.X)
        {
            newX += _retreatDistance;
        }
        else
        {
            newX -= _retreatDistance;
        }

        if (Body.Position.Y >= enemyPos.Y)
        {
            newY += _retreatDistance;
        }
        else
        {
            newY -= _retreatDistance;
        }

        if (newX > _mindLayer.Width)
        {
            newX = _mindLayer.Width;
        }
        if (newY > _mindLayer.Height)
        {
            newY = _mindLayer.Height;
        }
        if (newX < 0)
        {
            newX = 0;
        }
        if (newY < 0)
        {
            newY = 0;
        }
        
        return Position.CreatePosition(newX, newY);
    }

    #endregion
}