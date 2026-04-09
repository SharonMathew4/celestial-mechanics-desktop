using System;
using System.Windows;
namespace CelestialMechanics.App;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var app = new global::CelestialMechanics.Desktop.App();
        app.InitializeComponent();
        var mainWindow = new global::CelestialMechanics.Desktop.Views.MainWindow();
        app.MainWindow = mainWindow;
        app.Run(mainWindow);
    }
}
