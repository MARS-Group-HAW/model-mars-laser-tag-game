using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Core.Data;
using Mars.Interfaces;
using Mars.Interfaces.Data;
using Mars.Interfaces.Layers;

namespace LaserTagBox.Model
{
    public class PlayerMindLayer : ILayer
    {
        private readonly PlayerBodyLayer _battleground;
        private ISimulationContext _simulationContext;
        private bool _initialized;
        private Dictionary<Guid, PlayerMind> _minds;

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

            _minds = new Dictionary<Guid, PlayerMind>();
            var agentManager = layerInitData.Container.Resolve<IAgentManager>();

            var enumerator = layerInitData.AgentInitConfigs.GetEnumerator();
            //TODO init different agent types
            foreach (var body in _battleground.Bodies.Values)
            {
                var mind = agentManager.Spawn<PlayerMind, PlayerMindLayer>(null, mind => mind.Body = body).Take(1).First();
                _minds.Add(mind.ID, mind);
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