using System.Diagnostics;
using CelestialMechanics.Renderer;
using CelestialMechanics.Simulation;
using Silk.NET.OpenGL;

namespace CelestialMechanics.Desktop.Infrastructure;

/// <summary>
/// Dedicated render thread that owns the WGL context and drives GLRenderer.
/// </summary>
public class RenderLoop : IDisposable
{
    private Thread? _thread;
    private volatile bool _running;
    private OpenGLHost? _host;
    private GLRenderer? _renderer;
    private Func<Action<SimulationEngine>, bool>? _withEngineLock;
    private int _targetFps = 60;

    // Metrics (read by UI thread; written by render thread)
    // Using Interlocked for double fields since volatile is not allowed on double.
    private long _lastRenderTimeMsBits;
    private long _currentFpsBits;
    private volatile bool _initialized;

    public double LastRenderTimeMs => BitConverter.Int64BitsToDouble(Interlocked.Read(ref _lastRenderTimeMsBits));
    public double CurrentFps => BitConverter.Int64BitsToDouble(Interlocked.Read(ref _currentFpsBits));
    public bool IsInitialized => _initialized;

    /// <summary>
    /// Starts the render loop on a dedicated background thread.
    /// </summary>
    /// <param name="host">The HwndHost providing the WGL context.</param>
    /// <param name="renderer">The GLRenderer to drive.</param>
    /// <param name="withEngineLock">Callback that executes an action under the engine lock. Returns false if engine unavailable.</param>
    public void Start(OpenGLHost host, GLRenderer renderer, Func<Action<SimulationEngine>, bool> withEngineLock)
    {
        _host = host;
        _renderer = renderer;
        _withEngineLock = withEngineLock;
        _running = true;

        _thread = new Thread(Loop)
        {
            IsBackground = true,
            Name = "GL Render Thread",
            Priority = ThreadPriority.AboveNormal,
        };
        _thread.Start();
    }

    public void Stop()
    {
        _running = false;
        _thread?.Join(timeout: TimeSpan.FromMilliseconds(500));
        _thread = null;
    }

    private void Loop()
    {
        var host = _host!;
        var renderer = _renderer!;

        // 1. Bind WGL context to this thread
        Win32Interop.wglMakeCurrent(host.Hdc, host.Hglrc);

        var gl = host.Gl!;

        // 2. Initialize renderer (compiles shaders, creates GPU buffers)
        renderer.Initialize(gl);

        // 3. Set OpenGL state
        gl.Enable(EnableCap.DepthTest);
        gl.Enable(EnableCap.CullFace);
        gl.CullFace(TriangleFace.Back);
        gl.ClearColor(0.04f, 0.04f, 0.06f, 1.0f);

        _initialized = true;

        var sw = Stopwatch.StartNew();
        double lastTime = sw.Elapsed.TotalSeconds;
        int frameCount = 0;
        double fpsAccumulator = 0;

        while (_running)
        {
            double now = sw.Elapsed.TotalSeconds;
            float dt = (float)(now - lastTime);
            lastTime = now;

            int w = host.PixelWidth;
            int h = host.PixelHeight;

            if (w <= 0 || h <= 0)
            {
                Thread.Sleep(16);
                continue;
            }

            gl.Viewport(0, 0, (uint)w, (uint)h);
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Read simulation state under lock and update renderer
            _withEngineLock?.Invoke(engine =>
            {
                renderer.UpdateFromSimulation(engine);
            });

            // Render the scene
            var renderSw = Stopwatch.StartNew();
            renderer.Render(dt, w, h);
            renderSw.Stop();
            Interlocked.Exchange(ref _lastRenderTimeMsBits, BitConverter.DoubleToInt64Bits(renderSw.Elapsed.TotalMilliseconds));

            // Present
            Win32Interop.SwapBuffers(host.Hdc);

            // FPS tracking
            frameCount++;
            fpsAccumulator += dt;
            if (fpsAccumulator >= 0.5)
            {
                Interlocked.Exchange(ref _currentFpsBits, BitConverter.DoubleToInt64Bits(frameCount / fpsAccumulator));
                frameCount = 0;
                fpsAccumulator = 0;
            }

            // Frame pacing
            double frameMs = (sw.Elapsed.TotalSeconds - now) * 1000.0;
            double targetMs = 1000.0 / _targetFps;
            int sleepMs = (int)(targetMs - frameMs);
            if (sleepMs > 1)
                Thread.Sleep(sleepMs);
        }

        // Cleanup: release context from this thread
        Win32Interop.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
    }

    public void Dispose()
    {
        Stop();
    }
}
