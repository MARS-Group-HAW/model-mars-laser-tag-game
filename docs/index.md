---
_layout: landing
---

LaserTag is an agent-based simulation game inspired by the real-world recreational shooting sport known as laser tag.\
The game is developed with the [Multi-Agent Research and Simulation (MARS) Framework](https://mars-group-haw.github.io/index.html),   
which is written in [C#](https://learn.microsoft.com/en-us/dotnet/csharp/) and runs on [.NET](https://dotnet.microsoft.com/en-us/download).  

Users of LaserTag can implement their own agents with customized behavioral logic by using the provided agent interface.  
The interface provides properties and methods which enable agent movement, agent state management, agent-agent and  
 agent-environment interactions, and other functions.

This documentation consists of two main sections:

### **Docs**
It covers everything from gameplay concepts and simulation structure to agent behavior.

Use this section to understand:
- The game objectives and setup
- The simulation environment and logic
- How agents interact with each other and the world
- Design decisions and visualizations

### **API Reference**
The API section contains detailed technical information about the codebase, including:
- All available classes, interfaces, and methods
- Agent interaction mechanisms
- Data structures used in the simulation

This part is generated directly from the source code using DocFX and is ideal for developers implementing or extending agents.
