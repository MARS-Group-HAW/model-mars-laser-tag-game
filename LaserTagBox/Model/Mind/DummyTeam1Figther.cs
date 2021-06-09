using System;
using System.Linq;
using Mars.Components.Services.Planning;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;

namespace LaserTagBox.Model.Mind
{
    public class DummyTeam1Figther : AbstractPlayerMind
    {
        private Position _goal;

        public override void Init(PlayerMindLayer mindLayer)
        {
            //do something
        }

        public override void Tick()
        {
            if (_goal == null || Body.GetDistance(_goal) < 4)
            {
                _goal = Position.CreatePosition(RandomHelper.Random.Next(50), RandomHelper.Random.Next(50));
                Console.WriteLine("new goal "+_goal);
            }
            Body.GoTo(_goal);
            
            Console.WriteLine(Body.Position);
            // var exploreHills1 = Body.ExploreHills1();
            // if (exploreHills1 != null && exploreHills1.Any())
            // {
            //     var position = exploreHills1.First();
            //     Body.GoTo(position);
            // }
        }
    }
}