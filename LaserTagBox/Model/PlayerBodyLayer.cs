using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Components.Environments;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Data;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;

namespace LaserTagBox.Model
{
    public class PlayerBodyLayer : RasterLayer, ISteppedActiveLayer
    {
        public Dictionary<Guid, PlayerBody> Bodies { get; private set; }

        /// <summary>
        ///     Holds all agents in a 2-dimensional area for exploration purposes.
        /// </summary>
        public SpatialHashEnvironment<PlayerBody> FigtherEnv { get; private set; }

        /// <summary>
        ///     Holds all spots in a 2-dimensional area for exploration purposes.
        /// </summary>
        public SpatialHashEnvironment<Spot> SpotEnv { get; private set; }

        /// <summary>
        ///     Responsible to create new agents and initialize them with required dependencies
        /// </summary>
        public IAgentManager AgentManager { get; private set; }

        public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
            UnregisterAgent unregisterAgentHandle)
        {
            base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);

            FigtherEnv = new SpatialHashEnvironment<PlayerBody>(Width - 1, Height - 1) {CheckBoundaries = true};
            SpotEnv = new SpatialHashEnvironment<Spot>(Width - 1, Height - 1) {CheckBoundaries = true};
            AgentManager = layerInitData.Container.Resolve<IAgentManager>();

            // AgentManager.Spawn<Blue, Battleground>().ToList();
            // AgentManager.Spawn<Red, Battleground>().ToList();
            // AgentManager.Spawn<Green, Battleground>().ToList();
            // AgentManager.Spawn<Yellow, Battleground>().ToList();

            //TODO spawn different teams in dependence to the 
            Bodies = AgentManager.Spawn<PlayerBody, PlayerBodyLayer>().Take(3).ToDictionary(body => body.ID);

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var type = this[x, y];
                    var position = Position.CreatePosition(x, y);
                    var spot = CreatePassiveAgent(type, position);
                    SpotEnv.Insert(spot);
                }
            }

            return true;
        }

        private Spot CreatePassiveAgent(double type, Position position)
        {
            return type switch
            {
                1 => AgentManager.Spawn<Barrier, PlayerBodyLayer>(null, s => s.Position = position).Take(1).First(),
                2 => AgentManager.Spawn<Hill, PlayerBodyLayer>(null, s => s.Position = position).Take(1).First(),
                3 => AgentManager.Spawn<Ditch, PlayerBodyLayer>(null, s => s.Position = position).Take(1).First(),
                _ => null
            };
        }

        public Ditch NearestDitch(Position position)
        {
            return (Ditch) SpotEnv.Explore(position, -1, 1, spot => spot.GetType() == typeof(Ditch)).FirstOrDefault();
        }

        public Hill NearestHill(Position position)
        {
            return (Hill) SpotEnv.Explore(position, -1, 1, spot => spot.GetType() == typeof(Hill)).FirstOrDefault();
        }

        public Barrier NearestBarrier(Position position)
        {
            return (Barrier) SpotEnv.Explore(position, -1, 1, spot => spot.GetType() == typeof(Barrier))
                .FirstOrDefault();
        }


        public void Tick()
        {
            if (Context.CurrentTick % 20 == 0)
                Console.WriteLine($"Current tick: {Context.CurrentTick}");
        }

        public void PreTick()
        {
            //do nothing
        }

        public void PostTick()
        {
            //do nothing
        }

        public int GetintValue(double x, double y)
        {
            return (int) this[x, y];
        }

        public int GetintValue(Position position)
        {
            return (int) this[position.X, position.Y];
        }

        public List<PlayerBody> GetAll(Color color)
        {
            return FigtherEnv.Entities.Where(fighter => fighter.Color == color).ToList();
        }

        // implementation of Bresenham's Line Algorithm for obtaining a list of grid cells covered by a straight line between two points on the grid
        // http://tech-algorithm.com/articles/drawing-line-using-bresenham-algorithm/ for more information
        public bool HasBeeline(double x1, double y1, double x2, double y2)
        {
            var hasBeeline = true;
            var x = (int) x1;
            var y = (int) y1;
            var newX2 = (int) x2;
            var newY2 = (int) y2;
            var w = newX2 - x;
            var h = newY2 - y;
            var dx1 = 0;
            var dy1 = 0;
            var dx2 = 0;
            var dy2 = 0;
            if (w < 0)
            {
                dx1 = -1;
            }
            else if (w > 0)
            {
                dx1 = 1;
            }

            if (h < 0)
            {
                dy1 = -1;
            }
            else if (h > 0)
            {
                dy1 = 1;
            }

            if (w < 0)
            {
                dx2 = -1;
            }
            else if (w > 0)
            {
                dx2 = 1;
            }

            var longest = Math.Abs(w);
            var shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0)
                {
                    dy2 = -1;
                }
                else if (h > 0)
                {
                    dy2 = 1;
                }

                dx2 = 0;
            }

            var numerator = longest / 2;
            for (var i = 0; i < longest; i++)
            {
                if ((GetintValue(x, y) == 1) || (
                    GetintValue(x, y) == 2))
                {
                    hasBeeline = false;
                    return hasBeeline;
                }

                numerator = numerator + shortest;
                if (!(numerator < longest))
                {
                    numerator = numerator - longest;
                    x = x + dx1;
                    y = y + dy1;
                }
                else
                {
                    x = x + dx2;
                    y = y + dy2;
                }
            }

            return hasBeeline;
        }

        public PlayerBody GetAgentOn(Position position) =>
            FigtherEnv.Entities.FirstOrDefault(body => body.Position.Equals(position));
    }
}