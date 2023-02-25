# LaserTag: A Multi-Agent Simulation Game

LaserTag is an agent-based simulation game designed for developers to implement and test agent behaviors and let agent teams with different strategies compete against each other. The game runs on the multi-agent simulation framework [MARS (Multi-Agent Research and Simulation)](https://mars-group-haw.github.io/index.html).

For project setup, game rules, agent interfaces, and more information on everything described in this README, see the PDF documentation in the directory `Documentation/`.

## Usage

LaserTag is a [C#](https://learn.microsoft.com/en-us/dotnet/csharp/) application. To work with it, open the directory `LaserTagBox/` with [JetBrains Rider](https://www.jetbrains.com/rider/).

**Note:** MARS integrates into Rider via a [NuGet](https://www.nuget.org/) package. If the MARS dependencies of LaserTag are not resolved properly, use the NuGet package manager in Rider to search for and install the NuGet package `Mars.Life.Simulations`.

## Game Setup

To set up a LaserTag game, follow these steps:

1. Add your agents' implementation files to the directory `LaserTagBox/Model/Mind/`.

2. In the file `LaserTagBox/Program.cs`, add the following line per agent type:

   ```csharp
   description.AddAgent<MyAgentType, PlayerMindLayer>();
   ```

   **Note:** `MyAgentType` is the name of the main class of your agent type.

3. In the file `LaserTagBox/Program.cs`, specify a JSON configuration file for the simulation.

   ```csharp
   var file = File.ReadAllText("my_config_file.json");
   ```

   **Note:** `my_config_file` is the name of the JSON file that contains the game configuration.

   **Note:** The game is designed to be played by three of four teams. If fewer teams are specified in the file `Program.cs`, the remaining teams are placed in the game as "empty" agents without behavioral logic.

   **Note:** The default configuration files for three-player and four-player games (`config_3.json` and `config_4.json`, respectively) can be found in the directory `LaserTagBox/`.

4. In the configuration file, specify the map for the game.

   ```json
   ...
   "layers": [{
     ...
     "file": my_map.csv,
     ...
   }],
   ...
   ```

   **Note:** `my_map` is the name of the CSV file that contains the map encoding.

   In the directory `LaserTagBox/Resources/`, there are some default maps. You can generate your own maps using the following encoding:

   - 0 = empty cell (accessible)
   - 1 = `Barrier` (inaccessible)
   - 2 = `Hill` (accessible)
   - 3 = `Ditch` (accessible)

5. Run `Program.cs`.

6. After the simulation has finished, go to the directory `Analysis/` and run the file `vis.py` to visualize the game. You can double-click the file in your file explorer, or run it via a terminal:

   ```bash
   python3 vis.py
   ```
