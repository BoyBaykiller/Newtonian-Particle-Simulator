using System;

namespace ParticleSimulation
{
    public class ParticleSimulator
    {
        private int particleCount;

        public int ParticleCount
        {
            get { return particleCount; }
            private set { particleCount = value; }
        }

        public ParticleSimulator(int initialParticleCount)
        {
            if (initialParticleCount < 0)
            {
                throw new ArgumentException("Initial particle count cannot be negative.");
            }
            ParticleCount = initialParticleCount;
        }

        public void AddParticles(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("Cannot add a negative number of particles.");
            }
            ParticleCount += count;
        }

        public void RemoveParticles(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("Cannot remove a negative number of particles.");
            }
            if (count > ParticleCount)
            {
                throw new InvalidOperationException("Cannot remove more particles than currently exist.");
            }
            ParticleCount -= count;
        }

        public void RestartSimulation(int newParticleCount)
        {
            if (newParticleCount < 0)
            {
                throw new ArgumentException("New particle count cannot be negative.");
            }
            ParticleCount = newParticleCount;
        }
    }
}