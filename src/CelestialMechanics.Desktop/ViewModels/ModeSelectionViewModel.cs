using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// ViewModel for the initial Mode Selection modal.
/// Presents Simulation (active) and Observation (disabled/future) choices.
/// </summary>
public sealed partial class ModeSelectionViewModel : ObservableObject
{
    /// <summary>Raised when the user selects Simulation mode.</summary>
    public event Action? SimulationSelected;

    /// <summary>Raised when the user clicks Exit.</summary>
    public event Action? ExitRequested;

    [RelayCommand]
    private void SelectSimulation()
    {
        SimulationSelected?.Invoke();
    }

    [RelayCommand]
    private void Exit()
    {
        ExitRequested?.Invoke();
    }
}
