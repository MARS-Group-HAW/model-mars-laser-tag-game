using System.Linq;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Mind;

public class YourPlayerMindPleaseRename : AbstractPlayerMind
{
    private PlayerMindLayer _mindLayer;
    private Position _goal;

    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
    }

    public override void Tick()
    {
        if (Body.ActionPoints < 10)
        {
            return;  //TODO execution order fix
        }
        var enemies = Body.ExploreEnemies1();
        if (enemies.Any())
        {
            _goal = enemies.First().Position.Copy();
            if (Body.RemainingShots == 0) Body.Reload3();
            Body.Tag5(enemies.First().Position);
        }
            
        if (_goal == null || Body.GetDistance(_goal) == 1)
        {
            var newX = RandomHelper.Random.Next(_mindLayer.Width);
            var newY = RandomHelper.Random.Next(_mindLayer.Height);
            _goal = Position.CreatePosition(newX, newY);
        }

        var moved = Body.GoTo(_goal);
        if (!moved) _goal = null;

    }
}