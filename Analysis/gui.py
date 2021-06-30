import threading
import time
import tkinter as tk
from tkinter import filedialog
import statistics
import random as rnd
import numpy as np


class GUIFrame(object):
    def __init__(self, root, preframe=None, auto_init=True):
        self.root = root
        self.main_frame = preframe if preframe else tk.Frame(self.root)
        if auto_init:
            self.init_gui()
            self.post_init_gui()

    def init_gui(self):
        self.main_frame.destroy()
        self.main_frame = tk.Frame(self.root)
        self.init_gui_elements()
        self.init_gui_menu()
        self.main_frame.pack()

    def init_gui_elements(self):
        pass

    def init_gui_menu(self):
        pass

    def post_init_gui(self):
        pass

    def NextFrame(self, frame_type):
        return lambda: frame_type(self.root, self.main_frame)


class Main(GUIFrame):

    def init_gui_elements(self):
        l = tk.Label(self.main_frame, text="FLJ")
        l.pack()

    def init_gui_menu(self):
        menubar = tk.Menu(self.root)

        m0 = tk.Menu(menubar, tearoff=0)
        menubar.add_cascade(label="Aktionen", menu=m0)
        m0.add_command(label="Start", command=self.NextFrame(Visualizer))
        m0.add_separator()
        m0.add_command(label="Stop", command=self.NextFrame(Visualizer))

        self.root.config(menu=menubar)

    def post_init_gui(self):
        self.root.after(0, self.NextFrame(Visualizer))
        pass


