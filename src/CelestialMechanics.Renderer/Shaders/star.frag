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
float fbm(vec3 p) { float a = 0.5; float s = 0.0; for (int i = 0; i < 4; i++) { s += a * valueNoise3(p); p = p * 2.07 + vec3(3.1, 5.7, 1.9); a *= 0.5; } return s; }vec3 blackbodyColor(float tempK) { float t = clamp(tempK, 1000.0, 50000.0); vec3 color; if (t < 3500.0) { float f = (t - 1000.0) / 2500.0; color = mix(vec3(1.0, 0.10, 0.0), vec3(1.0, 0.55, 0.1), f); } else if (t < 6500.0) { float f = (t - 3500.0) / 3000.0; color = mix(vec3(1.0, 0.55, 0.1), vec3(1.0, 0.95, 0.9), f); } else if (t < 15000.0) { float f = (t - 6500.0) / 8500.0; color = mix(vec3(1.0, 0.95, 0.9), vec3(0.7, 0.8, 1.0), f); } else { float f = clamp((t - 15000.0) / 35000.0, 0.0, 1.0); color = mix(vec3(0.7, 0.8, 1.0), vec3(0.4, 0.5, 1.0), f); } return color; }
vec3 shadeExplosion(vec3 norm, vec3 localN, vec3 viewDir, float luminosity) { float edge = 1.0 - abs(dot(norm, viewDir)); float coreMask = smoothstep(0.4, 0.0, length(localN) * 0.8); float ringRadius = 0.6 + 0.15 * sin(uTime * 2.5); float ring = exp(-pow((length(localN.xz) - ringRadius) / 0.08, 2.0)); ring *= (1.0 - abs(localN.y) * 2.0); float debris = fbm(localN * 15.0 + vec3(uTime * 0.3)); float debrisMask = smoothstep(0.4, 0.75, debris) * edge; vec3 coreColor = vec3(10.0, 9.5, 8.5); vec3 ringColor = vec3(8.0, 5.0, 2.0); vec3 debrisColor = vec3(4.0, 2.0, 0.8); vec3 result = coreColor * coreMask + ringColor * ring * 0.6 + debrisColor * debrisMask * 0.3; result *= max(luminosity, 3.0); result += vec3(1.0, 0.85, 0.65) * pow(edge, 1.2) * luminosity * 2.0; return result; }
vec3 applyBodyTexture(vec3 baseColor, vec3 n, float visualType) { float gran = valueNoise3(n * 9.0 + vec3(0.0, uTime * 0.07, 0.0)); vec3 warm = vec3(0.78, 0.86, 1.0); vec3 hot = vec3(0.98, 0.99, 1.0); return mix(warm, hot, gran); }

void main() {
    float visualType = vVisual.x; float luminosity = vVisual.y; vec3 norm = normalize(vNormal); vec3 localN = normalize(vLocalNormal); vec3 viewDir = normalize(uViewPos - vFragPos);
    if(visualType >= 9.5) {
        vec3 explosionColor = shadeExplosion(norm, localN, viewDir, luminosity);
        if (uEnableHdr != 0) { vec3 hdrColor = explosionColor * max(uExposure, 0.01); explosionColor = toneMapAces(hdrColor); } else { explosionColor = clamp(explosionColor, 0.0, 1.0); }
        explosionColor = pow(explosionColor, vec3(1.0 / 2.2));
        FragColor = vec4(explosionColor, clamp(vColor.a, 0.0, 1.0));
        return;
    }
    vec3 albedo = applyBodyTexture(vColor.rgb, localN, visualType);
    if (vStarTemperatureK > 0.0) { vec3 tempColor = blackbodyColor(vStarTemperatureK); albedo = mix(albedo, tempColor, 0.78); }
    float starPulse = 0.92 + 0.08 * sin(uTime * 1.7 + localN.y * 8.0);
    float emissive = luminosity * starPulse;
    if (vStarTemperatureK > 0.0) { float tempBoost = clamp((vStarTemperatureK - 2600.0) / 18000.0, 0.0, 1.0); emissive *= (0.92 + 0.24 * tempBoost); }
    vec3 emissiveGlow = albedo * emissive * 0.15;
    vec3 result = emissive * uGlobalLuminosity * albedo + emissiveGlow;
    if (uEnableGlowScaling != 0) { float distanceToCamera = length(uViewPos - vFragPos); float distanceFactor = clamp(distanceToCamera / max(uGlowDistanceScale, 0.001), 0.0, 1.0); vec3 glowColor = albedo * emissive * distanceFactor * 5.0; result = mix(result, glowColor, distanceFactor); }
    float rim = pow(1.0 - max(dot(viewDir, norm), 0.0), 2.8);
    float bloom = pow(rim, 1.24) * 1.08;
    vec3 bloomTint = vec3(0.72, 0.84, 1.0);
    result += bloomTint * bloom * luminosity * uGlobalGlow;
    float luma = dot(result, vec3(0.2126, 0.7152, 0.0722)); result = mix(vec3(luma), result, clamp(uGlobalSaturation, 0.0, 2.0));
    if (uEnableHdr != 0) { vec3 hdrColor = result * max(uExposure, 0.01); result = vec3(1.0) - exp(-hdrColor); } else { result = clamp(result, 0.0, 1.0); }
    result = pow(result, vec3(1.0 / 2.2));
    FragColor = vec4(result, clamp(vColor.a, 0.0, 1.0));
}
