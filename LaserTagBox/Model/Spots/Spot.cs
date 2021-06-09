using System;
using LaserTagBox.Model.Body;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Spots
{
    public abstract class Spot : IAgent<PlayerBodyLayer>, IPositionable
    {
        public virtual void Init(PlayerBodyLayer battleground)
        {
            Battleground = battleground;
        }

        public Position Position { get; set; }
        protected PlayerBodyLayer Battleground { get; private set; }
        public Guid ID { get; set; }

        public virtual void Tick()
        {
            //do nothing
        }
    }
}