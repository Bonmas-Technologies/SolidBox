#version 330 core

uniform sampler2D sprite;

in vec2 uv;
out vec4 out_color;

void main()
{
    out_color = texture2D(sprite, uv);
}
