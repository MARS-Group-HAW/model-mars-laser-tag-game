using System.Linq;
using LaserTagBox.Model.Body;
using LaserTagBox.Model.Shared;

namespace LaserTagBox.Model.Spots;

/// <summary>
///     A ditch that agents can step into.
/// </summary>
public class Ditch : OccupiableSpot
{
}

/// <summary>
///     A hill that agents can step onto.
/// </summary>
public class Hill : OccupiableSpot
{
}

/// <summary>
///
/// </summary>
public class FlagStand : OccupiableSpot
{
    public Color Color { get; set; }
}

/// <summary>
///     An object of interest (OOI) that can be occupied by agents.
/// </summary>
public abstract class OccupiableSpot : Spot
{
    #region Properties
    /// <summary>
    ///     A flag that tracks the occupation status of the object. True if the object is free, otherwise false.
    /// </summary>
    public bool Free { get; set; }
    #endregion

    #region Initialization
    /// <summary>
    ///     Initialization routine of the object.
    /// </summary>
    /// <param name="battleground">A reference to the environment in which the object is to be situated.</param>
    public override void Init(PlayerBodyLayer battleground)
    {
        base.Init(battleground);
        Free = true;
    }
    #endregion

    #region Tick
    /// <summary>
    ///     Behavior routine of the object.
    /// </summary>
    public override void Tick()
    {
        if (Free) return;
            
        // clean up, if necessary
        var bodies = Battleground.FighterEnv.Entities;
        if (bodies.Any(agent => agent.Position.Equals(Position)))
        {
            Free = false;
        }
        else
        {
            Free = true;
        }
    }
    #endregion
}