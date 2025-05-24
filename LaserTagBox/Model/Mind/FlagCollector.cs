using System;
using System.Drawing;
using System.Linq;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Mind;

public class FlagCollector : AbstractPlayerMind
{
    private PlayerMindLayer _mindLayer;
    private Position _goal;
    private Position _enemyFlagStand;

    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
    }

    public override void Tick()
    {
        _enemyFlagStand ??= Body.ExploreEnemyFlagStands1()[0];
        if (Body.ActionPoints > 2 && !Body.CarryingFlag)
        {
            var flags = Body.ExploreFlags2();
            _goal = flags.Where(f => f.Team != Body.Color && f.PickedUp == false).Select(f => f.Position).FirstOrDefault();
        }
        if (Body.CarryingFlag)
        {
            var flagStand = Body.ExploreOwnFlagStand();
            _goal = flagStand;
        }
        var enemies = Body.ExploreEnemies1();
        if (enemies != null && enemies.Count > 0)
        {
            if (Body.RemainingShots == 0) Body.Reload3();
            Body.Tag5(enemies.First().Position);
        }

        if (_goal == null)
        {
            _goal = Body.Position;
        }
        var moved = Body.GoTo(_goal);
    }
}