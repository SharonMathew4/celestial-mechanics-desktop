#version 330 core
in vec3 vNormal;
in vec3 vFragPos;
in vec4 vColor;
in vec4 vVisual;
in vec3 vLocalNormal;
in vec3 vBodyCenter;
in float vBodyRadius;
in float vTextureLayer;
in float vStarTemperatureK;

uniform vec3 uViewPos;
uniform float uTime;
uniform float uGlobalLuminosity;
uniform float uGlobalGlow;
uniform float uGlobalSaturation;
uniform int uUseAlbedoAtlas;
uniform sampler2DArray uBodyAlbedoAtlas;
uniform sampler2D uScreenTexture;
uniform sampler2D uDepthTexture;
uniform float uAlbedoBlend;
uniform int uEnableStarLighting;
uniform int uStarLightCount;
uniform vec3 uStarLights[8];
uniform vec3 uStarLightColor[8];
uniform float uStarLightIntensity[8];
uniform float uStarLightFalloff;
uniform float uAmbientFloor;
uniform int uRayTraceShadows;
uniform int uRayOccluderCount;
uniform vec3 uRayOccluders[24];
uniform float uRayOccluderRadius[24];
uniform float uRayShadowStrength;
uniform float uRayShadowSoftness;
uniform int uEnableHdr;
uniform float uExposure;
uniform int uEnableReflections;
uniform float uReflectionScale;
uniform int uMaxReflectionSamples;
uniform vec2 uResolution;
uniform int uEnableGlowScaling;
uniform float uGlowDistanceScale;
out vec4 FragColor;

vec3 toneMapAces(vec3 x) { const float a = 2.51; const float b = 0.03; const float c = 2.43; const float d = 0.59; const float e = 0.14; return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0); }
float hash31(vec3 p) { p = fract(p * 0.1031); p += dot(p, p.yzx + 33.33); return fract((p.x + p.y) * p.z); }
float valueNoise3(vec3 p) { vec3 i = floor(p); vec3 f = fract(p); f = f * f * (3.0 - 2.0 * f); float n000 = hash31(i + vec3(0,0,0)); float n100 = hash31(i + vec3(1,0,0)); float n010 = hash31(i + vec3(0,1,0)); float n110 = hash31(i + vec3(1,1,0)); float n001 = hash31(i + vec3(0,0,1)); float n101 = hash31(i + vec3(1,0,1)); float n011 = hash31(i + vec3(0,1,1)); float n111 = hash31(i + vec3(1,1,1)); float n00 = mix(n000, n100, f.x); float n10 = mix(n010, n110, f.x); float n01 = mix(n001, n101, f.x); float n11 = mix(n011, n111, f.x); float n0 = mix(n00, n10, f.y); float n1 = mix(n01, n11, f.y); return mix(n0, n1, f.z); }
float fbm(vec3 p) { float a = 0.5; float s = 0.0; for (int i = 0; i < 4; i++) { s += a * valueNoise3(p); p = p * 2.07 + vec3(3.1, 5.7, 1.9); a *= 0.5; } return s; }vec3 shadeNebula(vec3 norm, vec3 localN, vec3 viewDir) {
    float density = fbm(localN * 4.0 + vec3(uTime * 0.008));
    float density2 = fbm(localN * 7.0 + vec3(-3.0, 2.0, 1.0) + uTime * 0.005);
    float shape = smoothstep(0.32, 0.78, density); float detail = smoothstep(0.45, 0.88, density2) * 0.6;
    vec3 nebColor1 = vec3(0.18, 0.08, 0.22); vec3 nebColor2 = vec3(0.22, 0.12, 0.08); vec3 nebColor3 = vec3(0.08, 0.12, 0.20);
    vec3 color = mix(nebColor1, nebColor2, density); color = mix(color, nebColor3, detail);
    float opacity = (shape + detail * 0.5) * 0.06;
    float edgeFade = 1.0 - smoothstep(0.6, 1.0, length(localN));
    return color * opacity * edgeFade;
}
void main() {
    vec3 norm = normalize(vNormal); vec3 localN = normalize(vLocalNormal); vec3 viewDir = normalize(uViewPos - vFragPos);
    vec3 nebulaColor = shadeNebula(norm, localN, viewDir);
    if (uEnableHdr != 0) { vec3 hdrColor = nebulaColor * max(uExposure, 0.01); nebulaColor = vec3(1.0) - exp(-hdrColor); }
    nebulaColor = pow(nebulaColor, vec3(1.0 / 2.2));
    float nebulaAlpha = clamp(length(nebulaColor) * 3.0, 0.0, 0.3);
    FragColor = vec4(nebulaColor, nebulaAlpha);
}
