using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using LaserTagBox.Model.Spots;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;
using Mars.Numerics;

namespace LaserTagBox.Model.Body;

/// <summary>
///     Represents the spatially explicit body of a LaserTag agent that is situated in the LaserTag environment.
/// </summary>
public class PlayerBody : MovingAgent, IPlayerBody
{
    #region Properties
    //	*********************** core attributes ***********************
    /// <summary>
    ///     The name of the team to which the agent belongs.
    /// </summary>
    public string TeamName { get; set; }
        
    /// <summary>
    ///     Represents the agent's vital state.
    /// </summary>
    public bool Alive { get; private set; } = true;
        
    /// <summary>
    ///     Points that can be spent on actions per tick.
    /// </summary>
    public int ActionPoints { get; private set; } = 10;
        
    /// <summary>
    ///     The agent's current energy count.
    /// </summary>
    public int Energy { get; private set; } = 100;
        
    /// <summary>
    ///     The agent's point count (for tagging enemy agents, etc.).
    /// </summary>
    public int GamePoints { get; private set; }
        
    //	*********************** tagging attributes ***********************
    /// <summary>
    ///     Returns true if the agent was tagged during the last tick (i.e., the tick before the current tick).
    /// </summary>
    public bool WasTaggedLastTick => _currentTick - 1 == _tickWhenLastTagged;
        
    /// <summary>
    ///     The remaining number of shots (tagging opportunities) of the agent.
    /// </summary>
    public int RemainingShots { get; private set; } = 5;
    #endregion

    #region Fields
    /// <summary>
    ///     Stores the current tick.
    /// </summary>
    private long _currentTick = -1;
        
    /// <summary>
    ///     Stores the most recent tick during which the agent was tagged by an enemy agent.
    /// </summary>
    private long _tickWhenLastTagged = -100;
    #endregion

    #region Tick
    /// <summary>
    ///     The behavior routine of the spatially explicit component of the LaserTag agent, which is executed
    ///     automatically by the LaserTag framework.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if this method is called manually</exception>
    public override void Tick()
    {
        base.Tick();
        
        if (!Alive) return;
        if (_currentTick == Battleground.GetCurrentTick())
            throw new InvalidOperationException("Don't call the Tick method, it's done by the system.");
        _currentTick = Battleground.GetCurrentTick();

        RefillPoints();
    }
    #endregion
    
    #region User Methods
    /// <summary>
    ///     Explores the hills in the agent's field of vision.
    /// </summary>
    /// <returns>A list of Hill objects</returns>
    public List<Position> ExploreHills1() => ExploreSpots(typeof(Hill));

    /// <summary>
    ///     Explores the barriers in the agent's field of vision.
    /// </summary>
    /// <returns>A list of Barrier objects</returns>
    public List<Position> ExploreBarriers1() => ExploreSpots(typeof(Barrier));

    /// <summary>
    ///     Explores the ditches in the agent's field of vision.
    /// </summary>
    /// <returns>A list of Ditch objects</returns>
    public List<Position> ExploreDitches1() => ExploreSpots(typeof(Ditch));
        
    /// <summary>
    ///     Explores the agent's team members across the entire environment.
    /// </summary>
    /// <returns>A list of FriendSnapshot objects</returns>
    public List<FriendSnapshot> ExploreTeam()
    {
        return new List<FriendSnapshot>(Battleground.FighterEnv
            .Entities.Where(body => body.Color == Color && body != this).Select(b =>
                new FriendSnapshot(b.ID, b.MemberId, b.Color, b.Stance, b.Position, b.Energy, b.VisualRange, b.VisibilityRange))
            .ToList());
    }
        
    /// <summary>
    ///     Explores enemy agents in the agent's field of vision.
    /// </summary>
    /// <returns>A list of EnemySnapshot objects</returns>
    public List<EnemySnapshot> ExploreEnemies1()
    {
        if (ActionPoints < 1) return null;
        ActionPoints -= 1;
        return Battleground.FighterEnv
            .Explore(Position, VisualRange, -1,
                player => IsEnemy(player) && HasBeeline(player) && IsVisible(player))
            .Select(player => new EnemySnapshot(player.ID, player.MemberId, player.Color, player.Stance, player.Position))
            .ToList();
    }
        
    /// <summary>
    ///     Determines whether there exists a direct line of sight between the caller and the given position
    /// </summary>
    /// <param name="other">The position whose line of sight relative to the caller is to be determined</param>
    /// <returns>boolean</returns>
    public bool HasBeeline1(Position other)
    {
        if (ActionPoints < 1) return false;
        ActionPoints -= 1;

        return HasBeeline(other);
    }
        
    /// <summary>
    ///     Changes the agent's current stance to the given stance.
    /// </summary>
    /// <param name="newStance">The given stance</param>
    public void ChangeStance2(Stance newStance)
    {
        if (ActionPoints < 2) return;
        ActionPoints -= 2;

        Stance = newStance;
        MovementDelayCounter = MovementDelayPenalty;
    }

