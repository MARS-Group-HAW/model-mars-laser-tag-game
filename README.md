# Multi-Agent LaserTag Simulation Game

This simulation game runs on the multi-agent simulation framework MARS (Multi-Agent Research and Simulation) to give AI developers an opportunity to implement and test AI for agents and let them compete against each other to see which AI wins the game. The goal of the game is to collect as many points as possible by "tagging" enemy agents. The agent with the highest score at the end of the simulation time wins the game.

For project and simulation setup, game rules, agent interfaces, and other information that might be helpful when working with LaserTag, please see `Documentation/`.

## Requirements

LaserTag is written in MARS C#, which can be used in, for example, JetBrains Rider with the MARS C# package (you can search for the package `Mars.Life.Simulations` using the NuGet package manager).

## Setting up a Game

To set up a LaserTag game, please follow these steps:

1. Add your agents' implementation files to `LaserTagBox/Model/Mind`.

2. In `LaserTagBox/Program.cs`, add the following line per agent:

   ```csharp
   description.AddAgent<<AgentName>, PlayerMindLayer>();
   ```

   (`<AgentName>` is the name of your agent's main class).

3. In `LaserTagBox/Program.cs`, specify a configuration file (JSON) for the simulation.

   **Note:** The default configuration files for three-player and four-player matches (`config_3.json` and `config_4.json`, respectively) can be found under `LaserTagBox/`.

   **Note:** The game is designed to be played by three of four teams. If fewer teams than expected are specified in `Program.cs`, the remaining teams are placed in the game as "empty" agents without behavioral logic.

4. Optional: In the configuration file, specify the map (`<FileName>` in the below code snippet).

   ```json
   ...
   "layers": [{
     ...
     "file": <FileName>,
     ...
   }],
   ...
   ```

   Under `LaserTagBox/Resources/`, there are some default maps (CSV). You can generate your own maps using the encoding given in the maps (0 = accessible cell, 1 = inaccessible cell).

5. Run `Program.cs`.

6. After the simulation has finished, go to `Analysis/` and run `vis.py` to visualize the game.
