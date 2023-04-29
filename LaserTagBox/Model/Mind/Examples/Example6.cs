using System;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Environments;
using ServiceStack;

namespace LaserTagBox.Model.Mind.Examples;

public class Example6 : AbstractPlayerMind
{
    private const int UpperMapLimit = 51;
    private const int LowerMapLimit = 0;
    private const int MaxCampCounter = 70;

    private Position _goal;
    private Position _lastPosition;
    private int _campCounter;
    private bool _wasRandomGoal;
    private bool _taggedEnemyLastTick;

    public override void Init(PlayerMindLayer mindLayer)
    {
    }

    public override void Tick()
    {
        if (!_wasRandomGoal && Body.Position.Equals(_goal))
        {
            Body.ChangeStance2(Stance.Kneeling);
        }

        if (_taggedEnemyLastTick)
        {
            _taggedEnemyLastTick = ShootIfEnemyInSight();
        }
        else
        {
            if (_campCounter > MaxCampCounter)
            {
                Body.ChangeStance2(Stance.Standing);
                _goal = new Position(new Random().Next((int) Body.Position.X - 10, (int) Body.Position.X + 10),
                    new Random().Next((int) Body.Position.Y - 10, (int) Body.Position.Y + 10));
                _goal = ReturnValidPosition(_goal);
                _wasRandomGoal = true;
            }

            ReloadIfEmpty();
            _taggedEnemyLastTick = ShootIfEnemyInSight();

            if (_goal == null || Body.Position.Equals(_goal) && _campCounter >= MaxCampCounter ||
                Body.Position.Equals(_goal) && _wasRandomGoal)
            {
                // Console.Write("Getting new Barrier.");
                var furthestNextBarrier = GetFurthestNextBarrier();
                if (furthestNextBarrier != null)
                    _goal = furthestNextBarrier;
                _wasRandomGoal = false;
            }

            // Console.WriteLine("Goal: " + _goal);
            _lastPosition = Body.Position;
            if (Body.GoTo(_goal))
            {
                _campCounter = 0;
            }

            // Console.WriteLine(Body.Position);
            if (Body.Position.Equals(_lastPosition))
            {
                _campCounter++;
            }
        }
    }

    private static Position ReturnValidPosition(Position pos)
    {
        if (pos.X < LowerMapLimit) pos.X = LowerMapLimit;
        if (pos.Y < LowerMapLimit) pos.Y = LowerMapLimit;
        if (pos.X > UpperMapLimit) pos.X = UpperMapLimit;
        if (pos.Y > UpperMapLimit) pos.Y = UpperMapLimit;
        return pos;
    }

    private void ReloadIfEmpty()
    {
        if (Body.RemainingShots <= 0)
        {
            Body.Reload3();
        }
    }

    private bool ShootIfEnemyInSight()
    {
        var hit = false;
        // if enemy visible, shoot him
        var nearbyEnemies = Body.ExploreEnemies1();
        if (!nearbyEnemies.IsEmpty())
        {
            var closestDistance = int.MaxValue;
            var closestEnemy = 0;
            for (var i = 0; i < nearbyEnemies.Count; i++)
            {
                if (Body.GetDistance(nearbyEnemies[i].Position) < closestDistance)
                {
                    closestDistance = Body.GetDistance(nearbyEnemies[i].Position);
                    closestEnemy = i;
                }
            }

            hit = Body.Tag5(nearbyEnemies[closestEnemy].Position);
        }

        return hit;
    }

    private static bool IsPositionValid(Position pos)
    {
        return (pos.X <= UpperMapLimit && pos.Y is >= LowerMapLimit and <= UpperMapLimit &&
                pos.X >= LowerMapLimit);
    }

    private Position GetFurthestNextBarrier()
    {
        var nearbyBarriers = Body.ExploreBarriers1();
        if (nearbyBarriers.IsEmpty()) return null;
        var pos = nearbyBarriers[^1];
        var res = new Position(pos.X - 1, pos.Y);
        if (!nearbyBarriers.Contains(res) && IsPositionValid(res)) return res;
        res = new Position(pos.X + 1, pos.Y);
        if (!nearbyBarriers.Contains(res) && IsPositionValid(res)) return res;
        res = new Position(pos.X, pos.Y + 1);
        if (!nearbyBarriers.Contains(res) && IsPositionValid(res)) return res;
        res = new Position(pos.X, pos.Y - 1);
        if (!nearbyBarriers.Contains(res) && IsPositionValid(res)) return res;
        return null;
    }
}