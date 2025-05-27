using System.Collections.Generic;
using LaserTagBox.Model.Items;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Shared;

/// <summary>
///     The <code>IPlayerBody</code> is the interface for any mind/AI to the world of agents.
///     Use it to explore and interact with the battleground.
/// </summary>
public interface IPlayerBody : IPositionable
{   
    //******************** costly ********************

    /// <summary>
    ///     Explores all <code>Barrier</code>s in sight of the agent. Costs 1 action point.
    /// </summary>
    /// <returns>A list of the barriers positions.</returns>
    List<Position> ExploreBarriers1();
    
    /// <summary>
    ///     Explores all <code>Water</code>s in sight of the agent. Costs 1 action point.
    /// </summary>
    /// <returns></returns>
    List<Position> ExploreWater1();
    
    /// <summary>
    ///     Explores all <code>ExplosiveBarrel</code>s in sight of the agent. Costs 1 action point.
    /// </summary>
    /// <returns></returns>
    List<Position> ExploreBarrels1();
    
    /// <summary>
    ///     Explores all explodable <code>ExplosiveBarrel</code>s in sight of the agent. Costs 1 action point.
    /// </summary>
    /// <returns></returns>
    List<Position> ExploreExplosiveBarrels1();

    /// <summary>
    ///     Explores all <code>Hill</code>s in sight of the agent. Costs 1 action point.
    /// </summary>
    /// <returns>A list of the hills positions.</returns>
    List<Position> ExploreHills1();

    /// <summary>
    ///     Explores all <code>Ditch</code>es in sight of the agent. Costs 1 action point.
    /// </summary>
    /// <returns>A list of the ditches positions.</returns>
    List<Position> ExploreDitches1();

    /// <summary> 
    ///     Explores all enemies in sight of the agent. Costs 1 action point.
    /// </summary>
    /// <returns>A list of the enemies information.</returns>
    List<EnemySnapshot> ExploreEnemies1();

    /// <summary>
    ///    Explores the enemy flag stands.
    /// </summary>
    List<Position> ExploreEnemyFlagStands1();

    /// <summary>
    ///    Explores the flags.
    /// </summary>
    /// <returns></returns>
    List<FlagSnapshot> ExploreFlags2();

    /// <summary>
    ///     Change the current stance of the agent. Costs 2 action points.
    /// </summary>
    /// <param name="newStance">The new stance of the agent.</param>
    void ChangeStance2(Stance newStance);

    /// <summary>
    ///     Reloads the weapons magazine count to 5 shots. Costs 3 action points.
    /// </summary>
    void Reload3();

    /// <summary>
    ///     Tries to tag an enemy at given position. The success depends on the distance, the enemies and own stance
    ///     value and a little luck. Costs 5 action points.
    /// </summary>
    /// <param name="aimedPosition">The position that is aimed on.</param>
    /// <returns>true if an enemy was tagged, false otherwise or if not enough action points are available</returns>
    bool Tag5(Position aimedPosition);
        
    /// <summary>
    ///     Indicates if there is a connecting line between given position and the agents position without any
    ///     obstacle in the way. Costs 1 action points.
    /// </summary>
    /// <param name="position">That is tested.</param>
    /// <returns>true if a beeline exists, false otherwise</returns>
    bool HasBeeline1(Position position);
        
    //******************** free of charge ********************

    /// <summary>
    ///     Provides all <code>IPlayerBody</code>s of the same team.
    /// </summary>
    /// <returns>A list of the teams player bodies.</returns>
    List<FriendSnapshot> ExploreTeam();  // TODO split into info and interaction part
    
    /// <summary>
    ///     Explores the position of the own flag stand.
    /// </summary>
    /// <returns></returns>
    Position ExploreOwnFlagStand();

    /// <summary>
    ///     Moves towards given position by using a D*-algorithm. The algorithm automatically moves around obstacles.
    ///     Always moves only one cell a tick. Can therefore only called once a tick for an agent.
    ///     If the agent is in another stance the moving delays (lying: 3 ticks/cell, kneeling 2 ticks/cell).
    /// </summary>
    /// <param name="goal">The position that should be reached.</param>
    /// <returns>true if the movement was successful, false otherwise.</returns>
    bool GoTo(Position goal);

    /// <summary>
    ///     Provides the distance between given position and the agents position.
    /// </summary>
    /// <param name="position">That is tested upon.</param>
    /// <returns>The distance in Manhattan calculation.</returns>
    int GetDistance(Position position); // Chebyshev diagonal = 1, manhattan diagonal = 2

    /// <summary>
    ///     The magazine count of the weapon. How many more tag-attempts can be made before reloading is required.
    /// </summary>
    int RemainingShots { get; }

    /// <summary>
    ///     The stance has influence on visibility (explorable by others) and visual range (extend of on exploration).
    /// </summary>
    Stance Stance { get; }

    /// <summary>
    ///     Current position of the agent on the grid.
    /// </summary>
    new Position Position { get; }

    /// <summary>
    ///     Remaining actions points of this agent for the current tick.
    /// </summary>
    int ActionPoints { get; }

    /// <summary>
    ///     Remaining energy of the agent.
    /// </summary>
    int Energy { get; }

    /// <summary>
    ///     Collected game points of the agent with regard to the game mode.
    /// </summary>
    int GamePoints { get; }

    /// <summary>
    ///     Indicates to witch team the agent belongs.
    /// </summary>
    Color Color { get; }

    /// <summary>
    ///     Provides a metric on the distance for exploring.
    /// </summary>
    double VisualRange { get; }

    /// <summary>
    ///     Provides a metric on how visible the agent is for enemies.
    /// </summary>
    double VisibilityRange { get; }

    /// <summary>
    ///     Indicates if the agent was tagged in the last tick.
    /// </summary>
    bool WasTaggedLastTick { get; }

    /// <summary>
    ///     Indicates if the agent is alive and can therefore act.
    /// </summary>
    bool Alive { get; }
    
    /// <summary>
    ///     Indicates if the agent is carrying the flag of opponent.
    /// </summary>
    bool CarryingFlag { get; }
}