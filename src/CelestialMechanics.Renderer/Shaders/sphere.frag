#version 330 core

in vec3 vNormal;
in vec3 vFragPos;
in vec3 vLocalPos;
in vec4 vColor;
flat in int vBodyType;
flat in int vInstanceId;
in float vTime;

uniform vec3 uViewPos;
uniform int uSelectedId;

out vec4 FragColor;

// ════════════════════════════════════════════════════════════════
// Simplex 3D Noise (compact GLSL implementation)
// ════════════════════════════════════════════════════════════════

vec4 permute(vec4 x) { return mod(((x*34.0)+1.0)*x, 289.0); }
vec4 taylorInvSqrt(vec4 r) { return 1.79284291400159 - 0.85373472095314 * r; }

float snoise(vec3 v)
{
    const vec2 C = vec2(1.0/6.0, 1.0/3.0);
    const vec4 D = vec4(0.0, 0.5, 1.0, 2.0);

    vec3 i  = floor(v + dot(v, C.yyy));
    vec3 x0 = v - i + dot(i, C.xxx);

    vec3 g = step(x0.yzx, x0.xyz);
    vec3 l = 1.0 - g;
    vec3 i1 = min(g.xyz, l.zxy);
    vec3 i2 = max(g.xyz, l.zxy);

    vec3 x1 = x0 - i1 + C.xxx;
    vec3 x2 = x0 - i2 + C.yyy;
    vec3 x3 = x0 - D.yyy;

    i = mod(i, 289.0);
    vec4 p = permute(permute(permute(
             i.z + vec4(0.0, i1.z, i2.z, 1.0))
           + i.y + vec4(0.0, i1.y, i2.y, 1.0))
           + i.x + vec4(0.0, i1.x, i2.x, 1.0));

    float n_ = 1.0/7.0;
    vec3  ns = n_ * D.wyz - D.xzx;

    vec4 j = p - 49.0 * floor(p * ns.z * ns.z);

    vec4 x_ = floor(j * ns.z);
    vec4 y_ = floor(j - 7.0 * x_);

    vec4 x  = x_ * ns.x + ns.yyyy;
    vec4 y  = y_ * ns.x + ns.yyyy;
    vec4 h  = 1.0 - abs(x) - abs(y);

    vec4 b0 = vec4(x.xy, y.xy);
    vec4 b1 = vec4(x.zw, y.zw);

    vec4 s0 = floor(b0)*2.0 + 1.0;
    vec4 s1 = floor(b1)*2.0 + 1.0;
    vec4 sh = -step(h, vec4(0.0));

    vec4 a0 = b0.xzyw + s0.xzyw*sh.xxyy;
    vec4 a1 = b1.xzyw + s1.xzyw*sh.zzww;

    vec3 p0 = vec3(a0.xy, h.x);
    vec3 p1 = vec3(a0.zw, h.y);
    vec3 p2 = vec3(a1.xy, h.z);
    vec3 p3 = vec3(a1.zw, h.w);

    vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2,p2), dot(p3,p3)));
    p0 *= norm.x; p1 *= norm.y; p2 *= norm.z; p3 *= norm.w;

    vec4 m = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
    m = m * m;
    return 42.0 * dot(m*m, vec4(dot(p0,x0), dot(p1,x1), dot(p2,x2), dot(p3,x3)));
}

float fbm(vec3 p, int octaves)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * snoise(p * frequency);
        amplitude *= 0.5;
        frequency *= 2.0;
    }
    return value;
}

// ════════════════════════════════════════════════════════════════
// Body Type Constants (must match C# BodyType enum)
// ════════════════════════════════════════════════════════════════
const int BODY_STAR        = 0;
const int BODY_PLANET      = 1;
const int BODY_GAS_GIANT   = 2;
const int BODY_ROCKY       = 3;
const int BODY_MOON        = 4;
const int BODY_ASTEROID    = 5;
const int BODY_NEUTRON     = 6;
const int BODY_BLACK_HOLE  = 7;
const int BODY_COMET       = 8;
const int BODY_CUSTOM      = 9;

// ════════════════════════════════════════════════════════════════
// Per-body-type procedural texturing
// ════════════════════════════════════════════════════════════════

