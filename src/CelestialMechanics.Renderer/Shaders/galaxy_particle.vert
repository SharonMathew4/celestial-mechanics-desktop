#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 aColor;
layout(location = 2) in float aPointSize;
layout(location = 3) in float aVisualType;
layout(location = 4) in float aLuminosity;

uniform mat4 uView;
uniform mat4 uProjection;
uniform float uViewportHeight;
uniform float uBasePointSize;

out vec4 vColor;
out float vVisualType;
out float vLuminosity;
out float vDistanceToCamera;

void main()
{
    vec4 viewPos = uView * vec4(aPosition, 1.0);
    gl_Position = uProjection * viewPos;

    vColor = aColor;
    vVisualType = aVisualType;
    vLuminosity = aLuminosity;
    vDistanceToCamera = -viewPos.z;

    // Point size scales with viewport and distance for billboard rendering.
    // This gives galaxy particles a natural depth cue without geometry overhead.
    float dist = max(-viewPos.z, 0.01);
    float sizeScale = uViewportHeight * aPointSize * uBasePointSize / dist;
    gl_PointSize = clamp(sizeScale, 1.0, 64.0);
}
