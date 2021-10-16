#version 430 core

struct Particle
{
    vec3 Postion;
    vec3 Velocity;
};

layout(std430, binding = 0) readonly restrict buffer ParticlesSSBO
{
    Particle[] particles;
} ssbo;

layout(location = 0) uniform mat4 projViewMatrix;

out vec4 Color;
void main()
{
    Particle particle = ssbo.particles[gl_VertexID];

    float red = mix(0.0, 1.0, 0.0169 * dot(particle.Velocity, particle.Velocity));
    float green = clamp(0.1 * max(particle.Velocity.x, max(particle.Velocity.y, particle.Velocity.z)), 0.2, 0.7);
    float blue = 0.7 - red;


    Color = vec4(red, green, blue, 0.25);
    gl_Position = projViewMatrix * vec4(particle.Postion, 1.0);
    gl_PointSize = 1.1;
}