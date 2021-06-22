using System;
using LaserTagBox.Model.Body;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;

namespace LaserTagBox.Model.Mind
{
    public class YourPlayerMindPleaseRename : AbstractPlayerMind
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
                _goal = Position.CreatePosition(48, 48);
                Console.WriteLine("new goal "+_goal);
            }

            Body.GoTo(_goal);
            if (Body.Position.Equals(_goal))
            {
                if (Body.RemainingShots == 0)
                {
                    Body.Reload3();
                }
                Body.Tag5(Position.CreatePosition(50, 50));
            }
            Console.WriteLine(Body.Position);
        }
    }
}