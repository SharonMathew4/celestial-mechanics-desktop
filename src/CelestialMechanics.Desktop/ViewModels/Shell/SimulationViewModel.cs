using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CelestialMechanics.Desktop.Services;
using CelestialMechanics.Desktop.Services.Physics;

namespace CelestialMechanics.Desktop.ViewModels.Shell;

public sealed partial class SimulationViewModel : ObservableObject
{
    private readonly PhysicsEngine _physicsEngine;
    private bool _started;

    public StatusBarViewModel StatusBar { get; }

    public SimulationViewModel(
        PhysicsEngine physicsEngine,
        CameraManager cameraManager,
        StatusBarViewModel statusBar)
    {
        _physicsEngine = physicsEngine;
        StatusBar = statusBar;
        _ = cameraManager;
    }

    public void EnsureStarted()
    {
        if (_started)
        {
            return;
        }

        _started = true;
        _physicsEngine.Start();
    }

    [RelayCommand]
    private void StartSimulation() => _physicsEngine.Start();

    [RelayCommand]
    private void PauseSimulation() => _physicsEngine.Pause();

    [RelayCommand]
    private void StopSimulation() => _physicsEngine.Stop();
}
