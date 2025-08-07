using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using System.Collections.Concurrent;
using LaserTag.Core.Model.Mind;
using LaserTag.Core.Model.Shared;


namespace CaptureTheFlag.Mind.Examples;

public class Example2 : AbstractPlayerMind
{
    private PlayerMindLayer _mindLayer;
    private Position _goal;
    private Position _enemyFlagStand;
    private static ConcurrentDictionary<Color, Position> sharedEnemyFlagTargets = new();

    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
    }

    public override void Tick()
    {
        _enemyFlagStand ??= Body.ExploreEnemyFlagStands1()?.FirstOrDefault();

        if (Body.ActionPoints > 2 && !Body.CarryingFlag)
        {
            var flags = Body.ExploreFlags2();
            var ownFlag = flags.FirstOrDefault(f => f.Team == Body.Color);
            var ownFlagStand = Body.ExploreOwnFlagStand();

            // Recover own flag if it's far from base
            if (Distance.Euclidean(ownFlagStand.X, ownFlagStand.Y, ownFlag.Position.X, ownFlag.Position.Y) > 2)
            {
                _goal = ownFlag.Position;
            }
            else
            {
                // Team coordination: share enemy flag target
                var enemyFlags = flags.Where(f => f.Team != Body.Color && !f.PickedUp).ToList();

                foreach (var flag in enemyFlags)
                {
                    sharedEnemyFlagTargets.TryAdd(Body.Color, flag.Position);

                }

                if (sharedEnemyFlagTargets.ContainsKey(Body.Color))
                {
                    _goal = sharedEnemyFlagTargets[Body.Color];
                }
            }
        }

        if (Body.CarryingFlag)
        {
            _goal = Body.ExploreOwnFlagStand();
        }

        var enemies = Body.ExploreEnemies1();
        if (enemies != null && enemies.Count > 0)
        {
            if (Body.RemainingShots == 0) Body.Reload3();
            Body.Tag5(enemies.First().Position);
        }

        if (_goal == null)
        {
            _goal = Body.Position; // stay put
        }
        Body.GoTo(_goal);
    }
}
