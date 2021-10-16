using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Newtonian_Particle_Simulator.Render;
using Newtonian_Particle_Simulator.Render.Objects;

namespace Newtonian_Particle_Simulator
{
    class MainWindow : GameWindow
    {
        public const int WORK_GROUP_SIZE_X = 128;
        public MainWindow() : base(832, 832, new GraphicsMode(0, 0, 0, 0), "Newtonian-Particle-Simulator") { }

        public readonly Camera PlayerCamera = new Camera(new Vector3(0, 0, 15), new Vector3(0, 1, 0));
        public Matrix4 Projection;

        int frames = 0, FPS;
        Query rasterizerTimer;
        Query computeTimer;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            rasterizerTimer.Start();
            Framebuffer.Clear(0, ClearBufferMask.ColorBufferBit);
            rasterizerProgram.Use();

            GL.DrawArrays(PrimitiveType.Points, 0, particleSimulator.NumParticles);
            rasterizerTimer.StopAndReset();

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
                Title = $"Newtonian-Particle-Simulator FPS: {FPS}; CP: {computeTimer.ElapsedMilliseconds}ms; RP: {rasterizerTimer.ElapsedMilliseconds}ms";
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
                        PlayerCamera.Velocity = Vector3.Zero;
                    }
                }

                if (KeyboardManager.IsKeyTouched(Key.F11))
                    WindowState = WindowState == WindowState.Normal ? WindowState.Fullscreen : WindowState.Normal;


                if (CursorVisible)
                    particleSimulator.ProcessInputs(this);
                else
                    PlayerCamera.ProcessInputs((float)e.Time);

                rasterizerProgram.Upload(0, PlayerCamera.View * Projection);

                if (!isStopped)
                {
                    computeTimer.Start();
                    particleSimulator.Run((float)e.Time);
                    computeTimer.StopAndReset();
                }
            }


            if (KeyboardManager.IsKeyDown(Key.Escape))
                Close();

            base.OnUpdateFrame(e);
        }

        readonly Stopwatch fpsTimer = Stopwatch.StartNew();
        ParticleSimulator particleSimulator;
        ShaderProgram rasterizerProgram;
        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine($"OpenGL: {Helper.APIMajor}.{Helper.APIMinor}");
            Console.WriteLine($"GLSL: {GL.GetString(StringName.ShadingLanguageVersion)}");
            Console.WriteLine($"GPU: {GL.GetString(StringName.Renderer)}");

            if (!Helper.IsCoreExtensionAvailable("GL_ARB_direct_state_access", 4, 5))
                throw new NotSupportedException("Your system does not support GL_ARB_direct_state_access");

            GL.Enable(EnableCap.VertexProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            computeTimer = new Query(100); rasterizerTimer = new Query(100);
            VSync = VSyncMode.Off;
            rasterizerProgram = new ShaderProgram(new Shader(ShaderType.VertexShader, "res/shaders/rasterizer/vertex.glsl".GetPathContent()), new Shader(ShaderType.FragmentShader, "res/shaders/rasterizer/fragment.glsl".GetPathContent()));

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
            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(103.0f), (float)Width / Height, 0.1f, 1000f);

            base.OnResize(e);
        }
        protected override void OnFocusedChanged(EventArgs e)
        {
            if (Focused)
                MouseManager.Update();
        }
    }
}
