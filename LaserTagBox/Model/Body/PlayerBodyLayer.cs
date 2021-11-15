using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using LaserTagBox.Model.Spots;
using Mars.Components.Environments.Cartesian;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Data;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

namespace LaserTagBox.Model.Body
{
    public class PlayerBodyLayer : RasterLayer, ISteppedActiveLayer
    {
        private Polygon _explorationWindow;
        public Dictionary<Guid, PlayerBody> Bodies { get; private set; }

        /// <summary>
        ///     Holds all entities that are part of the game.
        /// </summary>
        public ICollisionEnvironment<PlayerBody, Spot> Environment { get; private set; }


        /// <summary>
        ///     Responsible to create new agents and initialize them with required dependencies
        /// </summary>
        public IAgentManager AgentManager { get; private set; }

        public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle,
            UnregisterAgent unregisterAgentHandle)
        {
            base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);

            Environment = new CollisionEnvironment<PlayerBody, Spot>(); // {CheckBoundaries = true};
            AgentManager = layerInitData.Container.Resolve<IAgentManager>();

            Bodies = AgentManager.Spawn<PlayerBody, PlayerBodyLayer>().ToDictionary(body => body.ID);

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var type = this[x, y];
                    var spot = CreateSpots(type, Position.CreatePosition(x, y));
                    if (spot != null)
                    {
                        Environment.Insert(spot, new Point(x, y));
                    }
                }
            }
            
            _explorationWindow = new Polygon(new LinearRing(new[]
            {
                new Coordinate(LowerLeft.X, LowerLeft.Y),
                new Coordinate(LowerLeft.X, UpperRight.Y),
                new Coordinate(UpperRight.X, UpperRight.Y),
                new Coordinate(UpperRight.X, LowerLeft.Y),
                new Coordinate(LowerLeft.X, LowerLeft.Y),
            }));

            return true;
        }

        private Spot CreateSpots(double type, Position position)
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
            return (Ditch)Environment.ExploreObstacles(_explorationWindow, spot => spot.GetType() == typeof(Ditch))
                .OrderBy(spot => position.DistanceInMTo(spot.Position)).FirstOrDefault();
        }

        public Hill NearestHill(Position position)
        {
            return (Hill)Environment.ExploreObstacles(_explorationWindow, spot => spot.GetType() == typeof(Hill))
                .OrderBy(spot => position.DistanceInMTo(spot.Position)).FirstOrDefault();
        }

        protected Barrier NearestBarrier(Position position)
        {
            return (Barrier)Environment.ExploreObstacles(_explorationWindow, spot => spot.GetType() == typeof(Barrier))
                .OrderBy(spot => position.DistanceInMTo(spot.Position)).FirstOrDefault();
        }

        public void Tick()
        {
            if (Context.CurrentTick % 100 == 0)
                Console.WriteLine($"Current tick: {Context.CurrentTick}");

            if (Context.CurrentTimePoint == Context.EndTimePoint)
            {
                Console.WriteLine();

                foreach (var team in Bodies.Values.GroupBy(body => body.TeamName))
                {
                    Console.WriteLine($"{team.Key} {team.Sum(body => body.GamePoints)}");
                }

                Console.WriteLine();
            }
        }

        public void PreTick()
        {
            //do nothing
        }

        public void PostTick()
        {
            //do nothing
        }

        public int GetIntValue(double x, double y)
        {
            return (int)this[x, y];
        }

        public int GetIntValue(Position position)
        {
            return (int)this[position.X, position.Y];
        }

        public PlayerBody GetAgentOn(Position position) =>
            Environment.Entities.FirstOrDefault(body => body.Position.Equals(position));

        public List<PlayerBody> GetAll(Color color)
        {
            return Environment.Entities.Where(fighter => fighter.Color == color).ToList();
        }

        // implementation of Bresenham's Line Algorithm for obtaining a list of grid cells covered by a straight line between two points on the grid
        // http://tech-algorithm.com/articles/drawing-line-using-bresenham-algorithm/ for more information
        public bool HasBeeline(double x1, double y1, double x2, double y2)
        {
            var x = (int)x1;
            var y = (int)y1;
            var newX2 = (int)x2;
            var newY2 = (int)y2;
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
                var intValue = GetIntValue(x, y);
                if (intValue == 1 || intValue == 2)
                {
                    return false;
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

            return true;
        }
    }
}