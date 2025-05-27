using System;
using System.IO;
using LaserTagBox.Model.Body;
using LaserTagBox.Model.Items;
using LaserTagBox.Model.Mind;
using LaserTagBox.Model.Mind.Examples;
using LaserTagBox.Model.Spots;
using Mars.Components.Starter;
using Mars.Interfaces.Model;

namespace LaserTagBox;

internal static class Program
{
    private static void Main()
    {
        var description = new ModelDescription();
        description.AddLayer<PlayerMindLayer>();
        description.AddLayer<PlayerBodyLayer>();

        description.AddAgent<Hill, PlayerBodyLayer>();
        description.AddAgent<Ditch, PlayerBodyLayer>();
        description.AddAgent<Barrier, PlayerBodyLayer>();
        description.AddAgent<FlagStand, PlayerBodyLayer>();
        description.AddAgent<Flag, PlayerBodyLayer>();
        description.AddAgent<ExplosiveBarrel, PlayerBodyLayer>();
        description.AddAgent<Water, PlayerBodyLayer>();
        description.AddAgent<PlayerBody, PlayerBodyLayer>();

        // USER: Add agents here
        description.AddAgent<FlagCollector, PlayerMindLayer>();
        description.AddAgent<SecondFlagCollector, PlayerMindLayer>();

        // USER: Specify JSON configuration file here
        var file = File.ReadAllText("config.json");
        var config = SimulationConfig.Deserialize(file);
        
        for (int i = 0; i < 1; i++)
        {
            var starter = SimulationStarter.Start(description, config);
            var handle = starter.Run();
            Console.WriteLine("Successfully executed iterations: " + handle.Iterations);
            starter.Dispose();
        }
    }
}