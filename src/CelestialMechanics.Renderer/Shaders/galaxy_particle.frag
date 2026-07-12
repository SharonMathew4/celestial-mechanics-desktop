#version 330 core

in vec4 vColor;
in float vVisualType;
in float vLuminosity;
in float vDistanceToCamera;

uniform float uTime;
uniform float uJwstColorIntensity;
uniform int uEnableDiffractionSpikes;
uniform float uDiffractionSpikeThreshold;
uniform vec2 uResolution;
uniform int uEnableHdr;
uniform float uExposure;
out vec4 FragColor;

// ── JWST Color Palette ────────────────────────────────────────────────────
// Derived from inverse analysis of JWST imagery:
//
// Galaxy Disk (type 8): Warm white-yellow → golden at center
//   Reference: NGC 4622 smooth bulge (potw1811a), M101 face-on (heic0602a)
//
// Galaxy Bulge (type 9): Deep golden-orange, smooth
//   Reference: M104 Sombrero luminous core (user image 4), M77 nucleus (heic1305a)
//
// Galaxy Halo (type 10): Faint blue-gray, nearly invisible
//   Dark matter tracer, not directly imaged
//
// Dust Cloud (type 11): Deep orange/red-brown
//   Reference: M106 dark lanes (heic1302a), NGC 3256 dust (heic1811a),
//   Sombrero equatorial dust band (user image 4)
//
// Young Star Cluster (type 12): Electric blue/cyan
//   Reference: NGC 1792 blue clusters (potw2049a), Tadpole tail tip (heic0206a),
//   NGC 1073 arm tips (heic0706a)
//
// H-II Region (type 13): Magenta/pink
//   Reference: M81 pink knots (heic1302a), M77 spiral arm knots (heic1305a),
//   NGC 1073 spiral arm emission (heic0706a)

vec3 toneMapAces(vec3 x)
{
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0);
}

vec3 getJwstBaseColor(float visualType)
{
    // Galaxy Disk Particle — warm white-yellow with subtle variation
    if (visualType < 8.5)
        return vec3(0.95, 0.90, 0.75);

    // Galaxy Bulge Particle — deep golden-orange (M104 core)
    if (visualType < 9.5)
        return vec3(0.98, 0.82, 0.55);

    // Galaxy Halo Particle — faint blue-gray (dark matter tracer)
    if (visualType < 10.5)
        return vec3(0.35, 0.40, 0.55);

    // Dust Cloud — deep orange/red-brown (M106 dust lanes, Sombrero band)
    if (visualType < 11.5)
        return vec3(0.72, 0.28, 0.08);

    // Young Star Cluster — electric blue/cyan (NGC 1792, Tadpole tip)
    if (visualType < 12.5)
        return vec3(0.25, 0.78, 1.0);

    // H-II Region — magenta/pink (M81 knots, M77 emission)
    return vec3(0.92, 0.30, 0.68);
}

float getJwstAlphaBase(float visualType)
{
    if (visualType < 8.5)  return 0.55;  // Disk — moderate opacity
    if (visualType < 9.5)  return 0.72;  // Bulge — denser, more opaque
    if (visualType < 10.5) return 0.06;  // Halo — barely visible
    if (visualType < 11.5) return 0.45;  // Dust — semi-transparent
    if (visualType < 12.5) return 0.75;  // Young stars — bright
    return 0.70;                          // H-II — bright emission
}

