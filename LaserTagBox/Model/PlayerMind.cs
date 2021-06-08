using System;
using Mars.Interfaces.Agents;

namespace LaserTagBox.Model
{
    public class PlayerMind : IAgent<PlayerMindLayer>
    {
        public Guid ID { get; set; }
        public IPlayerBody Body { get; set; }

        public void Init(PlayerMindLayer mindLayer)
        {
        }

        public void Tick()
        {
        }

    }
}