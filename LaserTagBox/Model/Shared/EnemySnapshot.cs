using System;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Shared;

/// <summary>
///     Provides information about an explored enemy. 
/// </summary>
public readonly struct EnemySnapshot
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
    #endregion
        
    #region Constructor
    public EnemySnapshot(Guid id, int memberId, Color team, Stance stance, Position position)
    {
        Id = id;
        Team = team;
        Stance = stance;
        MemberId = memberId;
        Position = Position.CreatePosition(position.X, position.Y);
    }
    #endregion
}