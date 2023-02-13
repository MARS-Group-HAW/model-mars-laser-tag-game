# Multi-Agent LaserTag Simulation Game

This simulation game runs on the multi-agent simulation framework [MARS (Multi-Agent Research and Simulation)](https://mars-group-haw.github.io/index.html). The game enables developers to implement and test agent behaviors and let agent teams compete against each other. The goal of each agent team is to collect as many points as possible by "tagging" agents of enemy teams. The team with the highest score at the end of the simulation wins the game.

For project and simulation setup, game rules, agent interfaces, and other information about LaserTag, see `Documentation/`.

## Usage

LaserTag is written in MARS [C#](https://learn.microsoft.com/en-us/dotnet/csharp/). To use the project, open it with [JetBrains Rider](https://www.jetbrains.com/rider/).

**Note:** MARS integrate into the IDE via a [NuGet](https://www.nuget.org/) package. If the MARS dependencies are not resolved properly, use the NuGet package manager to search for and install the package `Mars.Life.Simulations`.

## Game Setup

To set up a LaserTag game, please follow these steps:

1. Add your agents' implementation files to `LaserTagBox/Model/Mind`.

2. In `LaserTagBox/Program.cs`, add the following line per agent:

   ```csharp
   description.AddAgent<<YourAgentClassName>, PlayerMindLayer>();
   ```

   **Note:** `<YourAgentClassName>` is the name of your agent's main class.

3. In `LaserTagBox/Program.cs`, specify a configuration file (JSON) for the simulation.

   **Note:** The default configuration files for three-player and four-player games (`config_3.json` and `config_4.json`, respectively) can be found in `LaserTagBox/`.

   **Note:** The game is designed to be played by three of four teams. If fewer teams than expected are specified in `Program.cs`, the remaining teams are placed in the game as "empty" agents without behavioral logic.

4. In the configuration file, specify the map for the game.

   ```json
   ...
   "layers": [{
     ...
     "file": <FileNameOfMap>,
     ...
   }],
   ...
   ```

   **Note:** `<FileNameOfMap>` if the name of the file that contains the map encoding.

   In `LaserTagBox/Resources/`, there are some default maps (CSV). You can generate your own maps using the following encoding:

   - 0 = accessible cell
   - 1 = inaccessible cell

5. Run `Program.cs`.

6. After the simulation has finished, go to `Analysis/` and run `vis.py` to visualize the game.
