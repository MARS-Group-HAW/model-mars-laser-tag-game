using System;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Agents;

namespace LaserTagBox.Model.Mind
{
    public abstract class PlayerMind : IAgent<PlayerMindLayer>
    {
        public Guid ID { get; set; }
        public IPlayerBody Body { get; set; }

        public abstract void Init(PlayerMindLayer mindLayer);

        public abstract void Tick();

    }
}