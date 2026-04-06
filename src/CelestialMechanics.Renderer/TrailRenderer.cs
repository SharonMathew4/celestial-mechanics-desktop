using System.Numerics;
using Silk.NET.OpenGL;

namespace CelestialMechanics.Renderer;

/// <summary>
/// Renders orbit trails for celestial bodies as fading line strips.
/// Stores a circular buffer of positions per body and renders them with
/// alpha fading from recent (bright) to old (transparent).
/// </summary>
public sealed class TrailRenderer : IDisposable
{
    private uint _vao;
    private uint _vbo;
    private GL? _gl;
    private bool _initialized;

    /// <summary>Maximum number of trail points per body.</summary>
    public int MaxTrailLength { get; set; } = 200;

    /// <summary>How many simulation frames to skip between recording trail points.</summary>
    public int RecordInterval { get; set; } = 3;

    // Trail data: body ID -> circular buffer of positions
    private readonly Dictionary<int, TrailBuffer> _trails = new();
    private int _frameCounter;

    // Vertex data for GPU upload: position(3) + color(4) = 7 floats per vertex
    private float[] _vertexData = new float[4096 * 7];
    private int _vertexCount;

    public void Initialize(GL gl)
    {
        _gl = gl;

        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        gl.BufferData(BufferTargetARB.ArrayBuffer, 0, ReadOnlySpan<byte>.Empty, BufferUsageARB.DynamicDraw);

        // Position (location 0)
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);

        // Color (location 1)
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));

        gl.BindVertexArray(0);
        _initialized = true;
    }

    /// <summary>
    /// Records current positions from the render state.
    /// Call every frame; internally throttles via RecordInterval.
    /// </summary>
    public void RecordPositions(RenderBody[] bodies, int bodyCount)
    {
        _frameCounter++;
        if (_frameCounter % RecordInterval != 0) return;

        // Track which body IDs are still active
        var activeIds = new HashSet<int>();

        for (int i = 0; i < bodyCount; i++)
        {
            ref var body = ref bodies[i];
            activeIds.Add(body.Id);

            if (!_trails.TryGetValue(body.Id, out var trail))
            {
                trail = new TrailBuffer(MaxTrailLength, GetTrailColor(body.BodyType));
                _trails[body.Id] = trail;
            }

            trail.Add(body.Position);
        }

        // Remove trails for bodies that no longer exist
        var staleIds = _trails.Keys.Where(id => !activeIds.Contains(id)).ToList();
        foreach (var id in staleIds)
            _trails.Remove(id);
    }

    /// <summary>
    /// Builds vertex data from all trails and uploads to GPU.
    /// </summary>
    public void Upload()
    {
        if (!_initialized || _gl == null) return;

        _vertexCount = 0;

        // Calculate required size
        int totalVertices = 0;
        foreach (var trail in _trails.Values)
            totalVertices += trail.Count;

        int requiredSize = totalVertices * 7;
        if (_vertexData.Length < requiredSize)
            _vertexData = new float[requiredSize * 2];

        foreach (var trail in _trails.Values)
        {
            int count = trail.Count;
            if (count < 2) continue;

            for (int i = 0; i < count; i++)
            {
                var pos = trail.GetAt(i);
                float alpha = (float)i / count; // 0 = oldest (faint), 1 = newest (bright)
                alpha = alpha * alpha * 0.6f; // Quadratic falloff, max 0.6 alpha

                int offset = _vertexCount * 7;
                _vertexData[offset + 0] = pos.X;
                _vertexData[offset + 1] = pos.Y;
                _vertexData[offset + 2] = pos.Z;
                _vertexData[offset + 3] = trail.Color.X;
                _vertexData[offset + 4] = trail.Color.Y;
                _vertexData[offset + 5] = trail.Color.Z;
                _vertexData[offset + 6] = alpha;
                _vertexCount++;
            }
        }

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        if (_vertexCount > 0)
        {
            unsafe
            {
                fixed (float* ptr = _vertexData)
                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_vertexCount * 7 * sizeof(float)), ptr, BufferUsageARB.DynamicDraw);
            }
        }
    }

    public void Render(GL gl, ShaderProgram shader)
    {
        if (!_initialized || _vertexCount < 2) return;

        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        shader.Use();
        gl.BindVertexArray(_vao);

        // Draw each trail as a separate line strip
        int drawOffset = 0;
        foreach (var trail in _trails.Values)
        {
            int count = trail.Count;
            if (count < 2)
            {
                drawOffset += count;
                continue;
            }
            gl.DrawArrays(PrimitiveType.LineStrip, drawOffset, (uint)count);
            drawOffset += count;
        }

        gl.BindVertexArray(0);
        gl.Disable(EnableCap.Blend);
    }

    public void Clear()
    {
        _trails.Clear();
        _vertexCount = 0;
        _frameCounter = 0;
    }

    private static Vector3 GetTrailColor(int bodyType) => bodyType switch
    {
        0 => new Vector3(1.0f, 0.8f, 0.3f),    // Star: golden
        1 => new Vector3(0.3f, 0.5f, 1.0f),     // Planet: blue
        2 => new Vector3(0.8f, 0.6f, 0.3f),     // GasGiant: amber
        3 => new Vector3(0.7f, 0.4f, 0.2f),     // Rocky: reddish
        4 => new Vector3(0.6f, 0.6f, 0.6f),     // Moon: gray
        5 => new Vector3(0.5f, 0.5f, 0.4f),     // Asteroid: brownish
        6 => new Vector3(0.4f, 0.7f, 1.0f),     // NeutronStar: cyan
        7 => new Vector3(0.6f, 0.2f, 0.8f),     // BlackHole: purple
        8 => new Vector3(0.5f, 0.8f, 0.9f),     // Comet: icy blue
        _ => new Vector3(0.5f, 0.5f, 0.5f),     // Custom: gray
    };

    public void Dispose()
    {
        if (_gl == null) return;
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
    }

    /// <summary>
    /// Circular buffer of 3D positions for a single body's trail.
    /// </summary>
    private class TrailBuffer
    {
        private readonly Vector3[] _positions;
        private int _head;
        private int _count;

        public Vector3 Color { get; }
        public int Count => _count;

        public TrailBuffer(int capacity, Vector3 color)
        {
            _positions = new Vector3[capacity];
            Color = color;
        }

        public void Add(Vector3 position)
        {
            _positions[_head] = position;
            _head = (_head + 1) % _positions.Length;
            if (_count < _positions.Length)
                _count++;
        }

        /// <summary>Gets position at index i where 0 is oldest and Count-1 is newest.</summary>
        public Vector3 GetAt(int i)
        {
            int start = (_head - _count + _positions.Length) % _positions.Length;
            return _positions[(start + i) % _positions.Length];
        }
    }
}
