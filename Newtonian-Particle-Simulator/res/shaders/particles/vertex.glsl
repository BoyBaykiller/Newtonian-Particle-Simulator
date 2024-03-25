#version 430 core
#define EPSILON 0.001
const float DRAG_COEF = log(0.998) * 176.0; // log(0.70303228048)

struct PackedVector3
{
    float X, Y, Z;
};

struct Particle
{
    PackedVector3 Position;
    PackedVector3 Velocity;
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

vec3 PackedVec3ToVec3(PackedVector3 vec);
PackedVector3 Vec3ToPackedVec3(vec3 vec);

out InOutVars
{
    vec3 Color;
} outData;

void main()
{
    PackedVector3 packedPosition = particleSSBO.Particles[gl_VertexID].Position;
    PackedVector3 packedVelocity = particleSSBO.Particles[gl_VertexID].Velocity;
    vec3 position = PackedVec3ToVec3(packedPosition);
    vec3 velocity = PackedVec3ToVec3(packedVelocity);

    const vec3 toMass = pointOfMass - position;
    
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

    velocity *= mix(1.0, exp(DRAG_COEF * dT), isRunning); // https://stackoverflow.com/questions/61812575/which-formula-to-use-for-drag-simulation-each-frame
    position += (dT * velocity + 0.5 * acceleration * dT * dT) * isRunning; // Euler integration
    velocity += acceleration * dT;

    particleSSBO.Particles[gl_VertexID].Position = Vec3ToPackedVec3(position);
    particleSSBO.Particles[gl_VertexID].Velocity = Vec3ToPackedVec3(velocity);

    const float red = 0.0045 * dot(velocity, velocity);
    const float green = clamp(0.08 * max(velocity.x, max(velocity.y, velocity.z)), 0.2, 0.5);
    const float blue = 0.7 - red;

    outData.Color = vec3(red, green, blue);
    gl_Position = projViewMatrix * vec4(position, 1.0);
}

vec3 PackedVec3ToVec3(PackedVector3 vec)
{
    return vec3(vec.X, vec.Y, vec.Z);
}

PackedVector3 Vec3ToPackedVec3(vec3 vec)
{
    return PackedVector3(vec.x, vec.y, vec.z);
}