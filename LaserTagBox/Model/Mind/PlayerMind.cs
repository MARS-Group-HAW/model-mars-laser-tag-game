using System;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Agents;

namespace LaserTagBox.Model.Mind
{
    public  class PlayerMind : IAgent<PlayerMindLayer>
    {
        public Guid ID { get; set; }
        public IPlayerBody Body { get; set; }

        public virtual void Init(PlayerMindLayer mindLayer)
        {
            
        }

        public virtual void Tick()
        {
            
        }

    }
}