# Installation

Open your terminal and navigate into this directory. 

Install `python3.9` (getting from [Python](https://www.python.org/downloads/))

Install `tkinter` by executing the following command:

```bash
brew install python-tk
```

# Start

Start the visualization by calling:

```bash
python3 vis.py
```

# Preconditions

The visualisation works on the result data of a finished simulation. So, start the `LaserTag` simulation before you want to visualize. It will create output csv-files that contain information about the agents for every tick. 

### Hint: Simulation configuration defines csv-output?
The simulation configuration (`config.json`) needs to specify `csv` as output format.

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

### Hint: Which map is referenced?
The visualization works on the agent-csv and displays the map. It requires to know, which map was used for the simulation run that should be visualized. Assure that the correct map is referenced in the main-method in `vis.py`.

```python
def main():
    map = map_read_in("../LaserTagBox/Resources/map_4_open.csv")
```

### Hint: Changed dotnet execution framework?
THe visualization assumes that the simulation code is run in `netcoreapp3.1`. Therefor the result data is stored in a folder with the same name. If you run the program in another target framework, then the result data is also stored in another subdirectory. Please change the path of the csv-file accordingly in the main-method in `vis.py`.

```python
def main():
    //...
    path = '../LaserTagBox/bin/Debug/netcoreapp3.1/PlayerBody.csv'
```


