using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Body;
using Mars.Components.Services;
using Mars.Interfaces;
using Mars.Interfaces.Data;
using Mars.Interfaces.Layers;

namespace LaserTagBox.Model.Mind
{
    public class PlayerMindLayer : ILayer
    {
        private readonly PlayerBodyLayer _battleground;
        private ISimulationContext _simulationContext;
        private bool _initialized;

        public PlayerMindLayer(PlayerBodyLayer battleground)
        {
            _battleground = battleground;
        }

        public bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle = null,
            UnregisterAgent unregisterAgentHandle = null)
        {
            if (_initialized) throw new NotSupportedException();
            _initialized = true;

            _simulationContext = layerInitData.Context;

            using var bodies = _battleground.Bodies.Values.GetEnumerator();

            foreach (var mapping in layerInitData.AgentInitConfigs)
            {
                for (var i = 0; i < 3; i++)
                {
                    bodies.MoveNext();

                    var agent = (AbstractPlayerMind) AgentManager.SpawnAgents(mapping, registerAgentHandle,
                        unregisterAgentHandle,
                        new List<ILayer> {this}, null, 1).First().Value;
                    agent.Body = bodies.Current;
                    agent.Init(this);
                }
            }

            return true;
        }

        public long GetCurrentTick()
        {
            return _simulationContext.CurrentTick;
        }

        public void SetCurrentTick(long currentStep)
        {
            //do nothing
        }
    }
}