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
    private readonly SimulationClock _clock;
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
    /// Raised on the simulation thread after each physics update.
    /// Subscribers must marshal to the UI thread.
    /// </summary>
    public event Action<SimulationState>? StateUpdated;

    public SimulationService()
    {
        _engine = new SimulationEngine();
        _clock = new SimulationClock();
    }

    public void StartSimThread()
    {
        if (_simThread != null) return;
        _running = true;
        _clock.Start();

        _simThread = new Thread(SimLoop)
        {
            IsBackground = true,
            Name = "Simulation Thread",
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
        double lastTime = sw.Elapsed.TotalSeconds;
        double uiUpdateAccumulator = 0;

        while (_running)
        {
            double now = sw.Elapsed.TotalSeconds;
            double dt = now - lastTime;
            lastTime = now;

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

            // Throttle UI updates to ~30 Hz
            uiUpdateAccumulator += dt;
            if (uiUpdateAccumulator >= 1.0 / 30.0)
            {
                uiUpdateAccumulator = 0;
                StateUpdated?.Invoke(_engine.CurrentState);
            }

            Thread.Sleep(1);
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

    public void Dispose()
    {
        StopSimThread();
    }
}
