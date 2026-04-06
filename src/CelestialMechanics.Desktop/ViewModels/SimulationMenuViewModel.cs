using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Simulation Menu modal.
/// Hub with options: New Project, File, Projects, Settings.
/// </summary>
public sealed partial class SimulationMenuViewModel : ObservableObject
{
    public event Action? NewProjectRequested;
    public event Action? FileRequested;
    public event Action? ProjectsRequested;
    public event Action? SettingsRequested;
    public event Action? BackRequested;

    [RelayCommand]
    private void NewProject()
    {
        NewProjectRequested?.Invoke();
    }

    [RelayCommand]
    private void File()
    {
        FileRequested?.Invoke();
    }

    [RelayCommand]
    private void Projects()
    {
        ProjectsRequested?.Invoke();
    }

    [RelayCommand]
    private void Settings()
    {
        SettingsRequested?.Invoke();
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke();
    }
}
