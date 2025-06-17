using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using LaserTagBox.Model.Spots;
using Mars.Components.Environments;
using Mars.Components.Layers;
using Mars.Core.Data;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Data;
using Mars.Interfaces.Environments;
using Mars.Interfaces.Layers;
using System.Threading;
using LaserTagBox.Model.Items;
using Microsoft.CodeAnalysis.Operations;
using Barrier = LaserTagBox.Model.Spots.Barrier;


namespace LaserTagBox.Model.Body;

/// <summary>
///     A layer type that represents the spatial environment and holds the spatially explicit bodies of the LaserTag
///     agents.
/// </summary>
public class PlayerBodyLayer : RasterLayer, ISteppedActiveLayer
{
    #region Properties
    /// <summary>
    ///    Determines whether the layer should be visualized in a separate window.
    /// </summary>
    [PropertyDescription]
    public bool Visualization { get; set; }
    
    /// <summary>
    ///    The timeout for the visualization server to wait for the next tick.
    /// </summary>
    [PropertyDescription]
    public int VisualizationTimeout { get; set; }
    
    /// <summary>
    ///    The game mode of the simulation.
    /// </summary>
    [PropertyDescription]
    public GameMode Mode { get; set; }
    
    /// <summary>
    ///     A dictionary of agent bodies, identified by their GUID.
    /// </summary>
    public Dictionary<Guid, PlayerBody> Bodies { get; private set; }
    
    /// <summary>
    ///    A dictionary of items, identified by their GUID.
    /// </summary>
    public Dictionary<Guid, Item> Items { get; private set; }
    
    /// <summary>
    ///    A dictionary of scores, identified by the team name.
    /// </summary>
    public Dictionary<Color, TeamScore> Score { get; private set; }

    /// <summary>
    ///     Holds all agents in a 2-dimensional area for exploration purposes.
    /// </summary>
    public SpatialHashEnvironment<PlayerBody> FighterEnv { get; private set; }

    /// <summary>
    ///     Holds all spots in a 2-dimensional area for exploration purposes.
    /// </summary>
    public SpatialHashEnvironment<Spot> SpotEnv { get; private set; }
    
    /// <summary>
    ///     Holds all items in a 2-dimensional area for exploration purposes.
    /// </summary>
    public SpatialHashEnvironment<Item> ItemEnv { get; private set; }

    /// <summary>
    ///     Responsible for creating new agents and initializing them with required dependencies.
    /// </summary>
    public IAgentManager AgentManager { get; private set; }
    #endregion

