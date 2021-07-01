using System;
using System.Collections.Generic;
using LaserTagBox.Model.Body;
using LaserTagBox.Model.Shared;
using Mars.Common.Core.Collections.NonBlockingDictionary;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;

namespace LaserTagBox.Model.Mind
{
    public class LucaNiklasTimSchlaegertruppe : AbstractPlayerMind
    {        
        private const int ageThreshold = 25;
        private const double squadRange = 2;
        private const int mapX = 49;
        private const int mapY = 49;
        private static Position _goal;
        private Position localGoal;
        private Position knownGoal;
        private int currentTick;
        private int ticksUnmoved; 
        
        private static ExploredEnemy pubTarget;
        private static bool pubTargetSet = true;
        
        private static ConcurrentDictionary<Guid, ExploredEnemy> locatedEnemies = new ConcurrentDictionary<Guid, ExploredEnemy>();
        public override void Init(PlayerMindLayer mindLayer)
        {
            currentTick = 0;
            ticksUnmoved = 0;
        }

        public override void Tick()
        {
            //Clean old entries
            List<Guid> oldEntries = new List<Guid>();
            lock (locatedEnemies)
            {
                foreach (var entry in locatedEnemies)
                {
                    if (currentTick - entry.Value.tick > ageThreshold)
                        oldEntries.Add(entry.Key);
                }
            }
            oldEntries.ForEach(guid => locatedEnemies.Remove(guid));

            //Control Movement
            if (_goal == null || ticksUnmoved > 5)
            {
                _goal = Position.CreatePosition(RandomHelper.Random.Next(50), RandomHelper.Random.Next(50));
            }

            if (knownGoal == null || !knownGoal.Equals(_goal))
            {
                knownGoal = _goal.Clone() as Position;
                var xOffset = RandomHelper.Random.Next((int)squadRange) - squadRange / 2;
                var yOffset = RandomHelper.Random.Next((int)squadRange) - squadRange / 2;
                var x = (int)Math.Clamp(_goal.X + xOffset, 1, mapX);
                var y = (int)Math.Clamp(_goal.Y + yOffset, 1, mapY);
                localGoal = Position.CreatePosition(x, y);
            }
            bool moved = Body.GoTo(localGoal);
            if (!moved)
            {
                ticksUnmoved++;
            }
            else
            {
                ticksUnmoved = 0;
            }
            
            //Shooting and Reload
            if(Body.RemainingShots < 1)
                Body.Reload3();
            var enemies = Body.ExploreEnemies1();
            if (enemies != null)
            {
                foreach (var enemySnapshot in enemies)
                {
                    lock (locatedEnemies)
                    {
                        if (!locatedEnemies.ContainsKey(enemySnapshot.Id))
                            locatedEnemies.Add(enemySnapshot.Id, new ExploredEnemy(currentTick, enemySnapshot));
                        else
                            locatedEnemies[enemySnapshot.Id] = new ExploredEnemy(currentTick, enemySnapshot);
                    }
                    if (Body.ActionPoints >= 5)
                    {
                        Body.Tag5(enemySnapshot.Position);
                    }
                }
            }

            ExploredEnemy target = null;
            double minDist = Double.MaxValue;

            //Find public target
            if (pubTargetSet)
            {
                pubTargetSet = false;
                lock (locatedEnemies)
                {
                    foreach (var exploredEnemy in locatedEnemies.Values)
                    {
                        if (pubTarget == null)
                        {
                            pubTarget = exploredEnemy;
                            minDist = Body.GetDistance(pubTarget.enemy.Position);
                        }

                        if (Body.GetDistance(exploredEnemy.enemy.Position) < minDist)
                        {
                            pubTarget = exploredEnemy;
                        }
                    }
                }

                pubTargetSet = true;
            }
            while(!pubTargetSet){}
            
            //Check public target
            if (pubTarget != null && !Body.HasBeeline1(pubTarget.enemy.Position))
            {
                lock (locatedEnemies)
                {
                    foreach (var exploredEnemy in locatedEnemies.Values)
                    {
                        if (target == null)
                        {
                            target = exploredEnemy;
                            minDist = Body.GetDistance(target.enemy.Position);
                        }

                        if (Body.GetDistance(exploredEnemy.enemy.Position) < minDist)
                        {
                            target = exploredEnemy;
                        }
                    }
                }
            }
            else
            {
                target = pubTarget;
            }

            //Tag target
            if (target != null)
            {
                if (Body.RemainingShots > 0 && Body.ActionPoints >= 5)
                {
                    if (Body.HasBeeline1(target.enemy.Position))
                    {
                        Body.Tag5(target.enemy.Position);
                    }
                }
                else if (Body.RemainingShots < 1)
                {
                    if (Body.ActionPoints > 3 + 1 + 5)
                    {
                        Body.Reload3();
                        if (Body.HasBeeline1(target.enemy.Position))
                        {
                            Body.Tag5(target.enemy.Position);
                        }
                    }
                }
            }

            currentTick++;
        }
        private class ExploredEnemy
        {
            public int tick;
            public EnemySnapshot enemy;

            public ExploredEnemy(int tick, EnemySnapshot enemy)
            {
                this.tick = tick;
                this.enemy = enemy;
            }
        }
    }
}