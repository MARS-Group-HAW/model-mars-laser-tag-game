using System;
using System.IO;
using TeamDeathmatch.Mind;
using TeamDeathmatch.Mind.Examples;
using LaserTag.Core.Model.Body;
using LaserTag.Core.Model.Items;
using LaserTag.Core.Model.Mind;
using LaserTag.Core.Model.Spots;
using Mars.Components.Starter;
using Mars.Interfaces.Model;
using Barrier = LaserTag.Core.Model.Spots.Barrier;

namespace TeamDeathmatch;

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
        description.AddAgent<Example1, PlayerMindLayer>();
        description.AddAgent<Example2, PlayerMindLayer>();

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