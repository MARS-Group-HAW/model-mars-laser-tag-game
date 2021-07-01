using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;
using ServiceStack;

namespace LaserTagBox.Model.Mind
{
    public class Myron : AbstractPlayerMind
    {
        private Position _goal;

        private static List<Myron> _teamMates = new List<Myron>();

        // private static HashSet<Position> _barrierPositions = new HashSet<Position>();
        //private static HashSet<Position> HillPositions = new HashSet<Position>();
        //private static HashSet<Position> DitchPositions = new HashSet<Position>();

        private PlayerMindLayer _playerMindLayer;

        private static Myron teamLeader;

        private bool isTeamLeader;

        private static bool printPoints = true;

        private int mapWidth;
        private int mapHeight;
        
        public override void Init(PlayerMindLayer mindLayer)
        {
            mapHeight = 50;         // Access map height and width?
            mapWidth = 50;

            _playerMindLayer = mindLayer;
            if (_teamMates.IsEmpty())
            {
                isTeamLeader = true;
                teamLeader = this;
            }
            else
            {
                isTeamLeader = false;
            }

            _teamMates.Add(this); // make 3 different roles to distinguish ?
        }

        private void DropShot(List<EnemySnapshot> enemies)
        {
            if (!enemies.IsEmpty())
            {
                var closestEnemy = enemies.Map(enemy => enemy.Position).Aggregate((closest, next) =>
                    Body.GetDistance(next) < Body.GetDistance(closest) ? next : closest);

                if (Body.RemainingShots == 0)
                {
                    Body.Reload3();
                }

                if (Body.Stance != Stance.Lying)
                {
                    Body.ChangeStance2(Stance.Lying);
                }

                var hit = Body.Tag5(closestEnemy);
                if (hit)
                {
                    //Console.WriteLine("Enemy hit");
                }
            }
        }

        public override void Tick()
        {
            if (Body.ActionPoints == 10)
            {
                if (!Body.Stance.Equals(Stance.Standing))
                {
                    Body.ChangeStance2(Stance.Standing);
                }

                var nearbyEnemies = Body.ExploreEnemies1();

                if (isTeamLeader)
                {
                    if (_goal == null || Body.Position.Equals(_goal))
                    {
                        ExploreEnvironment();
                    }
                    else if(!nearbyEnemies.IsEmpty())
                    {
                        var closestEnemy = nearbyEnemies.Map(enemy => enemy.Position).Aggregate((closest, next) =>
                            Body.GetDistance(next) < Body.GetDistance(closest) ? next : closest);
                        _goal = closestEnemy;
                        Body.GoTo(_goal);
                    }
                    else
                    {
                        Body.GoTo(_goal);
                    }
                }
                else
                {
                    if (!teamLeader.Body.Alive)
                    {
                        //Console.WriteLine("Teamleader died - new Teamleader selected");
                        SelectNewLeader();
                        return;
                    }
                    FollowLeader();
                }

                DropShot(nearbyEnemies);

                if (!Body.Stance.Equals(Stance.Lying))
                {
                    Body.ChangeStance2(Stance.Lying);
                }

                Body.Reload3(); // If Action Points left reload
            }

            if (_playerMindLayer.GetCurrentTick() == 1800 && printPoints)
            {
                //Console.WriteLine("Quan Points: " + _teamMates.Map(mate => mate.Body.GamePoints).Sum());
                printPoints = false;
            }
        }

        private void ExploreEnvironment()
        {
            if (_goal == null || Body.GetDistance(_goal) < 2)
            {
                _goal = new Position(RandomHelper.Random.Next(0, mapWidth + 1), RandomHelper.Random.Next(0, mapHeight + 1)); 
            }

            bool reachable = false;
            try
            {
                reachable = Body.GoTo(_goal);
            }
            catch (ArgumentOutOfRangeException e)       // if map size to large
            {
                // do nothing
            }
            
            
            if (!reachable)
            {
                _goal = null;
            }
        }

        private void FollowLeader()
        {
            _goal = teamLeader.Body.Position;
            if (Body.GetDistance(_goal) > 1)
            {
                Body.GoTo(_goal);
            }
        }

        private void SelectNewLeader()
        {
            teamLeader.isTeamLeader = false;
            isTeamLeader = true;
            teamLeader = this;
        }
    }
}