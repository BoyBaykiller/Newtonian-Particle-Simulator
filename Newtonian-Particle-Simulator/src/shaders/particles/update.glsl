#version 430 core
#define EPSILON 0.001

layout(local_size_x = 128, local_size_y = 1, local_size_z = 1) in;

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

    vec3 position = ssbo.particles[gl_GlobalInvocationID.x].Postion;
    vec3 toMass = pointOfMass - position;
    float distSquared = max(dot(toMass, toMass), EPSILON);

    vec3 velocity = ssbo.particles[gl_GlobalInvocationID.x].Velocity * 0.9945;
    vec3 newVelocity = velocity + isActive / distSquared * toMass;

    ssbo.particles[gl_GlobalInvocationID.x] = Particle(position + dT * newVelocity, newVelocity);
}