class Visualizer(GUIFrame):
    def __init__(self, root, preframe=None, auto_init=True):
        global AGENTS
        self.fps = 10
        self.playdelay = 1/self.fps
        self.playdelaydelta = 0
        self.playstep = 1
        self.play_running = False
        self.playframe = 0
        self.playframe_min = 0
        self.playframe_max = 100
        self.playframe_loop = False
        self.agents = AGENTS
        l = [0]
        for agent in AGENTS:
            l.append(len(agent.states))
        self.playframe_max = max(l)-1
        self.canvas_graph_data = []
        self.canvas_graph_cursor = 0
        self.canvas_graph_cursor_id = None
        super().__init__(root, preframe, auto_init)

    def init_gui_elements(self):
        global AGENT_MAP
        canvasframe = tk.Frame(self.main_frame, width=300, height=300)
        canvasframe.grid(row=0, column=0)
        try:
            h, w = AGENT_MAP.get_canvas_dimension()
            self.canvas = tk.Canvas(canvasframe, bg="#AAA", height=h, width=w)
            self.canvas.config(width=800,height=800)
            self.canvas.config(scrollregion=(0,0,h,w))
        except:
            self.canvas = tk.Canvas(canvasframe, bg="#F00", height=500, width=500)
        hbar = tk.Scrollbar(canvasframe,orient=tk.HORIZONTAL)
        hbar.pack(side=tk.BOTTOM,fill=tk.X)
        hbar.config(command=self.canvas.xview)
        vbar = tk.Scrollbar(canvasframe,orient=tk.VERTICAL)
        vbar.pack(side=tk.RIGHT,fill=tk.Y)
        vbar.config(command=self.canvas.yview)
        self.canvas.config(xscrollcommand=hbar.set, yscrollcommand=vbar.set)
        self.canvas.pack(side=tk.LEFT,expand=True,fill=tk.BOTH)

        infoframe = tk.Frame(self.main_frame)
        infoframe.grid(row=0, column=1)

        mediaframe = tk.LabelFrame(infoframe, text="Simulation")
        mediaframe.grid(row=0, column=0)
        self.stepslider = tk.Scale(mediaframe, from_=self.playframe_min, to=self.playframe_max,
                                   orient=tk.HORIZONTAL, length=400, command=self._canvas_update)
        self.stepslider.grid(row=0, column=0)
        
        mediaframeactionblock = tk.Frame(mediaframe)
        mediaframeactionblock.grid(row=1, column=0)
        b_start = tk.Button(mediaframeactionblock, text="|<<", width=5, command=self._start)
        b_start.grid(row=1, column=0)
        b_playback = tk.Button(mediaframeactionblock, text="<|", width=5, command=self._playback)
        b_playback.grid(row=1, column=1)
        b_pause = tk.Button(mediaframeactionblock, text="||", width=5, command=self._pause)
        b_pause.grid(row=1, column=2)
        b_play = tk.Button(mediaframeactionblock, text="|>", width=5, command=self._play)
        b_play.grid(row=1, column=3)
        b_end = tk.Button(mediaframeactionblock, text=">>|", width=5, command=self._end)
        b_end.grid(row=1, column=4)
        self.animationloop = tk.BooleanVar()
        cb_loop = tk.Checkbutton(mediaframe, text="Loop", variable=self.animationloop)
        cb_loop.grid(row=2, column=0)
        
        advancedmediaframeactionblock = tk.LabelFrame(mediaframe, text="FPS")
        advancedmediaframeactionblock.grid(row=3, column=0)
        self.fpslable = advancedmediaframeactionblock
        self.fpsslider = tk.Scale(advancedmediaframeactionblock, from_=1, to=200,orient=tk.HORIZONTAL, length=400, command=self._update_fps)
        self.fpsslider.grid(row=0, column=0)
        self.fpsslider.set(50)

        red = "N/A"
        blue = "N/A"
        yellow = "N/A"
        green = "N/A"

        for agent in self.agents:
            for state in agent.states:
                if state.stage == "Red":
                    red = state.team
                if state.stage == "Blue":
                    blue = state.team
                if state.stage == "Green":
                    green = state.team
                if state.stage == "Yellow":
                    yellow = state.team


        graphframe = tk.LabelFrame(infoframe, text="Stats")
        graphframe.grid(row=4, column=0)
        graphframeinfo = tk.Frame(graphframe)
        graphframeinfo.pack(side=tk.TOP, expand=True, fill=tk.BOTH)
        self.graph_lable_red = tk.Label(graphframeinfo, bg="red", fg="white", text=red)
        self.graph_lable_red.grid(row=0, column=0)
        self.graph_lable_blue = tk.Label(graphframeinfo,bg="blue", fg="white", text=blue)
        self.graph_lable_blue.grid(row=1, column=0)
        self.graph_lable_yellow = tk.Label(graphframeinfo, bg="yellow", fg="black", text=yellow)
        self.graph_lable_yellow.grid(row=2, column=0)
        self.graph_lable_green = tk.Label(graphframeinfo,bg="green", fg="white", text=green)
        self.graph_lable_green.grid(row=3, column=0)


    def init_gui_menu(self):
        menubar = tk.Menu(self.root)

        m0 = tk.Menu(menubar, tearoff=0)
        menubar.add_cascade(label="Aktionen", menu=m0)
        m0.add_command(label="Start", command=self.NextFrame(Visualizer))
        m0.add_separator()
        m0.add_command(label="Stop", command=self.NextFrame(Visualizer))

        self.root.config(menu=menubar)

    def post_init_gui(self):
        self._end()
        self._start()

    def NextFrame(self, frame_type):
        self.play_running = False
        return super().NextFrame(frame_type)

    def _update_fps(self, e):
        self.fps = int(self.fpsslider.get())
        self.playdelay = 1/self.fps
        self.playdelaydelta = 0
            
    def _canvas_update(self, e):
        global AGENT_MAP
        AGENT_MAP.update(self.canvas, int(e))
        for agent in self.agents:
            agent.update(self.canvas, int(e), AGENT_MAP)

    def _update(self):
        mt = threading.main_thread()
        try:
            t = time.time()
            tdticks = 0
            fpsrow = [0]
            fpssec = 1
            while self.play_running and mt.is_alive():
                x = self.playframe + self.playstep
                if x < self.playframe_min or x > self.playframe_max:
                    if self.animationloop.get():
                        x = x%self.playframe_max
                    else:
                        self._pause()
                self.playframe = min(self.playframe_max, max(self.playframe_min, x))
                self.stepslider.set(self.playframe)
                tdticks += 1
                tt = time.time()
                if tt >= t+fpssec:
                    fps = round(tdticks/(tt-t),2)
                    fpsrow.append(fps)
                    fpsdif = self.fps - statistics.median(fpsrow)
                    fpsdifdelta = fpsdif * 0.2
                    if -0.1 < fpsdif < 0.1:
                        fpsdifdelta = 0
                    else:
                        self.playdelaydelta += fpsdifdelta
                    print("FPS :",fps," | ",self.fps,round(self.fps  + self.playdelaydelta,4))
                    self.fpslable.config(text="FPS : "+str(fps))
                    self.playdelay = fpssec / max((self.fps  + self.playdelaydelta),fpssec)
                    fpsrow = fpsrow[-4::]
                    t = time.time()
                    tdticks = 0
                time.sleep(self.playdelay)
        except:
            pass

    def _start(self):
        self.playframe = self.playframe_min
        self.stepslider.set(self.playframe)
        print("start")

    def _end(self):
        self.playframe = self.playframe_max
        self.stepslider.set(self.playframe)
        print("end")

    def _pause(self):
        self.play_running = False
        self.fpslable.config(text="FPS")
        print("pause")

    def _play(self):
        self.playstep = abs(self.playstep)
        if not self.play_running:
            self.playframe = self.stepslider.get()
            self.play_running = True
            threading.Thread(target=self._update).start()
        print("play")

    def _playback(self):
        self.playstep = abs(self.playstep)*-1
        if not self.play_running:
            self.playframe = self.stepslider.get()
            self.play_running = True
            threading.Thread(target=self._update).start()
        print("playback")


AGENTS = []
AGENT_MAP = None


def main(agents=[], agent_map=None):
    global AGENTS, AGENT_MAP
    AGENTS = agents
    AGENT_MAP = agent_map
    root = tk.Tk()
    root.title("Laser Tag")
    app = Main(root)
    root.mainloop()


if __name__ == '__main__':
    main()
