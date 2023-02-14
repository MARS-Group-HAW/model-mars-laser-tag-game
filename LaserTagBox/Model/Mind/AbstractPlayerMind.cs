using System;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Agents;

namespace LaserTagBox.Model.Mind;

/// <summary>
///     Create a subclass of <code>AbstractPlayerMind</code> for your own artificial intelligence.
/// </summary>
public abstract class AbstractPlayerMind : IAgent<PlayerMindLayer>
{
    /// <summary>
    ///     Just a unique identification.
    /// </summary>
    public Guid ID { get; set; }

    /// <summary>
    ///     The body is the mind's interface to the world for exploration and interaction.
    /// </summary>
    public IPlayerBody Body { get; set; }

    /// <summary>
    ///     Provides the possibility to initialize the players mind.
    /// </summary>
    /// <param name="mindLayer">Can be ignored.</param>
    public abstract void Init(PlayerMindLayer mindLayer);

    /// <summary>
    ///     Provides the possibility to act.
    /// </summary>
    public abstract void Tick();
}