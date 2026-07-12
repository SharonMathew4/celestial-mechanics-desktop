using System.Numerics;
using Silk.NET.OpenGL;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Renderer;

/// <summary>
/// High-performance point-sprite billboard renderer for galaxy particles.
///
/// DESIGN RATIONALE
/// ────────────────
/// Galaxy simulations require rendering 50,000+ particles per frame.
/// The existing InstancedSphereRenderer generates full sphere geometry per body,
/// which at 50K bodies produces 50K × ~1000 vertices = 50M vertices/frame.
///
/// This renderer uses GL_POINTS with programmatic point size:
/// - 1 vertex per particle → 50K vertices total
/// - 1 draw call for all particles
/// - Point sprite fragment shader handles billboard appearance
/// - ~100x faster than instanced spheres at N > 10K
///
/// VERTEX LAYOUT (per particle)
/// ─────────────────────────────
///   Position   : vec3   (12 bytes)
///   Color      : vec4   (16 bytes)
///   PointSize  : float  (4 bytes)
///   VisualType : float  (4 bytes)  — maps to JWST palette
///   Luminosity : float  (4 bytes)
///   Total      : 40 bytes/particle
///
/// At 50K particles: 2 MB vertex buffer (fits in GPU L2 cache).
///
/// BLENDING
/// ────────
/// Uses additive blending for emissive types (stars, H-II) to create
/// the characteristic diffuse glow of galaxy imagery. Dust particles
/// use standard alpha blending for absorption effects.
/// </summary>
public class GalaxyParticleRenderer : IDisposable
{
    // Vertex data layout: pos(3) + color(4) + pointSize(1) + visualType(1) + luminosity(1) = 10 floats
    private const int FloatsPerParticle = 10;
    private const int BytesPerFloat = 4;
    private const int StrideBytes = FloatsPerParticle * BytesPerFloat;

    private GL? _gl;
    private uint _vao;
    private uint _vbo;
    private ShaderProgram? _shader;
    private float[] _vertexData = Array.Empty<float>();
    private int _particleCount;
    private int _capacity;
    private bool _initialized;
    private bool _disposed;

    /// <summary>Base point size multiplier.</summary>
    public float BasePointSize { get; set; } = 0.4f;

    /// <summary>JWST color intensity multiplier.</summary>
    public float JwstColorIntensity { get; set; } = 1.15f;

    /// <summary>Enable 6-pointed Webb diffraction spikes.</summary>
    public bool EnableDiffractionSpikes { get; set; } = true;

    /// <summary>Luminosity threshold above which spikes are rendered.</summary>
    public float DiffractionSpikeThreshold { get; set; } = 1.5f;

