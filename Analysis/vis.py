import random as rnd
import tkinter as tk
import csv

import gui

BITMAPS = ["error", "gray75", "gray50", "gray25", "gray12",
           "hourglass", "info", "questhead", "question", "warning"]


class AgentState(object):
    def __init__(self, x, y, tick, status = 0):
        self.x = x
        self.y = y
        self.tick = tick
        self.status = status
        self.fg = "#444"
        self._update_color()
        self.stage = 0
    # TODO: update colors
    def _update_color(self):
        if self.status == 1:
            self.fg = "#0101DF"
        elif self.status == 2:
            self.fg = "#FFFF00"
        elif self.status == 3:
            self.fg = "#FF0000"
        elif self.status == 4:
            self.fg = "#01DF01"
    def infected(self):
        return 1 < self.status < 4
    def immun(self):
        return self.status == 4
    def __eq__(self,other):
        return self.tick == other.tick
    def __lt__(self,other):
        return self.tick < other.tick
    def __le__(self,other):
        return self < other or self == other
    def __gt__(self,other):
        return self.tick > other.tick
    def __ge__(self,other):
        return self > other or self == other


class Agent(object):
    def __init__(self, states=[]):
        self.states = states
        self.states.sort()
        self.statedic = None
        self._updateneeded = True
        self._canvas_id = None

    def get_state(self, step=0):
        try:
            return self.states[step]
        except:
            return None

    def get_staterow(self):
        d = {}
        for x in self.states:
            if x.stage != 0:
                indicator = 0
                if x.infected():
                    indicator = 1
                if x.immun():
                    indicator = 2
                d[x.tick] = indicator
        return d

    def draw(self, canvas, step=0, agent_map=None):
        state = self.get_state(step)
        try:
            xy = agent_map.get_position(state.x, state.y-1)
        except:
            xy = (0, 0)
        if not state and len(self.states) > 0:
            state = self.states[-1]
            xy = agent_map.get_position(state.x, state.y)
            cid = canvas.create_bitmap(xy, bitmap=BITMAPS[7], foreground="#DDD")
        else:
            cid = canvas.create_bitmap(xy, bitmap=BITMAPS[7], foreground=state.fg)
        self._canvas_id = cid
    def delete(self, canvas, step=0, agent_map=None):
        canvas.delete(self._canvas_id)
    def update(self, canvas, step=0, agent_map=None):
        if self._updateneeded:
            self.delete(canvas, step, agent_map)
            self.draw(canvas, step, agent_map)


class AgentMap(object):
    def __init__(self, x_size, y_size):
        self.x = x_size
        self.y = y_size
        self.boarder = 10
        self.box_size = 20
        #self.color = "#FFF"

    def get_canvas_dimension(self):
        return (self.x*self.box_size+self.boarder*2, self.y*self.box_size+self.boarder*2)

    def get_position(self, x, y, delta=True):
        dx = 0
        dy = 0
        if delta:
            dx = self.box_size//2
            dy = self.box_size//2
        newx = self.boarder + x*self.box_size + dx
        newy = self.boarder + y*self.box_size + dy
        return (newx, newy)

    def draw(self, canvas, step=0):
        d = 0
        for i in range(self.x):
            for j in range(self.y):
                x1, y1 = self.get_position(i, j, False)
                x2, y2 = self.get_position(i+1, j+1, False)
                if d == step:
                    canvas.create_rectangle((x1, y1, x2, y2), fill="#F00")
                else:
                    canvas.create_rectangle((x1, y1, x2, y2), fill=self.color)
                d += 1
                d = 0

class AdvancedAgentMapField(object):
    def __init__(self,x1,y1,x2,y2,color="#FFF"):
        self.fieldpos = (x1,y1,x2,y2)
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
            self.delete(canvas,step)
            self.draw(canvas,step)

class AdvancedAgentMap(object):
    def __init__(self, x_size, y_size, color="#FFF"):
        self.x = x_size
        self.y = y_size
        self.boarder = 10
        self.box_size = 20
        self.color = color
        self.map = []
        for y in range(self.y):
            l = []
            for x in range(self.x):
                x1, y1 = self.get_position(y, x, False)
                x2, y2 = self.get_position(y+1, x+1, False)
                l.append(AdvancedAgentMapField(y1,x1,y2,x2,self.color))
            self.map.append(l)

    def get_canvas_dimension(self):
        return (self.x*self.box_size+self.boarder*2, self.y*self.box_size+self.boarder*2)

    def get_position(self, x, y, delta=True):
        dx = 0
        dy = 0
        if delta:
            dx = self.box_size//2
            dy = self.box_size//2
        newx = self.boarder + x*self.box_size + dx
        newy = self.boarder + y*self.box_size + dy
        return (newx, newy)

    def draw(self, canvas, step=0):
        for y in self.map:
            for x in y:
                try:
                    x.draw(canvas,step)
                except Exception as e:
                    pass
    def delete(self, canvas, step=0):
        for y in self.map:
            for x in y:
                try:
                    x.delete(canvas,step)
                except Exception as e:
                    pass
    def update(self, canvas, step=0):
        for y in self.map:
            for x in y:
                try:
                    x.update(canvas,step)
                except Exception as e:
                    pass


def clip(x, mini, maxi):
    return min(maxi, max(mini, x))


def map_readin(file=""):
    x = 0
    y = 0
    l = []
    with open(file,"r") as f:
        reader = csv.reader(f, delimiter=";")
        for row in reader:
            y += 1
            x = max(x,len(row))
            l.append(row)
    m = AdvancedAgentMap(x,y)
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


def agent_readin(file,agent_map):
    agents = {}
    l = []
    with open(file,"r") as f:
        reader = csv.reader(f, delimiter=";")
        for row in reader:
            l.append(row)
    columnids = {}
    for i in range(len(l[0])):
        x = l[0][i]
        if "ID" in x:
            columnids["ID"] = i
        elif "0_Position" in x:
            columnids["X"] = i
        elif "1_Position" in x:
            columnids["Y"] = i
        elif "Tick" in x:
            columnids["TICK"] = i
        elif "energy" in x:
            columnids["ENGERGY"] = i
        elif "currStance" in x:
            columnids["CURRSTANCE"] = i
        elif "color" in x:
            columnids["COLOR"] = i
    for row in l[1::]:
        aid = row[columnids["ID"]]
        if not (aid in agents):
            agents[aid] = Agent([])
        agent = agents[aid]
        x = int(row[columnids["X"]].split(",")[0])
        y = int(row[columnids["Y"]].split(",")[0])
        y = len(agent_map.map)-y
        tick = int(row[columnids["TICK"]].split(".")[0])
        status = row[columnids["COLOR"]]
        # blue
        if status == "blue":
            state = AgentState(x,y,tick,1)
        # yellow
        elif status == "yellow":
            state = AgentState(x,y,tick,2)
        # red
        elif status == "red":
            state = AgentState(x,y,tick,3)
        # green
        elif status == "green":
            state = AgentState(x, y, tick, 4)
        else:
            state = AgentState(x,y,tick,0)
        #state = AgentState(x,y,tick)
        #TEST
        #bl=[False,False,False,True]
        #state = AgentState(x,y,tick,rnd.choice(bl),rnd.choice(bl))
        #print(agent)
        state.stage = row[columnids["COLOR"]]
        agent.states.append(state)
    return list(agents.values())


def main():
    am = map_readin("map.csv")
    #print(len(am.map[0]),len(am.map))
    a = agent_readin("agents.csv",am)
    #am = AgentMap(20, 20)
    gui.main(a, am)


if __name__ == "__main__":
    main()
    pass