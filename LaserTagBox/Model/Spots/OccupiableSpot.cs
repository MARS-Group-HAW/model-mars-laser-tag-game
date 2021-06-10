using System.Linq;
using LaserTagBox.Model.Body;

namespace LaserTagBox.Model.Spots
{
    public class Barrier : Spot
    {
    }

    public class Ditch : OccupiableSpot
    {
    }

    public class Hill : OccupiableSpot
    {
    }

    public abstract class OccupiableSpot : Spot
    {
        public bool Free { get; set; }

        public override void Init(PlayerBodyLayer battleground)
        {
            base.Init(battleground);
            Free = true;
        }

        public override void Tick()
        {
            if (Free) return;
            
            // clean up, if necessary
            var bodies = Battleground.FigtherEnv.Entities;
            if (bodies.Any(agent => agent.Position.Equals(Position)))
            {
                Free = false;
            }
        }
    }
}