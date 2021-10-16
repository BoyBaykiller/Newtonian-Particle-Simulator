using Newtonian_Particle_Simulator.Render.Objects;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL4;

namespace Newtonian_Particle_Simulator.Render
{
    class ParticleSimulator
    {
        public readonly int NumParticles;
        public readonly BufferObject ParticleBuffer;
        public readonly ShaderProgram ShaderProgram;
        public ParticleSimulator(Particle[] particles)
        {
            NumParticles = particles.Length;

            ShaderProgram = new ShaderProgram(new Shader(ShaderType.ComputeShader, "res/shaders/particles/update.glsl".GetPathContent()));
            ParticleBuffer = new BufferObject(BufferRangeTarget.ShaderStorageBuffer, 0);
            ParticleBuffer.ImmutableAllocate(System.Runtime.CompilerServices.Unsafe.SizeOf<Particle>() * NumParticles, particles, BufferStorageFlags.ClientStorageBit);

            ShaderProgram.Upload("numParticles", NumParticles);
        }

        
        public void Run(float dT)
        {
            ShaderProgram.Use();
            ShaderProgram.Upload(0, dT);
            GL.DispatchCompute((NumParticles + MainWindow.WORK_GROUP_SIZE_X - 1) / MainWindow.WORK_GROUP_SIZE_X, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }

        public void ProcessInputs(MainWindow mainWindow)
        {
            if (MouseManager.LeftButton == ButtonState.Pressed)
            {
                System.Drawing.Point windowSpaceCoords = mainWindow.PointToClient(new System.Drawing.Point(MouseManager.WindowPositionX, MouseManager.WindowPositionY)); windowSpaceCoords.Y = mainWindow.Height - windowSpaceCoords.Y; // [0, Width][0, Height]
                Vector2 normalizedDeviceCoords = Vector2.Divide(new Vector2(windowSpaceCoords.X, windowSpaceCoords.Y), new Vector2(mainWindow.Width, mainWindow.Height)) * 2.0f - new Vector2(1.0f); // [-1.0, 1.0][-1.0, 1.0]
                Vector3 dir = GetWorldSpaceRay(mainWindow.Projection.Inverted(), mainWindow.PlayerCamera.View.Inverted(), normalizedDeviceCoords);

                Vector3 pointOfMass = mainWindow.PlayerCamera.Position + dir * 40.0f;
                ShaderProgram.Upload(1, pointOfMass);
                ShaderProgram.Upload(2, 1.0f);
            }
            else
                ShaderProgram.Upload(2, 0.0f);
        }

        public static Vector3 GetWorldSpaceRay(Matrix4 inverseProjection, Matrix4 inverseView, Vector2 normalizedDeviceCoords)
        {
            Vector4 rayEye = new Vector4(normalizedDeviceCoords.X, normalizedDeviceCoords.Y, -1.0f, 1.0f) * inverseProjection; rayEye.Z = -1.0f; rayEye.W = 0.0f;
            return (rayEye * inverseView).Xyz.Normalized();
        }
    }
}
