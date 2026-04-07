using System.Diagnostics;
using System.Numerics;
using Silk.NET.OpenGL;
using CelestialMechanics.Simulation;

namespace CelestialMechanics.Renderer;

public class GLRenderer : IDisposable
{
    private GL? _gl;
    private InstancedSphereRenderer _sphereRenderer = new();
    private GridRenderer _gridRenderer = new();
    private LineRenderer _lineRenderer = new();
    private LineRenderer _previewLineRenderer = new(); // For velocity/trajectory preview
    private StarfieldRenderer _starfieldRenderer = new();
    private TrailRenderer _trailRenderer = new();
    private ShaderProgram? _sphereShader;
    private ShaderProgram? _gridShader;
    private ShaderProgram? _lineShader;
    private ShaderProgram? _starfieldShader;
    private ShaderProgram? _trailShader;
    private Camera _camera = new();
    private RenderState _renderState = new();

    // Running time for animated shaders
    private float _time;

    public Camera Camera => _camera;
    public RenderState RenderState => _renderState;
    public bool ShowGrid { get; set; } = true;
    public bool ShowVelocityArrows { get; set; } = false;
    public bool ShowStarfield { get; set; } = true;
    public bool ShowTrails { get; set; } = true;

    /// <summary>
    /// Index of the currently selected body instance (-1 = none).
    /// Set from the UI thread when user clicks a body.
    /// </summary>
    public int SelectedInstanceIndex { get; set; } = -1;

    // ── Ghost Body & Placement Preview (Module B) ────────────────────

    /// <summary>Whether to show the ghost body during placement.</summary>
    public bool ShowGhost { get; set; }

    /// <summary>Position of the ghost body.</summary>
    public Vector3 GhostPosition { get; set; }

    /// <summary>Radius of the ghost body.</summary>
    public float GhostRadius { get; set; } = 0.1f;

    /// <summary>Alpha transparency of the ghost body (0.4 during position, 1.0 when confirmed).</summary>
    public float GhostAlpha { get; set; } = 0.4f;

    /// <summary>Body type index for ghost rendering.</summary>
    public int GhostBodyType { get; set; }

    /// <summary>Whether to show the velocity preview line.</summary>
    public bool ShowVelocityPreview { get; set; }

    /// <summary>Start point of velocity preview line (placed position).</summary>
    public Vector3 VelocityPreviewStart { get; set; }

    /// <summary>End point of velocity preview line (cursor position).</summary>
    public Vector3 VelocityPreviewEnd { get; set; }

    /// <summary>Whether to show the trajectory preview.</summary>
    public bool ShowTrajectoryPreview { get; set; }

    /// <summary>Trajectory preview points (gravity-bent path).</summary>
    public Vector3[]? TrajectoryPreview { get; set; }

    public void Initialize(GL gl)
    {
        _gl = gl;

        string shaderDir = FindShaderDirectory();

        // Sphere shader
        string sphereVert = File.ReadAllText(Path.Combine(shaderDir, "sphere.vert"));
        string sphereFrag = File.ReadAllText(Path.Combine(shaderDir, "sphere.frag"));
        _sphereShader = new ShaderProgram(gl, sphereVert, sphereFrag);

        // Grid shader
        string gridVert = File.ReadAllText(Path.Combine(shaderDir, "grid.vert"));
        string gridFrag = File.ReadAllText(Path.Combine(shaderDir, "grid.frag"));
        _gridShader = new ShaderProgram(gl, gridVert, gridFrag);

        // Line shader
        string lineVert = File.ReadAllText(Path.Combine(shaderDir, "line.vert"));
        string lineFrag = File.ReadAllText(Path.Combine(shaderDir, "line.frag"));
        _lineShader = new ShaderProgram(gl, lineVert, lineFrag);

        // Starfield shader
        string starfieldVert = File.ReadAllText(Path.Combine(shaderDir, "starfield.vert"));
        string starfieldFrag = File.ReadAllText(Path.Combine(shaderDir, "starfield.frag"));
        _starfieldShader = new ShaderProgram(gl, starfieldVert, starfieldFrag);

        // Trail shader
        string trailVert = File.ReadAllText(Path.Combine(shaderDir, "trail.vert"));
        string trailFrag = File.ReadAllText(Path.Combine(shaderDir, "trail.frag"));
        _trailShader = new ShaderProgram(gl, trailVert, trailFrag);

        // Initialize renderers
        _sphereRenderer.Initialize(gl);
        _gridRenderer.Initialize(gl);
        _lineRenderer.Initialize(gl);
        _previewLineRenderer.Initialize(gl);
        _starfieldRenderer.Initialize(gl);
        _trailRenderer.Initialize(gl);
    }

