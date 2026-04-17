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
uniform int uEnableHdr;
uniform float uExposure;
uniform vec2 uResolution;
uniform int uBhQualityTier;
uniform int uBhPreset;
uniform float uBhRingThickness;
uniform float uBhLensStrength;
uniform float uBhDopplerBoost;
uniform float uBhOpticalDepth;
uniform float uBhTemperatureScale;
uniform float uBhBloomScale;
uniform int uBhDebugMode;
uniform float uBhParticleHeat;
uniform float uBhParticleDensity;

out vec4 FragColor;
vec3 toneMapAces(vec3 x) { const float a = 2.51; const float b = 0.03; const float c = 2.43; const float d = 0.59; const float e = 0.14; return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0); }
vec3 blackbodyColor(float tempK) { float t = clamp(tempK, 1000.0, 50000.0); vec3 color; if (t < 3500.0) { float f = (t - 1000.0) / 2500.0; color = mix(vec3(1.0, 0.10, 0.0), vec3(1.0, 0.55, 0.1), f); } else if (t < 6500.0) { float f = (t - 3500.0) / 3000.0; color = mix(vec3(1.0, 0.55, 0.1), vec3(1.0, 0.95, 0.9), f); } else if (t < 15000.0) { float f = (t - 6500.0) / 8500.0; color = mix(vec3(1.0, 0.95, 0.9), vec3(0.7, 0.8, 1.0), f); } else { float f = clamp((t - 15000.0) / 35000.0, 0.0, 1.0); color = mix(vec3(0.7, 0.8, 1.0), vec3(0.4, 0.5, 1.0), f); } return color; }vec3 shadeBlackHole(vec3 norm, vec3 viewDir, vec3 localN, out float horizonMask, out float ringMask, out float warpMask, out float opticalDepthMask) {
    float edge = 1.0 - abs(dot(norm, viewDir)); float equator = 1.0 - abs(localN.y); float equatorBand = exp(-pow(localN.y / 0.14, 2.0));
    horizonMask = smoothstep(0.18, 0.92, edge);
    float ringCenter = 0.68 + 0.08 * clamp(uBhLensStrength, 0.0, 2.5); float ringWidth = max(0.03, uBhRingThickness * 0.16);
    ringMask = exp(-pow((edge - ringCenter) / ringWidth, 2.0)) * (0.4 + 0.6 * equator);
    float arcSep = (uBhPreset == 2) ? 0.24 : ((uBhPreset == 1) ? 0.16 : 0.20); float arcWidth = (uBhQualityTier == 0) ? 0.24 : ((uBhQualityTier == 1) ? 0.18 : 0.13);
    float upperArc = exp(-pow((localN.y - arcSep) / arcWidth, 2.0)); float lowerArc = exp(-pow((localN.y + arcSep) / arcWidth, 2.0));
    warpMask = clamp((upperArc + lowerArc) * edge * uBhLensStrength, 0.0, 1.0);
    vec3 tangent = normalize(vec3(-localN.z, 0.0, localN.x)); float beta = dot(tangent, viewDir) * 0.32 * uBhDopplerBoost; float gFactor = clamp(1.0 + beta, 0.25, 2.8);
    float presetTemp = (uBhPreset == 0) ? 7600.0 : ((uBhPreset == 1) ? 6200.0 : 9100.0); float particleGain = 0.8 + 0.5 * clamp(uBhParticleHeat, 0.0, 1.0);
    float temp = presetTemp * uBhTemperatureScale * particleGain * (0.62 + 0.85 * ringMask + 0.55 * warpMask + 0.65 * equatorBand);
    opticalDepthMask = clamp(uBhOpticalDepth * (0.55 + 0.45 * equator) * (0.8 + 0.7 * uBhParticleDensity), 0.0, 4.0); float transmittance = exp(-opticalDepthMask);
    vec3 source = blackbodyColor(temp) * pow(gFactor, 3.0); vec3 ringGlow = source * (ringMask * 0.95 + warpMask * 0.55 + equatorBand * 0.85); vec3 scattered = source * (1.0 - transmittance) * (0.55 + 0.45 * ringMask);
    vec3 core = vec3(0.0, 0.0, 0.0); float shadow = 1.0 - horizonMask; vec3 result = mix(ringGlow + scattered, core, shadow * 0.98);
    if (uBhQualityTier >= 1) { float secondaryRing = exp(-pow((edge - (ringCenter + 0.12)) / (ringWidth * 1.4), 2.0)); result += source * secondaryRing * 0.20; }
    if (uBhQualityTier >= 2) { float halo = pow(edge, 1.4) * (0.2 + 0.35 * warpMask); result += source * halo * 0.18; }
    result += source * pow(edge, 1.1) * 0.12 * uBhBloomScale;
    return result;
}

void main() {
    vec3 norm = normalize(vNormal); vec3 localN = normalize(vLocalNormal); vec3 viewDir = normalize(uViewPos - vFragPos);
    float horizonMask; float ringMask; float warpMask; float opticalDepthMask;
    vec3 bhColor = shadeBlackHole(norm, viewDir, localN, horizonMask, ringMask, warpMask, opticalDepthMask);
    if (uBhDebugMode == 1) { FragColor = vec4(vec3(horizonMask), 1.0); return; }
    if (uBhDebugMode == 2) { FragColor = vec4(vec3(ringMask), 1.0); return; }
    if (uBhDebugMode == 3) { FragColor = vec4(warpMask, 1.0 - warpMask, 0.0, 1.0); return; }
    if (uBhDebugMode == 4) { FragColor = vec4(vec3(opticalDepthMask / 4.0), 1.0); return; }
    vec3 result = toneMapAces(bhColor); result = pow(result, vec3(1.0 / 2.2));
    FragColor = vec4(result, clamp(vColor.a, 0.0, 1.0));
}