vec3 starTexture(vec3 localPos, float time)
{
    // Animated plasma surface with turbulent convection
    vec3 p = localPos * 3.0;
    float n1 = fbm(p + vec3(time * 0.15, time * 0.1, -time * 0.08), 5);
    float n2 = fbm(p * 2.0 + vec3(-time * 0.2, time * 0.12, time * 0.05), 4);

    // Base color: yellow-orange with hot spots
    vec3 coolColor = vec3(1.0, 0.6, 0.1);   // Orange
    vec3 hotColor  = vec3(1.0, 1.0, 0.8);   // White-yellow
    vec3 darkColor = vec3(0.8, 0.3, 0.05);  // Dark orange (sunspots)

    float hotSpots = smoothstep(0.2, 0.6, n1);
    float sunspots = smoothstep(0.3, 0.5, n2) * 0.3;

    vec3 color = mix(coolColor, hotColor, hotSpots);
    color = mix(color, darkColor, sunspots);

    // Limb darkening: edges appear darker
    float fresnel = 1.0 - abs(dot(normalize(localPos), normalize(localPos)));
    color *= 0.8 + 0.2 * (1.0 - fresnel * 0.5);

    return color;
}

vec3 planetTexture(vec3 localPos)
{
    // Earth-like planet with continents and oceans
    vec3 p = localPos * 4.0;
    float continent = fbm(p + vec3(42.0, 17.0, 89.0), 6);

    vec3 oceanColor    = vec3(0.05, 0.15, 0.5);
    vec3 shallowOcean  = vec3(0.1, 0.3, 0.6);
    vec3 landGreen     = vec3(0.15, 0.4, 0.12);
    vec3 landBrown     = vec3(0.45, 0.35, 0.2);
    vec3 mountain      = vec3(0.55, 0.5, 0.4);
    vec3 snow          = vec3(0.9, 0.92, 0.95);

    vec3 color;
    if (continent < -0.1)
    {
        color = mix(oceanColor, shallowOcean, smoothstep(-0.4, -0.1, continent));
    }
    else if (continent < 0.15)
    {
        color = mix(landGreen, landBrown, smoothstep(-0.1, 0.15, continent));
    }
    else if (continent < 0.35)
    {
        color = mix(landBrown, mountain, smoothstep(0.15, 0.35, continent));
    }
    else
    {
        color = mix(mountain, snow, smoothstep(0.35, 0.55, continent));
    }

    // Polar ice caps
    float latitude = abs(localPos.y);
    if (latitude > 0.75)
    {
        float iceFactor = smoothstep(0.75, 0.9, latitude);
        color = mix(color, snow, iceFactor);
    }

    return color;
}

vec3 gasGiantTexture(vec3 localPos, float time)
{
    // Jupiter-like banded atmosphere with swirling storms
    float latitude = localPos.y;

    // Horizontal bands
    float bands = sin(latitude * 18.0) * 0.5 + 0.5;
    float turbulence = snoise(vec3(localPos.x * 5.0 + time * 0.05, latitude * 10.0, localPos.z * 5.0)) * 0.15;
    bands += turbulence;

    // Band colors
    vec3 lightBand = vec3(0.85, 0.75, 0.55);
    vec3 darkBand  = vec3(0.6, 0.45, 0.3);
    vec3 redSpot   = vec3(0.8, 0.35, 0.2);

    vec3 color = mix(darkBand, lightBand, bands);

    // Great Red Spot analog
    vec2 spotCenter = vec2(0.3, -0.2);
    float spotDist = length(vec2(localPos.x - spotCenter.x, localPos.y - spotCenter.y));
    float spotSwirl = snoise(vec3(localPos.xz * 8.0 + time * 0.1, localPos.y * 8.0));
    float spotMask = smoothstep(0.3, 0.1, spotDist + spotSwirl * 0.05);
    color = mix(color, redSpot, spotMask * 0.7);

    return color;
}

vec3 rockyPlanetTexture(vec3 localPos)
{
    // Mars-like rocky terrain with canyons and craters
    vec3 p = localPos * 5.0;
    float terrain = fbm(p + vec3(73.0, 31.0, 55.0), 5);
    float craters = snoise(localPos * 12.0);

    vec3 dustColor    = vec3(0.6, 0.35, 0.15);
    vec3 rockColor    = vec3(0.45, 0.3, 0.18);
    vec3 darkRock     = vec3(0.25, 0.18, 0.1);
    vec3 highlightColor = vec3(0.7, 0.5, 0.3);

    vec3 color = mix(dustColor, rockColor, smoothstep(-0.2, 0.3, terrain));
    color = mix(color, darkRock, smoothstep(0.3, 0.5, terrain));

    // Crater rims
    float rim = smoothstep(0.4, 0.5, craters) - smoothstep(0.5, 0.6, craters);
    color = mix(color, highlightColor, rim * 0.4);

    // Polar frost
    float latitude = abs(localPos.y);
    if (latitude > 0.85)
    {
        float frostFactor = smoothstep(0.85, 0.95, latitude);
        color = mix(color, vec3(0.85, 0.88, 0.9), frostFactor * 0.6);
    }

    return color;
}

