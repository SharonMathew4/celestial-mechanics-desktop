using System.Windows;
using CelestialMechanics.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CelestialMechanics.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        var appState = App.Services.GetRequiredService<CelestialMechanics.Desktop.Core.AppState>();
        if (appState.CurrentMode == CelestialMechanics.Desktop.Core.AppMode.Simulation)
        {
            var projectService = App.Services.GetRequiredService<CelestialMechanics.Desktop.Services.ProjectService>();
            if (!projectService.IsSaved)
            {
                var result = MessageBox.Show(
                    "You have unsaved simulation changes. Save before exiting?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Cancel)
                {
                    // Cancel the window closing
                    e.Cancel = true;
                    return;
                }

                if (result == MessageBoxResult.Yes)
                {
                    // Attempt to trigger the Save command on the SimulationViewModel
                    var simVm = App.Services.GetService<SimulationViewModel>();
                    simVm?.SaveCommand.Execute(null);
                }
            }
        }
    }
}
