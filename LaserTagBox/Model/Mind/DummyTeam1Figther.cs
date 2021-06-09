using System.Linq;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Mind
{
    public class DummyTeam1Figther : PlayerMind
    {
        public override void Init(PlayerMindLayer mindLayer)
        {
            //do something
        }

        public override void Tick()
        {
            Body.GoTo(Position.CreatePosition(4, 4));
            
            var exploreHills1 = Body.ExploreHills1();
            if (exploreHills1 != null && exploreHills1.Any())
            {
                var position = exploreHills1.First();
                Body.GoTo(position);
            }
        }
    }
}