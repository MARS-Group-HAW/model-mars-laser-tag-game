using System;
using System.Drawing;
using System.Linq;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Mind;

public class IdleAgent : AbstractPlayerMind
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
    }
}