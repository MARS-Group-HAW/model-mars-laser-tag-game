# LaserTag Visualization Tool

This visualization tool can be used to visualize a finished LaserTag game simulation.

## Installation

The visualization tool requires [Python 3](https://www.python.org/downloads/) and [Tkinter](https://docs.python.org/3/library/tkinter.html).

You might need to install `tkinter` manually, either via your IDE or a package manager in the terminal:

- On macOS: `brew install python-tk`
- On Windows: `choco install python-tkinter`

## Preconditions

The visualization works on the result data of a **finished** LaserTag simulation. So, start a `LaserTag` simulation and let it finish **before** starting the visualization tool. The simulation will create output CSV files that contain tick-based information about the agents.

## Usage

Open a terminal, navigate to the directory `Analysis/`, and start the visualization:

- On macOS/Linus: `python3 vis.py`
- On Windows: `python vis.py`

## Troubleshooting

Below are descriptions of some common issues and known fixes.

### Simulation configuration defines CSV output?

The JSON configuration file for the LaserTag simulation (`config.json` in the directory `LaserTagBox/`) needs to specify `csv` as output format. This will make the MARS Framework produce a CSV output file with agent attributes per tick. This CSV file can be used be the visualization tool to render agent positions and other attributes.

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
