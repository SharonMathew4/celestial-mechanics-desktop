#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in float aBrightness;
layout(location = 2) in vec3 aStarColor;

uniform mat4 uView;
uniform mat4 uProjection;

out float vBrightness;
out vec3 vStarColor;

void main()
{
    // Remove translation from view matrix so stars stay at infinity
    mat4 viewNoTranslation = mat4(mat3(uView));
    gl_Position = uProjection * viewNoTranslation * vec4(aPosition, 1.0);
    gl_PointSize = mix(1.0, 3.5, aBrightness);
    vBrightness = aBrightness;
    vStarColor = aStarColor;
}
