using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// ViewModel for the File Menu modal.
/// Provides Save, Save As, Import, Share, and Preferences options.
/// </summary>
public sealed partial class FileMenuViewModel : ObservableObject
{
    public event Action? SaveRequested;
    public event Action? SaveAsRequested;
    public event Action? ImportRequested;
    public event Action? PreferencesRequested;
    public event Action? BackRequested;

    [RelayCommand]
    private void Save()
    {
        SaveRequested?.Invoke();
    }

    [RelayCommand]
    private void SaveAs()
    {
        SaveAsRequested?.Invoke();
    }

    [RelayCommand]
    private void Import()
    {
        ImportRequested?.Invoke();
    }

    [RelayCommand]
    private void Share()
    {
        // Future feature — disabled in the view
    }

    [RelayCommand]
    private void Preferences()
    {
        PreferencesRequested?.Invoke();
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke();
    }
}
