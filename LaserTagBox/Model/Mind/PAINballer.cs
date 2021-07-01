using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using LaserTagBox.Model.Spots;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;
using ServiceStack;

namespace LaserTagBox.Model.Mind
{
    public class PAINballer : AbstractPlayerMind
    {
        private Position _goal;
        private Position _lastPos;

        public override void Init(PlayerMindLayer mindLayer)
        {
            _goal = getRandomPosition(); //so _goal is not null
            _lastPos = Body.Position;
        }

        public override void Tick()
        {
            //Body.GoTo(_goal);
            if (Body.RemainingShots == 0) Body.Reload3(); //reload case start of move

            //look for enemies
            List<EnemySnapshot> enemies = Body.ExploreEnemies1();
            if (enemies != null && enemies.Count > 0) //enemies are around
            {
                //increase chance of hitting when enough points available to tag after
                if (Body.ActionPoints >= 7) Body.ChangeStance2(Stance.Lying);
                Body.Tag5(enemies.First().Position);
                //Body.ChangeStance2(Stance.Lying); //lower own chance of getting hit
            }
            else if (!Body.Stance.Equals(Stance.Standing))
            {
                Body.ChangeStance2(Stance.Standing);
                Body.Reload3();
            }

            if (Body.Energy <= 20) //switch to passive playstyle when low
            {
                if (Body.Position.GetType() == typeof(Ditch)) //go camping in ditch when low
                {
                    _goal = RandomHelper.Random.Next(2) == 1
                        ? MOVE_AS_ONE()
                        : Body.Position; //randomly move out of ditch
                    var randomStance = RandomHelper.Random.Next(3);
                    Body.ChangeStance2(randomStance switch
                    {
                        0 => Stance.Kneeling,
                        1 => Stance.Lying,
                        2 => Stance.Standing,
                        _ => Stance.Standing
                    });
                }
                else
                {
                    _goal = getNearestDitch();
                }
            }
            else
            {
                if (Body.Position.GetType() == typeof(Hill)) //if on hill move down
                {
                    _goal = MOVE_AS_ONE();
                }
                else
                {
                    //look for nearest hill and set as new goal
                    List<Position> hills = Body.ExploreHills1();

                    if (hills != null && hills.Count > 0)
                    {
                        Position nearest = hills.First();
                        foreach (var position in hills)
                        {
                            if (Body.GetDistance(position) < Body.GetDistance(nearest))
                                _goal = nearest; //find nearest hill
                        }
                    }
                }
            }

            if (_goal.Equals(Body.Position) || _lastPos.Equals(Body.Position)) _goal = MOVE_AS_ONE();

            Body.GoTo(_goal); //stay on the move
            _lastPos = Body.Position;

            if (Body.RemainingShots == 0) Body.Reload3(); //reload case end of move

            //confuse enemies with console outputs
            Console.WriteLine("Mah Collah " + Body.Color);
            //Console.WriteLine("Mah Pos " + Body.Position);
            //Console.WriteLine("Mah goal " + _goal);
            Console.WriteLine("Mah Points " + Body.GamePoints);
            Console.WriteLine("Mah Energy " + Body.Energy);
        }

        private Position getRandomPosition()
        {
            Position randomPos;
            do
            {
                randomPos = Position.CreatePosition(RandomHelper.Random.Next(50) + 1, RandomHelper.Random.Next(45) + 1);
            } while (randomPos.X < 1 || randomPos.X > 50 || randomPos.Y < 1 || randomPos.Y > 50);
            //while (randomPos.GetType() == typeof(Barrier));

            return randomPos;
        }

        private Position getNearestDitch()
        {
            List<Position> ditches = Body.ExploreDitches1();
            Position chosen = Body.Position;
            if (ditches != null && ditches.Count > 0)
            {
                Position nearest = ditches.First();
                foreach (var position in ditches)
                {
                    if (Body.GetDistance(position) < Body.GetDistance(nearest))
                        chosen = nearest; //find nearest ditch
                }
            }

            return chosen;
        }

        private Position MOVE_AS_ONE()
        {
            Position chosen;
            if (Body.ExploreTeam().IsEmpty() || Body.ExploreTeam().Any(agent => agent.Position.Equals(Body.Position)))
            {
                chosen = getRandomPosition();
            }
            else
            {
                chosen = Body.ExploreTeam().First().Position;
            }

            return chosen;
        }
    }
}