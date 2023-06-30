using System;
using System.Linq;
using LaserTagBox.Model.Shared;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Mind.Examples;

public class Example10Rule : AbstractPlayerMind
{
    private PlayerMindLayer _mindLayer;
    private Position _goal;
    private Position _shootingTarget;
    private Boolean _isTeamLeader;
    private Boolean didMoveLastTick;
    

    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
    }

    
    public override void Tick()
    {
        /*
         if(_mindLayer.GetCurrentTick() %100 == 0)
        {
            var friends = Body.ExploreTeam();
            Console.WriteLine($"[{ID}] Current Tick: {_mindLayer.GetCurrentTick()} with {friends.Count} friends being seen");
        }
        */
        if (Body.ActionPoints < 10)
        {
            return;  //TODO execution order fix
        }

        var teamMates = Body.ExploreTeam();
        // check if any of the teammates have a larger than own. If any do, this agent is not the team leader 
        _isTeamLeader = !teamMates.Any((friend) => idIsLargerThanOwn(friend.Id));
        // if this agent is not the teamleader, get the position of one of the teammates that has a larger Id to follow them
        if (!_isTeamLeader)
        {
            _goal = teamMates.First((friend) => idIsLargerThanOwn(friend.Id)).Position.Copy();
        }
        


        var enemies = Body.ExploreEnemies1(); // 1 AP
        if ((teamMates.Count >= 1) && enemies.Count >= (teamMates.Count + 1)) // run away if not outnumbering enemies
        {
            if (_isTeamLeader)
            {
                var enemyPos = enemies.First().Position.Copy();
                var ownPos = Body.Position;
                var xDif = enemyPos.X - ownPos.X;
                var yDif = enemyPos.Y - ownPos.Y;
    
                var newX = xDif >= 0 ? ownPos.X+1 : ownPos.X-1;
                var newY = yDif >= 0 ? ownPos.Y + 1 : ownPos.Y - 1;
                _goal = Position.CreatePosition(newX, newY); 
            }
            
            // while running, should also be shooting! 
            if (Body.Stance != Stance.Standing)
            {
                Body.ChangeStance2(Stance.Standing); // 3 AP
            }
            else
            {
                Body.Reload3();
            }

            Body.Tag5(enemies.First().Position);
        }
        else if (enemies.Any()) // todo if there are more than 1 enemy, compute what directions are save and run away.
        {
            var enemy = enemies.First();

            // var goPureAttackingStrategy = (enemy.Stance == Stance.Kneeling || enemy.Stance == Stance.Lying);
            if (_isTeamLeader)
            {
                _goal = enemy.Position.Copy();
            }

            _shootingTarget = enemy.Position.Copy();
            var distance = Body.GetDistance(_shootingTarget);

            // if (goPureAttackingStrategy)
            // {

                if ((Body.Stance != Stance.Lying) && (distance <= 4))
                {
                    Body.ChangeStance2(Stance.Lying); // 3 AP
                }
                else if (((distance <= 7 && distance > 4) && Body.Stance != Stance.Kneeling))
                {
                    Body.ChangeStance2(Stance.Kneeling); // 3 AP
                }
                else
                {
                    Body.Reload3(); // 4 AP
                }

                Body.Tag5(enemies.First().Position); // keep this like this, in case the enemy moved?}
            
        }
        else if (_shootingTarget != null && Body.Stance == Stance.Lying)  // means the agent had somebody in range before
        {
            Body.ChangeStance2(Stance.Kneeling);    // 3 ActionPoints
            enemies = Body.ExploreEnemies1(); // 4 AP
            if (enemies.Any())
            {
                _shootingTarget = enemies.First().Position.Copy();
                Body.Tag5(enemies.First().Position); // shoot at enemy => 9 AP
            }
            else // means the enemy died?!
            {
                Body.ChangeStance2(Stance.Standing); // 6 AP
                _shootingTarget = null;
            }
        }
        else
        {
            if (Body.Stance != Stance.Standing && teamMates.Any((teamMate) => teamMate.Stance == Stance.Standing))
            {
                Body.ChangeStance2(Stance.Standing);
            }
        }
            
        // if no enemy is in sight or the _goal was reached(?) move randomly 
        
        /*
        if (_isTeamLeader && (_goal == null || Body.GetDistance(_goal) == 1))
        {
            
            var newX = RandomHelper.Random.Next(_mindLayer.Width);
            var newY = RandomHelper.Random.Next(_mindLayer.Height);
            _goal = Position.CreatePosition(newX, newY);
            
            setNewRandomGoal();
        }
        */
        if (_goal == null || Body.GetDistance(_goal) == 1)
        {
            setNewRandomGoal();
        }
        
        /*
         else if (!_isTeamLeader && Body.GetDistance(_goal) == 1)
        {
            _goal = Body.Position;
        } // todo das hier macht vlt gar nichts --> scheint nicht wie erwartet zu funktionieren??
        */
        
        if (!didMoveLastTick) setNewRandomGoal(); 
        didMoveLastTick = Body.GoTo(_goal);

        
        
        // if the agent could not move, set _goal to null
        // means that if the enemy moved out of sight, its last position is still the goal, so we should be able  

    }

    /// <summary>
    /// If the Id of the friend is longer, return true
    /// Otherwise, check chars from the last position and once they are different, return true if friends char is larger
    /// Else return false 
    /// </summary>
    /// <param name="friendId"></param>
    /// <returns></returns>
    private Boolean idIsLargerThanOwn(Guid friendId)
    {
        var agentIdString = ID.ToString();
        var friendIdString = friendId.ToString();
        var agentIdStringLength = agentIdString.Length;
        var friendIdStringLength = friendIdString.Length;
        if ( (agentIdStringLength != friendIdStringLength) && 
             (agentIdStringLength < friendIdStringLength) ) return true;

        for (int index = agentIdStringLength - 1; index >= 0; index--)
        {
            var agentChar = agentIdString[index];
            var friendChar = friendIdString[index];
            if (agentChar > friendChar) return false;
            if (agentChar < friendChar) return true;
        }

        return false; // should never be reached, unless they are equal. 
    }

    private void calcGoalToMoveAwayFromEnemy(EnemySnapshot enemy)
    {
        var enemyPos = enemy.Position.Copy();
        var ownPos = Body.Position;
        var xDif = enemyPos.X - ownPos.X;
        var yDif = enemyPos.Y - ownPos.Y;
    
        var newX = xDif >= 0 ? ownPos.X+1 : ownPos.X-1;
        var newY = yDif >= 0 ? ownPos.Y + 1 : ownPos.Y - 1;
        _goal = Position.CreatePosition(newX, newY); 
    }

    private void setNewRandomGoal()
    {
        var ownX = Body.Position.X;
        var ownY = Body.Position.Y;
        /*
        var randXInRange = -8 + RandomHelper.Random.Next(17);
        var randYInRange = -8 + RandomHelper.Random.Next(17);
        var newX = ownX + randXInRange;
        var newY = ownY + randYInRange;
        
        if (newX > _mindLayer.Width) newX = _mindLayer.Width - 1;
        else if (newX < 0) newX = 0;
        
        if (newY > _mindLayer.Height) newX = _mindLayer.Height - 1;
        else if (newY < 0) newY = 0;
        */
        var middleX = _mindLayer.Width / 2;
        var middleY = _mindLayer.Height / 2;
        var newX = ownX >= middleX ? ownX - RandomHelper.Random.Next(middleX) : ownX + RandomHelper.Random.Next(middleX);
        var newY = ownY >= middleY ? ownY - RandomHelper.Random.Next(middleY) : ownY + RandomHelper.Random.Next(middleY);
        
        _goal = Position.CreatePosition(newX, newY);
    }
}