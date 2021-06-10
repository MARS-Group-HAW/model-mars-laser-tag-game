using System;
using System.IO;
using LaserTagBox.Model.Body;
using LaserTagBox.Model.Mind;
using LaserTagBox.Model.Spots;
using Mars.Components.Starter;
using Mars.Interfaces.Model;

namespace LaserTagBox
{
    class Program
    {
        private static void Main()
        {
            var description = new ModelDescription();
            description.AddLayer<PlayerMindLayer>();
            description.AddLayer<PlayerBodyLayer>();

            description.AddAgent<Hill, PlayerBodyLayer>();
            description.AddAgent<Ditch, PlayerBodyLayer>();
            description.AddAgent<Barrier, PlayerBodyLayer>();
            description.AddAgent<PlayerBody, PlayerBodyLayer>();
            
            // add the AIs
            description.AddAgent<YourPlayerMindPleaseRename, PlayerMindLayer>();
            // description.AddAgent<YourPlayerMindPleaseRename2, PlayerMindLayer>();
            // description.AddAgent<YourPlayerMindPleaseRename3, PlayerMindLayer>();
            // description.AddAgent<YourPlayerMindPleaseRename4, PlayerMindLayer>();

            var file = File.ReadAllText("config.json");
            var config = SimulationConfig.Deserialize(file);

            var starter = SimulationStarter.Start(description, config);
            var handle = starter.Run();
            Console.WriteLine("Successfully executed iterations: " + handle.Iterations);
            starter.Dispose();
        }
    }
}