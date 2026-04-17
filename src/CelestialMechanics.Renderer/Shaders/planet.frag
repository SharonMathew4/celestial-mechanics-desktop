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
float fbm(vec3 p) { float a = 0.5; float s = 0.0; for (int i = 0; i < 4; i++) { s += a * valueNoise3(p); p = p * 2.07 + vec3(3.1, 5.7, 1.9); a *= 0.5; } return s; }

vec2 sphereUv(vec3 n) { vec3 nn = normalize(n); float u = atan(nn.z, nn.x) * 0.15915494309 + 0.5; float v = asin(clamp(nn.y, -1.0, 1.0)) * 0.31830988618 + 0.5; return vec2(u, v); }
vec3 applyBodyTexture(vec3 baseColor, vec3 n, float visualType) {
    if (visualType >= 1.0 && visualType < 1.5) { float continents = fbm(n * 6.5); float clouds = fbm(n * 18.0 + vec3(uTime * 0.01)); vec3 ocean = baseColor * vec3(0.55, 0.75, 1.0); vec3 land = vec3(0.22, 0.45, 0.18); vec3 c = mix(ocean, land, smoothstep(0.45, 0.62, continents)); c = mix(c, vec3(0.95), smoothstep(0.72, 0.86, clouds) * 0.35); return c; }
    else if (visualType >= 1.5 && visualType < 2.5) { float lat = n.y * 0.5 + 0.5; float bands = sin((lat * 18.0 + fbm(n * 8.0) * 2.0) * 3.14159); float storms = fbm(vec3(n.xz * 15.0, n.y * 5.0 + uTime * 0.03)); vec3 c1 = vec3(0.90, 0.75, 0.55); vec3 c2 = vec3(0.65, 0.45, 0.30); vec3 c = mix(c1, c2, bands * 0.5 + 0.5); c *= 0.9 + 0.2 * storms; return c; }
    else if (visualType >= 2.5 && visualType < 3.5) { float craters = fbm(n * 22.0); vec3 rock = vec3(0.62, 0.62, 0.60); return rock * (0.75 + 0.4 * craters); }
    else if (visualType >= 3.5 && visualType < 4.5) { float rough = fbm(n * 20.0); vec3 rock = vec3(0.52, 0.48, 0.42); return rock * (0.68 + 0.45 * rough); }
    else if (visualType >= 4.5 && visualType < 5.5) { float pulse = 0.5 + 0.5 * sin(uTime * 2.2); return mix(vec3(0.55, 0.85, 1.0), vec3(0.9, 0.98, 1.0), pulse * 0.45); }
    else if (visualType >= 5.5 && visualType < 6.5) { float ring = smoothstep(0.1, 0.75, 1.0 - abs(n.z)); vec3 core = vec3(0.02, 0.02, 0.03); vec3 acc = vec3(0.80, 0.55, 1.0); return mix(core, acc, ring * 0.22); }
    return baseColor;
}
float traceShadowRay(vec3 origin, vec3 lightPos, vec3 selfCenter, float selfRadius) { if (uRayTraceShadows == 0 || uRayOccluderCount == 0) return 1.0; vec3 toLight = lightPos - origin; float maxDist = length(toLight); if (maxDist < 1e-5) return 1.0; vec3 dir = toLight / maxDist; float visibility = 1.0; float softness = clamp(uRayShadowSoftness, 0.0005, 0.20); for (int i = 0; i < 24; i++) { if (i >= uRayOccluderCount) break; vec3 center = uRayOccluders[i]; float radius = uRayOccluderRadius[i]; if (distance(center, selfCenter) < max(0.01, selfRadius * 0.55)) continue; vec3 oc = origin - center; float b = dot(oc, dir); float c = dot(oc, oc) - radius * radius; float h = b * b - c; if (h <= 0.0) continue; float t = -b - sqrt(h); if (t <= softness || t >= maxDist) continue; float blockerBias = 1.0 - smoothstep(0.0, maxDist, t); float hit = clamp(uRayShadowStrength * (0.55 + 0.45 * blockerBias), 0.0, 1.0); visibility *= (1.0 - hit); if (visibility <= 0.01) { visibility = 0.0; break; } } return clamp(visibility, 0.05, 1.0); }
vec3 sampleScreenReflections(vec3 viewDir, vec3 normal) { if (uEnableReflections == 0) return vec3(0.0); vec2 safeResolution = max(uResolution, vec2(1.0)); vec2 uv = gl_FragCoord.xy / safeResolution; vec3 reflected = reflect(-viewDir, normal); int samples = clamp(uMaxReflectionSamples, 1, 16); vec3 accum = vec3(0.0); float wsum = 0.0; for (int i = 0; i < 16; i++) { if (i >= samples) break; float t = float(i + 1) / float(samples); vec2 suv = uv + reflected.xy * uReflectionScale * t; if (suv.x <= 0.0 || suv.x >= 1.0 || suv.y <= 0.0 || suv.y >= 1.0) break; float sceneDepth = texture(uDepthTexture, suv).r; float depthWeight = smoothstep(0.05, 1.0, sceneDepth); float weight = (1.0 - 0.6 * t) * depthWeight; accum += texture(uScreenTexture, suv).rgb * weight; wsum += weight; } if (wsum <= 1e-5) return vec3(0.0); return accum / wsum; }

