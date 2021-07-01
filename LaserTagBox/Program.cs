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
            //description.AddAgent<LucaNiklasTimSchlaegertruppe, PlayerMindLayer>();
            description.AddAgent<MasterOfDisasterMind, PlayerMindLayer>();
            description.AddAgent<Myron, PlayerMindLayer>();
            description.AddAgent<YoloTeaBagTralala, PlayerMindLayer>();
            //description.AddAgent<JamesBond007, PlayerMindLayer>();
            //description.AddAgent<RenamedPlayerMind, PlayerMindLayer>();
            //description.AddAgent<PAINballer, PlayerMindLayer>();
            //description.AddAgent<SuperMind, PlayerMindLayer>();
            //description.AddAgent<MostIntelligentKombatAgent, PlayerMindLayer>();
            //description.AddAgent<YourPlayerMindPleaseRename, PlayerMindLayer>();

            var file = File.ReadAllText("config_3.json");
            var config = SimulationConfig.Deserialize(file);

            var starter = SimulationStarter.Start(description, config);
            var handle = starter.Run();
            Console.WriteLine("Successfully executed iterations: " + handle.Iterations);
            starter.Dispose();
        }
    }
}