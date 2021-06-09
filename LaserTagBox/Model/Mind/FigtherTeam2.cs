using System.Linq;

namespace LaserTagBox.Model.Mind
{
    public class FigtherTeam2 : PlayerMind
    {
        public override void Tick()
        {
            base.Tick();
            
            var exploreHills1 = Body.ExploreHills1();
            if (exploreHills1 != null && exploreHills1.Any())
            {
                var position = exploreHills1.First();
                Body.GoTo(position);
            }
        }
    }
}