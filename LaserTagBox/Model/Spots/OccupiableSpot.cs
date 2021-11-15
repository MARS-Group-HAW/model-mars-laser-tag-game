using System.Linq;
using LaserTagBox.Model.Body;
using Mars.Components.Environments.Cartesian;

namespace LaserTagBox.Model.Spots
{
    public class Barrier : Spot
    {
        public override bool IsRoutable(ICharacter character) => false;

        public override CollisionKind? HandleCollision(ICharacter character) => CollisionKind.Block;

        public override VisibilityKind? HandleExploration(ICharacter explorer) => VisibilityKind.Opaque;
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
            var bodies = Battleground.Environment.Entities;
            Free = !bodies.Any(agent => agent.Position.Equals(Position));
        }

        public override bool IsRoutable(ICharacter character) => true;

        public override CollisionKind? HandleCollision(ICharacter character) => CollisionKind.Pass;

        public override VisibilityKind? HandleExploration(ICharacter explorer) => VisibilityKind.Transparent;
    }
}