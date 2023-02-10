import csv
import tkinter as tk
import gui
from pathlib import Path

BITMAPS = [
    "error",
    "gray75",
    "gray50",
    "gray25",
    "gray12",
    "hourglass",
    "info",
    "questhead",
    "question",
    "warning",
]

COLOR_LOOKUP = {
    "Blue": "#0101DF",
    "Red": "#FF0000",
    "Green": "#01DF01",
    "Yellow": "#FFFF00",
    "Black": "#000000",
    "Dead": "#DDD",
}

BOXSIZE = 15


class AgentState(object):
    def __init__(self, x, y, tick):
        self.x = x
        self.y = y
        self.tick = tick
        self.alive = True
        self.color = "Dead"
        self.team = "N/A"
        self.vrange = 10

    def __eq__(self, other):
        return self.tick == other.tick

    def __lt__(self, other):
        return self.tick < other.tick

    def __le__(self, other):
        return self < other or self == other

    def __gt__(self, other):
        return self.tick > other.tick

    def __ge__(self, other):
        return self > other or self == other


class Agent(object):
    def __init__(self, states=[]):
        self.states = states
        self.states.sort()
        self.statedic = None
        self._updateneeded = True
        self._canvas_id = []

    def get_state(self, step=0):
        try:
            return self.states[step]
        except:
            return None

    def draw(self, canvas, step=0, agent_map=None):
        cid = []
        state = self.get_state(step)
        try:
            xy = agent_map.get_position(state.x, state.y - 1)
        except:
            xy = (0, 0)
        if not state and len(self.states) > 0:
            state = self.states[-1]
            xy = agent_map.get_position(state.x, state.y)

            cid.append(canvas.create_bitmap(xy, bitmap=BITMAPS[7], foreground="#DDD"))
        else:
            if state.alive:
                cid.append(
                    canvas.create_bitmap(
                        xy, bitmap=BITMAPS[7], foreground=COLOR_LOOKUP[state.color]
                    )
                )
                cid.append(
                    canvas.create_circle(
                        xy,
                        state.vrange * BOXSIZE + (BOXSIZE / 2),
                        fill="",
                        outline=COLOR_LOOKUP[state.color],
                        width=4,
                    )
                )
                if state.gotshot:
                    cid.append(
                        canvas.create_cross(xy, outline=COLOR_LOOKUP["Black"], width=3)
                    )
            else:
                cid.append(
                    canvas.create_bitmap(
                        xy, bitmap=BITMAPS[7], foreground=COLOR_LOOKUP["Dead"]
                    )
                )
        self._canvas_id = cid

    def delete(self, canvas, step=0, agent_map=None):
        for i in self._canvas_id:
            canvas.delete(i)

    def update(self, canvas, step=0, agent_map=None):
        if self._updateneeded:
            self.delete(canvas, step, agent_map)
            self.draw(canvas, step, agent_map)


class AdvancedAgentMapField(object):
    def __init__(self, x1, y1, x2, y2, color="#FFF"):
        self.fieldpos = (x1, y1, x2, y2)
        self.color = color
        self._updateneeded = True
        self._canvas_id = None

    def draw(self, canvas, step=0):
        x = canvas.create_rectangle(self.fieldpos, fill=self.color)
        self._canvas_id = x
        return x

    def delete(self, canvas, step=0):
        canvas.delete(self._canvas_id)

    def update(self, canvas, step=0):
        if self._updateneeded:
            self._updateneeded = False
            self.delete(canvas, step)
            self.draw(canvas, step)


class AdvancedAgentMap(object):
    def __init__(self, x_size, y_size, color="#FFF"):
        self.x = x_size
        self.y = y_size
        self.boarder = 10
        self.box_size = BOXSIZE
        self.color = color
        self.map = []
        for y in range(self.y):
            l = []
            for x in range(self.x):
                x1, y1 = self.get_position(y, x, False)
                x2, y2 = self.get_position(y + 1, x + 1, False)
                l.append(AdvancedAgentMapField(y1, x1, y2, x2, self.color))
            self.map.append(l)

    def get_canvas_dimension(self):
        return (
            self.x * self.box_size + self.boarder * 2,
            self.y * self.box_size + self.boarder * 2,
        )

    def get_position(self, x, y, delta=True):
        dx = 0
        dy = 0
        if delta:
            dx = self.box_size // 2
            dy = self.box_size // 2
        newx = self.boarder + x * self.box_size + dx
        newy = self.boarder + y * self.box_size + dy
        return (newx, newy)

    def draw(self, canvas, step=0):
        for y in self.map:
            for x in y:
                try:
                    x.draw(canvas, step)
                except Exception as e:
                    pass

    def delete(self, canvas, step=0):
        for y in self.map:
            for x in y:
                try:
                    x.delete(canvas, step)
                except Exception as e:
                    pass

    def update(self, canvas, step=0):
        for y in self.map:
            for x in y:
                try:
                    x.update(canvas, step)
                except Exception as e:
                    pass


