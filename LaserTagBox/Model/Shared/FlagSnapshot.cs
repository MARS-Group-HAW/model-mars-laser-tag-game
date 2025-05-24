using System;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Shared;

public struct FlagSnapshot
{
    #region Properties
    /// <summary>
    ///     Identifies the item.
    /// </summary>
    public Guid Id { get; }
        
    /// <summary>
    ///     Indicates to which team the item belongs.
    /// </summary>
    public Color Team { get; }
        
    /// <summary>
    ///     Current position of the agent on the grid.
    /// </summary>
    public Position Position { get; }
    
    /// <summary>
    ///    Indicates whether the item is picked up by an agent.
    /// </summary>
    public bool PickedUp { get; }
    #endregion

    #region Constructor

    public FlagSnapshot(Guid id, Color team, Position position, bool pickedUp)
    {
        Id = id;
        Team = team;
        Position = Position.CreatePosition(position.X, position.Y);
        PickedUp = pickedUp;
    }
    #endregion
}