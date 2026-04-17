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
uniform int uEnableAtmosphere;
uniform int uEnableNightLights;
uniform int uEnableHighQualityShading;

const float PI = 3.14159265359;

out vec4 FragColor;

vec3 toneMapAces(vec3 x) { const float a = 2.51; const float b = 0.03; const float c = 2.43; const float d = 0.59; const float e = 0.14; return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0); }
float hash31(vec3 p) { p = fract(p * 0.1031); p += dot(p, p.yzx + 33.33); return fract((p.x + p.y) * p.z); }
float valueNoise3(vec3 p) { vec3 i = floor(p); vec3 f = fract(p); f = f * f * (3.0 - 2.0 * f); float n000 = hash31(i + vec3(0,0,0)); float n100 = hash31(i + vec3(1,0,0)); float n010 = hash31(i + vec3(0,1,0)); float n110 = hash31(i + vec3(1,1,0)); float n001 = hash31(i + vec3(0,0,1)); float n101 = hash31(i + vec3(1,0,1)); float n011 = hash31(i + vec3(0,1,1)); float n111 = hash31(i + vec3(1,1,1)); float n00 = mix(n000, n100, f.x); float n10 = mix(n010, n110, f.x); float n01 = mix(n001, n101, f.x); float n11 = mix(n011, n111, f.x); float n0 = mix(n00, n10, f.y); float n1 = mix(n01, n11, f.y); return mix(n0, n1, f.z); }
float fbm(vec3 p) { float a = 0.5; float s = 0.0; for (int i = 0; i < 4; i++) { s += a * valueNoise3(p); p = p * 2.07 + vec3(3.1, 5.7, 1.9); a *= 0.5; } return s; }

vec2 sphereUv(vec3 n) { vec3 nn = normalize(n); float u = atan(nn.z, nn.x) * 0.15915494309 + 0.5; float v = asin(clamp(nn.y, -1.0, 1.0)) * 0.31830988618 + 0.5; return vec2(u, v); }

struct Material {
    vec3 Albedo;
    vec3 Normal;
    float Roughness;
    float Metallic;
    float AO;
    float Emissive;
};