vec3 moonTexture(vec3 localPos)
{
    // Rocky gray surface with craters
    vec3 p = localPos * 6.0;
    float surface = fbm(p + vec3(19.0, 53.0, 37.0), 5);
    float craters = snoise(localPos * 15.0);
    float smallCraters = snoise(localPos * 30.0);

    vec3 lightGray = vec3(0.65, 0.63, 0.6);
    vec3 darkGray  = vec3(0.35, 0.33, 0.3);
    vec3 maria     = vec3(0.25, 0.25, 0.28);

    // Base terrain
    vec3 color = mix(lightGray, darkGray, smoothstep(-0.2, 0.3, surface));

    // Maria (dark volcanic plains)
    float mariaMask = smoothstep(-0.3, -0.1, surface);
    color = mix(maria, color, mariaMask);

    // Large craters
    float craterRim = smoothstep(0.4, 0.5, craters) - smoothstep(0.5, 0.65, craters);
    float craterFloor = smoothstep(0.5, 0.55, craters);
    color = mix(color, lightGray * 1.1, craterRim * 0.5);
    color = mix(color, darkGray * 0.8, craterFloor * 0.3);

    // Small craters
    float smallRim = smoothstep(0.45, 0.5, smallCraters) - smoothstep(0.5, 0.55, smallCraters);
    color = mix(color, lightGray, smallRim * 0.2);

    return color;
}

vec3 asteroidTexture(vec3 localPos)
{
    // Rough, irregular brownish-gray rock
    vec3 p = localPos * 8.0;
    float rock = fbm(p + vec3(91.0, 47.0, 23.0), 4);
    float roughness = snoise(localPos * 20.0);

    vec3 baseColor = vec3(0.4, 0.38, 0.32);
    vec3 darkPatch = vec3(0.25, 0.22, 0.18);
    vec3 lightPatch = vec3(0.55, 0.5, 0.42);

    vec3 color = mix(baseColor, darkPatch, smoothstep(-0.1, 0.2, rock));
    color = mix(color, lightPatch, smoothstep(0.3, 0.5, rock) * 0.5);

    // Surface roughness / regolith
    color += roughness * 0.06;

    return color;
}

vec3 neutronStarTexture(vec3 localPos, float time)
{
    // Bright blue-white with pulsing magnetic pole beams
    float pulse = sin(time * 8.0) * 0.5 + 0.5;

    vec3 baseColor = vec3(0.5, 0.7, 1.0);
    vec3 hotColor  = vec3(0.8, 0.9, 1.0);

    // Magnetic pole hot spots
    float poleDist = abs(localPos.y);
    float poleBright = smoothstep(0.7, 1.0, poleDist) * pulse;

    vec3 color = mix(baseColor, hotColor, poleBright);

    // Surface crackle pattern
    float crackle = snoise(localPos * 20.0 + time * 2.0);
    color += vec3(0.2, 0.3, 0.5) * smoothstep(0.3, 0.5, crackle) * 0.3;

    return color;
}

vec3 blackHoleTexture(vec3 localPos, vec3 viewDir, float time)
{
    // Dark center with event horizon rim glow
    float fresnel = pow(1.0 - max(dot(normalize(localPos), -viewDir), 0.0), 3.0);

    // Event horizon: nearly black center
    vec3 horizonColor = vec3(0.02, 0.0, 0.03);

    // Rim glow: hot orange-yellow accretion glow at edges
    vec3 rimGlow = vec3(1.0, 0.5, 0.1) * fresnel * 2.0;

    // Photon ring: bright ring at the edge
    float ring = smoothstep(0.85, 0.95, fresnel) * 1.5;
    vec3 ringColor = vec3(1.0, 0.8, 0.4) * ring;

    // Swirling accretion hint
    float swirl = snoise(vec3(localPos.xz * 4.0 + time * 0.3, localPos.y * 2.0));
    rimGlow += vec3(0.8, 0.3, 0.1) * swirl * fresnel * 0.5;

    return horizonColor + rimGlow + ringColor;
}

vec3 cometTexture(vec3 localPos)
{
    // Icy/dusty surface with bright spots
    vec3 p = localPos * 6.0;
    float surface = fbm(p + vec3(61.0, 83.0, 11.0), 4);

    vec3 iceColor  = vec3(0.7, 0.75, 0.85);
    vec3 dustColor = vec3(0.35, 0.3, 0.25);
    vec3 brightSpot = vec3(0.9, 0.95, 1.0);

    vec3 color = mix(dustColor, iceColor, smoothstep(-0.1, 0.3, surface));

    // Outgassing bright spots
    float spots = smoothstep(0.4, 0.5, snoise(localPos * 10.0));
    color = mix(color, brightSpot, spots * 0.5);

    return color;
}