    #region Initialization
    /// <summary>
    ///     Initialization routine of the layer type. Returns true if initialization was successful, otherwise false.
    /// </summary>
    /// <param name="layerInitData">External initialization and configuration data for constructing the layer and
    /// initializing agents.</param>
    /// <param name="registerAgentHandle">A handle for registering agents with the simulation.</param>
    /// <param name="unregisterAgentHandle">A handle for unregistering agents from the simulation.</param>
    /// <returns>boolean</returns>
    public override bool InitLayer(LayerInitData layerInitData, RegisterAgent registerAgentHandle = null,
        UnregisterAgent unregisterAgentHandle = null)
    {
        base.InitLayer(layerInitData, registerAgentHandle, unregisterAgentHandle);
        
        if (Visualization) DataVisualizationServer.RunInBackground();
        
        FighterEnv = new SpatialHashEnvironment<PlayerBody>(Width - 1, Height - 1) {CheckBoundaries = true};
        SpotEnv = new SpatialHashEnvironment<Spot>(Width - 1, Height - 1) {CheckBoundaries = true};
        ItemEnv = new SpatialHashEnvironment<Item>(Width - 1, Height - 1) {CheckBoundaries = true};
        AgentManager = layerInitData.Container.Resolve<IAgentManager>();
        
        Bodies = AgentManager.Spawn<PlayerBody, PlayerBodyLayer>().ToDictionary(body => body.ID);
        
        Items = new Dictionary<Guid, Item>();
        
        Score = new Dictionary<Color, TeamScore>();
        
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var type = this[x, y];
                var position = Position.CreatePosition(x, y);
                var spot = CreateSpots(type, position);
                SpotEnv.Insert(spot);
            }
        }
        return true;
    }
    #endregion

    #region Tick
    public void Tick()
    {
        if (Context.CurrentTick % 100 == 0)
            Console.WriteLine($"Current tick: {Context.CurrentTick}");
    }

    public void PreTick()
    {
    }

    public void PostTick()
    {
        if (Score.Count == 0)
        {
            foreach (var team in Bodies.Values.GroupBy(body => body.Color))
            {
                var teamColor = team.Key;
                var teamName = team.First().TeamName;
                Score.Add(teamColor, new TeamScore(teamName, teamColor, 0));
            }
        }
        
        switch (Mode)
        {
            case GameMode.TeamDeathmatch:
                foreach (var team in Bodies.Values.GroupBy(body => body.Color))
                {
                    var teamColor = team.Key;
                    var gamePoints = team.Sum(b => b.GamePoints);
                    Score[teamColor].GamePoints = gamePoints;
                }
                break;
            case GameMode.CaptureTheFlag:
                var flags = Items.Values.OfType<Flag>().ToList();
                var flagStands = SpotEnv.Entities.OfType<FlagStand>().ToList();

                foreach (var flag in flags)
                {
                    var player = FighterEnv.Explore(flag.Position, 0, -1, body => body.Alive).FirstOrDefault();
                    if (player != null)
                    {
                        var flagstand = SpotEnv.Explore(flag.Position, 0, 1)
                            .OfType<FlagStand>()      
                            .FirstOrDefault();  

                        if (flagstand == null && !flag.PickedUp || flagstand != null && !flag.PickedUp && flagstand.Color != player.Color)
                        {
                            if (flag.Color != player.Color) // Pick up flag
                            {
                                flag.PickUp(player);
                                player.CarryingFlag = true;
                            }
                            else // Bring flag back to flag stand
                            {
                                var homestand  = flagStands.First(fs => fs.Color == player.Color).Position;
                                ItemEnv.PosAt(flag, homestand.X, homestand.Y);
                            }
                        }
                        if (flagstand != null && flagstand.Color == player.Color && flag.PickedUp) // Drop flag
                        {
                            flag.Owner.CarryingFlag = false;
                            flag.Drop();
                        }
                    }
                }
                
                // Check if all flags are at the flag stand
                foreach (var flagStand in flagStands)
                {
                    int flagsAtStand = ItemEnv.Explore(flagStand.Position, 0, -1, item => item.GetType() == typeof(Flag)).Count();

                    if (flagsAtStand == Score.Keys.Count)
                    {
                        Score[flagStand.Color].GamePoints += 1;
                        foreach (var flag in flags) // reset flag position
                        {
                            var homestand  = flagStands.First(fs => fs.Color == flag.Color).Position;
                            ItemEnv.PosAt(flag, homestand.X, homestand.Y);
                        }
                        break; 
                    }
                }
                break;
        }
        
        if (Context.CurrentTick == Context.MaxTicks)
        {
            Console.WriteLine();

            foreach (var team in Score)
            {
                Console.WriteLine($"Team: {team.Value.Name} Score: {team.Value.GamePoints}");
            }

            Console.WriteLine();
        }
        
        if (Visualization)
        {
            while (!DataVisualizationServer.Connected())
            {
                Thread.Sleep(1000);
                Console.WriteLine("Waiting for live visualization to run.");
            }
            
            DataVisualizationServer.SendData(Bodies.Values, Items.Values, Score, SpotEnv.Entities.OfType<ExplosiveBarrel>());
            while (DataVisualizationServer.CurrentTick != Context.CurrentTick + 1)
            {
                Thread.Sleep(VisualizationTimeout);
            }
        }
    }
    #endregion
        
    #region Methods
    /// <summary>
    ///     Gets the Ditch object with the smallest distance to the given position.
    /// </summary>
    /// <param name="position">The position from which the nearest Ditch is to be minimal</param>
    /// <returns>The identified Ditch object</returns>
    public Ditch NearestDitch(Position position) =>
        (Ditch) SpotEnv.Explore(position, -1, 1, spot => spot.GetType() == typeof(Ditch)).FirstOrDefault();

    /// <summary>
    ///     Gets the Hill object with the smallest distance to the given position.
    /// </summary>
    /// <param name="position">The position from which the nearest Hill is to be minimal</param>
    /// <returns>The identified Hill object</returns>
    public Hill NearestHill(Position position) =>
        (Hill) SpotEnv.Explore(position, -1, 1, spot => spot.GetType() == typeof(Hill)).FirstOrDefault();

    /// <summary>
    ///     Gets the Barrier object with the smallest distance to the given position.
    /// </summary>
    /// <param name="position">The position from which the nearest Barrier is to be minimal</param>
    /// <returns>The identified Barrier object</returns>
    public Barrier NearestBarrier(Position position) =>
        (Barrier) SpotEnv.Explore(position, -1, 1, spot => spot.GetType() == typeof(Barrier)).FirstOrDefault();

    /// <summary>
    ///     Gets the integer value stored in the cell identified by the given coordinates.
    /// </summary>
    /// <param name="x">The first dimension of the cell</param>
    /// <param name="y">The second dimension of the cell</param>
    /// <returns>The value of the cell</returns>
    public int GetIntValue(double x, double y) => (int) this[x, y];

    /// <summary>
    ///     Gets the integer value stored in the cell identified by the given position.
    /// </summary>
    /// <param name="position">The position whose value is requested</param>
    /// <returns>The value of the cell</returns>
    public int GetIntValue(Position position) => (int) this[position.X, position.Y];

    /// <summary>
    ///     Gets the agent body located on the given position.
    /// </summary>
    /// <param name="position">The position</param>
    /// <returns>The agent body located on the given position</returns>
    public PlayerBody GetAgentOn(Position position) =>
        FighterEnv.Entities.FirstOrDefault(body => body.Position.Equals(position));

    /// <summary>
    ///     Gets all agent bodies of the given color.
    /// </summary>
    /// <param name="color">The color of the requested agent bodies</param>
    /// <returns>A list of all identified agent bodies</returns>
    public List<PlayerBody> GetAll(Color color) =>
        FighterEnv.Entities.Where(fighter => fighter.Color == color).ToList();

    /// <summary>
    /// Returns true of there exists a straight line of sight between the given source position and target position,
    /// otherwise false.
    /// Implements Bresenham's Line Algorithm, which obtains a list of grid cells covered by a straight line
    /// between two points on a grid-based environment.
    /// For more information: http://tech-algorithm.com/articles/drawing-line-using-bresenham-algorithm/
    /// </summary>
    /// <param name="x1">The first dimension of the source position</param>
    /// <param name="y1">The second dimension of the source position</param>
    /// <param name="x2">The first dimension of the target position</param>
    /// <param name="y2">The second dimension of the target position</param>
    /// <returns>boolean</returns>
    public bool HasBeeline(double x1, double y1, double x2, double y2)
    {
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
            var intValue = GetIntValue(x, y);
            if (intValue is 1 or 2)
            {
                if (!((x == (int)x1 && y == (int)y1) || (x == (int)x2 && y == (int)y2)))
                {
                    return false;
                }
            }

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
        return true;
    }
        
    /// <summary>
    ///     Positions an object of interest OOI of the given type at the given position. 
    /// </summary>
    /// <param name="type">The type of OOI to create</param>
    /// <param name="position">The requested position of the OOI</param>
    /// <returns>A reference to the initialized OOI</returns>
    private Spot CreateSpots(double type, Position position)
    {
        Spot spot = null;

        switch (type)
        {
            case 1:
                spot = AgentManager.Spawn<Barrier, PlayerBodyLayer>(null, s => s.Position = position).Take(1).First();
                break;
            case 2:
                spot = AgentManager.Spawn<Hill, PlayerBodyLayer>(null, s => s.Position = position).Take(1).First();
                break;
            case 3:
                spot = AgentManager.Spawn<Ditch, PlayerBodyLayer>(null, s => s.Position = position).Take(1).First();
                break;
            case 4:
                spot = AgentManager.Spawn<Water, PlayerBodyLayer>(null, s => s.Position = position).Take(1).First();
                break;
            case 5:
                spot = AgentManager.Spawn<ExplosiveBarrel, PlayerBodyLayer>(null, s => s.Position = position).Take(1).First();
                break;
            case 7:
                spot = AgentManager.Spawn<FlagStand, PlayerBodyLayer>(null, s => s.Position = position).Take(1).First();
                ((FlagStand)spot).Color = Color.Red;
                var redFlag = AgentManager.Spawn<Flag, PlayerBodyLayer>(null, f => f.Position = position).Take(1).First();
                redFlag.Color = Color.Red;
                Items.Add(redFlag.ID, redFlag);
                break;
            case 8:
                spot = AgentManager.Spawn<FlagStand, PlayerBodyLayer>(null, s => s.Position = position).Take(1).First();
                ((FlagStand)spot).Color = Color.Yellow;
                var yellowFlag = AgentManager.Spawn<Flag, PlayerBodyLayer>(null, f => f.Position = position).Take(1).First();
                yellowFlag.Color = Color.Yellow;
                Items.Add(yellowFlag.ID, yellowFlag);
                break;
        }

        return spot;
    }
    #endregion
}