    private static string FindShaderDirectory()
    {
        // Look for Shaders directory relative to the executable
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string shaderDir = Path.Combine(baseDir, "Shaders");
        if (Directory.Exists(shaderDir))
            return shaderDir;

        // Try looking relative to the source directory
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

    public void UpdateFromSimulation(SimulationEngine engine)
    {
        _renderState.UpdateFrom(engine);

        // Update sphere instances
        _sphereRenderer.UpdateInstances(_renderState.Bodies, _renderState.BodyCount);

        // Record trail positions
        if (ShowTrails)
        {
            _trailRenderer.RecordPositions(_renderState.Bodies, _renderState.BodyCount);
        }

        // Update velocity arrows if enabled
        if (ShowVelocityArrows && engine.Bodies != null)
        {
            _lineRenderer.Clear();
            foreach (var body in engine.Bodies)
            {
                if (!body.IsActive) continue;
                var pos = new Vector3((float)body.Position.X, (float)body.Position.Y, (float)body.Position.Z);
                var vel = new Vector3((float)body.Velocity.X, (float)body.Velocity.Y, (float)body.Velocity.Z);
                float velLen = vel.Length();
                if (velLen > 0.001f)
                {
                    var end = pos + Vector3.Normalize(vel) * MathF.Min(velLen * 0.5f, 2f);
                    _lineRenderer.AddLine(pos, end, new Vector4(0.0f, 1.0f, 0.4f, 0.8f));
                }
            }
            _lineRenderer.Upload();
        }
    }

    public void Render(float deltaTime, int width, int height)
    {
        if (_gl == null) return;

        _time += deltaTime;
        _camera.Update(deltaTime);

        float aspect = width / (float)System.Math.Max(height, 1);
        var view = _camera.GetViewMatrix();
        var projection = _camera.GetProjectionMatrix(aspect);
        var viewPos = _camera.Position;

        // ── Pass 1: Starfield background (render first, no depth write) ──
        if (ShowStarfield)
        {
            _gl.Disable(EnableCap.DepthTest);
            _starfieldShader!.Use();
            _starfieldShader.SetUniform("uView", view);
            _starfieldShader.SetUniform("uProjection", projection);
            _starfieldRenderer.Render(_gl, _starfieldShader);
            _gl.Enable(EnableCap.DepthTest);
        }

        // ── Pass 2: Grid ─────────────────────────────────────────────
        if (ShowGrid)
        {
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _gridShader!.Use();
            _gridShader.SetUniform("uView", view);
            _gridShader.SetUniform("uProjection", projection);
            _gridRenderer.Render(_gl, _gridShader);
            _gl.Disable(EnableCap.Blend);
        }

        // ── Pass 3: Orbit trails ─────────────────────────────────────
        if (ShowTrails)
        {
            _trailRenderer.Upload();
            _trailShader!.Use();
            _trailShader.SetUniform("uView", view);
            _trailShader.SetUniform("uProjection", projection);
            _trailRenderer.Render(_gl, _trailShader);
        }

        // ── Pass 4: Celestial bodies (main pass) ─────────────────────
        _sphereShader!.Use();
        _sphereShader.SetUniform("uView", view);
        _sphereShader.SetUniform("uProjection", projection);
        _sphereShader.SetUniform("uViewPos", viewPos);
        _sphereShader.SetUniform("uTime", _time);
        _sphereShader.SetUniform("uSelectedId", SelectedInstanceIndex);
        _sphereRenderer.Render(_gl, _sphereShader);

        // ── Pass 5: Velocity arrows ──────────────────────────────────
        if (ShowVelocityArrows)
        {
            _lineShader!.Use();
            _lineShader.SetUniform("uView", view);
            _lineShader.SetUniform("uProjection", projection);
            _lineRenderer.Render(_gl, _lineShader);
        }

        // ── Pass 6: Ghost body & Placement previews (Module B) ───────
        RenderPlacementPreview(view, projection, viewPos);
    }

    /// <summary>
    /// Renders the ghost body, velocity vector, and trajectory preview during placement.
    /// </summary>
    private void RenderPlacementPreview(Matrix4x4 view, Matrix4x4 projection, Vector3 viewPos)
    {
        if (_gl == null) return;

        // Render ghost body as a semi-transparent sphere
        if (ShowGhost)
        {
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Create a single-instance ghost body render
            var ghostBody = new RenderBody
            {
                Id = -999, // Special ID for ghost
                Position = GhostPosition,
                Radius = GhostRadius,
                Color = GetGhostColor(GhostBodyType, GhostAlpha),
                BodyType = GhostBodyType
            };

            // Render ghost using sphere shader
            _sphereShader!.Use();
            _sphereShader.SetUniform("uView", view);
            _sphereShader.SetUniform("uProjection", projection);
            _sphereShader.SetUniform("uViewPos", viewPos);
            _sphereShader.SetUniform("uTime", _time);
            _sphereShader.SetUniform("uSelectedId", -1); // Don't highlight ghost

            // Use single-instance render for ghost
            _sphereRenderer.RenderSingleInstance(_gl, _sphereShader, ghostBody);

            _gl.Disable(EnableCap.Blend);
        }

        // Render velocity preview line and trajectory
        if (ShowVelocityPreview || ShowTrajectoryPreview)
        {
            _previewLineRenderer.Clear();

            // Velocity vector line (cyan)
            if (ShowVelocityPreview)
            {
                _previewLineRenderer.AddLine(
                    VelocityPreviewStart,
                    VelocityPreviewEnd,
                    new Vector4(0.0f, 1.0f, 1.0f, 0.9f)); // Cyan
            }

            // Trajectory preview (orange, multiple segments)
            if (ShowTrajectoryPreview && TrajectoryPreview != null && TrajectoryPreview.Length > 1)
            {
                var trajectoryColor = new Vector4(1.0f, 0.6f, 0.0f, 0.7f); // Orange
                for (int i = 0; i < TrajectoryPreview.Length - 1; i++)
                {
                    // Fade alpha along trajectory
                    float alpha = 0.7f * (1.0f - (float)i / TrajectoryPreview.Length);
                    var color = new Vector4(1.0f, 0.6f, 0.0f, alpha);
                    _previewLineRenderer.AddLine(TrajectoryPreview[i], TrajectoryPreview[i + 1], color);
                }
            }

            _previewLineRenderer.Upload();

            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _lineShader!.Use();
            _lineShader.SetUniform("uView", view);
            _lineShader.SetUniform("uProjection", projection);
            _previewLineRenderer.Render(_gl, _lineShader);
            _gl.Disable(EnableCap.Blend);
        }
    }

    /// <summary>Gets the color for the ghost body based on body type.</summary>
    private static Vector4 GetGhostColor(int bodyType, float alpha) => bodyType switch
    {
        0 => new Vector4(1.0f, 0.9f, 0.3f, alpha), // Star
        1 or 3 => new Vector4(0.2f, 0.4f, 0.8f, alpha), // Planet/RockyPlanet
        2 => new Vector4(0.8f, 0.7f, 0.5f, alpha), // GasGiant
        4 => new Vector4(0.7f, 0.7f, 0.7f, alpha), // Moon
        5 or 8 => new Vector4(0.5f, 0.5f, 0.4f, alpha), // Asteroid/Comet
        6 => new Vector4(0.5f, 0.8f, 1.0f, alpha), // NeutronStar
        7 => new Vector4(0.1f, 0.0f, 0.1f, alpha), // BlackHole
        _ => new Vector4(0.6f, 0.6f, 0.6f, alpha),
    };

    /// <summary>Clears all trail data (e.g., on simulation reset).</summary>
    public void ClearTrails()
    {
        _trailRenderer.Clear();
    }

    public void Dispose()
    {
        _sphereRenderer.Dispose();
        _gridRenderer.Dispose();
        _lineRenderer.Dispose();
        _previewLineRenderer.Dispose();
        _starfieldRenderer.Dispose();
        _trailRenderer.Dispose();
        _sphereShader?.Dispose();
        _gridShader?.Dispose();
        _lineShader?.Dispose();
        _starfieldShader?.Dispose();
        _trailShader?.Dispose();
    }
}
