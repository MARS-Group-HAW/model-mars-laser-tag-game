# Installation

This visualization tool requires [Python 3](https://www.python.org/downloads/) and `Tk`.

You might need to install `tkinter` manually. On macOS this can be done with [Homebrew](https://brew.sh/) by executing the following command:

```bash
brew install python-tk
```

## Preconditions

The visualization works on the result data of a **finished** LaserTag simulation. So, start a `LaserTag` simulation and let it finish **before** starting the visualization tool. The simulation will create output CSV files that contain tick-based information about the agents.

## Usage

Open a terminal, navigate to the directory `Analysis/`, and start the visualization:

```bash
python3 vis.py
```

## Troubleshooting

Below are descriptions of some common issues and known fixes.

### Simulation configuration defines CSV output?

The JSON configuration file for the LaserTag simulation (`config.json` in the directory `LaserTagBox/`) needs to specify `csv` as output format.

```json
{
 "globals": {
   "startTime": "2021-01-01T10:00:00.000Z",
   "endTime": "2021-01-01T10:30:00.000Z",
   "deltaT": 1,
   "deltaTUnit": "seconds",
   
   "output": "csv",
   "options": {
     "delimiter": ";",
     "numberFormat": "G",
     "culture": "en-EN"
   }
 }
  // ... agent, entities and layer mappings
}
```

### Which map is referenced?

The visualization works on the agent output CSV files and displays the map. It must know which map was used for the simulation that should be visualized. Make sure that the correct map is referenced in the `main()` method in `vis.py`.

```python
def main():
    map = map_read_in("../LaserTagBox/Resources/map_4_open.csv")
```

### Changed .NET execution framework?

The visualization assumes that the simulation code is run in `net6.0`. Therefore, the result data are stored in a folder with the same name (e.g., `LaserTagBox/bin/Debug/net6.0`). If you run the program on another target framework, then the result data is also stored in subdirectory with a corresponding name (e.g., `LaserTagBox/bin/Debug/net7.0`). Please change the path of the CSV file accordingly in the `main()` method in `vis.py`.

```python
def main():
    #...
    path = '../LaserTagBox/bin/Debug/net6.0/PlayerBody.csv'
```
