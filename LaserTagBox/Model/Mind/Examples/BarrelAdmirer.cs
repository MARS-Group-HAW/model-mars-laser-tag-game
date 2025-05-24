using System;
using System.Drawing;
using System.Linq;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;
using Mars.Numerics;

namespace LaserTagBox.Model.Mind.Examples;

public class BarrelAdmirer : AbstractPlayerMind
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
        _goal = Position.CreatePosition(5, 1);
        var moved = Body.GoTo(_goal);
    }
}