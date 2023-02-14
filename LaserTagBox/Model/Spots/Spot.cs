using System;
using LaserTagBox.Model.Body;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Spots;

/// <summary>
///     A barrier that acts as an obstacle to agents.
/// </summary>
public class Barrier : Spot
{
}

/// <summary>
///     An object of interest (OOI) in the environment.
/// </summary>
public abstract class Spot : IAgent<PlayerBodyLayer>, IPositionable
{
    #region Properties
    /// <summary>
    ///     A reference to the environment in which the object is situated.
    /// </summary>
    protected PlayerBodyLayer Battleground { get; private set; }

    /// <summary>
    ///     The position of the object in the environment.
    /// </summary>
    public Position Position { get; set; }

    /// <summary>
    ///     The unique identifier of the object.
    /// </summary>
    public Guid ID { get; set; }
    #endregion
    
    #region Initialization
    /// <summary>
    ///     Initialization routine of the object.
    /// </summary>
    /// <param name="battleground">A reference to the environment in which the object is to be situated.</param>
    public virtual void Init(PlayerBodyLayer battleground)
    {
        Battleground = battleground;
    }
    #endregion

    #region Tick
    /// <summary>
    ///     The behavior routine of the object. (This is an abstract object without behavior).
    /// </summary>
    public virtual void Tick()
    {
        //do nothing
    }
    #endregion
}