using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Interfaces.Agents;
using System.Text.Json;
using Fleck;
using System.Threading;
using System.Threading.Tasks;
using LaserTagBox.Model.Items;
using LaserTagBox.Model.Shared;
using LaserTagBox.Model.Spots;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Body;
public static class DataVisualizationServer
{
    public static volatile int CurrentTick = 1; 
    private static IWebSocketConnection _client; 
    private static WebSocketServer _server; 
    private static CancellationTokenSource _cts; 
    private static Task _serverTask; 
    private static string _lastMessage = string.Empty;

    public static void Start()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _server = new WebSocketServer("ws://127.0.0.1:8181");

        _server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                _client = socket; 
            };

            socket.OnMessage = message =>
            {
                if (int.TryParse(message, out var tick))
                {
                    if (tick != CurrentTick + 1)
                    {
                        if (_client != null && _client.IsAvailable)
                        {
                            _client.Send(_lastMessage);
                        }
                        return;
                    }
                    CurrentTick = tick;
                }
            };

            socket.OnClose = () =>
            {
                _client = null;
            };
        });
        
        while (!token.IsCancellationRequested)
        {
            Thread.Sleep(100);
        }
    }
    
    public static void RunInBackground()
    {
        _serverTask = Task.Run(() => Start());
    }

    public static void Stop()
    {
        if (_cts == null)
            return;
        
        if (_client != null && _client.IsAvailable)
        {
            _client.Send("close");
            Thread.Sleep(100);
        }
        
        _cts.Cancel();       
        _serverTask?.Wait(); 
        _server?.Dispose();  

        _cts.Dispose();
        _cts = null;
        _serverTask = null;
        _server = null;
        _client = null;
    }
        
    public static void SendData(IEnumerable<PlayerBody> bodies, IEnumerable<Item> items, Dictionary<Color, TeamScore> scores, IEnumerable<ExplosiveBarrel> explosiveBarrels)
    {
        var payload = new
        {
            expectingTick = CurrentTick + 1,
            agents = bodies.Select(b => new
            {
                id = b.ID,
                x = b.Position.X,
                y = b.Position.Y,
                alive = b.Alive,
                color = b.Color.ToString(),
                team = b.TeamName,
                visualRange = b.VisualRange,
                gotShot = b.WasTaggedLastTick,
                stance = b.Stance,
                taggerID = b.TaggerID,
            }),
            items = items.Select<Item, object>(i =>
            {
                if (i is Flag flag)
                {
                    return new
                    {
                        id = flag.ID,
                        x = flag.Position.X,
                        y = flag.Position.Y,
                        color = flag.Color.ToString(),
                        type = flag.GetType().Name,
                        pickedUp = flag.PickedUp,
                        ownerID = flag.OwnerID
                    };
                }

                return new
                {
                    type = "UnknownItem"
                };
            }),
            explosiveBarrels = explosiveBarrels.Select(barrel => new
            {
                id = barrel.ID,
                x = barrel.Position.X,
                y = barrel.Position.Y,
                hasExploded = barrel.HasExploded,
            }),
            scores = scores.Values.Select(t => new
            {
                teamName = t.Name,
                teamColor = t.Color.ToString(),
                score = t.GamePoints
            })
        };
        _lastMessage = JsonSerializer.Serialize(payload);
        _client?.Send(_lastMessage);
    }
    
    
        
    public static bool Connected()
    {
        return _client != null && _client.IsAvailable;
    }
}