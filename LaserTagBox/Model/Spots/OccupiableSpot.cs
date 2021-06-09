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
            if (!Free)
            {
                var greenTeam = Battleground.FigtherEnv.Entities;

                var freeMe = true;

                foreach (var agent in greenTeam)
                    if (agent.Position.Equals(Position))
                        freeMe = false;

                //TODO do this for all 4 teams

                if (freeMe)
                {
                    Free = true;
                }
            }
        }
    }
}