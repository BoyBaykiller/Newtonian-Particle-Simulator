using OpenTK;

namespace Newtonian_Particle_Simulator
{
    struct Particle
    {
        public Vector3 Position;
        private readonly int _pad0;
        public Vector3 Velocity;
        private readonly int _pad1;
    }
}
