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
            #self.canvas.config(width=800,height=800)
            #self.canvas.config(width=3000,height=1000)
            self.canvas.config(width=1400,height=900)
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
        #self.canvas.grid(row=0, column=0)
        #self.canvas.create_arc((10,10,200,200), start=0, extent=359, fill="red")
        #self.canvas.create_oval((10,10,20,20), fill="red")
        # self.canvas.create_bitmap((100,100), bitmap="questhead")#, foreground="red")
        # self.canvas.create_rectangle((90,90,110,110))

        infoframe = tk.Frame(self.main_frame)
        infoframe.grid(row=0, column=1)

        mediaframe = tk.LabelFrame(infoframe, text="Simulation")
        mediaframe.grid(row=0, column=0)
        self.stepslider = tk.Scale(mediaframe, from_=self.playframe_min, to=self.playframe_max,
                                   orient=tk.HORIZONTAL, length=400, command=self._canvas_update)
        #self.stepslider.bind('<<RangeChanged>>', self._canvas_update)
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

        graphframe = tk.LabelFrame(infoframe, text="Graph")
        graphframe.grid(row=1, column=0)
        graphframeinfo = tk.Frame(graphframe)
        graphframeinfo.pack(side=tk.TOP,expand=True,fill=tk.BOTH)
        self.graph_lable_uinf = tk.Label(graphframeinfo,text = "Uninfected : ??? % |")
        self.graph_lable_uinf.grid(row=0, column=0)
        self.graph_lable_inf = tk.Label(graphframeinfo,text = "Infected : ??? % |")
        self.graph_lable_inf.grid(row=0, column=1)
        self.graph_lable_immun = tk.Label(graphframeinfo,text = "Immun : ??? % |")
        self.graph_lable_immun.grid(row=0, column=2)
        self.graph_lable_guests = tk.Label(graphframeinfo,text = " ( Gäste : ????? ) ")
        self.graph_lable_guests.grid(row=0, column=3)
        self.canvas_graph = tk.Canvas(graphframe, bg="#000", height=250, width=400)
        self.canvas_graph.config(scrollregion=(0,0,0,0))
        hbar = tk.Scrollbar(graphframe,orient=tk.HORIZONTAL)
        hbar.pack(side=tk.BOTTOM,fill=tk.X)
        hbar.config(command=self.canvas_graph.xview)
        self.canvas_graph_hbar = hbar
        self.canvas_graph.config(xscrollcommand=hbar.set)
        self.canvas_graph.pack(side=tk.LEFT,expand=True,fill=tk.BOTH)

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
        #self.rnd_graphdata()
        self.agent_graphdata()

    def NextFrame(self, frame_type):
        self.play_running = False
        return super().NextFrame(frame_type)

    def rnd_graphdata(self):
        l = []
        for i in range(600):
            x3 = rnd.randint(0,20)
            x2 = rnd.randint(0,40)
            x1 = rnd.randint(20,40)
            l.append((x1,x2,x3))
        self._update_graphdata(l)

    def agent_graphdata1(self):
        datarow = []
        agentstaterows = []
        imax = 0
        for agent in self.agents:
            ar = agent.get_staterow()
            imax = max(list(ar.keys())+[imax])
            agentstaterows.append(ar)
        datamax = 0
        for i in range(imax+1):
            data = [0]*3
            for x in agentstaterows:
                if i in x:
                    data[x[i]] += 1
            datarow.append(data)
            datamax = max(data+[datamax])
        #normalize
        datarownormalized = np.array(datarow)
        datarownormalized = datarownormalized*100/datamax
        self._update_graphdata(datarownormalized)
        
    def agent_graphdata(self):
        datarow = []
        agentstaterows = []
        imax = 0
        for agent in self.agents:
            ar = agent.get_staterow()
            if len(ar.keys()) > 0:
                imax = max(list(ar.keys())+[imax])
                agentstaterows.append(ar)
        datamax = len(agentstaterows)
        for i in range(imax+1):
            data = list((0,0,0))
            for x in agentstaterows:
                if i in x:
                    data[x[i]] += 1
            l = []
            for x in data:
                l.append(x/datamax*100)
            l.append(sum(data))
            datarow.append(l)
        #normalize
        self._update_graphdata(datarow)

    def _update_graphdata(self,data):
        """list of 3tuple normalized(0-100)
1:Uninfected
2:Infected
3:Immun"""
        self.canvas_graph_data = data
        xdistance = 10
        self.canvas_graph.delete(tk.ALL)
        self.canvas_graph.config(scrollregion=(-1*xdistance,0,(len(data)+1)*xdistance,0))
        points = [(len(data)-1)*xdistance + xdistance//2,250,xdistance//2,250]
        for x in range(len(data)):
            y = 250 - (data[x][0] + data[x][1] + data[x][2])*2
            points.append(x*xdistance + xdistance//2)
            points.append(int(y))
        self.canvas_graph.create_polygon(points, outline="#00F", fill="#77F", width=4)
        points = [(len(data)-1)*xdistance + xdistance//2,250,xdistance//2,250]
        for x in range(len(data)):
            y = 250 - (data[x][1] + data[x][2])*2
            points.append(x*xdistance + xdistance//2)
            points.append(int(y))
        self.canvas_graph.create_polygon(points, outline="#F00", fill="#F77", width=4)
        points = [(len(data)-1)*xdistance + xdistance//2,250,xdistance//2,250]
        for x in range(len(data)):
            y = 250 - data[x][2]*2
            points.append(x*xdistance + xdistance//2)
            points.append(int(y))
        self.canvas_graph.create_polygon(points, outline="#0F0", fill="#7F7", width=4)
        self.canvas_graph.create_line(-1*xdistance,50,(len(data)+1)*xdistance,50, fill="#FFF", width=1)
        self.canvas_graph.create_line(-1*xdistance,100,(len(data)+1)*xdistance,100, fill="#AAA", width=1)
        self.canvas_graph.create_line(-1*xdistance,150,(len(data)+1)*xdistance,150, fill="#AAA", width=1)
        self.canvas_graph.create_line(-1*xdistance,200,(len(data)+1)*xdistance,200, fill="#AAA", width=1)
        self._update_graph_cursor()

    def _update_graph_cursor(self,step=0):
        self.canvas_graph_cursor = step
        self.canvas_graph.delete(self.canvas_graph_cursor_id)
        xdistance = 10
        x = xdistance * step + xdistance//2
        self.canvas_graph_cursor_id = self.canvas_graph.create_line(x,0,x,250, fill="#FFF", width=2)
        self._update_graph_labels(step)

    def _update_graph_labels(self,step=0):
        x0,x1,x2,xsum = self.canvas_graph_data[step]
        self.graph_lable_uinf.config(text="Uninfected : " + str(int(x0)).zfill(3)+" % |")
        self.graph_lable_inf.config(text="Infected : " + str(int(x1)).zfill(3)+" % |")
        self.graph_lable_immun.config(text="Immun : " + str(int(x2)).zfill(3)+" %")
        self.graph_lable_guests.config(text=" ( Gäste : " + str(int(xsum)).zfill(5)+" ) ")

    def _update_fps(self, e):
        self.fps = int(self.fpsslider.get())
        self.playdelay = 1/self.fps
        self.playdelaydelta = 0
            
    def _canvas_update(self, e):
        global AGENT_MAP
        AGENT_MAP.update(self.canvas, int(e))
        for agent in self.agents:
            agent.update(self.canvas, int(e), AGENT_MAP)
        self._update_graph_cursor(int(e))

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
                    #print("FPS :",fps)
                    fpsdif = self.fps - statistics.median(fpsrow)
                    fpsdifdelta = fpsdif * 0.2
                    if -0.1 < fpsdif < 0.1:
                        fpsdifdelta = 0
                    else:
                        self.playdelaydelta += fpsdifdelta
                    #print("FPS :",fps," | ",self.playdelay,self.playdelaydelta,fpsdif,fpsdifdelta)
                    print("FPS :",fps," | ",self.fps,round(self.fps  + self.playdelaydelta,4))
                    self.fpslable.config(text="FPS : "+str(fps))
                    self.playdelay = fpssec / max((self.fps  + self.playdelaydelta),fpssec)
                    fpsrow = fpsrow[-4::]
                    t = time.time()
                    tdticks = 0
                #time.sleep(self.playdelay / 1000.0)
                #time.sleep(1/self.fps)
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
    root.title("Virus Visualizer")
    app = Main(root)
    root.mainloop()


if __name__ == '__main__':
    main()
