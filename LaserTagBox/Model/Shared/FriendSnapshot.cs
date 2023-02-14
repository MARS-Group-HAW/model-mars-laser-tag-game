using System;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Shared;

/// <summary>
///     Provides information about other team members. 
/// </summary>
public  struct FriendSnapshot
{
    #region Properties
    /// <summary>
    ///     Identifies the player.
    /// </summary>
    public Guid Id { get; }
        
    /// <summary>
    ///     Alternative identifier of the player.
    /// </summary>
    public int MemberId { get; }
        
    /// <summary>
    ///     Indicates to witch team the agent belongs.
    /// </summary>
    public Color Team { get; }
        
    /// <summary>
    ///     The stance has influence on visibility (explorable by others) and visual range (extend of on exploration).
    /// </summary>
    public Stance Stance { get; }
        
    /// <summary>
    ///     Current position of the agent on the grid.
    /// </summary>
    public Position Position { get; }
        
    /// <summary>
    ///     Remaining energy of the agent.
    /// </summary>
    public int Energy { get; }
        
    /// <summary>
    ///     Provides a metric on the distance for exploring.
    /// </summary>
    public double VisualRange { get; }

    /// <summary>
    ///     Provides a metric on how visible the agent is for enemies.
    /// </summary>
    public double VisibilityRange { get; }
    #endregion

    #region Constructor

    public FriendSnapshot(Guid id, int memberId, Color team, Stance stance, Position position, int energy,
        double visualRange, double visibilityRange)
    {
        Id = id;
        MemberId = memberId;
        Team = team;
        Stance = stance;
        Position = Position.CreatePosition(position.X, position.Y);
        Energy = energy;
        VisualRange = visualRange;
        VisibilityRange = visibilityRange;
    }
    #endregion
}