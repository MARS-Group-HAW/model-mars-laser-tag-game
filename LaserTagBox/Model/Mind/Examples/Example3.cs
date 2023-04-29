using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Mind.Examples;

public class Example3 : AbstractPlayerMind
{
    private static Position _groupDirection;
    private static Guid _teamLeader;
    private static Position _teamLeaderPosition;

    //private const int RANGE = 50;
    private const int MaxDistanceToLeader = 5;

    private Position _goal;
    private List<EnemySnapshot> _enemies;
    private List<Position> _hills;
    private bool _directionPossible;
    private bool _setDirection;
    private bool _setHill;

    private readonly Random _r = new();
        
    public override void Init(PlayerMindLayer mindLayer)
    {
        //do something
        _directionPossible = false;
        _setDirection = false;
        _setHill = false;

        if (_teamLeader == Guid.Empty)
        {
            _teamLeader = this.ID;
        }
    }

    public override void Tick()
    {
        if (!_setHill || !_directionPossible)
        {
            _hills = Body.ExploreHills1();
            if (_hills is { Count: > 0 })
            {
                _goal = Position.CreatePosition(_hills[0].PositionArray);
                _setHill = true;
            }
        }

        if ((_setDirection || _setHill) || !_directionPossible )
        {
            double xRand = _r.Next(0, 50);
            double yRand = _r.Next(0, 50);
            _goal = Position.CreatePosition(xRand, yRand);
                
            if (_teamLeader == this.ID || _groupDirection is null)
            {
                _teamLeaderPosition = Body.Position;
                _groupDirection = _goal;
            }
            else if (MaxDistanceToLeader < (Body.GetDistance(_teamLeaderPosition)))
            {
                _goal = _groupDirection;
            }

            _setDirection = false;
        }
            
        if (((long)Body.Position.X==(long)_goal.X) && ((long)Body.Position.Y==(long)_goal.Y))
        {
            _setDirection = false;
            _setHill = false;
        }
        _directionPossible = Body.GoTo(_goal);
            
        _enemies = Body.ExploreEnemies1();
        if (_enemies is { Count: > 0 })
        {
            foreach (var t in _enemies.Where(t => Body.HasBeeline1(t.Position)))
            {
                if (Body.RemainingShots == 0)
                {
                    Body.Reload3();
                }
                if (Body.GetDistance(t.Position) <= 8)
                {
                    if (Body.Stance != Stance.Kneeling)
                    {
                        Body.ChangeStance2(Stance.Kneeling);
                    }
                }
                Body.Tag5(t.Position);
                Body.ChangeStance2(Stance.Standing);
            }
        }
        //Console.WriteLine(Body.Energy);
    }
}