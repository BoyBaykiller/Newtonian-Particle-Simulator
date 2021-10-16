#version 430 core
#define EPSILON 0.001

layout(local_size_x = 64, local_size_y = 1, local_size_z = 1) in;

struct Particle
{
    vec3 Postion;
    vec3 Velocity;
};

layout(std430, binding = 0) restrict buffer ParticlesSSBO
{
    Particle[] particles;
} ssbo;

uniform int numParticles;
layout(location = 0) uniform float dT;
layout(location = 1) uniform vec3 pointOfMass;
layout(location = 2) uniform float isActive;

void main()
{
    if (gl_GlobalInvocationID.x > numParticles)
        return;

    vec3 toMass = pointOfMass - ssbo.particles[gl_GlobalInvocationID.x].Postion;
    float distSquared = max(dot(toMass, toMass), EPSILON);

    ssbo.particles[gl_GlobalInvocationID.x].Velocity *= 0.9945;
    ssbo.particles[gl_GlobalInvocationID.x].Velocity += isActive / distSquared * toMass;
    ssbo.particles[gl_GlobalInvocationID.x].Postion += dT * ssbo.particles[gl_GlobalInvocationID.x].Velocity;
}