    public void Initialize(GL gl, int initialCapacity = 65536)
    {
        _gl = gl;
        _capacity = initialCapacity;
        _vertexData = new float[_capacity * FloatsPerParticle];

        _vao = gl.GenVertexArray();
        _vbo = gl.GenBuffer();

        gl.BindVertexArray(_vao);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // Allocate buffer with GL_DYNAMIC_DRAW for per-frame updates
        unsafe
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(_capacity * StrideBytes), null,
                BufferUsageARB.DynamicDraw);
        }

        // Position (location 0): vec3
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
            (uint)StrideBytes, 0);

        // Color (location 1): vec4
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false,
            (uint)StrideBytes, 3 * BytesPerFloat);

        // PointSize (location 2): float
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false,
            (uint)StrideBytes, 7 * BytesPerFloat);

        // VisualType (location 3): float
        gl.EnableVertexAttribArray(3);
        gl.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false,
            (uint)StrideBytes, 8 * BytesPerFloat);

        // Luminosity (location 4): float
        gl.EnableVertexAttribArray(4);
        gl.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false,
            (uint)StrideBytes, 9 * BytesPerFloat);

        gl.BindVertexArray(0);

        // Load shaders
        string shaderDir = FindShaderDirectory();
        string vertSrc = File.ReadAllText(Path.Combine(shaderDir, "galaxy_particle.vert"));
        string fragSrc = File.ReadAllText(Path.Combine(shaderDir, "galaxy_particle.frag"));
        _shader = new ShaderProgram(gl, vertSrc, fragSrc);

        _initialized = true;
    }

    /// <summary>
    /// Update the particle data from the current render state.
    /// Only galaxy body types are included; non-galaxy bodies are skipped.
    /// </summary>
    public void UpdateParticles(RenderBody[] bodies, int bodyCount, Vector3 frameOrigin)
    {
        if (!_initialized || _gl == null) return;

        // Count galaxy particles
        int galaxyCount = 0;
        for (int i = 0; i < bodyCount; i++)
        {
            if (IsGalaxyType(bodies[i].BodyType))
                galaxyCount++;
        }

        if (galaxyCount == 0)
        {
            _particleCount = 0;
            return;
        }

        // Resize buffer if needed
        if (galaxyCount > _capacity)
        {
            _capacity = NextPowerOfTwo(galaxyCount);
            _vertexData = new float[_capacity * FloatsPerParticle];

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            unsafe
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(_capacity * StrideBytes), null,
                    BufferUsageARB.DynamicDraw);
            }
        }

        // Fill vertex data
        int writeIdx = 0;
        for (int i = 0; i < bodyCount; i++)
        {
            ref readonly var body = ref bodies[i];
            if (!IsGalaxyType(body.BodyType)) continue;

            int offset = writeIdx * FloatsPerParticle;

            // Position (already in frame-relative coords from RenderState)
            _vertexData[offset + 0] = body.Position.X;
            _vertexData[offset + 1] = body.Position.Y;
            _vertexData[offset + 2] = body.Position.Z;

            // Color (RGBA from RenderState)
            _vertexData[offset + 3] = body.Color.X;
            _vertexData[offset + 4] = body.Color.Y;
            _vertexData[offset + 5] = body.Color.Z;
            _vertexData[offset + 6] = body.Color.W;

            // Point size based on radius and type
            float pointSize = body.Radius * GetPointSizeMultiplier(body.BodyType);
            _vertexData[offset + 7] = pointSize;

            // Visual type (matches JWST shader palette indices)
            _vertexData[offset + 8] = body.VisualParams.X;

            // Luminosity
            _vertexData[offset + 9] = body.VisualParams.Y;

            writeIdx++;
        }
        _particleCount = writeIdx;

        // Upload to GPU
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            fixed (float* ptr = _vertexData)
            {
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
                    (nuint)(_particleCount * StrideBytes), ptr);
            }
        }
    }

    /// <summary>
    /// Render all galaxy particles in a single draw call.
    /// </summary>
    public void Render(Matrix4x4 view, Matrix4x4 projection,
        float viewportHeight, float time, Vector2 resolution,
        bool enableHdr, float exposure)
    {
        if (!_initialized || _gl == null || _shader == null || _particleCount == 0)
            return;

        _gl.Enable(EnableCap.ProgramPointSize);
        _gl.Enable(EnableCap.Blend);
        _gl.DepthMask(false);  // Don't write to depth — particles are transparent

        // Additive blending for emissive galaxy glow
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        _shader.Use();

        // Set uniforms
        _shader.SetUniform("uView", view);
        _shader.SetUniform("uProjection", projection);
        _shader.SetUniform("uViewportHeight", viewportHeight);
        _shader.SetUniform("uBasePointSize", BasePointSize);
        _shader.SetUniform("uTime", time);
        _shader.SetUniform("uJwstColorIntensity", JwstColorIntensity);
        _shader.SetUniform("uEnableDiffractionSpikes", EnableDiffractionSpikes ? 1 : 0);
        _shader.SetUniform("uDiffractionSpikeThreshold", DiffractionSpikeThreshold);
        _shader.SetUniform("uResolution", resolution);
        _shader.SetUniform("uEnableHdr", enableHdr ? 1 : 0);
        _shader.SetUniform("uExposure", exposure);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Points, 0, (uint)_particleCount);
        _gl.BindVertexArray(0);

        // Restore state
        _gl.DepthMask(true);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Disable(EnableCap.ProgramPointSize);
    }

    /// <summary>
    /// Returns true if the visual type ID corresponds to a galaxy body type.
    /// These types should be routed through this renderer instead of
    /// the instanced sphere renderer.
    /// </summary>
    public static bool IsGalaxyType(int bodyTypeInt)
    {
        return bodyTypeInt >= (int)BodyType.GalaxyDiskParticle
            && bodyTypeInt <= (int)BodyType.HIIRegion;
    }

    private static float GetPointSizeMultiplier(int bodyType)
    {
        return bodyType switch
        {
            (int)BodyType.GalaxyDiskParticle => 1.0f,
            (int)BodyType.GalaxyBulgeParticle => 1.4f,
            (int)BodyType.GalaxyHaloParticle => 0.5f,
            (int)BodyType.DustCloud => 1.8f,       // Larger for diffuse dust
            (int)BodyType.YoungStarCluster => 1.2f,
            (int)BodyType.HIIRegion => 1.5f,        // Emission regions are diffuse
            _ => 1.0f,
        };
    }

    private static int NextPowerOfTwo(int n)
    {
        int p = 1;
        while (p < n) p <<= 1;
        return p;
    }

    private static string FindShaderDirectory()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string shaderDir = Path.Combine(baseDir, "Shaders");
        if (Directory.Exists(shaderDir))
            return shaderDir;

        string? dir = baseDir;
        for (int i = 0; i < 6 && dir != null; i++)
        {
            string candidate = Path.Combine(dir, "src", "CelestialMechanics.Renderer", "Shaders");
            if (Directory.Exists(candidate))
                return candidate;
            dir = Path.GetDirectoryName(dir);
        }

        throw new FileNotFoundException($"Could not find Shaders directory. Searched from: {baseDir}");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_gl != null)
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
        }

        _shader?.Dispose();
    }
}
