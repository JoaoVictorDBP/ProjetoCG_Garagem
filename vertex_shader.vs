#version 330 core
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 texture_coord;
layout (location = 2) in vec3 normal;

out vec2 out_texture;
out vec3 FragPos;
out vec3 Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    vec4 worldPosition = model * vec4(position, 1.0);
    FragPos = vec3(worldPosition);
    
    // Correcão matemática da normal usando a transposta da inversa da matriz model
    Normal = mat3(transpose(inverse(model))) * normal;
    
    out_texture = texture_coord;
    gl_Position = projection * view * worldPosition;
}