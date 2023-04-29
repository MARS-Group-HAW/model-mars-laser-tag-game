using System;
using System.Collections.Generic;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Mind.Examples;

public class Example2 : AbstractPlayerMind
{
    private Position _goal;
    private PlayerMindLayer _mindLayer;
    private List<Position> _barriersInSight;
    private List<Position> _ditchesInSight;
    private List<EnemySnapshot> _enemiesInSight;
    private List<Position> _hillsInSight;

    private readonly Random _rnd = new();

    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
        _goal = Position.CreatePosition(_rnd.Next(1, 50), _rnd.Next(1, 50));
        // Console.WriteLine(_goal);
    }

    public override void Tick()
    {
        //First run to Random first 20 Ticks, so the players split up
        if (_mindLayer.GetCurrentTick() < 20)
        {
            Body.GoTo(_goal);
        }
        else
        {
            //Scan the area for everything possible, dont know if this is the smartest way..
            /*_barriersInSight = Body.ExploreBarriers1();
            _ditchesInSight = Body.ExploreDitches1();
            _hillsInSight = Body.ExploreHills1();
            */
            _enemiesInSight = Body.ExploreEnemies1();

            //Shoot enemies, if they are in sight
            //Reload first, if you dont have enough bullets
            if (_enemiesInSight is { Count: > 0 })
            {
                Body.GoTo(_goal);
                if (Body.RemainingShots == 0)
                {
                    Body.Reload3();
                }

                //When enemies in sight are less than 5 blocks away, lay down first to increase chances of hitting
                //the shots and decreasing the probability of being shot
                if (Body.GetDistance(_enemiesInSight[0].Position) < 6)
                {
                    Body.ChangeStance2(Stance.Lying);
                    Body.Tag5(_enemiesInSight[0].Position);
                    MoveAgentAfterShooting();
                }
                else
                {
                    Body.Tag5(_enemiesInSight[0].Position);
                    Body.ChangeStance2(Stance.Standing);
                    MoveAgentAfterShooting();
                }
            }
            else
            {
                //First Attempt of making agents move 
                //Should add looking for ditches, barriers, hills etc.. 
                //Maybe use them to hide and wait for few ticks?
                //If no enemies in sight, go on and look for new spot
                if ((_mindLayer.GetCurrentTick() % 10) == 0)
                {
                    _goal = Position.CreatePosition(_rnd.Next(1, 50), _rnd.Next(1, 50));
                }

                //exploreAndGoToHill();
                Body.GoTo(_goal);
            }
        }
    }

    private void MoveAgentAfterShooting()
    {
        var tmpPos = Position.CreatePosition(Body.Position.X + 1, Body.Position.Y);
        Body.GoTo(tmpPos);
    }

    private void exploreAndGoToHill()
    {
        _hillsInSight = Body.ExploreHills1();
        if (_hillsInSight is { Count: > 0 })
        {
            Body.GoTo(_hillsInSight[0]);
        }
    }
}