Material getPlanetMaterial(vec3 localN, float visualType, vec3 baseColor, float distToCam) {
    Material mat;
    mat.Albedo = baseColor;
    mat.Normal = localN;
    mat.Roughness = 0.8;
    mat.Metallic = 0.0;
    mat.AO = 1.0;
    mat.Emissive = 0.0;
    
    int octaves = 2; // Far LOD
    if (uEnableHighQualityShading != 0) {
        if (distToCam < 30.0) octaves = 8; // Near 8K
        else if (distToCam < 100.0) octaves = 4; // Mid 2K
    }
    
    if (visualType >= 1.0 && visualType < 1.5) {
        float continents = 0.0; float a = 0.5; vec3 p = localN * 6.5;
        for(int i=0; i<octaves; i++) { continents += a*valueNoise3(p); p=p*2.07; a*=0.5; }
        
        float clouds = 0.0; a = 0.5; vec3 pp = localN * 18.0 + vec3(uTime*0.01);
        for(int i=0; i<octaves; i++) { clouds += a*valueNoise3(pp); pp=pp*2.07; a*=0.5; }
        
        vec3 ocean = baseColor * vec3(0.55, 0.75, 1.0);
        vec3 land = vec3(0.22, 0.45, 0.18);
        float isLand = smoothstep(0.45, 0.62, continents);
        mat.Albedo = mix(ocean, land, isLand);
        mat.Roughness = mix(0.15, 0.85, isLand); 
        
        mat.Albedo = mix(mat.Albedo, vec3(0.95), smoothstep(0.72, 0.86, clouds)*0.35);
        mat.Roughness = mix(mat.Roughness, 0.9, smoothstep(0.72, 0.86, clouds));
    }
    else if (visualType >= 1.5 && visualType < 2.5) {
        float lat = localN.y * 0.5 + 0.5;
        vec2 uv = sphereUv(localN);
        uv.x += uTime * 0.005;
        vec3 offsetN = vec3(cos(uv.x*PI*2.0)*sin(uv.y*PI), cos(uv.y*PI), sin(uv.x*PI*2.0)*sin(uv.y*PI));
        
        float bands = sin((lat * 18.0 + fbm(offsetN * 8.0) * 2.0) * 3.14159);
        float storms = fbm(vec3(offsetN.xz * 15.0, offsetN.y * 5.0 + uTime * 0.03));
        vec3 c1 = vec3(0.90, 0.75, 0.55);
        vec3 c2 = vec3(0.65, 0.45, 0.30);
        mat.Albedo = mix(c1, c2, bands * 0.5 + 0.5) * (0.9 + 0.2 * storms);
        mat.Roughness = 0.6;
    }
    else if (visualType >= 2.5 && visualType < 4.5) {
        float rough = 0.0; float a = 0.5; vec3 p = localN * 20.0;
        for(int i=0; i<octaves; i++) { rough += a*valueNoise3(p); p=p*2.07; a*=0.5; }
        mat.Albedo = vec3(0.55, 0.52, 0.50) * (0.6 + 0.5*rough);
        mat.Roughness = 0.85 + 0.15*rough;
        
        vec3 dPdx = dFdx(localN);
        vec3 dPdy = dFdy(localN); // bump fake normal
        mat.Normal = normalize(localN + (dPdx + dPdy) * (rough*0.1));
    }
    else if (visualType >= 4.5 && visualType < 5.5) {
        float ice = fbm(localN * 25.0);
        mat.Albedo = mix(vec3(0.65, 0.85, 0.95), vec3(0.9, 0.98, 1.0), ice);
        mat.Roughness = mix(0.1, 0.4, ice);
    }
    else {
        float cracks = fbm(localN * 12.0);
        mat.Albedo = vec3(0.1, 0.08, 0.08) * (0.8 + 0.4*fbm(localN * 30.0));
        mat.Roughness = 0.9;
        
        float isCrack = smoothstep(0.4, 0.45, abs(cracks - 0.5)*2.0);
        isCrack = 1.0 - isCrack;
        mat.Emissive = isCrack * 2.5; 
        if (isCrack > 0.0) {
            mat.Albedo = mix(mat.Albedo, vec3(1.0, 0.3, 0.05), isCrack);
        }
    }
    
    return mat;
}

float DistributionGGX(vec3 N, vec3 H, float roughness) {
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
    float num = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness) {
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;
    float num = NdotV;
    float denom = NdotV * (1.0 - k) + k;
    return num / denom;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness) {
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);
    return ggx1 * ggx2;
}

