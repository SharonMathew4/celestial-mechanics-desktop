using System.Windows;
using CelestialMechanics.Desktop.ViewModels;
using CelestialMechanics.Desktop.Views;

namespace CelestialMechanics.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel(System.Windows.Threading.Dispatcher.CurrentDispatcher)
        };

        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
