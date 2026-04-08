using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Silk.NET.OpenGL;
using CelestialMechanics.Simulation;

namespace CelestialMechanics.Renderer;

public class GLRenderer : IDisposable
{
    private GL? _gl;
    private InstancedSphereRenderer _sphereRenderer  = new();
    private GridRenderer            _gridRenderer     = new();
    private LineRenderer            _lineRenderer     = new();
    private LineRenderer            _previewLineRenderer = new(); // velocity/trajectory preview
    private StarfieldRenderer       _starfieldRenderer = new();
    private TrailRenderer           _trailRenderer    = new();
    private ShaderProgram? _sphereShader;
    private ShaderProgram? _gridShader;
    private ShaderProgram? _lineShader;
    private ShaderProgram? _starfieldShader;
    private ShaderProgram? _trailShader;
    private Camera    _camera     = new();
    private RenderState _renderState = new();

    // Running time for animated shaders
    private float _time;

    public Camera      Camera      => _camera;
    public RenderState RenderState => _renderState;
    public bool ShowGrid          { get; set; } = true;
    public bool ShowVelocityArrows{ get; set; } = false;
    public bool ShowStarfield     { get; set; } = true;
    public bool ShowTrails        { get; set; } = true;

    /// <summary>Index of the currently selected body instance (-1 = none).</summary>
    public int SelectedInstanceIndex { get; set; } = -1;

    // ── Ghost Body & Placement Preview ───────────────────────────────────
    public bool ShowGhost            { get; set; }
    public Vector3 GhostPosition     { get; set; }
    public float GhostRadius         { get; set; } = 0.1f;
    public float GhostAlpha          { get; set; } = 0.4f;
    public int GhostBodyType         { get; set; }
    public bool ShowVelocityPreview  { get; set; }
    public Vector3 VelocityPreviewStart { get; set; }
    public Vector3 VelocityPreviewEnd   { get; set; }
    public bool ShowTrajectoryPreview   { get; set; }
    public Vector3[]? TrajectoryPreview { get; set; }

    // ── Collision Flash Effects ───────────────────────────────────────────
    private readonly struct CollisionFlash
    {
        public readonly Vector3 Position;
        public readonly float   StartTime;
        public const float      Duration = 0.55f; // seconds the flash lasts
        public CollisionFlash(Vector3 pos, float t) { Position = pos; StartTime = t; }
    }
    private readonly List<CollisionFlash> _collisionFlashes = new();

    // ── Star Glow (per-frame list, rebuilt from render state) ────────────
    private readonly List<(Vector3 pos, float radius)> _starGlows = new();

    public void Initialize(GL gl)
    {
        _gl = gl;

        string shaderDir = FindShaderDirectory();

        string sphereVert    = File.ReadAllText(Path.Combine(shaderDir, "sphere.vert"));
        string sphereFrag    = File.ReadAllText(Path.Combine(shaderDir, "sphere.frag"));
        _sphereShader = new ShaderProgram(gl, sphereVert, sphereFrag);

        string gridVert      = File.ReadAllText(Path.Combine(shaderDir, "grid.vert"));
        string gridFrag      = File.ReadAllText(Path.Combine(shaderDir, "grid.frag"));
        _gridShader = new ShaderProgram(gl, gridVert, gridFrag);

        string lineVert      = File.ReadAllText(Path.Combine(shaderDir, "line.vert"));
        string lineFrag      = File.ReadAllText(Path.Combine(shaderDir, "line.frag"));
        _lineShader = new ShaderProgram(gl, lineVert, lineFrag);

        string starfieldVert = File.ReadAllText(Path.Combine(shaderDir, "starfield.vert"));
        string starfieldFrag = File.ReadAllText(Path.Combine(shaderDir, "starfield.frag"));
        _starfieldShader = new ShaderProgram(gl, starfieldVert, starfieldFrag);

        string trailVert     = File.ReadAllText(Path.Combine(shaderDir, "trail.vert"));
        string trailFrag     = File.ReadAllText(Path.Combine(shaderDir, "trail.frag"));
        _trailShader = new ShaderProgram(gl, trailVert, trailFrag);

        _sphereRenderer.Initialize(gl);
        _gridRenderer.Initialize(gl);
        _lineRenderer.Initialize(gl);
        _previewLineRenderer.Initialize(gl);
        _starfieldRenderer.Initialize(gl);
        _trailRenderer.Initialize(gl);
    }

