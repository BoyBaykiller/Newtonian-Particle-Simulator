#version 430 core
layout(location = 0) out vec4 FragColor;

layout(location = 0) in struct
{
    vec4 Color;
} inData;

void main()
{
    FragColor = vec4(inData.Color);
}