#version 330 core

uniform mat4 local;
uniform mat4 world;
uniform mat4 view;

out vec2 uv;

void main()
{
	float x = mod(gl_VertexID, 2.);
	float y = floor(gl_VertexID / 2.);

	uv = vec2(x, y);

	gl_Position = view * world * local * vec4(x -.5, y -.5, 0., 1.);
}
