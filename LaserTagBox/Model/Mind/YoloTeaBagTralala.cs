using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Body;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Mind
{
    public class YoloTeaBagTralala : AbstractPlayerMind
    {

        private const int UNKNOWN = -1;
        private const int PLAIN = 0;
        private const int BARRIER = 1;
        private const int HILL = 2;
        private const int DITCH = 3;

        private bool teaBagMode;
        private int teaBagCounter;
        private Random posFinder = new Random();
        
        
        
        
        private Position _goal = Position.CreatePosition(40, 40);
        private PlayerMindLayer _mindLayer;

        // TODO:: implement
        private int[,] mapState;
        
        public override void Init(PlayerMindLayer mindLayer)
        {
            //do something
            _mindLayer = mindLayer;
            mapState = new int[52, 52];
            for (int i = 0; i < mapState.GetLength(0); i++)
            {
                for (int j = 0; j < mapState.GetLength(1); j++)
                {
                    mapState[i, j] = UNKNOWN;
                }
            }
        }

        public override void Tick()
        {
            if (Body.ActionPoints == 10)
            {

                if (teaBagMode)
                {
                    if (Equals(Body.Position, _goal))
                    {
                        if (teaBagCounter < 3)
                        {
                            while (Body.ActionPoints > 1 )
                            {
                                if (Body.Stance == Stance.Standing) Body.ChangeStance2(Stance.Kneeling);
                                else if (Body.Stance == Stance.Kneeling) Body.ChangeStance2(Stance.Standing);
                                teaBagCounter++;
                            }
                        }
                        else
                        {
                            teaBagCounter = 0;
                            teaBagMode = false;
                        }
                    }
                    else
                    {
                        Body.GoTo(_goal);
                        if (Body.RemainingShots < 5) Body.Reload3();
                    }
                }

                var enemiesInSight = Body.ExploreEnemies1();


                if (enemiesInSight != null)
                {
                    if (enemiesInSight.Any())
                    {
                        Fight(enemiesInSight);
                    }
                    else
                    {
                        ExploreMap();
                    }
                }
                
                if (Body.ActionPoints >= 3 && ShouldBeExplored()) UpdateMapState();
            }
        }

        private void ExploreMap()
        {
            if (!Equals(Body.Position, _goal))
            {
                Body.GoTo(_goal);
            }
            else
            {
                _goal = Position.CreatePosition(posFinder.Next(1,52), posFinder.Next(1, 52));
            }
        }

        private List<int> GetSurroundingEnvironment()
        {
            var stuff = new List<int>();
            for (int y = (int) Body.Position.Y + 1; y >= (int) Body.Position.Y - 1; y--)
            {
                for (int x = (int) Body.Position.X -1 ; x <= (int) Body.Position.X + 1; x++)
                {
                    stuff.Add(mapState[y,x]);
                }
            }

            return stuff;
        }

        private bool ShouldBeExplored()
        {
            int tilesInSight = 0;
            int unknownTilesInSight = 0;
            for (int i = 0; i < mapState.GetLength(0); i++)
            {
                for (int j = 0; j < mapState.GetLength(1); j++)
                {
                    if (CalcDistance(Position.CreatePosition(i,j)) <= Body.VisualRange)
                    {
                        tilesInSight++;
                        if (mapState[i,j] == UNKNOWN) unknownTilesInSight++;
                    }

                }
            }

            return unknownTilesInSight >= tilesInSight / 3;
        }

        private double CalcDistance(Position other)
        {

            return Body.GetDistance(other);
            //return Math.Sqrt(Math.Pow(me.X - other.X, 2) + Math.Pow(me.Y - other.Y, 2));
        }

        private void UpdateMapState()
        {
            // do stuff
            var hills = Body.ExploreHills1();
            var ditches = Body.ExploreDitches1();
            var barriers = Body.ExploreBarriers1();

            foreach (var hill in hills)
            {
                mapState[(int) hill.X, (int) hill.Y] = HILL;
            }
            
            foreach (var ditch in ditches)
            {
                mapState[(int) ditch.X, (int) ditch.Y] = DITCH;
            }
            
            foreach (var barrier in barriers)
            {
                mapState[(int) barrier.X, (int) barrier.Y] = BARRIER;
            }



            for (int x = 0; x < mapState.GetLength(0); x++)
            {
                for (int y = 0; y < mapState.GetLength(1); y++)
                {
                    if (CalcDistance(Position.CreatePosition(x,y)) <= Body.VisualRange && mapState[y,x] == -1)
                    {
                        if (FreeHasBeeline((int) Body.Position.X, (int) Body.Position.Y, x, y)) mapState[y, x] = PLAIN;
                    }
                }
            }
        }

        private void Fight(List<EnemySnapshot> enemySnapshots)
        {
            var best = enemySnapshots[0];
            var highest = -1;
            foreach (var enemySnapshot in enemySnapshots)
            {
                int sum = 0;
                sum += GetStanceValue(enemySnapshot.Stance);
                sum += GetEnvironmentValue(mapState[(int) enemySnapshot.Position.Y, (int) enemySnapshot.Position.X]);
                if (highest < sum)
                {
                    highest = sum;
                    best = enemySnapshot;
                }
            }
            

            var surroundings = GetSurroundingEnvironment();
            Position hasNoBeeline = null;
            if (!FreeHasBeeline((int)Body.Position.X, (int)Body.Position.Y + 1, (int) best.Position.X, (int) best.Position.Y)) 
                hasNoBeeline = Position.CreatePosition(Body.Position.X, Body.Position.Y + 1);
            else if (!FreeHasBeeline((int)Body.Position.X - 1, (int)Body.Position.Y, (int) best.Position.X, (int) best.Position.Y)) 
                hasNoBeeline = Position.CreatePosition(Body.Position.X - 1, Body.Position.Y);
            else if (!FreeHasBeeline((int)Body.Position.X + 1, (int)Body.Position.Y, (int) best.Position.X, (int) best.Position.Y)) 
                hasNoBeeline = Position.CreatePosition(Body.Position.X + 1, Body.Position.Y);
            else if (!FreeHasBeeline((int)Body.Position.X, (int)Body.Position.Y - 1, (int) best.Position.X, (int) best.Position.Y)) 
                hasNoBeeline = Position.CreatePosition(Body.Position.X, Body.Position.Y - 1);

            
            if (hasNoBeeline != null)
            {
                if (Body.RemainingShots > 0)
                {
                    var pointBefore = Body.GamePoints;
                    Body.Tag5(best.Position);
                    if (pointBefore + 15 < Body.GamePoints)
                    {
                        teaBagMode = true;
                        _goal = best.Position;
                        Body.GoTo(_goal);
                    }
                    else
                    {
                        Body.GoTo(hasNoBeeline);
                        if (Body.RemainingShots < 2 && Body.ActionPoints > 3) Body.Reload3();
                    }
                }
            }
            else
            {
                Position ditch = null;
                if (surroundings[1] == DITCH) ditch = Position.CreatePosition(Body.Position.X, Body.Position.Y + 1);
                else if (surroundings[3] == DITCH) ditch = Position.CreatePosition(Body.Position.X - 1, Body.Position.Y);
                else if (surroundings[5] == DITCH) ditch = Position.CreatePosition(Body.Position.X + 1, Body.Position.Y);
                else if (surroundings[7] == DITCH) ditch = Position.CreatePosition(Body.Position.X, Body.Position.Y - 1);
                if (ditch != null) Body.GoTo(ditch);
                Body.ChangeStance2(Stance.Lying);
            }
        }

        private int GetStanceValue(Stance stance)
        {
            return stance switch
            {
                Stance.Standing => 2,
                Stance.Kneeling => 1,
                Stance.Lying => 0,
                _ => -1
            };
        }

        private int GetEnvironmentValue(int env)
        {
            return env switch
            {
                HILL => 2,
                DITCH => 0,
                PLAIN => 1,
                _ => 0
            };
        }
        
        public bool FreeHasBeeline(int x1,int y1,int x2, int y2)
        {

            var x = x1;
            var y = y1;
            var w = x2 - x;
            var h = y2 - y;
            var dx1 = 0;
            var dy1 = 0;
            var dx2 = 0;
            var dy2 = 0;
            if (w < 0) dx1 = -1;
            else if (w > 0) dx1 = 1;
            
            if (h < 0) dy1 = -1;
            else if (h > 0) dy1 = 1;
            
            if (w < 0) dx2 = -1;
            else if (w > 0) dx2 = 1;
            
            var longest = Math.Abs(w);
            var shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1;
                else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            var numerator = longest / 2;
            for (var i = 0; i < longest; i++)
            {
                var intValue = mapState[y, x];
                if (intValue == 1 || intValue == 2) return false;

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }

            return true;
        }
    }
}