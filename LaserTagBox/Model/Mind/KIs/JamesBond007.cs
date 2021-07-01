using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using LaserTagBox.Model.Body;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;

namespace LaserTagBox.Model.Mind
{
    public class JamesBond007 : AbstractPlayerMind
    {
        private Position _goal;
        private Position shoot_pos;
        private PlayerMindLayer Layer;
        private bool moving;

        public static int xMax = 50;
        public static int yMax = 50;
        public static int PatrolPoints = 8;

        public static long LastTick = -1;
        public static FriendSnapshot Leader = new FriendSnapshot();
        public static bool Initialized = false;
        public static List<EnemySnapshot> Enemies = new List<EnemySnapshot>();
        public static EnemySnapshot EnemyToShoot = new EnemySnapshot();
        public static bool WaitForCommand = true;

        public static int WaypointIndex = 0;
        public static List<Position> Waypoints = InitWaypoints();
        
        public override void Init(PlayerMindLayer mindLayer)
        {
            Layer = mindLayer;
            moving = false;
            // Console.WriteLine(Body.Color);
        }

        private static List<Position> InitWaypoints()
        {
            List<Position> points = new List<Position>();
            for (int i = 0; i < PatrolPoints; i++)
            {
                var pos = CreateRandomPosition();
                points.Add(pos);
            }

            return points;
        }

        private static Position CreateRandomPosition()
        {
            Position goal = null;
            Random rnd = new Random();
            while (goal == null)
            {
                try
                {
                    var x = rnd.Next(0, xMax+1);
                    var y = rnd.Next(0, yMax+1);
                    goal = Position.CreatePosition(x, y);
                        
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return goal;
        }

        private void SelectLeader(List<FriendSnapshot> team)
        {
            var currentTick = Layer.GetCurrentTick();

            if (currentTick > LastTick)
            {
                LastTick = currentTick;
                Leader = new FriendSnapshot(ID, Body.Color, Body.Stance, Body.Position, Body.Energy, Body.VisualRange,
                    Body.VisibilityRange);
                // Console.WriteLine("{0}:New Leader selected!", ID);
            }
        }

        private bool LeaderAlive(List<FriendSnapshot> team)
        {
            if (!Initialized)
            {
                // inital Leader Selection
                return false;
            }
            
            foreach (var member in team)
            {
                if (member.Id.Equals(Leader.Id))
                {
                    return true;
                }
            }

            return false;
        }

        private void ChangeStance(Stance stance)
        {
            if (Body.Stance != stance)
            {
                Body.ChangeStance2(stance);
            }
        }

        private void ShootEnemy(EnemySnapshot enemy)
        {
            if (Body.ActionPoints < 5 || Body.RemainingShots == 0)
            {
                Body.Reload3();
            }
            else
            {
                var hit = Body.Tag5(enemy.Position);
                // Console.WriteLine("{3}: Shot at Position: {0},{1}, Hit: {2}", enemy.Position.X, enemy.Position.Y, hit, ID);
            }
        }

        private void ShootCycle()
        {
            if (Enemies.Count > 0)
            {
                while (Body.ActionPoints >= 5)
                {
                    ShootEnemy(EnemyToShoot);
                }
            }
            else
            {
                if (Body.RemainingShots < 5)
                {
                    Body.Reload3();
                }
            }
        }

        private void LeaderMove(List<FriendSnapshot> team)
        {
            bool move = true;
            foreach (var ally in team)
            {
                if (!ally.Position.Equals(Leader.Position))
                {
                    move = false;
                }
            }

            if (move)
            {
                var index = WaypointIndex % Waypoints.Count;
                var reachable = Body.GoTo(Waypoints[index]);
                if (!reachable)
                {
                    // Console.WriteLine("GOAL UNVAILD!");
                    Waypoints[index] = CreateRandomPosition();
                }
            }
            // Leader Movement: Move along specified points as long the game goes on 
            if (Body.Position.Equals(Waypoints[WaypointIndex % Waypoints.Count]))
            {
                WaypointIndex++;
                // Console.WriteLine("Moving to new Goal: X: {0}, Y: {1}",Waypoints[WaypointIndex % Waypoints.Count].X, Waypoints[WaypointIndex % Waypoints.Count].Y);
                // Console.WriteLine("Index: {0}", WaypointIndex % Waypoints.Count);
            }
        }

        public override void Tick()
        {
            if (Body.Alive)
            {
                var team = Body.ExploreTeam();
                SelectLeader(team);

                if (ID.Equals(Leader.Id))
                {
                    // do leader stuff
                    
                    ChangeStance(Stance.Standing);
                    LeaderMove(team);
                    
                    // Console.WriteLine("{0}, Leader: Moving to Position: {1},{2}", Layer.GetCurrentTick(), Waypoints[WaypointIndex % Waypoints.Count].X, Waypoints[WaypointIndex % Waypoints.Count].Y);
                    Enemies = Body.ExploreEnemies1();
                    ChangeStance(Stance.Lying);
                    if (Enemies.Count > 0)
                    {
                        EnemyToShoot = Enemies[0];

                        foreach (var e in Enemies)
                        {
                            if (e.Stance > EnemyToShoot.Stance)
                            {
                                EnemyToShoot = e;
                            }
                        }
                        //Console.WriteLine("LEader has Beeline: {0}", Body.HasBeeline1(EnemyToShoot.Position));
                    }
                    WaitForCommand = false;
                    
                    ShootCycle();
                }
                else
                {
                    // be sure to move to the same field as Leader
                    Body.GoTo(Leader.Position);
                    ChangeStance(Stance.Lying);
                    if (WaitForCommand)
                    {
                        //Console.WriteLine("SNIPER doing stuff without Command!");
                    }

                    if (Enemies.Count > 0)
                    {
                        //Console.WriteLine("Sniper has Beeline: {0}", Body.HasBeeline1(EnemyToShoot.Position));
                    }
                    ShootCycle();
                }
            }
            else
            {
                // Console.WriteLine("Agent {0}: Ticked while DEAD!!", ID);
            }
        }
    }
}
