# C# OpenGL Newtonian-Particle-Simulator

![gif](Video.gif?raw=true)
![img1](sample.png?raw=true)

This is a very simple interactive particle simulator.
The whole pipeline consists out of a [single shader program](Newtonian-Particle-Simulator/res/shaders/particles).
The vertex shader computes and renders particles at the same time. The fragment shader just outputs the computed colors with blending enabled.
The actual particles are stored in a Shader Storage Buffer which is a really useful arbitrary read/write interface of global GPU memory.

Requires OpenGL 4.5.

Also see https://youtu.be/NhnoNYqIhTI.

### **KeyBoard:**
* W, A, S, D => Movment
* E => Toggle cursor visibility
* T => Toggle simulation
* V => Toggle VSync
* F11 => Toggle fullscreen
* LShift => Faster movment speed
* LControl => Slower movment speed
* Esc => Close

### **Mouse:**
* LButton => Set point of mass