/// Compute 6-pointed diffraction spikes mimicking Webb's hexagonal PSF.
///
/// Webb's primary mirror consists of 18 hexagonal segments, producing a
/// characteristic 6-pointed diffraction pattern (plus two shorter spikes
/// from the secondary mirror support struts). Bright point sources in
/// Webb imagery show these spikes prominently.
///
/// We approximate this with 6 radial rays at 60° intervals, each with
/// an exponential falloff from the particle's screen center.
vec3 computeDiffractionSpikes(vec2 fragCoord, vec2 pointCenter, float luminosity, float pointRadius)
{
    float spike = 0.0;
    float spikeLength = max(luminosity * 3.0, 4.0);

    for (int i = 0; i < 6; i++)
    {
        float angle = float(i) * 3.14159265 / 3.0;
        vec2 dir = vec2(cos(angle), sin(angle));

        // Distance from the spike line (perpendicular distance)
        vec2 delta = fragCoord - pointCenter;
        float along = dot(delta, dir);
        float perp = abs(dot(delta, vec2(-dir.y, dir.x)));

        // Only render spikes extending outward (along > 0 or along < 0)
        float falloff = exp(-perp * 6.0 / max(luminosity * 0.5, 0.5));
        float lengthFade = exp(-abs(along) / spikeLength);

        spike += falloff * lengthFade * 0.12;
    }

    // Spike color: warm white with slight blue tint (Webb characteristic)
    return vec3(0.92, 0.90, 0.95) * spike * luminosity * 0.5;
}

void main()
{
    // Point sprite: gl_PointCoord is [0,1] within the point quad
    vec2 pc = gl_PointCoord * 2.0 - 1.0;  // [-1, 1]
    float dist2 = dot(pc, pc);

    // Soft circular falloff for billboard rendering
    // Inner core is solid, outer region fades smoothly
    float alpha = 1.0 - smoothstep(0.3, 1.0, sqrt(dist2));

    if (alpha < 0.005)
        discard;

    vec3 baseColor = getJwstBaseColor(vVisualType);
    float baseAlpha = getJwstAlphaBase(vVisualType);

    // Apply JWST color intensity scaling
    baseColor *= uJwstColorIntensity;

    // Luminosity-based brightening
    float lumBoost = 1.0 + vLuminosity * 0.8;
    baseColor *= lumBoost;

    // Distance fade for depth perception
    float distFade = clamp(1.0 / (1.0 + vDistanceToCamera * 0.02), 0.1, 1.0);

    // For emissive types (young stars, H-II), add a central glow
    if (vVisualType > 11.5)
    {
        float centralGlow = exp(-dist2 * 4.0);
        baseColor += centralGlow * baseColor * 0.6;
    }

    // Dust clouds get absorptive (darkening) behaviour at edges
    if (vVisualType > 10.5 && vVisualType < 11.5)
    {
        float edgeDarken = smoothstep(0.0, 0.7, sqrt(dist2));
        baseColor *= 1.0 - edgeDarken * 0.5;
        alpha *= 0.85;  // Slightly more transparent for dust
    }

    // Bulge particles get smooth radial gradient (golden core fading outward)
    if (vVisualType > 8.5 && vVisualType < 9.5)
    {
        float radialGrad = 1.0 - smoothstep(0.0, 0.8, sqrt(dist2));
        baseColor *= 0.7 + 0.5 * radialGrad;
    }

    // Subtle animation: twinkle for young stars
    if (vVisualType > 11.5 && vVisualType < 12.5)
    {
        float twinkle = 0.92 + 0.08 * sin(uTime * 3.0 + vDistanceToCamera * 5.0);
        baseColor *= twinkle;
    }

    // Diffraction spikes for bright particles (above luminosity threshold)
    vec3 spikeContribution = vec3(0.0);
    if (uEnableDiffractionSpikes != 0 && vLuminosity > uDiffractionSpikeThreshold)
    {
        vec2 pointCenter = vec2(0.0);  // Center of point sprite in [-1,1] coords
        spikeContribution = computeDiffractionSpikes(pc, pointCenter, vLuminosity, 1.0);
    }

    vec3 finalColor = baseColor + spikeContribution;

    // HDR tone mapping
    if (uEnableHdr != 0)
    {
        vec3 hdrColor = finalColor * max(uExposure, 0.01);
        finalColor = toneMapAces(hdrColor);
    }
    else
    {
        finalColor = clamp(finalColor, 0.0, 1.0);
    }

    // Gamma correction
    finalColor = pow(finalColor, vec3(1.0 / 2.2));

    float finalAlpha = alpha * baseAlpha * distFade * clamp(vColor.a, 0.0, 1.0);
    FragColor = vec4(finalColor, finalAlpha);
}
