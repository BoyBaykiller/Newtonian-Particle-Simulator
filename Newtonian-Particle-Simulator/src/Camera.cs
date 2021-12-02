using System;
using OpenTK;
using OpenTK.Input;

namespace Newtonian_Particle_Simulator
{
    class Camera
    {
        public Vector3 Position;
        public Vector3 ViewDir;
        public Vector3 Up;
        public Vector3 Velocity;
        public float MovmentSpeed;
        public float MouseSensitivity;
        public Matrix4 View { get; private set; }
        public Camera(Vector3 position, Vector3 up, float lookX = -90.0f, float lookY = 0.0f, float mouseSensitivity = 0.1f, float speed = 10)
        {
            Look.X = lookX;
            Look.Y = lookY;

            ViewDir.X = MathF.Cos(MathHelper.DegreesToRadians(Look.X)) * MathF.Cos(MathHelper.DegreesToRadians(Look.Y));
            ViewDir.Y = MathF.Sin(MathHelper.DegreesToRadians(Look.Y));
            ViewDir.Z = MathF.Sin(MathHelper.DegreesToRadians(Look.X)) * MathF.Cos(MathHelper.DegreesToRadians(Look.Y));

            View = GenerateMatrix(position, ViewDir, up);
            Position = position;
            Up = up;
            MovmentSpeed = speed;
            MouseSensitivity = mouseSensitivity;
        }

        private Vector2 Look = new Vector2();
        public void ProcessInputs(float dT)
        {
            Vector2 mouseDelta = MouseManager.DeltaPosition;

            Look.X += mouseDelta.X * MouseSensitivity;
            Look.Y -= mouseDelta.Y * MouseSensitivity;

            if (Look.Y >= 90) Look.Y = 89.999f;
            if (Look.Y <= -90) Look.Y = -89.999f;

            ViewDir.X = MathF.Cos(MathHelper.DegreesToRadians(Look.X)) * MathF.Cos(MathHelper.DegreesToRadians(Look.Y));
            ViewDir.Y = MathF.Sin(MathHelper.DegreesToRadians(Look.Y));
            ViewDir.Z = MathF.Sin(MathHelper.DegreesToRadians(Look.X)) * MathF.Cos(MathHelper.DegreesToRadians(Look.Y));

            Vector3 acceleration = Vector3.Zero;
            if (KeyboardManager.IsKeyDown(Key.W))
                acceleration += ViewDir;
            
            if (KeyboardManager.IsKeyDown(Key.S))
                acceleration -= ViewDir;
            
            if (KeyboardManager.IsKeyDown(Key.D))
                acceleration += Vector3.Cross(ViewDir, Up).Normalized();

            if (KeyboardManager.IsKeyDown(Key.A))
                acceleration -= Vector3.Cross(ViewDir, Up).Normalized();

            
            Velocity += KeyboardManager.IsKeyDown(Key.LShift) ? acceleration * 5 : (KeyboardManager.IsKeyDown(Key.LControl) ? acceleration * 0.35f : acceleration);
            if (Vector3.Dot(Velocity, Velocity) < 0.01f)
                Velocity = Vector3.Zero;
            Position += Velocity * dT;
            Velocity *= 0.95f;
            View = GenerateMatrix(Position, ViewDir, Up);
        }

        public static Matrix4 GenerateMatrix(Vector3 position, Vector3 viewDir, Vector3 up) => Matrix4.LookAt(position, position + viewDir, up);
    }
}