// ════════════════════════════════════════════════════════════════
// Main fragment shader
// ════════════════════════════════════════════════════════════════

void main()
{
    vec3 norm    = normalize(vNormal);
    vec3 viewDir = normalize(uViewPos - vFragPos);
    vec3 lightDir = normalize(vec3(0.3, 1.0, 0.5));

    // ── Get procedural surface color based on body type ──────────
    vec3 surfaceColor;
    float emissive = 0.0;

    if (vBodyType == BODY_STAR)
    {
        surfaceColor = starTexture(vLocalPos, vTime);
        emissive = 0.7;
    }
    else if (vBodyType == BODY_PLANET)
    {
        surfaceColor = planetTexture(vLocalPos);
    }
    else if (vBodyType == BODY_GAS_GIANT)
    {
        surfaceColor = gasGiantTexture(vLocalPos, vTime);
    }
    else if (vBodyType == BODY_ROCKY)
    {
        surfaceColor = rockyPlanetTexture(vLocalPos);
    }
    else if (vBodyType == BODY_MOON)
    {
        surfaceColor = moonTexture(vLocalPos);
    }
    else if (vBodyType == BODY_ASTEROID)
    {
        surfaceColor = asteroidTexture(vLocalPos);
    }
    else if (vBodyType == BODY_NEUTRON)
    {
        surfaceColor = neutronStarTexture(vLocalPos, vTime);
        emissive = 0.6;
    }
    else if (vBodyType == BODY_BLACK_HOLE)
    {
        surfaceColor = blackHoleTexture(vLocalPos, viewDir, vTime);
        emissive = 0.4;
    }
    else if (vBodyType == BODY_COMET)
    {
        surfaceColor = cometTexture(vLocalPos);
    }
    else
    {
        // Custom / fallback: use instance color
        surfaceColor = vColor.rgb;
    }

    // ── Lighting ─────────────────────────────────────────────────

    // Ambient
    float ambient = 0.12;

    // Diffuse
    float diff = max(dot(norm, lightDir), 0.0);

    // Specular (Blinn-Phong)
    vec3 halfDir = normalize(lightDir + viewDir);
    float shininess = (vBodyType == BODY_GAS_GIANT || vBodyType == BODY_PLANET) ? 32.0 : 64.0;
    float spec = pow(max(dot(norm, halfDir), 0.0), shininess);
    float specStrength = (vBodyType == BODY_ASTEROID || vBodyType == BODY_MOON) ? 0.1 : 0.3;

    // Fresnel rim light (subtle blue-ish rim on all bodies)
    float fresnel = pow(1.0 - max(dot(norm, viewDir), 0.0), 4.0);
    vec3 rimLight = vec3(0.15, 0.2, 0.35) * fresnel * 0.5;

    // Combine
    vec3 result = (ambient + diff + spec * specStrength + emissive) * surfaceColor + rimLight;

    // ── Selection highlight ──────────────────────────────────────
    if (uSelectedId >= 0 && vInstanceId == uSelectedId)
    {
        // Pulsing cyan outline glow
        float pulse = sin(vTime * 4.0) * 0.15 + 0.85;
        float edge = pow(fresnel, 2.0) * pulse;
        result += vec3(0.0, 0.8, 1.0) * edge * 1.5;

        // Slight overall tint
        result = mix(result, result + vec3(0.0, 0.1, 0.15), 0.15);
    }

    // ── Atmosphere rim (for planets and gas giants) ──────────────
    if (vBodyType == BODY_PLANET || vBodyType == BODY_GAS_GIANT || vBodyType == BODY_ROCKY)
    {
        float atmoFresnel = pow(1.0 - max(dot(norm, viewDir), 0.0), 3.0);
        vec3 atmoColor;
        if (vBodyType == BODY_PLANET)
            atmoColor = vec3(0.3, 0.5, 1.0);      // Blue atmosphere
        else if (vBodyType == BODY_GAS_GIANT)
            atmoColor = vec3(0.6, 0.5, 0.3);       // Hazy atmosphere
        else
            atmoColor = vec3(0.7, 0.4, 0.2);       // Thin dusty atmosphere
        result += atmoColor * atmoFresnel * 0.4;
    }

    FragColor = vec4(result, 1.0);
}
