# Installation

The visualization requires [Python 3](https://www.python.org/downloads/) and `Tk`.

You might need to install `tkinter` manually. On macOS this can be done with [Homebrew](https://brew.sh/) by executing the following command:

```bash
brew install python-tk
```

# Preconditions

The visualization works on the result data of a **finished** simulation. So, start the `LaserTag` simulation before you want to visualize. It will create output CSV files that contain information about the agents for every tick. 

# Start

Open your terminal and navigate into this directory and Start the visualization by calling:

```bash
python3 vis.py
```



### Hint: Simulation configuration defines CSV output?

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

The visualization works on the agent-csv and displays the map. It requires knowing, which map was used for the simulation run that should be visualized. Assure that the correct map is referenced in the `main()`-method in `vis.py`.

```python
def main():
    map = map_read_in("../LaserTagBox/Resources/map_4_open.csv")
```

### Hint: Changed .NET execution framework?

The visualization assumes that the simulation code is run in `net6.0`. Therefor the result data is stored in a folder with the same name. If you run the program in another target framework, then the result data is also stored in another subdirectory. Please change the path of the CSV file accordingly in the `main()`-method in `vis.py`.

```python
def main():
    #...
    path = '../LaserTagBox/bin/Debug/net6.0/PlayerBody.csv'
```