void main() {
    vec3 norm = normalize(vNormal); vec3 localN = normalize(vLocalNormal); vec3 viewDir = normalize(uViewPos - vFragPos);
    float visualType = vVisual.x; float luminosity = vVisual.y; float glowStrength = vVisual.z; float atmosphere = vVisual.w;
    float diff = 0.0; vec3 directDiffuse = vec3(0.0); vec3 directSpec = vec3(0.0);
    if (uEnableStarLighting != 0 && uStarLightCount > 0) {
        for (int i = 0; i < 8; i++) {
            if (i >= uStarLightCount) break;
            vec3 lightVec = uStarLights[i] - vFragPos; float dist = length(lightVec); if (dist < 1e-5) continue;
            vec3 lightDir = lightVec / dist; float distanceSq = dist * dist + 0.001; float attenuation = 1.0 / distanceSq;
            float intensity = max(uStarLightIntensity[i], 0.0); vec3 radiance = uStarLightColor[i] * intensity * attenuation * 50.0;
            if (dot(radiance, vec3(0.333)) < 0.005) continue;
            vec3 shadowOrigin = vFragPos + norm * (0.002 + 0.02 * vBodyRadius); float shadow = traceShadowRay(shadowOrigin, uStarLights[i], vBodyCenter, max(vBodyRadius, 0.01));
            float lambert = max(dot(norm, lightDir), 0.0); vec3 diffuse = radiance * lambert * shadow;
            directDiffuse += diffuse; diff += dot(diffuse, vec3(0.2126, 0.7152, 0.0722));
            vec3 halfDir = normalize(lightDir + viewDir); float specTerm = pow(max(dot(norm, halfDir), 0.0), 64.0); directSpec += radiance * specTerm * 0.28 * shadow;
        }
    }
    vec3 albedo = applyBodyTexture(vColor.rgb, localN, visualType);
    if (uUseAlbedoAtlas != 0 && visualType >= 1.0 && visualType < 6.5) {
        float layer = clamp(vTextureLayer, 0.0, 7.0); vec3 texAlbedo = texture(uBodyAlbedoAtlas, vec3(sphereUv(localN), layer)).rgb;
        albedo = mix(albedo, texAlbedo, clamp(uAlbedoBlend, 0.0, 1.0));
    }
    float emissive = luminosity;
    float rim = pow(1.0 - max(dot(viewDir, norm), 0.0), 2.8);
    vec3 rimColor = mix(albedo * 0.55, vec3(0.9, 0.95, 1.0), 0.35);
    vec3 lit = directDiffuse * albedo + directSpec;
    vec3 glow = rimColor * (rim * glowStrength + atmosphere * rim * 0.45) * uGlobalGlow;
    vec3 emissiveGlow = albedo * emissive * 0.15;
    vec3 result = lit + emissive * uGlobalLuminosity * albedo + emissiveGlow + glow;
    
    if (uEnableGlowScaling != 0) { float distanceToCamera = length(uViewPos - vFragPos); float distanceFactor = clamp(distanceToCamera / max(uGlowDistanceScale, 0.001), 0.0, 1.0); vec3 glowColor = albedo * emissive * distanceFactor * 5.0; result = mix(result, glowColor, distanceFactor); }
    
    vec3 reflectionColor = sampleScreenReflections(viewDir, norm);
    float reflectivity = clamp(0.05 + glowStrength * 0.25, 0.02, 0.7);
    float fresnel = pow(1.0 - max(dot(viewDir, norm), 0.0), 5.0);
    result = mix(result, reflectionColor, reflectivity) + reflectionColor * fresnel;
    
    float bloom = 0.0;
    if (visualType >= 5.0 && visualType < 6.5) bloom = pow(rim, 1.05) * 1.35;
    else if (luminosity > 0.7) bloom = pow(rim, 1.45) * 0.75;
    vec3 bloomTint = (visualType >= 5.0 && visualType < 6.5) ? vec3(0.66, 0.80, 1.0) : vec3(0.72, 0.84, 1.0);
    result += bloomTint * bloom * luminosity * uGlobalGlow;

    float luma = dot(result, vec3(0.2126, 0.7152, 0.0722)); result = mix(vec3(luma), result, clamp(uGlobalSaturation, 0.0, 2.0));
    if (uEnableHdr != 0) { vec3 hdrColor = result * max(uExposure, 0.01); result = vec3(1.0) - exp(-hdrColor); } else { result = clamp(result, 0.0, 1.0); }
    result = pow(result, vec3(1.0 / 2.2));
    FragColor = vec4(result, clamp(vColor.a, 0.0, 1.0));
}
