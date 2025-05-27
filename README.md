# LaserTag: A Multi-Agent Simulation Game
**Version 1.0.0** – *Released: May 27, 2025*


LaserTag is an agent-based simulation game designed for developers to implement and test agent behaviors and let agent teams with different strategies compete against each other. The game runs on the multi-agent simulation framework [MARS (Multi-Agent Research and Simulation)](https://mars-group-haw.github.io/index.html).

For project setup, game rules, agent interfaces, and more information on everything described in this README, see the PDF documentation in the directory `Documentation/`.

## Usage

LaserTag is a [C#](https://learn.microsoft.com/en-us/dotnet/csharp/) application. To work with it, open the directory `LaserTagBox/` with a .NET IDE of your choice (e.g., [Visual Studio](https://visualstudio.microsoft.com/), [JetBrains Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/)).

**Note:** The MARS Framework integrates into LaserTag via a [nuget](https://www.nuget.org/) package. If the MARS dependencies of LaserTag are not resolved properly, use the nuget package manager of your IDE to search for and install the nuget package `Mars.Life.Simulations`.

## Visualization

To visualize the results of a simulation, prebuilt visualization tools are provided for Linux, macOS, and Windows.

### How to Use

1. Go to the directory `Visualization/`.
2. Choose the file corresponding to your operating system:
   - `visualization_windows.zip` for Windows
   - `visualization_macOS.app.tar.xz` for macOS
   - `visualization_linux.zip` for Linux
3. Extract the archive.
4. Run the extracted application (e.g., `visualization.exe`, `visualization.app`, or the Linux binary).

The visualization reads game data produced during a simulation and displays it in a graphical interface.



### Note

The visualization was created using the [Godot Engine](https://godotengine.org/). No installation is required – the exported version runs as a standalone application.

### macOS Gatekeeper Warning

macOS may block the visualization app when it's downloaded from the internet.  
If you encounter a warning that the app is from an "unidentified developer", you can allow it manually.

After extracting the archive, run the following command in Terminal, replacing the path as needed:

```bash
xattr -d com.apple.quarantine /path/to/model-mars-laser-tag-game/Visualization/visualization_macOS.app
```
For example, if your project is in your home directory:
```bash
xattr -d com.apple.quarantine ~/model-mars-laser-tag-game/Visualization/visualization_macOS.app
```

After that, the app should start normally.
Alternatively, you can right-click the app and choose “Open”, then confirm the dialog once.

### Disabling Visualization
If you do not want to use the visualization, you can disable it in the simulation configuration.

Open your JSON configuration file `config.json` Set the "Visualization" mapping parameter to false like this:
```json
...
"layers": [
  {
    "name": "PlayerBodyLayer",
    "file": "Resources/ctf_Battleground.csv",
    "dimensionx": 51,
    "dimensiony": 51,
    "mapping": [
      {
        "parameter": "Visualization",
        "value": false
      }
    ]
  }
]
...
```

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

   **Note:** The game is designed to be played by two, three of four teams. If fewer teams are specified in the file `Program.cs`, the remaining teams are placed in the game as "empty" agents without behavioral logic.

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
   - 4 = `Water` (inaccessible)
   - 5 = `ExplosiveBarrel` (inaccessible)
   - 7 = `FlagStand (red)` (accessible)
   - 8 = `FlagStand (yellow)` (accessible)

5. Run `Program.cs`.

6. After the simulation has finished, go to the directory `Analysis/` and run the file `vis.py` to visualize the results. You can double-click the file in your file explorer, or run it via a terminal:

   ```bash
   python3 vis.py
   ```

## Contact

Prof. Dr. Thomas Clemen, Berliner Tor 7, 20099 Hamburg, Germany  
eMail: [thomas.clemen@haw-hamburg.de](mailto:thomas.clemen@haw-hamburg.de)  
Web: [www.mars-group.org](https://www.mars-group.org)

Ersan Baran, Berliner Tor 7, 20099 Hamburg, Germany  
eMail: [ersan.baran@haw-hamburg.de](mailto:ersan.baran@haw-hamburg.de)  
