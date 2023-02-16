#version 430 core
#define EPSILON 0.001
const float DRAG_COEF = log(0.998) * 176.0; // log(0.70303228048)

struct Particle
{
    vec3 Position;
    vec3 Velocity;
};

layout(std430, binding = 0) restrict buffer ParticlesSSBO
{
    Particle Particles[];
} particleSSBO;

layout(location = 0) uniform float dT;
layout(location = 1) uniform vec3 pointOfMass;
layout(location = 2) uniform float isActive;
layout(location = 3) uniform float isRunning;
layout(location = 4) uniform mat4 projViewMatrix;

out InOutVars
{
    vec4 Color;
} outData;

void main()
{
    Particle particle = particleSSBO.Particles[gl_VertexID];

    const vec3 toMass = pointOfMass - particle.Position;
    
    /// Implementation of Newton's law of gravity
    const float m1 = 1.0;   // constant particle mass
    const float m2 = 176.0; // (user controlled) pointOfMass mass
    const float G  = 1.0;   // gravitational constant 
    const float m1_m2 = m1 * m2; // mass of both objects multiplied
    const float rSquared = max(dot(toMass, toMass), EPSILON * EPSILON); // distance between objects squared
    // Technically toMass would have to be normalized but feels better without
    const vec3 force = toMass * (G * ((m1_m2) / rSquared));
    
    // acceleration = force / m. 
    const vec3 acceleration = (force * isRunning * isActive) / m1;

    particle.Velocity *= mix(1.0, exp(DRAG_COEF * dT), isRunning); // https://stackoverflow.com/questions/61812575/which-formula-to-use-for-drag-simulation-each-frame
    particle.Position += (dT * particle.Velocity + 0.5 * acceleration * dT * dT) * isRunning; // Euler integration
    particle.Velocity += acceleration * dT;
    particleSSBO.Particles[gl_VertexID] = particle;

    const float red = 0.0045 * dot(particle.Velocity, particle.Velocity);
    const float green = clamp(0.08 * max(particle.Velocity.x, max(particle.Velocity.y, particle.Velocity.z)), 0.2, 0.5);
    const float blue = 0.7 - red;

    outData.Color = vec4(red, green, blue, 0.25);
    gl_Position = projViewMatrix * vec4(particle.Position, 1.0);
}