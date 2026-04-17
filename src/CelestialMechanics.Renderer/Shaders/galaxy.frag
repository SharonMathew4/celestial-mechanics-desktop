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
float fbm(vec3 p) { float a = 0.5; float s = 0.0; for (int i = 0; i < 4; i++) { s += a * valueNoise3(p); p = p * 2.07 + vec3(3.1, 5.7, 1.9); a *= 0.5; } return s; }vec3 shadeGalaxy(vec3 norm, vec3 localN, vec3 viewDir) {
    float diskThickness = exp(-pow(localN.y / 0.12, 2.0));
    if (diskThickness < 0.01) return vec3(0.0);
    float angle = atan(localN.z, localN.x); float radius = length(localN.xz); float spiralFactor = 3.0;
    float spiral = 0.5 + 0.5 * sin(angle * spiralFactor + radius * 8.0 + uTime * 0.05);
    float armNoise = valueNoise3(localN * 12.0 + vec3(uTime * 0.01));
    spiral = spiral * (0.7 + 0.3 * armNoise);
    float coreBulge = exp(-pow(radius / 0.15, 2.0));
    float starDensity = spiral * diskThickness * (0.3 + 0.7 * smoothstep(0.05, 0.8, radius));
    vec3 coreColor = vec3(0.92, 0.88, 0.72) * 0.12; vec3 armColor = vec3(0.65, 0.70, 0.82) * 0.06;
    vec3 result = mix(armColor, coreColor, coreBulge) * (starDensity + coreBulge * 0.8);
    return result * 0.08;
}
void main() {
    vec3 norm = normalize(vNormal); vec3 localN = normalize(vLocalNormal); vec3 viewDir = normalize(uViewPos - vFragPos);
    vec3 galaxyColor = shadeGalaxy(norm, localN, viewDir);
    if (uEnableHdr != 0) { vec3 hdrColor = galaxyColor * max(uExposure, 0.01); galaxyColor = vec3(1.0) - exp(-hdrColor); }
    galaxyColor = pow(galaxyColor, vec3(1.0 / 2.2));
    FragColor = vec4(galaxyColor, clamp(vColor.a, 0.0, 1.0));
}
