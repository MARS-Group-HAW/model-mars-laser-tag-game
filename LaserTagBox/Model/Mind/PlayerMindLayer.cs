using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Body;
using Mars.Components.Services;
using Mars.Interfaces;
using Mars.Interfaces.Data;
using Mars.Interfaces.Layers;

namespace LaserTagBox.Model.Mind;

/// <summary>
///     A layer type that holds the minds of the LaserTag agents.
/// </summary>
public class PlayerMindLayer : ILayer
{
    #region Fields

    /// <summary>
    ///     
    /// </summary>
    private readonly PlayerBodyLayer _playerBodyLayer;
    
    /// <summary>
    ///     A simulation context object that holds layer initialization data and simulation metadata.
    /// </summary>
    private ISimulationContext _simulationContext;
    
    /// <summary>
    ///     A flag that tracks if layer initialization has happened.
    /// </summary>
    private bool _initialized;
    #endregion

    #region Properties
    /// <summary>
    ///     The width of the grid-based environment.
    /// </summary>
    public int Width => _playerBodyLayer.Width;

    /// <summary>
    ///     The height of the grid-based environment.
    /// </summary>
    public int Height => _playerBodyLayer.Height;
    #endregion

    #region Constructor and Initialization
    /// <summary>
    ///     Constructor of the layer type for receiving and storing the PlayerBodyLayer.
    /// </summary>
    /// <param name="playerBodyLayer">The given instance of the layer type PlayerBodyLayer.</param>
    public PlayerMindLayer(PlayerBodyLayer playerBodyLayer)
    {
        _playerBodyLayer = playerBodyLayer;
    }

    /// <summary>
    ///     Initialization routine of the layer type. Returns true if initialization was successful, otherwise false.
    /// </summary>
    /// <param name="layerInitData">External initialization and configuration data for constructing the layer and
    /// initializing agents.</param>
    /// <param name="registerAgentHandle">A handle for registering agents with the simulation.</param>
    /// <param name="unregisterAgentHandle">A handle for unregistering agents from the simulation.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Thrown if this method is called after the layer has already been
    /// initialized.</exception>
    /// <returns>boolean</returns>
    public bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle = null,
        UnregisterAgent unregisterAgentHandle = null)
    {
        if (_initialized) throw new NotSupportedException();
        _initialized = true;

        _simulationContext = layerInitData.Context;

        using var bodies = _playerBodyLayer.Bodies.Values.GetEnumerator();

        foreach (var mapping in layerInitData.AgentInitConfigs)
        {
            for (var i = 0; i < 3; i++)
            {
                bodies.MoveNext();

                var agent = (AbstractPlayerMind) AgentManager.SpawnAgents(mapping, registerAgentHandle,
                    unregisterAgentHandle,
                    new List<ILayer> {this}, null, 1).First().Value;
                agent.Body = bodies.Current;
                if (bodies.Current != null) bodies.Current.TeamName = mapping.Type.Name;
                agent.Init(this);
            }
        }

        return true;
    }
    #endregion

    #region Methods
    /// <summary>
    ///     Gets the current tick from the simulation context.
    /// </summary>
    /// <returns></returns>
    public long GetCurrentTick() => _simulationContext.CurrentTick;

    /// <summary>
    ///     Sets the current tick of the simulation to the given value.
    /// </summary>
    /// <param name="currentStep">The value to which to set the current tick of the simulation</param>
    public void SetCurrentTick(long currentStep)
    {
        //do nothing
    }
    #endregion
}