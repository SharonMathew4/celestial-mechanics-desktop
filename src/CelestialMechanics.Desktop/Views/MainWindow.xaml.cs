using System.Windows;
using CelestialMechanics.Desktop.ViewModels;

namespace CelestialMechanics.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnWindowLoaded;
        Closing += OnWindowClosing;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        Viewport.Initialize(vm.Renderer, vm.SimService);
        Viewport.ViewModel = vm;
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        Viewport.Shutdown();
        if (DataContext is MainWindowViewModel vm)
        {
            vm.Dispose();
        }
    }
}