vec3 fresnelSchlick(float cosTheta, vec3 F0) {
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float traceShadowRay(vec3 origin, vec3 lightPos, vec3 selfCenter, float selfRadius) { if (uRayTraceShadows == 0 || uRayOccluderCount == 0) return 1.0; vec3 toLight = lightPos - origin; float maxDist = length(toLight); if (maxDist < 1e-5) return 1.0; vec3 dir = toLight / maxDist; float visibility = 1.0; float softness = clamp(uRayShadowSoftness, 0.0005, 0.20); for (int i = 0; i < 24; i++) { if (i >= uRayOccluderCount) break; vec3 center = uRayOccluders[i]; float radius = uRayOccluderRadius[i]; if (distance(center, selfCenter) < max(0.01, selfRadius * 0.55)) continue; vec3 oc = origin - center; float b = dot(oc, dir); float c = dot(oc, oc) - radius * radius; float h = b * b - c; if (h <= 0.0) continue; float t = -b - sqrt(h); if (t <= softness || t >= maxDist) continue; float blockerBias = 1.0 - smoothstep(0.0, maxDist, t); float hit = clamp(uRayShadowStrength * (0.55 + 0.45 * blockerBias), 0.0, 1.0); visibility *= (1.0 - hit); if (visibility <= 0.01) { visibility = 0.0; break; } } return clamp(visibility, 0.05, 1.0); }
vec3 sampleScreenReflections(vec3 viewDir, vec3 normal) { if (uEnableReflections == 0) return vec3(0.0); vec2 safeResolution = max(uResolution, vec2(1.0)); vec2 uv = gl_FragCoord.xy / safeResolution; vec3 reflected = reflect(-viewDir, normal); int samples = clamp(uMaxReflectionSamples, 1, 16); vec3 accum = vec3(0.0); float wsum = 0.0; for (int i = 0; i < 16; i++) { if (i >= samples) break; float t = float(i + 1) / float(samples); vec2 suv = uv + reflected.xy * uReflectionScale * t; if (suv.x <= 0.0 || suv.x >= 1.0 || suv.y <= 0.0 || suv.y >= 1.0) break; float sceneDepth = texture(uDepthTexture, suv).r; float depthWeight = smoothstep(0.05, 1.0, sceneDepth); float weight = (1.0 - 0.6 * t) * depthWeight; accum += texture(uScreenTexture, suv).rgb * weight; wsum += weight; } if (wsum <= 1e-5) return vec3(0.0); return accum / wsum; }

void main() {
    vec3 geomNormal = normalize(vNormal); 
    vec3 localN = normalize(vLocalNormal); 
    vec3 viewDir = normalize(uViewPos - vFragPos);
    
    float visualType = vVisual.x; 
    float atmosphereAmount = vVisual.w;
    float distToCam = length(uViewPos - vFragPos);
    
    Material mat = getPlanetMaterial(localN, visualType, vColor.rgb, distToCam);
    
    if (visualType >= 6.5 && visualType < 7.5) {
        // Fast path for simple effect/explosion bodies
        float rim = pow(1.0 - max(dot(viewDir, geomNormal), 0.0), 3.0);
        float glowIntensity = vVisual.z;
        float luminosity = vVisual.y;
        vec3 color = vColor.rgb * luminosity * 1.5;
        vec3 result = color + color * rim * glowIntensity;
        float luma = dot(result, vec3(0.2126, 0.7152, 0.0722));
        result = mix(vec3(luma), result, clamp(uGlobalSaturation, 0.0, 2.0));
        if (uEnableHdr != 0) { result = toneMapAces(result * max(uExposure, 0.01)); }
        else { result = clamp(result, 0.0, 1.0); }
        FragColor = vec4(pow(result, vec3(1.0/2.2)), clamp(vColor.a, 0.0, 1.0));
        return;
    }
    
    if (uUseAlbedoAtlas != 0 && visualType >= 1.0 && visualType < 6.5) {
        float layer = clamp(vTextureLayer, 0.0, 7.0); 
        vec3 texAlbedo = texture(uBodyAlbedoAtlas, vec3(sphereUv(localN), layer)).rgb;
        mat.Albedo = mix(mat.Albedo, texAlbedo, clamp(uAlbedoBlend, 0.0, 1.0));
    }

    vec3 N = normalize(mat.Normal);
    vec3 V = viewDir;
    
    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, mat.Albedo, mat.Metallic);
    
    vec3 Lo = vec3(0.0);
    float NdotV = max(dot(N, V), 0.0);
    
    if (uEnableStarLighting != 0 && uStarLightCount > 0) {
        for (int i = 0; i < 8; i++) {
            if (i >= uStarLightCount) break;
            vec3 lightVec = uStarLights[i] - vFragPos; 
            float dist = length(lightVec); 
            if (dist < 1e-5) continue;
            
            vec3 L = normalize(lightVec); 
            vec3 H = normalize(V + L);
            
            float distanceSq = dist * dist + 0.001; 
            float attenuation = 1.0 / (4.0 * PI * distanceSq); // Physical inverse square
            
            // Adjust intensity so scene remains visible (engine uses large values)
            vec3 radiance = uStarLightColor[i] * max(uStarLightIntensity[i], 0.0) * attenuation * 3000.0;
            if (dot(radiance, vec3(0.333)) < 0.005) continue;
            
            vec3 shadowOrigin = vFragPos + geomNormal * (0.002 + 0.02 * vBodyRadius); 
            float shadow = traceShadowRay(shadowOrigin, uStarLights[i], vBodyCenter, max(vBodyRadius, 0.01));
            
            float NdotL = dot(N, L);
            
            if (NdotL > 0.0) {
                float NDF = DistributionGGX(N, H, mat.Roughness);   
                float G   = GeometrySmith(N, V, L, mat.Roughness);      
                vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);       
                
                vec3 numerator    = NDF * G * F; 
                float denominator = 4.0 * NdotV * NdotL + 0.0001; 
                vec3 specular = numerator / denominator;
                
                vec3 kS = F;
                vec3 kD = vec3(1.0) - kS;
                kD *= 1.0 - mat.Metallic;	  
                
                vec3 diffuse = (kD * mat.Albedo / PI);
                
                // Terminator Softness
                float terminator = smoothstep(-0.1, 0.1, NdotL);
                
                Lo += (diffuse + specular) * radiance * NdotL * shadow * terminator;
            } else if (uEnableNightLights != 0 && visualType >= 1.0 && visualType < 1.5) {
                // City lights on dark side
                float pop = pow(valueNoise3(localN * 24.0), 3.0);
                if (pop > 0.4) {
                    float fade = smoothstep(0.0, -0.2, NdotL); // Only visible deep in night
                    vec3 cityColor = vec3(1.0, 0.8, 0.4) * fade * (pop - 0.4) * 2.0;
                    Lo += cityColor;
                }
            }
        }
    }
    
    vec3 ambient = vec3(0.02) * mat.Albedo * mat.AO;
    vec3 result = ambient + Lo;
    
    // Add Emissive strictly for Lava/cracks
    result += mat.Emissive * mat.Albedo;
    
    // Atmospheric Scattering
    if (uEnableAtmosphere != 0 && atmosphereAmount > 0.0 && visualType < 5.0) {
        float scatter = pow(1.0 - max(dot(V, geomNormal), 0.0), 4.0);
        vec3 atmosphereColor = vec3(0.5, 0.7, 1.0) * scatter * atmosphereAmount * 0.8;
        
        vec3 mieTotal = vec3(0.0);
        for (int i = 0; i < uStarLightCount; i++) {
            vec3 L = normalize(uStarLights[i] - vFragPos);
            float LdotN = dot(L, geomNormal);
            float mie = pow(max(dot(L, V), 0.0), 16.0);
            float dayScatterMask = smoothstep(-0.2, 0.2, LdotN);
            mieTotal += vec3(1.0, 0.9, 0.8) * mie * uStarLightIntensity[i] * 0.005 * dayScatterMask;
        }
        
        result += atmosphereColor + (mieTotal * atmosphereAmount);
    }
    
    vec3 reflectionColor = sampleScreenReflections(viewDir, geomNormal);
    float reflectivity = clamp(0.02, mat.Metallic + 0.05, 0.7);
    float fresnel = pow(1.0 - max(dot(viewDir, geomNormal), 0.0), 5.0);
    result = mix(result, reflectionColor, reflectivity) + reflectionColor * fresnel;
    
    float luma = dot(result, vec3(0.2126, 0.7152, 0.0722)); 
    result = mix(vec3(luma), result, clamp(uGlobalSaturation, 0.0, 2.0));
    
    if (uEnableHdr != 0) { 
        vec3 hdrColor = result * max(uExposure, 0.01); 
        result = toneMapAces(hdrColor); 
    } else { 
        result = clamp(result, 0.0, 1.0); 
    }
    
    result = pow(result, vec3(1.0 / 2.2));
    FragColor = vec4(result, clamp(vColor.a, 0.0, 1.0));
}

