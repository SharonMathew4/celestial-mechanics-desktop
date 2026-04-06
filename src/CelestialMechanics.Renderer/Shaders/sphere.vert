#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
// Per-instance (locations 2-5 for mat4, 6 for color, 7 for bodyType)
layout(location = 2) in mat4 instanceModel;
layout(location = 6) in vec4 instanceColor;
layout(location = 7) in float instanceBodyType;

uniform mat4 uView;
uniform mat4 uProjection;
uniform float uTime;

out vec3 vNormal;
out vec3 vFragPos;
out vec3 vLocalPos;
out vec4 vColor;
flat out int vBodyType;
flat out int vInstanceId;
out float vTime;

void main()
{
    vec4 worldPos = instanceModel * vec4(aPosition, 1.0);
    gl_Position = uProjection * uView * worldPos;
    vNormal = mat3(transpose(inverse(instanceModel))) * aNormal;
    vFragPos = worldPos.xyz;
    vLocalPos = aPosition;
    vColor = instanceColor;
    vBodyType = int(instanceBodyType + 0.5);
    vInstanceId = gl_InstanceID;
    vTime = uTime;
}
