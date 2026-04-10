using CommunityToolkit.Mvvm.Messaging;
using CelestialMechanics.Desktop.Services.Messaging;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.Services.Physics;

public sealed class PhysicsEngine : IDisposable
{
    private readonly SimulationService _simulationService;
    private readonly TimeSpan _uiBroadcastInterval = TimeSpan.FromMilliseconds(1000.0 / 60.0);
    private readonly object _loopGate = new();
    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public PhysicsEngine(SimulationService simulationService)
    {
        _simulationService = simulationService;
        _simulationService.SetIntegrator("Verlet");
    }

    public bool IsRunning => _loopTask is { IsCompleted: false };

    public void Start()
    {
        lock (_loopGate)
        {
            if (IsRunning)
            {
                return;
            }

            _cts = new CancellationTokenSource();
            _simulationService.Play();
            _loopTask = Task.Run(() => RunLoop(_cts.Token), _cts.Token);
        }
    }

    public void Pause()
    {
        _simulationService.Pause();
    }

    public void Stop()
    {
        lock (_loopGate)
        {
            _cts?.Cancel();
        }
        _simulationService.Pause();
    }

    private async Task RunLoop(CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var lastTick = stopwatch.Elapsed;
        var lastUiPush = stopwatch.Elapsed;

        while (!cancellationToken.IsCancellationRequested)
        {
            var now = stopwatch.Elapsed;
            var dt = (now - lastTick).TotalSeconds;
            lastTick = now;

            _simulationService.AdvanceTick(dt);

            if (now - lastUiPush >= _uiBroadcastInterval)
            {
                lastUiPush = now;
                if (_simulationService.LastSimState is SimulationState state)
                {
                    WeakReferenceMessenger.Default.Send(new StateUpdateMessage(
                        state,
                        _simulationService.LastSimTime,
                        _simulationService.LastPhysicsTimeMs));
                }
            }

            await Task.Yield();
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