def map_read_in(file=""):
    x = 0
    y = 0
    l = []
    with open(file, "r") as f:
        reader = csv.reader(f, delimiter=";")
        for row in reader:
            y += 1
            x = max(x, len(row))
            l.append(row)
    m = AdvancedAgentMap(x, y)
    # colors of grid cells
    for iy in range(y):
        for ix in range(x):
            if l[iy][ix] == "0":
                m.map[iy][ix].color = "#FFF"
            # barrier
            elif l[iy][ix] == "1":
                m.map[iy][ix].color = "#000"
            # hill
            elif l[iy][ix] == "2":
                m.map[iy][ix].color = "#676767"
            # ditch
            elif l[iy][ix] == "3":
                m.map[iy][ix].color = "#60460f"
    return m


def agent_read_in(file, map):
    agents = {}
    l = []

    my_file = Path(file)
    if not my_file.is_file():
        print(f"Could not find agent CSV output file at `{file}`. Please make sure the path is correct, you might be running the simulation with a different .NET Framework version and need to change the path in the main() method of this file.")
        exit()

    with open(file, "r") as f:
        reader = csv.reader(f, delimiter=";")
        for row in reader:
            l.append(row)
    columnids = {}
    for i in range(len(l[0])):
        x = l[0][i]
        if "ID" in x:
            columnids["ID"] = i
        elif "Alive" in x:
            columnids["ALIVE"] = i
        elif "X" in x:
            columnids["X"] = i
        elif "Y" in x:
            columnids["Y"] = i
        elif "Step" in x:
            columnids["TICK"] = i
        elif "Energy" in x:
            columnids["ENGERGY"] = i
        elif "Stance" in x:
            columnids["CURRSTANCE"] = i
        elif "Color" in x:
            columnids["COLOR"] = i
        elif "TeamName" in x:
            columnids["TEAM"] = i
        elif "VisualRange" in x:
            columnids["VRANGE"] = i
        elif "WasTaggedLastTick" in x:
            columnids["GOTSHOT"] = i

    for row in l[1::]:
        agent_id = row[columnids["ID"]]
        if not (agent_id in agents):
            agents[agent_id] = Agent([])
        agent = agents[agent_id]
        x = int(row[columnids["X"]].split(",")[0])
        y = int(row[columnids["Y"]].split(",")[0])
        y = len(map.map) - y
        alive = row[columnids["ALIVE"]] == "True"
        tick = int(row[columnids["TICK"]])
        color = row[columnids["COLOR"]]
        team = row[columnids["TEAM"]]
        vrange = int(row[columnids["VRANGE"]])
        gotshot = row[columnids["GOTSHOT"]] == "True"
        state = AgentState(x, y, tick)
        state.alive = alive
        state.color = color
        state.team = team
        state.vrange = vrange
        state.gotshot = gotshot
        agent.states.append(state)
    return list(agents.values())


def _create_circle(self, xy, r, **kwargs):
    x, y = xy
    return self.create_oval(x - r, y - r, x + r, y + r, **kwargs)


def _create_cross(self, xy, **kwargs):
    x, y = xy
    x0, y0 = x - BOXSIZE, y - BOXSIZE
    x1, y1 = x + BOXSIZE, y + BOXSIZE
    x2, y2 = x - BOXSIZE, y + BOXSIZE
    x3, y3 = x + BOXSIZE, y - BOXSIZE
    return self.create_polygon(x0, y0, x1, y1, x, y, x2, y2, x3, y3, x, y, **kwargs)


def main():
    tk.Canvas.create_circle = _create_circle
    tk.Canvas.create_cross = _create_cross
    map = map_read_in("../LaserTagBox/Resources/map_4_open.csv")
    path = "../LaserTagBox/bin/Debug/net6.0/PlayerBody.csv"
    agent_data = agent_read_in(path, map)
    gui.main(agent_data, map)


if __name__ == "__main__":
    main()
    pass
