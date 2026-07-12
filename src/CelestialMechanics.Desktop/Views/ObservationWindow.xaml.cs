using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CelestialMechanics.Observation.Core;
using CelestialMechanics.Observation.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CelestialMechanics.Desktop.Views;

/// <summary>
/// Interaction logic for ObservationWindow.xaml
/// </summary>
public partial class ObservationWindow : Window
{
    private readonly ObservationBootstrap _bootstrap;
    private readonly ObservationController _controller;
    private readonly ICameraService _cameraService;
    private readonly IRenderService _renderService;
    private readonly ISelectionService _selectionService;
    private readonly ILayerService _layerService;

    private Point _lastMousePosition;
    private bool _isRightDragging;
    private bool _isLeftDragging;
    private DateTime _lastTick = DateTime.Now;

    public ObservationWindow()
    {
        InitializeComponent();
        
        _bootstrap = ObservationModule.CreateBootstrap();
        _bootstrap.Initialize();
        _controller = _bootstrap.Controller;

        // Resolve services from DI Container
        _cameraService = _bootstrap.ServiceProvider.GetRequiredService<ICameraService>();
        _renderService = _bootstrap.ServiceProvider.GetRequiredService<IRenderService>();
        _selectionService = _bootstrap.ServiceProvider.GetRequiredService<ISelectionService>();
        _layerService = _bootstrap.ServiceProvider.GetRequiredService<ILayerService>();
        
        UpdateCameraInfo();
        
        Closed += (s, e) =>
        {
            CompositionTarget.Rendering -= OnRendering;
            _bootstrap.Shutdown();
        };

        CompositionTarget.Rendering += OnRendering;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        var deltaTime = (float)(now - _lastTick).TotalSeconds;
        _lastTick = now;

        if (deltaTime > 0.1f) deltaTime = 0.1f;

        ProcessKeyboardInput(deltaTime);
        _controller.Update(deltaTime);
        UpdateCameraInfo();
    }

    private void ProcessKeyboardInput(float deltaTime)
    {
        bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

        if (isShift)
        {
            if (Keyboard.IsKeyDown(Key.Up))
            {
                _cameraService.MoveVertical(1.0f, deltaTime);
            }
            if (Keyboard.IsKeyDown(Key.Down))
            {
                _cameraService.MoveVertical(-1.0f, deltaTime);
            }
        }
        else
        {
            if (Keyboard.IsKeyDown(Key.Left))
            {
                _cameraService.MoveHorizontal(-1.0f, deltaTime);
            }
            if (Keyboard.IsKeyDown(Key.Right))
            {
                _cameraService.MoveHorizontal(1.0f, deltaTime);
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.C)
        {
            _cameraService.ContinuousForward = !_cameraService.ContinuousForward;
        }
        else if (e.Key == Key.OemPlus || e.Key == Key.Add)
        {
            _cameraService.MovementSpeed = System.Math.Min(1000.0f, _cameraService.MovementSpeed + 10.0f);
        }
        else if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
        {
            _cameraService.MovementSpeed = System.Math.Max(1.0f, _cameraService.MovementSpeed - 10.0f);
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ReturnToModeSelect_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void AboutObservation_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "CELESTIAL MECHANICS — OBSERVATION MODE\n" +
            "Version: 0.1.0-alpha\n\n" +
            "Real-time telescope viewport and star catalog indexing engine.\n" +
            "Enables geocentric and free-flight sky visualization.",
            "About Observation Mode",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void Viewport_MouseMove(object sender, MouseEventArgs e)
    {
        var currentPos = e.GetPosition(GlViewport);
        var deltaX = (float)(currentPos.X - _lastMousePosition.X);
        var deltaY = (float)(currentPos.Y - _lastMousePosition.Y);
        _lastMousePosition = currentPos;

        if (_isRightDragging)
        {
            _cameraService.Look(deltaX, deltaY);
        }
        else if (_isLeftDragging)
        {
            _cameraService.Pan(deltaX, deltaY);
        }
    }

    private void Viewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isLeftDragging = true;
        _lastMousePosition = e.GetPosition(GlViewport);
        GlViewport.CaptureMouse();
    }

    private void Viewport_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isLeftDragging = false;
        if (!_isRightDragging)
        {
            GlViewport.ReleaseMouseCapture();
        }
    }

    private void Viewport_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isRightDragging = true;
        _lastMousePosition = e.GetPosition(GlViewport);
        GlViewport.CaptureMouse();
    }

    private void Viewport_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isRightDragging = false;
        if (!_isLeftDragging)
        {
            GlViewport.ReleaseMouseCapture();
        }
    }

    private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        _cameraService.Zoom(e.Delta / 120.0f);
    }

    private void ResetCamera_Click(object sender, RoutedEventArgs e)
    {
        _cameraService.Reset();
    }

    private void ToggleAxes_Click(object sender, RoutedEventArgs e)
    {
        _renderService.ShowCoordinateAxes = !_renderService.ShowCoordinateAxes;
        AxesOverlay.Visibility = _renderService.ShowCoordinateAxes ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateCameraInfo()
    {
        if (_cameraService != null)
        {
            CameraInfoText.Text = $"Camera: Yaw={_cameraService.Yaw:F1}°, Pitch={_cameraService.Pitch:F1}°, Dist={_cameraService.Distance:F1}";
            StatusZoom.Text = $"Zoom: {1.0 / (_cameraService.Distance / 50.0):F2}x";
            
            double raDeg = (_cameraService.Yaw + 180.0) % 360.0;
            if (raDeg < 0) raDeg += 360.0;
            double decDeg = -_cameraService.Pitch;
            
            double raHours = raDeg / 15.0;
            int raH = (int)raHours;
            int raM = (int)((raHours - raH) * 60.0);
            
            int decD = (int)decDeg;
            int decM = (int)(System.Math.Abs(decDeg - decD) * 60.0);
            
            StatusCoords.Text = $"RA: {raH}h {raM}m | Dec: {decD}° {decM}'";
            FpsText.Text = $"Speed: {_cameraService.MovementSpeed:F0} u/s" + (_cameraService.ContinuousForward ? " (Auto)" : "");
        }
    }
}
