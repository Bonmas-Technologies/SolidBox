#version 330 core

layout (location = 0) in vec3 aPosition;

uniform mat4 local;
uniform mat4 world;
uniform mat4 view;

void main()
{
    gl_Position = view * world * local * vec4(aPosition, 1.0);
}
