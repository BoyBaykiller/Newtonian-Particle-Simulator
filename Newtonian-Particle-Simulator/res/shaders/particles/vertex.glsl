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
    Particle particles[];
} ssbo;

layout(location = 0) uniform float dT;
layout(location = 1) uniform vec3 pointOfMass;
layout(location = 2) uniform float isActive;
layout(location = 3) uniform float isRunning;
layout(location = 4) uniform mat4 projViewMatrix;

layout(location = 0) out struct
{
    vec4 Color;
} outData;

void main()
{
    Particle particle = ssbo.particles[gl_VertexID];

    vec3 toMass = pointOfMass - particle.Position;
    float dist = max(length(toMass), EPSILON);
    
    vec3 acceleration = 176.0 * isRunning * isActive / dist * (toMass / dist);
    particle.Velocity *= mix(1.0, exp(DRAG_COEF * dT), isRunning); // https://stackoverflow.com/questions/61812575/which-formula-to-use-for-drag-simulation-each-frame
    particle.Velocity += acceleration * dT;
    particle.Position += (dT * particle.Velocity + 0.5 * acceleration * dT * dT) * isRunning;
    ssbo.particles[gl_VertexID] = particle;


    float red = 0.0045 * dot(particle.Velocity, particle.Velocity);
    float green = clamp(0.08 * max(particle.Velocity.x, max(particle.Velocity.y, particle.Velocity.z)), 0.2, 0.5);
    float blue = 0.7 - red;

    outData.Color = vec4(red, green, blue, 0.25);
    gl_Position = projViewMatrix * vec4(particle.Position, 1.0);
}