    private static string FindShaderDirectory()
    {
        string baseDir    = AppDomain.CurrentDomain.BaseDirectory;
        string shaderDir  = Path.Combine(baseDir, "Shaders");
        if (Directory.Exists(shaderDir)) return shaderDir;

        string? dir = baseDir;
        for (int i = 0; i < 6 && dir != null; i++)
        {
            string candidate = Path.Combine(dir, "src", "CelestialMechanics.Renderer", "Shaders");
            if (Directory.Exists(candidate)) return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        throw new FileNotFoundException($"Could not find Shaders directory. Searched from: {baseDir}");
    }

    public void UpdateFromSimulation(SimulationEngine engine)
    {
        _renderState.UpdateFrom(engine);

        // Collect new collision flash events
        foreach (var pos in _renderState.NewCollisionPositions)
            _collisionFlashes.Add(new CollisionFlash(pos, _time));

        // Update sphere instances
        _sphereRenderer.UpdateInstances(_renderState.Bodies, _renderState.BodyCount);

        // Record trail positions
        if (ShowTrails)
            _trailRenderer.RecordPositions(_renderState.Bodies, _renderState.BodyCount);

        // Rebuild star glow list
        _starGlows.Clear();
        for (int i = 0; i < _renderState.BodyCount; i++)
        {
            ref var b = ref _renderState.Bodies[i];
            if (b.BodyType == 0 /* Star */ || b.BodyType == 6 /* NeutronStar */)
                _starGlows.Add((b.Position, b.Radius));
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

        // Expire old collision flashes
        _collisionFlashes.RemoveAll(f => _time - f.StartTime > CollisionFlash.Duration);

        float aspect     = width / (float)System.Math.Max(height, 1);
        var   view       = _camera.GetViewMatrix();
        var   projection = _camera.GetProjectionMatrix(aspect);
        var   viewPos    = _camera.Position;

        // ── Pass 1: Starfield background (no depth write) ─────────────────
        if (ShowStarfield)
        {
            _gl.Disable(EnableCap.DepthTest);
            _starfieldShader!.Use();
            _starfieldShader.SetUniform("uView",       view);
            _starfieldShader.SetUniform("uProjection", projection);
            _starfieldRenderer.Render(_gl, _starfieldShader);
            _gl.Enable(EnableCap.DepthTest);
        }

        // ── Pass 2: Grid ───────────────────────────────────────────────────
        if (ShowGrid)
        {
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _gridShader!.Use();
            _gridShader.SetUniform("uView",       view);
            _gridShader.SetUniform("uProjection", projection);
            _gridRenderer.Render(_gl, _gridShader);
            _gl.Disable(EnableCap.Blend);
        }

        // ── Pass 3: Orbit trails ───────────────────────────────────────────
        if (ShowTrails)
        {
            _trailRenderer.Upload();
            _trailShader!.Use();
            _trailShader.SetUniform("uView",       view);
            _trailShader.SetUniform("uProjection", projection);
            _trailRenderer.Render(_gl, _trailShader);
        }

        // ── Pass 4: Celestial bodies (main pass) ───────────────────────────
        _sphereShader!.Use();
        _sphereShader.SetUniform("uView",       view);
        _sphereShader.SetUniform("uProjection", projection);
        _sphereShader.SetUniform("uViewPos",    viewPos);
        _sphereShader.SetUniform("uTime",       _time);
        _sphereShader.SetUniform("uSelectedId", SelectedInstanceIndex);
        _sphereRenderer.Render(_gl, _sphereShader);

        // ── Pass 5: Star glow / corona (additive blend, after bodies) ─────
        RenderStarGlows(view, projection, viewPos);

        // ── Pass 6: Velocity arrows ────────────────────────────────────────
        if (ShowVelocityArrows)
        {
            _lineShader!.Use();
            _lineShader.SetUniform("uView",       view);
            _lineShader.SetUniform("uProjection", projection);
            _lineRenderer.Render(_gl, _lineShader);
        }

        // ── Pass 7: Collision flash effects ───────────────────────────────
        RenderCollisionFlashes(view, projection, viewPos);

        // ── Pass 8: Ghost body & placement previews ───────────────────────
        RenderPlacementPreview(view, projection, viewPos);
    }

    // ── Star Glow / Corona ────────────────────────────────────────────────

    private void RenderStarGlows(Matrix4x4 view, Matrix4x4 projection, Vector3 viewPos)
    {
        if (_gl == null || _starGlows.Count == 0) return;

        _gl.Enable(EnableCap.Blend);
        // Additive blending for the bright halo effect
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        _gl.DepthMask(false);
        _gl.Disable(EnableCap.CullFace); // render both faces of glow sphere

        _sphereShader!.Use();
        _sphereShader.SetUniform("uView",       view);
        _sphereShader.SetUniform("uProjection", projection);
        _sphereShader.SetUniform("uViewPos",    viewPos);
        _sphereShader.SetUniform("uTime",       _time);
        _sphereShader.SetUniform("uSelectedId", -1); // Don't highlight glow

        foreach (var (pos, radius) in _starGlows)
        {
            // Inner glow — moderate size, warm yellow
            var inner = new RenderBody
            {
                Id       = -900,
                Position = pos,
                Radius   = radius * 1.4f,
                Color    = new Vector4(1.0f, 0.85f, 0.35f, 0.22f),
                BodyType = 9 // Custom = plain colour from vColor
            };
            _sphereRenderer.RenderSingleInstance(_gl, _sphereShader, inner);

            // Outer corona — larger size, faint white-gold
            var outer = new RenderBody
            {
                Id       = -901,
                Position = pos,
                Radius   = radius * 2.2f,
                Color    = new Vector4(1.0f, 0.96f, 0.7f, 0.09f),
                BodyType = 9
            };
            _sphereRenderer.RenderSingleInstance(_gl, _sphereShader, outer);
        }

        _gl.Enable(EnableCap.CullFace);
        _gl.DepthMask(true);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Disable(EnableCap.Blend);
    }

    // ── Collision Flashes ─────────────────────────────────────────────────

    private void RenderCollisionFlashes(Matrix4x4 view, Matrix4x4 projection, Vector3 viewPos)
    {
        if (_gl == null || _collisionFlashes.Count == 0) return;

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One); // Additive
        _gl.DepthMask(false);
        _gl.Disable(EnableCap.CullFace);

        _sphereShader!.Use();
        _sphereShader.SetUniform("uView",       view);
        _sphereShader.SetUniform("uProjection", projection);
        _sphereShader.SetUniform("uViewPos",    viewPos);
        _sphereShader.SetUniform("uTime",       _time);
        _sphereShader.SetUniform("uSelectedId", -1);

        foreach (var flash in _collisionFlashes)
        {
            float elapsed  = _time - flash.StartTime;
            float progress = elapsed / CollisionFlash.Duration; // 0→1
            float alpha    = MathF.Max(0f, 1f - progress);
            float scale    = 0.3f + progress * 2.5f; // Expanding flash

            // Bright white-yellow expanding sphere
            var body = new RenderBody
            {
                Id       = -800,
                Position = flash.Position,
                Radius   = scale,
                Color    = new Vector4(1.0f, 0.9f, 0.5f, alpha * 0.85f),
                BodyType = 9 // Custom colour path
            };
            _sphereRenderer.RenderSingleInstance(_gl, _sphereShader, body);

            // Bright core flash (white)
            if (progress < 0.25f)
            {
                float coreAlpha = alpha * 1.5f;
                var core = new RenderBody
                {
                    Id       = -801,
                    Position = flash.Position,
                    Radius   = scale * 0.35f,
                    Color    = new Vector4(1.0f, 1.0f, 1.0f, System.Math.Min(1f, coreAlpha)),
                    BodyType = 9
                };
                _sphereRenderer.RenderSingleInstance(_gl, _sphereShader, core);
            }
        }

        _gl.Enable(EnableCap.CullFace);
        _gl.DepthMask(true);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Disable(EnableCap.Blend);
    }

    // ── Placement Preview (ghost + velocity arrow with arrowhead + trajectory) ──

    private void RenderPlacementPreview(Matrix4x4 view, Matrix4x4 projection, Vector3 viewPos)
    {
        if (_gl == null) return;

        // ── Ghost body ────────────────────────────────────────────────────
        if (ShowGhost)
        {
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _gl.DepthMask(false);

            // Lift ghost above the grid plane to avoid z-fighting
            var ghostPos = GhostPosition;
            ghostPos.Y = GhostPosition.Y + GhostRadius;

            var ghostBody = new RenderBody
            {
                Id       = -999,
                Position = ghostPos,
                Radius   = GhostRadius,
                Color    = GetGhostColor(GhostBodyType, GhostAlpha),
                BodyType = GhostBodyType
            };

            _sphereShader!.Use();
            _sphereShader.SetUniform("uView",       view);
            _sphereShader.SetUniform("uProjection", projection);
            _sphereShader.SetUniform("uViewPos",    viewPos);
            _sphereShader.SetUniform("uTime",       _time);
            _sphereShader.SetUniform("uSelectedId", -1);
            _sphereRenderer.RenderSingleInstance(_gl, _sphereShader, ghostBody);

            _gl.DepthMask(true);
            _gl.Disable(EnableCap.Blend);
        }

        // ── Velocity preview line + arrowhead + trajectory ────────────────
        if (ShowVelocityPreview || ShowTrajectoryPreview)
        {
            _previewLineRenderer.Clear();

            if (ShowVelocityPreview)
            {
                var start = VelocityPreviewStart;
                var end   = VelocityPreviewEnd;
                var dir   = end - start;
                float len = dir.Length();

                // Main velocity line — bright cyan
                var lineColor = new Vector4(0.0f, 1.0f, 1.0f, 0.95f);
                _previewLineRenderer.AddLine(start, end, lineColor);

                // ── Arrowhead at tip ──────────────────────────────────────
                if (len > 0.01f)
                {
                    var normDir = dir / len;

                    // Compute a perpendicular in the XZ plane (or fallback to Y)
                    Vector3 perp;
                    if (MathF.Abs(normDir.Y) < 0.9f)
                        perp = Vector3.Normalize(Vector3.Cross(normDir, Vector3.UnitY));
                    else
                        perp = Vector3.Normalize(Vector3.Cross(normDir, Vector3.UnitX));

                    float arrowLen   = MathF.Min(len * 0.22f, 0.5f);
                    float arrowWidth = arrowLen * 0.45f;

                    var arrowBase = end - normDir * arrowLen;
                    var arrowL    = arrowBase + perp * arrowWidth;
                    var arrowR    = arrowBase - perp * arrowWidth;

                    var arrowColor = new Vector4(0.0f, 1.0f, 0.85f, 1.0f);
                    _previewLineRenderer.AddLine(end, arrowL, arrowColor);
                    _previewLineRenderer.AddLine(end, arrowR, arrowColor);
                    // Cross-bar for bold look
                    _previewLineRenderer.AddLine(arrowL, arrowR, arrowColor);
                }
            }

            // Gravity-bent trajectory preview — fading orange
            if (ShowTrajectoryPreview && TrajectoryPreview != null && TrajectoryPreview.Length > 1)
            {
                for (int i = 0; i < TrajectoryPreview.Length - 1; i++)
                {
                    float alpha = 0.75f * (1.0f - (float)i / TrajectoryPreview.Length);
                    var color   = new Vector4(1.0f, 0.55f, 0.0f, alpha);
                    _previewLineRenderer.AddLine(TrajectoryPreview[i], TrajectoryPreview[i + 1], color);
                }
            }

            _previewLineRenderer.Upload();

            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _lineShader!.Use();
            _lineShader.SetUniform("uView",       view);
            _lineShader.SetUniform("uProjection", projection);
            _previewLineRenderer.Render(_gl, _lineShader);
            _gl.Disable(EnableCap.Blend);
        }
    }

    private static Vector4 GetGhostColor(int bodyType, float alpha) => bodyType switch
    {
        0 => new Vector4(1.0f, 0.9f, 0.3f, alpha),         // Star
        1 or 3 => new Vector4(0.2f, 0.4f, 0.8f, alpha),    // Planet/RockyPlanet
        2 => new Vector4(0.8f, 0.7f, 0.5f, alpha),         // GasGiant
        4 => new Vector4(0.7f, 0.7f, 0.7f, alpha),         // Moon
        5 or 8 => new Vector4(0.5f, 0.5f, 0.4f, alpha),    // Asteroid/Comet
        6 => new Vector4(0.5f, 0.8f, 1.0f, alpha),         // NeutronStar
        7 => new Vector4(0.1f, 0.0f, 0.1f, alpha),         // BlackHole
        _ => new Vector4(0.6f, 0.6f, 0.6f, alpha),
    };

    /// <summary>Clears all trail data (on simulation reset).</summary>
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
