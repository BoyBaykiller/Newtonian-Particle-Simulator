using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL4;
using Newtonian_Particle_Simulator.Render.Objects;

namespace Newtonian_Particle_Simulator.Render
{
    class ParticleSimulator
    {
        public readonly int NumParticles;
        private readonly BufferObject particleBuffer;
        private readonly ShaderProgram shaderProgram;
        public unsafe ParticleSimulator(ReadOnlySpan<Particle> particles)
        {
            NumParticles = particles.Length;

            shaderProgram = new ShaderProgram(
                new Shader(ShaderType.VertexShader, File.ReadAllText("res/shaders/particles/vertex.glsl")),
                new Shader(ShaderType.FragmentShader, File.ReadAllText("res/shaders/particles/fragment.glsl")));

            particleBuffer = new BufferObject(BufferRangeTarget.ShaderStorageBuffer, 0);
            particleBuffer.ImmutableAllocate(sizeof(Particle) * (nint)NumParticles, particles[0], BufferStorageFlags.None);

            IsRunning = true;
        }


        private bool _isRunning;
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }

            set
            {
                _isRunning = value;
                shaderProgram.Upload(3, _isRunning ? 1.0f : 0.0f);
            }
        }
        public void Run(float dT)
        {
            GL.PointSize(1.1f);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            shaderProgram.Use();
            shaderProgram.Upload(0, dT);

            GL.DrawArrays(PrimitiveType.Points, 0, NumParticles);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }

        public void ProcessInputs(GameWindow gameWindow, in Vector3 camPos, in Matrix4 view, in Matrix4 projection)
        {
            if (gameWindow.CursorVisible)
            {
                if (MouseManager.LeftButton == ButtonState.Pressed)
                {
                    System.Drawing.Point windowSpaceCoords = gameWindow.PointToClient(new System.Drawing.Point(MouseManager.WindowPositionX, MouseManager.WindowPositionY)); windowSpaceCoords.Y = gameWindow.Height - windowSpaceCoords.Y; // [0, Width][0, Height]
                    Vector2 normalizedDeviceCoords = Vector2.Divide(new Vector2(windowSpaceCoords.X, windowSpaceCoords.Y), new Vector2(gameWindow.Width, gameWindow.Height)) * 2.0f - new Vector2(1.0f); // [-1.0, 1.0][-1.0, 1.0]
                    Vector3 dir = GetWorldSpaceRay(projection.Inverted(), view.Inverted(), normalizedDeviceCoords);

                    Vector3 pointOfMass = camPos + dir * 25.0f;
                    shaderProgram.Upload(1, pointOfMass);
                    shaderProgram.Upload(2, 1.0f);
                }
                else
                    shaderProgram.Upload(2, 0.0f);
            }

            if (KeyboardManager.IsKeyTouched(Key.T))
                IsRunning = !IsRunning;

            shaderProgram.Upload(4, view * projection);
        }

        public static Vector3 GetWorldSpaceRay(Matrix4 inverseProjection, Matrix4 inverseView, Vector2 normalizedDeviceCoords)
        {
            Vector4 rayEye = new Vector4(normalizedDeviceCoords.X, normalizedDeviceCoords.Y, -1.0f, 1.0f) * inverseProjection; rayEye.Z = -1.0f; rayEye.W = 0.0f;
            return (rayEye * inverseView).Xyz.Normalized();
        }
    }
}
