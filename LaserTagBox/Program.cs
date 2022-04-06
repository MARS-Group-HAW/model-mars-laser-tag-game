using System;
using System.IO;
using LaserTagBox.Model.Body;
using LaserTagBox.Model.Mind;
using LaserTagBox.Model.Spots;
using Mars.Components.Starter;
using Mars.Interfaces.Model;

namespace LaserTagBox
{
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
            description.AddAgent<PlayerBody, PlayerBodyLayer>();
            
            // add the AIs
            description.AddAgent<YourPlayerMindPleaseRename, PlayerMindLayer>();
            description.AddAgent<YourPlayerMindPleaseRenameCopy, PlayerMindLayer>();

            var file = File.ReadAllText("config_4.json");
            var config = SimulationConfig.Deserialize(file);

            var starter = SimulationStarter.Start(description, config);
            var handle = starter.Run();
            Console.WriteLine("Successfully executed iterations: " + handle.Iterations);
            starter.Dispose();
        }
    }
}