#version 330 core

uniform sampler2D buffer;

in vec2 UV;
out vec4 out_color;

void main()
{
    out_color = texture(buffer, UV);
}
