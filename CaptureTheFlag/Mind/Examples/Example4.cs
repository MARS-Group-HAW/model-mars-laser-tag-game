using System;
using System.Collections.Generic;
using System.Linq;
using LaserTag.Core.Model.Mind;
using LaserTag.Core.Model.Shared;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;
namespace CaptureTheFlag.Mind.Examples;
public class Example4 : AbstractPlayerMind
{
    private PlayerMindLayer _mindLayer;
    private Position _currentGoal;
    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
    }
    public override void Tick()
    {
        
        var enemiesInSight = Body.ExploreEnemies1(); 
        if (enemiesInSight != null && enemiesInSight.Any()) // Check if any enemies are visible.
        {
            
            var targetEnemy = enemiesInSight.OrderBy(e => Body.GetDistance(e.Position)).First(); // Target the closest enemy.

            
            
            //the enemy is within the agent's full visual range and got body shots remianing 
            if (Body.RemainingShots > 0) // Engage if within full visual range
            {
                
                if (Body.Stance != Stance.Kneeling && Body.ActionPoints >= 2)
                {
                    Body.ChangeStance2(Stance.Kneeling);
                }
                Body.Tag5(targetEnemy.Position); // Shoot immediately from current stance.
                Body.ChangeStance2(Stance.Standing);
                
            }
            else Body.Reload3();
            Body.Tag5(targetEnemy.Position); // Shoot immediately from current stance.
        }
        
        // Flag Management
        // If carrying the enemy flag, goal is to return to own flag stand
        if (Body.CarryingFlag)
        {
           
            var ownFlagStand = Body.ExploreOwnFlagStand();
            if (ownFlagStand != null)
            {
                if (Body.GoTo(ownFlagStand))
                {
                    _currentGoal = ownFlagStand;
                    // Standing for maximum speed when carrying flag
                    if (Body.Stance != Stance.Standing && Body.ActionPoints >= 2)
                    {
                        Body.ChangeStance2(Stance.Standing);
                    }
                   
                }
            }
        }
        else // Not carrying flag, try to capture or defend
        {
            
            // Check for dropped flags
            if (Body.ActionPoints >= 2)
            {
                var allFlags = Body.ExploreFlags2();
                if (allFlags != null && allFlags.Any())
                {
                    // Look for enemy flag that is dropped and not carried
                    var enemyDroppedFlag = allFlags.FirstOrDefault(f => f.Team != Body.Color && !f.PickedUp);
                    
                    if (enemyDroppedFlag.Id != Guid.Empty)
                    {
                        if (Body.GoTo(enemyDroppedFlag.Position))
                        {
                            _currentGoal = enemyDroppedFlag.Position;
                            return;
                        }
                    }
                    // Check if own flag is dropped and needs returning
                    var ownDroppedFlag = allFlags.FirstOrDefault(f => f.Team == Body.Color && !f.PickedUp && Body.GetDistance(f.Position) > 1);
                    if (ownDroppedFlag.Id != Guid.Empty && Body.ActionPoints >= 1)
                    {
                        if (Body.GoTo(ownDroppedFlag.Position))
                        {
                            _currentGoal = ownDroppedFlag.Position;
                            return;
                        }
                    }
                }
            }
            // Move towards enemy flag stand to capture
            var enemyFlagStands = Body.ExploreEnemyFlagStands1();
            if (enemyFlagStands != null && enemyFlagStands.Any() && Body.ActionPoints >= 1)
            {
                var closestEnemyFlagStand = enemyFlagStands.OrderBy(fs => Body.GetDistance(fs)).FirstOrDefault();
                if (closestEnemyFlagStand != null)
                {
                    if (Body.GoTo(closestEnemyFlagStand))
                    {
                        _currentGoal = closestEnemyFlagStand;
                        return;
                    }
                }
            }
        }
        
        
        // Survival and Critical States
        if (Body.WasTaggedLastTick || Body.Energy < 30)
        {
            
            // If no ditch, try to move to own flag stand (safe zone) if nearby and not targeted by enemies
            var ownFlagStand = Body.ExploreOwnFlagStand();
            if (ownFlagStand != null && Body.GetDistance(ownFlagStand) < 15 && Body.ActionPoints >= 1)
            {
                // Calculate distance between ownFlagStand and enemy position
                var enemiesNearFlagStand = Body.ExploreEnemies1()
                    ?.Any(e => Mars.Numerics.Distance.Manhattan(ownFlagStand.PositionArray, e.Position.PositionArray) < 5) ?? false;
                if (!enemiesNearFlagStand && Body.GoTo(ownFlagStand))
                {
                    _currentGoal = ownFlagStand;
                    return;
                }
            }
            // If still threatened, try to move away from the closest enemy
            var enemies = Body.ExploreEnemies1();
            if (enemies != null && enemies.Any() && Body.ActionPoints >= 1)
            {
                var closestEnemy = enemies.OrderBy(e => Body.GetDistance(e.Position)).First();
                Position evadePosition = CalculateEvadePosition(closestEnemy.Position);
                if (Body.GoTo(evadePosition))
                {
                    _currentGoal = evadePosition;
                    return;
                }
            }
            // If all else fails, just try to move randomly to evade
            if (Body.ActionPoints >= 1)
            {
                MoveToRandomSafeSpot();
                return;
            }
        }
        
        // General Exploration
        if (Body.ActionPoints >= 1)
        {
            _currentGoal = null;
            MoveToRandomSafeSpot(); 
            if (_currentGoal != null && Body.GoTo(_currentGoal))
            {
                return;
            }
        }
        // If current goal is null or reached, find a new one.
        if (_currentGoal == null || Body.GetDistance(_currentGoal) <= 1)
        {
            MoveToRandomStrategicSpot();
        }
        // Try to move towards the current goal if there's one
        if (_currentGoal != null && Body.ActionPoints >= 1)
        {
            var moved = Body.GoTo(_currentGoal);
            if (!moved)
            {
                _currentGoal = null;
            }
        }
    }
    // Helper methods 
    private Position CalculateEvadePosition(Position enemyPosition)
    {
        var currentX = Body.Position.X;
        var currentY = Body.Position.Y;
        var enemyX = enemyPosition.X;
        var enemyY = enemyPosition.Y;
        var deltaX = currentX - enemyX;
        var deltaY = currentY - enemyY;
        // Try to move 5 units away in the opposite direction
        var evadeX = currentX + Math.Sign(deltaX) * 5;
        var evadeY = currentY + Math.Sign(deltaY) * 5;
        evadeX = Math.Max(0, Math.Min(evadeX, _mindLayer.Width - 1));
        evadeY = Math.Max(0, Math.Min(evadeY, _mindLayer.Height - 1));
        return Position.CreatePosition(evadeX, evadeY);
    }
    private void MoveToRandomStrategicSpot()
    {
        // Explore Hills for vision, or just a random open spot
        var hills = Body.ExploreHills1();
        if (hills != null && hills.Any() && Body.ActionPoints >= 1)
        {
            _currentGoal = hills[RandomHelper.Random.Next(hills.Count)].Copy();
            if (Body.Stance != Stance.Standing && Body.ActionPoints >= 2)
            {
                Body.ChangeStance2(Stance.Standing);
            }
            return;
        }
        // If no hills, pick a random open spot on the map
        _currentGoal = Position.CreatePosition(
            RandomHelper.Random.Next(_mindLayer.Width),
            RandomHelper.Random.Next(_mindLayer.Height)
        );
        if (Body.Stance != Stance.Standing && Body.ActionPoints >= 2)
        {
            Body.ChangeStance2(Stance.Standing);
        }
    }
    private void MoveToRandomSafeSpot()
    {
        _currentGoal = Position.CreatePosition(
            RandomHelper.Random.Next(_mindLayer.Width),
            RandomHelper.Random.Next(_mindLayer.Height)
        );
        if (Body.Stance != Stance.Standing && Body.ActionPoints >= 2)
        {
            Body.ChangeStance2(Stance.Standing);
        }
    }
}