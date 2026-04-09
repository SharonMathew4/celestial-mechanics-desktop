#version 330 core

in float vBrightness;
in vec3 vStarColor;

out vec4 FragColor;

void main()
{
    // Point sprite: circular soft dot
    vec2 coord = gl_PointCoord * 2.0 - 1.0;
    float dist = dot(coord, coord);
    if (dist > 1.0) discard;

    // Soft falloff for a natural star appearance
    float alpha = (1.0 - dist) * vBrightness;

    // Twinkle: slight core brightening
    float core = exp(-dist * 4.0);
    vec3 color = vStarColor * (0.6 + 0.4 * core);

    FragColor = vec4(color, alpha);
}
