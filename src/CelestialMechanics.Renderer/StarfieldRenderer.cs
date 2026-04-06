using System.Numerics;
using Silk.NET.OpenGL;

namespace CelestialMechanics.Renderer;

/// <summary>
/// Renders a procedural starfield background using point sprites.
/// Stars are placed on a large sphere and rendered without depth writing
/// so they always appear behind the scene.
/// </summary>
public sealed class StarfieldRenderer : IDisposable
{
    private uint _vao;
    private uint _vbo;
    private int _starCount;
    private GL? _gl;
    private bool _initialized;

    // Floats per star: position(3) + brightness(1) + color(3) = 7
    private const int FloatsPerStar = 7;

    public void Initialize(GL gl, int starCount = 4000, float radius = 5000f)
    {
        _gl = gl;
        _starCount = starCount;

        var data = new float[starCount * FloatsPerStar];
        var rng = new Random(42); // Deterministic seed for consistent starfield

        for (int i = 0; i < starCount; i++)
        {
            // Uniform distribution on sphere using spherical coordinates
            float theta = (float)(rng.NextDouble() * 2.0 * System.Math.PI);
            float phi = MathF.Acos((float)(2.0 * rng.NextDouble() - 1.0));

            float x = radius * MathF.Sin(phi) * MathF.Cos(theta);
            float y = radius * MathF.Sin(phi) * MathF.Sin(theta);
            float z = radius * MathF.Cos(phi);

            // Brightness: most stars dim, few bright (exponential distribution)
            float brightness = MathF.Pow((float)rng.NextDouble(), 2.5f);
            brightness = MathF.Max(0.15f, brightness);

            // Star color: based on spectral type
            float colorSeed = (float)rng.NextDouble();
            float r, g, b;
            if (colorSeed < 0.1f)
            {
                // Red/orange (M-type)
                r = 1.0f; g = 0.6f + (float)rng.NextDouble() * 0.2f; b = 0.4f;
            }
            else if (colorSeed < 0.25f)
            {
                // Yellow (G-type, Sun-like)
                r = 1.0f; g = 0.95f; b = 0.7f + (float)rng.NextDouble() * 0.1f;
            }
            else if (colorSeed < 0.5f)
            {
                // White (A-type)
                r = 0.95f + (float)rng.NextDouble() * 0.05f;
                g = 0.95f + (float)rng.NextDouble() * 0.05f;
                b = 1.0f;
            }
            else
            {
                // Blue-white (B/O-type)
                r = 0.7f + (float)rng.NextDouble() * 0.1f;
                g = 0.8f + (float)rng.NextDouble() * 0.1f;
                b = 1.0f;
            }

            int offset = i * FloatsPerStar;
            data[offset + 0] = x;
            data[offset + 1] = y;
            data[offset + 2] = z;
            data[offset + 3] = brightness;
            data[offset + 4] = r;
            data[offset + 5] = g;
            data[offset + 6] = b;
        }

        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            fixed (float* ptr = data)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
        }

        uint stride = FloatsPerStar * sizeof(float);

        // Position (location 0)
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

        // Brightness (location 1)
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

        // Color (location 2)
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 4 * sizeof(float));

        gl.BindVertexArray(0);
        _initialized = true;
    }

    public void Render(GL gl, ShaderProgram shader)
    {
        if (!_initialized || _starCount == 0) return;

        gl.Enable(EnableCap.ProgramPointSize);
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One); // Additive blending for stars
        gl.DepthMask(false); // Don't write to depth buffer

        shader.Use();
        gl.BindVertexArray(_vao);
        gl.DrawArrays(PrimitiveType.Points, 0, (uint)_starCount);
        gl.BindVertexArray(0);

        gl.DepthMask(true);
        gl.Disable(EnableCap.Blend);
        gl.Disable(EnableCap.ProgramPointSize);
    }

    public void Dispose()
    {
        if (_gl == null) return;
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
    }
}