    /// <summary>
    ///     Shoots at the given Position. Returns true if shot was successfully executed, otherwise false.
    /// </summary>
    /// <param name="aimedPosition">The position to be shot at</param>
    /// <returns>boolean</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if a player has an invalid Stance</exception>
    public bool Tag5(Position aimedPosition)
    {
        if (ActionPoints < 5) return false;
        ActionPoints -= 5;

        if (RemainingShots < 1) return false;
        RemainingShots--;

        if (!HasBeeline(aimedPosition)) return false;

        var enemy = Battleground.GetAgentOn(aimedPosition);
        if (enemy == null) return false;
        if (enemy.Color == Color) return false;

        var fieldType = Battleground.GetIntValue(aimedPosition);
        var enemySpot = fieldType switch
        {
            3 => 0, // in ditch
            2 => 2, // on hill
            _ => 1  // on normal ground
        };
        var enemyStance = enemy.Stance switch
        {
            Stance.Standing => 2,
            Stance.Kneeling => 1,
            Stance.Lying => 0,
            _ => throw new ArgumentOutOfRangeException()
        };

        var stanceValue = Stance switch
        {
            Stance.Standing => 8,
            Stance.Kneeling => 6,
            Stance.Lying => 4,
            _ => throw new ArgumentOutOfRangeException()
        };

        var success = RandomHelper.Random.Next(10) + enemyStance + enemySpot > stanceValue;
        if (success)
        {
            GamePoints += 10;
            if (enemy.Tagged()) GamePoints += 10; // bonus points
            return true;
        }

        return false;
    }
        
    /// <summary>
    ///     Refills the ammunition of the agent's tagging weapon.
    /// </summary>
    public void Reload3()
    {
        if (ActionPoints < 3) return;
        ActionPoints -= 3;
        RemainingShots = 5;
    }
    #endregion

    #region Internal Methods
    /// <summary>
    ///     Inserts the agent into the LaserTag environment.
    /// </summary>
    protected override void InsertIntoEnv() => Battleground.FighterEnv.Insert(this);
        
    /// <summary>
    ///     Generic exploration method for exploring points of interest (POIs) in the environment.
    /// </summary>
    /// <param name="type">The type of POI to be explored</param>
    /// <returns>A list of POIs of the requested type</returns>
    private List<Position> ExploreSpots(Type type)
    {
        if (ActionPoints < 1) return null;
        ActionPoints -= 1;
        return Battleground.SpotEnv
            .Explore(Position, VisualRange, -1, spot => spot.GetType() == type && HasBeeline(spot))
            .Select(spot => Position.CreatePosition(spot.Position.X, spot.Position.Y)).ToList();
    }
        
    /// <summary>
    ///     Returns true if the caller has a direct line of sight to the given positionable object, otherwise false 
    /// </summary>
    /// <param name="other">The positionable object whose line of sight relative to the caller is to be determined</param>
    /// <returns>boolean</returns>
    private bool HasBeeline(IPositionable other) =>
        Battleground.HasBeeline(Position.X, Position.Y, other.Position.X, other.Position.Y);
        
    /// <summary>
    ///     Determines whether there exists a direct line of sight between the caller and the given position
    /// </summary>
    /// <param name="other">The position whose line of sight relative to the caller is to be determined</param>
    /// <returns>boolean</returns>
    private bool HasBeeline(Position other) =>
        Battleground.HasBeeline(Position.X, Position.Y, other.X, other.Y);
        
    /// <summary>
    ///     Returns the distance from the caller to the given position, using the Manhattan distance measure.
    /// </summary>
    /// <param name="position">The position whose distance to the caller is to be determined</param>
    /// <returns>An integer that represents the calculated distance</returns>
    public int GetDistance(Position position) =>
        (int) Distance.Manhattan(Position.PositionArray, position.PositionArray);

    /// <summary>
    ///     Returns true if the given PlayerBody is member of a different team than the caller.
    /// </summary>
    /// <param name="enemy">The PlayerBody whose team membership is to be compared</param>
    /// <returns>boolean</returns>
    private bool IsEnemy(PlayerBody enemy) => enemy.Color != Color;

    /// <summary>
    ///     Returns true if the given PlayerBody is visible from the caller's current position.
    /// </summary>
    /// <param name="enemy">The PlayerBody whose visibility relative to the caller is to be determined</param>
    /// <returns>boolean</returns>
    private bool IsVisible(PlayerBody enemy) => enemy.VisibilityRange >= GetDistance(enemy.Position);
        
    /// <summary>
    ///     Moves the agent to the given position.
    /// </summary>
    /// <param name="position">The position to which the agent is to be moved.</param>
    /// <returns>The position to which the agent was moved</returns>
    protected override Position MoveToPosition(Position position) =>
        Battleground.FighterEnv.PosAt(this, position.PositionArray);

    /// <summary>
    ///     Handles a successful tag of the agent.
    /// </summary>
    /// <returns>true if the agent's energy is negative, otherwise false</returns>
    private bool Tagged()
    {
        _tickWhenLastTagged = _currentTick;
        Energy -= 10;
        if (Energy >= 0) return false;

        Die();
        return true;
    }

    /// <summary>
    ///     Handles the agent's death.
    /// </summary>
    private void Die()
    {
        Battleground.FighterEnv.Remove(this);
        // Do not remove agent from tick cycle for evaluation purposes
            
        ActionPoints = 0;
        Alive = false;
    }

    /// <summary>
    ///     Refills the action points of the agent to their default maximum value.
    /// </summary>
    private void RefillPoints()
    {
        ActionPoints = 10;
        if (MovementDelayCounter > 0) MovementDelayCounter--;
        HasMoved = false;
        RegenerateEnergy();
    }

    /// <summary>
    ///     Increments the agent's energy with a default positive value.
    /// </summary>
    private void RegenerateEnergy()
    {
        if (Alive && Energy < 100)
        {
            Energy += 1;
        }
    }
    #endregion
}