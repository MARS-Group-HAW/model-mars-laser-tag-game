using System;
using System.IO;
using System.Linq;
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

            description.AddAgent<DummyTeam1Figther, PlayerMindLayer>();
            description.AddAgent<PlayerBody, PlayerBodyLayer>();

            var file = File.ReadAllText("config.json");
            var config = SimulationConfig.Deserialize(file);

            var starter = SimulationStarter.Start(description, config);
            var handle = starter.Run();
            Console.WriteLine("Successfully executed iterations: " + handle.Iterations);
            starter.Dispose();

            foreach (var type in description.AgentTypeMappings.Where(type =>
                type.Key != typeof(PlayerBody) && !typeof(Spot).IsAssignableFrom(type.Key)))
            {
                Console.WriteLine(type.Key);
            }
        }
    }
}