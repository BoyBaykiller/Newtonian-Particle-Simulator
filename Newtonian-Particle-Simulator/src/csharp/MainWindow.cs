﻿using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Newtonian_Particle_Simulator.Render;

namespace Newtonian_Particle_Simulator
{
    class MainWindow : GameWindow
    {
        public const int WORK_GROUP_SIZE_X = 128;
        public MainWindow() : base(832, 832, new GraphicsMode(0, 0, 0, 0), "Newtonian-Particle-Simulator") { }

        readonly Camera camera = new Camera(new Vector3(0, 0, 15), new Vector3(0, 1, 0));
        Matrix4 projection;

        int frames = 0, FPS;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            particleSimulator.Run((float)e.Time);

            SwapBuffers();
            frames++;
            base.OnRenderFrame(e);
        }

        bool isStopped = false;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (fpsTimer.ElapsedMilliseconds >= 1000)
            {
                FPS = frames;
                Title = $"Newtonian-Particle-Simulator FPS: {FPS}";
                frames = 0;
                fpsTimer.Restart();
            }

            if (Focused)
            {
                KeyboardManager.Update();
                MouseManager.Update();

                if (KeyboardManager.IsKeyTouched(Key.T))
                    isStopped = !isStopped;

                if (KeyboardManager.IsKeyTouched(Key.V))
                    VSync = VSync == VSyncMode.Off ? VSyncMode.On : VSyncMode.Off;

                if (KeyboardManager.IsKeyTouched(Key.E))
                {
                    CursorVisible = !CursorVisible;
                    CursorGrabbed = !CursorGrabbed;

                    if (!CursorVisible)
                    {
                        MouseManager.Update();
                        camera.Velocity = Vector3.Zero;
                    }
                }

                if (KeyboardManager.IsKeyTouched(Key.F11))
                    WindowState = WindowState == WindowState.Normal ? WindowState.Fullscreen : WindowState.Normal;


                particleSimulator.ProcessInputs(this, camera.Position, camera.View, projection);
                if (!CursorVisible)
                    camera.ProcessInputs((float)e.Time);
            }


            if (KeyboardManager.IsKeyDown(Key.Escape))
                Close();

            base.OnUpdateFrame(e);
        }

        readonly Stopwatch fpsTimer = Stopwatch.StartNew();
        ParticleSimulator particleSimulator;
        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine($"OpenGL: {Helper.APIMajor}.{Helper.APIMinor}");
            Console.WriteLine($"GLSL: {GL.GetString(StringName.ShadingLanguageVersion)}");
            Console.WriteLine($"GPU: {GL.GetString(StringName.Renderer)}");

            if (!Helper.IsCoreExtensionAvailable("GL_ARB_direct_state_access", 4, 5))
                throw new NotSupportedException("Your system does not support GL_ARB_direct_state_access");

            GL.PointSize(1.1f);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            VSync = VSyncMode.Off;

            GL.GetInteger((GetIndexedPName)All.MaxComputeWorkGroupCount, 0, out int maxWorkGroupountX);
            ulong maxParticles = (ulong)maxWorkGroupountX * WORK_GROUP_SIZE_X;
            ulong numParticles;
            do
                Console.Write($"Number of particles (max: {maxParticles}): ");
            while ((!ulong.TryParse(Console.ReadLine(), out numParticles)) || numParticles > maxParticles || numParticles < 0);

            Random rng = new Random();
            Particle[] particles = new Particle[numParticles];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = new Vector3((float)rng.NextDouble() * 100 - 50, (float)rng.NextDouble() * 100 - 50, -(float)rng.NextDouble() * 100);
                //particles[i].Position = Helper.RandomUnitVector(rng) * 50.0f;
            }
            particleSimulator = new ParticleSimulator(particles);

            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(103.0f), (float)Width / Height, 0.1f, 1000f);

            base.OnResize(e);
        }
        protected override void OnFocusedChanged(EventArgs e)
        {
            if (Focused)
                MouseManager.Update();
        }
    }
}