using System;
using System.Linq;
using LaserTagBox.Model.Body;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using ServiceStack;

namespace LaserTagBox.Model.Spots;

/// <summary>
///     A barrier that acts as an obstacle to agents.
/// </summary>
public class Barrier : Spot
{
}

/// <summary>
///     A spot that represents a water body in the environment.
/// </summary>
public class Water : Spot
{
}

/// <summary>
///     A spot that represents an explosive barrel in the environment.
/// </summary>
public class ExplosiveBarrel : Spot
{
    /// <summary>
    ///     The radius of the explosion when the barrel is hit.
    /// </summary>
    private int _explosionRadius = 3;

    /// <summary>
    ///     The damage caused by the explosion.
    /// </summary>
    private int _damage = 100;

    private bool _hasBeenHit = false;
    
    /// <summary>
    ///     Indicates whether the barrel has exploded.
    /// </summary>
    public bool HasExploded { get; private set; }

    private void Explode()
    {
        HasExploded = true;
        var agents = Battleground.FighterEnv.Explore(Position, _explosionRadius).ToList();
        var barrels = Battleground.SpotEnv.Explore(Position, _explosionRadius, -1,
            spot => spot.GetType() == typeof(ExplosiveBarrel)).ToList();
        foreach (var agent in agents)
        {
            agent.TakeExplosionDamage(_damage, ID);
        }

        foreach (var barrel in barrels)
        {
            if (barrel != this)
            {
                ((ExplosiveBarrel)barrel).Tagged();
            }
        }
        // Console.WriteLine($"Explosive barrel at {Position} exploded, damaging {agents.Count} agents and tagging {barrels.Count - 1} barrels.");
    }

    public void Tagged()
    {
        _hasBeenHit = true;
    }
    
    public override void Tick()
    {
        if (!HasExploded && _hasBeenHit)
        {
            Explode();
        }
    }
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