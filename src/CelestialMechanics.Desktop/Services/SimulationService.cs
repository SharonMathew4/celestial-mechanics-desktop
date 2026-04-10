using CelestialMechanics.Math;
using CelestialMechanics.Physics.Types;
using CelestialMechanics.Simulation;

namespace CelestialMechanics.Desktop.Services;

/// <summary>
/// Thread-safe wrapper around SimulationEngine.
/// All access to the engine is serialized through _engineLock.
/// </summary>
public class SimulationService : IDisposable
{
    private readonly SimulationEngine _engine;
    private readonly object _engineLock = new();
    private Thread? _simThread;
    private volatile bool _running;
    private long _timeScaleBits = BitConverter.DoubleToInt64Bits(1.0);

    /// <summary>
    /// Time scale multiplier for the simulation (0.1x to 10x).
    /// Updated from the UI thread, read from the sim thread.
    /// </summary>
    public double TimeScale
    {
        get => BitConverter.Int64BitsToDouble(Interlocked.Read(ref _timeScaleBits));
        set => Interlocked.Exchange(ref _timeScaleBits,
            BitConverter.DoubleToInt64Bits(System.Math.Clamp(value, 0.1, 10.0)));
    }

    // Snapshot of state for UI reads (updated from sim thread)
    private volatile EngineState _lastState = EngineState.Stopped;
    private long _lastSimTimeBits;
    private long _lastPhysicsTimeMsBits;
    private SimulationState? _lastSimState;

    public EngineState LastState => _lastState;
    public double LastSimTime => BitConverter.Int64BitsToDouble(Interlocked.Read(ref _lastSimTimeBits));
    public double LastPhysicsTimeMs => BitConverter.Int64BitsToDouble(Interlocked.Read(ref _lastPhysicsTimeMsBits));
    public SimulationState? LastSimState => _lastSimState;

    /// <summary>
    /// Raised when state is advanced.
    /// </summary>
    public event Action<SimulationState>? StateUpdated;

    public SimulationService()
    {
        _engine = new SimulationEngine();
    }

    // Backward-compatible thread API used by legacy ViewModels.
    public void StartSimThread()
    {
        if (_simThread != null) return;
        _running = true;
        _simThread = new Thread(SimLoop)
        {
            IsBackground = true,
            Name = "Simulation Thread"
        };
        _simThread.Start();
    }

    public void StopSimThread()
    {
        _running = false;
        _simThread?.Join(timeout: TimeSpan.FromMilliseconds(500));
        _simThread = null;
    }

    private void SimLoop()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var last = sw.Elapsed.TotalSeconds;
        while (_running)
        {
            var now = sw.Elapsed.TotalSeconds;
            var dt = now - last;
            last = now;
            AdvanceTick(dt);
            Thread.Sleep(1);
        }
    }

    public void AdvanceTick(double dt)
    {
        var physicsSw = System.Diagnostics.Stopwatch.StartNew();
        lock (_engineLock)
        {
            _engine.Update(dt * TimeScale);
            _lastState = _engine.State;
            Interlocked.Exchange(ref _lastSimTimeBits, BitConverter.DoubleToInt64Bits(_engine.CurrentTime));
            _lastSimState = _engine.CurrentState;
        }
        physicsSw.Stop();
        Interlocked.Exchange(ref _lastPhysicsTimeMsBits, BitConverter.DoubleToInt64Bits(physicsSw.Elapsed.TotalMilliseconds));

        if (_lastSimState is not null)
        {
            StateUpdated?.Invoke(_lastSimState);
        }
    }

    // ── Thread-safe commands ────────────────────────────────────────

    public void Play()
    {
        lock (_engineLock) _engine.Start();
    }

    public void Pause()
    {
        lock (_engineLock) _engine.Pause();
    }

    public void Step()
    {
        lock (_engineLock) _engine.StepOnce();
    }

    /// <summary>
    /// Stops the simulation and clears all bodies.
    /// Results in an empty universe — no auto-spawned objects.
    /// </summary>
    public void ResetScene()
    {
        lock (_engineLock)
        {
            _engine.Stop();
            _engine.SetBodies(Array.Empty<PhysicsBody>());
        }
    }

    public void SetIntegrator(string name)
    {
        lock (_engineLock) _engine.SetIntegrator(name);
    }

    public string GetIntegratorName()
    {
        lock (_engineLock) return _engine.GetIntegratorName();
    }

    public void AddBody(PhysicsBody body)
    {
        lock (_engineLock) _engine.AddBody(body);
    }

    public void RemoveBody(int id)
    {
        lock (_engineLock) _engine.RemoveBody(id);
    }

    public void LoadBodies(PhysicsBody[] bodies)
    {
        lock (_engineLock) _engine.SetBodies(bodies);
    }

    public PhysicsBody[] GetBodies()
    {
        lock (_engineLock) return _engine.Bodies.ToArray();
    }

    public void ApplyConfig(Action<PhysicsConfig> mutate)
    {
        lock (_engineLock)
        {
            mutate(_engine.Config);
            _engine.ApplyConfig();
        }
    }

    /// <summary>
    /// Executes an action under the engine lock. Used by the render thread
    /// to safely read simulation state.
    /// </summary>
    public bool WithEngineLock(Action<SimulationEngine> action)
    {
        lock (_engineLock)
        {
            action(_engine);
            return true;
        }
    }

    /// <summary>
    /// Offsets a body's position by the given delta (for Edit mode dragging).
    /// Uses body index for efficiency.
    /// </summary>
    public void OffsetBodyPosition(int bodyIndex, float dx, float dy, float dz)
    {
        lock (_engineLock)
        {
            var bodies = _engine.Bodies;
            if (bodies == null || bodyIndex < 0 || bodyIndex >= bodies.Length) return;

            ref var body = ref bodies[bodyIndex];
            body.Position = new Vec3d(body.Position.X + dx, body.Position.Y + dy, body.Position.Z + dz);
        }
    }

    public void Dispose()
    {
        StopSimThread();
    }
}
