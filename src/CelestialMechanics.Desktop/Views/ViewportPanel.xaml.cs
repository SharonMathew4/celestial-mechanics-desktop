using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CelestialMechanics.Desktop.Infrastructure;
using CelestialMechanics.Desktop.Services;
using CelestialMechanics.Desktop.ViewModels;
using CelestialMechanics.Renderer;

namespace CelestialMechanics.Desktop.Views;

/// <summary>
/// OpenGL viewport panel with full 3D interaction:
/// - Middle mouse + drag: orbit camera
/// - Shift + Middle mouse + drag: pan camera
/// - Scroll wheel: zoom
/// - Shift + scroll: FOV zoom
/// - WASD / QE: keyboard camera navigation
/// - Left click: place object (Add mode) or select body (Edit/Idle mode)
/// - Left drag (Idle/Simulate): orbit camera
/// - Double-click: fly camera to clicked body
/// - Right click: cancel placement / deselect
/// - Home: reset camera to default
/// - F: focus on selected body
/// </summary>
public partial class ViewportPanel : UserControl
{
    private OpenGLHost? _glHost;
    private RenderLoop? _renderLoop;
    private GLRenderer? _renderer;
    private SimulationService? _simService;

    private bool _middleMouseDown;
    private bool _leftMouseDown;
    private bool _rightMouseDown;
    private Point _lastMousePos;
    private bool _leftMouseDragged;

    // Keyboard input polling timer
    private DispatcherTimer? _keyboardTimer;
    private DateTime _lastDoubleClick = DateTime.MinValue;

    public RenderLoop? RenderLoop => _renderLoop;

    /// <summary>
    /// Reference to the MainWindowViewModel for interaction commands.
    /// Set by code-behind after ViewModel is created.
    /// </summary>
    public MainWindowViewModel? ViewModel { get; set; }

    public ViewportPanel()
    {
        InitializeComponent();
    }

    public void Initialize(GLRenderer renderer, SimulationService simService)
    {
        _renderer = renderer;
        _simService = simService;

        // Create the HwndHost (this triggers BuildWindowCore and WGL context creation)
        _glHost = new OpenGLHost();
        HostContainer.Children.Add(_glHost);

        // Wire mouse events on this control (not on the HwndHost, since it swallows input)
        MouseDown += OnViewportMouseDown;
        MouseUp += OnViewportMouseUp;
        MouseMove += OnViewportMouseMove;
        MouseWheel += OnViewportMouseWheel;
        PreviewKeyDown += OnViewportKeyDown;

        // Keyboard polling timer (60 Hz for smooth WASD movement)
        _keyboardTimer = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = TimeSpan.FromMilliseconds(16),
        };
        _keyboardTimer.Tick += OnKeyboardPoll;
        _keyboardTimer.Start();

        // Start the render loop on a dedicated thread
        _renderLoop = new RenderLoop();
        _renderLoop.Start(_glHost, renderer, action => simService.WithEngineLock(action));
    }

    public void Shutdown()
    {
        _keyboardTimer?.Stop();
        _keyboardTimer = null;

        _renderLoop?.Dispose();
        _renderLoop = null;

        if (_glHost != null)
        {
            HostContainer.Children.Remove(_glHost);
            _glHost.Dispose();
            _glHost = null;
        }
    }

    // ── Keyboard Polling (WASD / QE) ────────────────────────────────

    private void OnKeyboardPoll(object? sender, EventArgs e)
    {
        if (_renderer == null || (!IsFocused && !IsKeyboardFocusWithin)) return;

        const float dt = 0.016f; // ~60fps

        if (Keyboard.IsKeyDown(Key.W))
            _renderer.Camera.ProcessKeyboard(CameraMovement.Forward, dt);
        if (Keyboard.IsKeyDown(Key.S) && !Keyboard.IsKeyDown(Key.LeftCtrl))
            _renderer.Camera.ProcessKeyboard(CameraMovement.Backward, dt);
        if (Keyboard.IsKeyDown(Key.A))
            _renderer.Camera.ProcessKeyboard(CameraMovement.Left, dt);
        if (Keyboard.IsKeyDown(Key.D))
            _renderer.Camera.ProcessKeyboard(CameraMovement.Right, dt);
        if (Keyboard.IsKeyDown(Key.Q))
            _renderer.Camera.ProcessKeyboard(CameraMovement.Down, dt);
        if (Keyboard.IsKeyDown(Key.E))
            _renderer.Camera.ProcessKeyboard(CameraMovement.Up, dt);
    }

    // ── Keyboard Events ─────────────────────────────────────────────

    private void OnViewportKeyDown(object sender, KeyEventArgs e)
    {
        if (_renderer == null) return;

        switch (e.Key)
        {
            case Key.Home:
                _renderer.Camera.ResetToDefault();
                e.Handled = true;
                break;

            case Key.F:
                FocusOnSelectedBody();
                e.Handled = true;
                break;
        }
    }

    // ── Mouse Input ─────────────────────────────────────────────────

    private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_renderer == null) return;

        // Ensure the viewport has keyboard focus for WASD navigation
        Focus();

        if (e.ChangedButton == MouseButton.Middle)
        {
            _middleMouseDown = true;
            _lastMousePos = e.GetPosition(this);
            CaptureMouse();
            Cursor = Cursors.SizeAll;
        }
        else if (e.ChangedButton == MouseButton.Left)
        {
            // Check for double-click
            if (e.ClickCount == 2)
            {
                HandleDoubleClick(e.GetPosition(this));
                _lastDoubleClick = DateTime.UtcNow;
                return;
            }

            _leftMouseDown = true;
            _leftMouseDragged = false;
            _lastMousePos = e.GetPosition(this);
            CaptureMouse();
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            _rightMouseDown = true;
            _lastMousePos = e.GetPosition(this);
            CaptureMouse();
        }
    }

    private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            _middleMouseDown = false;
            UpdateCursorForMode();
        }
        else if (e.ChangedButton == MouseButton.Left)
        {
            // If it was a click (not a drag), handle placement/selection
            if (!_leftMouseDragged && (DateTime.UtcNow - _lastDoubleClick).TotalMilliseconds > 300)
            {
                HandleLeftClick(e.GetPosition(this));
            }
            _leftMouseDown = false;
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            if (!_rightMouseDown) return;
            _rightMouseDown = false;
            HandleRightClick();
        }

        if (!_leftMouseDown && !_rightMouseDown && !_middleMouseDown)
            ReleaseMouseCapture();
    }

    private void OnViewportMouseMove(object sender, MouseEventArgs e)
    {
        if (_renderer == null) return;

        var pos = e.GetPosition(this);
        float deltaX = (float)(pos.X - _lastMousePos.X);
        float deltaY = (float)(pos.Y - _lastMousePos.Y);
        _lastMousePos = pos;

        // Middle mouse drag: camera orbit/pan
        if (_middleMouseDown)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                _renderer.Camera.ProcessMousePan(deltaX, deltaY);
            }
            else
            {
                _renderer.Camera.ProcessMouseOrbit(deltaX, deltaY);
            }
        }

        // Left mouse drag tracking
        if (_leftMouseDown)
        {
            if (System.Math.Abs(deltaX) > 2 || System.Math.Abs(deltaY) > 2)
            {
                _leftMouseDragged = true;
            }

            // In Idle or Simulate mode, left-drag also orbits the camera
            if (_leftMouseDragged && ViewModel != null &&
                (ViewModel.CurrentMode == UiMode.Idle || ViewModel.CurrentMode == UiMode.Simulate))
            {
                _renderer.Camera.ProcessMouseOrbit(deltaX, deltaY);
            }
        }
    }

    private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_renderer == null) return;

        float delta = e.Delta / 120f;

        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
        {
            // Shift + scroll = FOV zoom
            _renderer.Camera.ProcessFovZoom(delta);
        }
        else
        {
            // Normal scroll = distance zoom
            _renderer.Camera.ProcessMouseZoom(delta);
        }
    }

    // ── Interaction Handlers ────────────────────────────────────────

    private void HandleLeftClick(Point screenPos)
    {
        if (ViewModel == null || _renderer == null) return;

        if (ViewModel.IsPlacingObject)
        {
            var (worldX, worldY, worldZ) = ScreenToWorldXZPlane(screenPos);
            ViewModel.PlaceObjectAt(worldX, worldY, worldZ);
        }
        else
        {
            // Selection via raycasting
            var renderState = _renderer.RenderState;
            int hitIndex = SelectionHelper.Raycast(
                (float)screenPos.X, (float)screenPos.Y,
                (float)ActualWidth, (float)ActualHeight,
                _renderer.Camera, renderState.Bodies, renderState.BodyCount);

            if (hitIndex >= 0)
            {
                _renderer.SelectedInstanceIndex = hitIndex;
                int bodyId = renderState.Bodies[hitIndex].Id;
                ViewModel.SelectBodyById(bodyId);
            }
            else
            {
                // Clicked empty space: deselect
                _renderer.SelectedInstanceIndex = -1;
                ViewModel.DeselectBody();
            }
        }
    }

    /// <summary>Double-click: fly the camera to the clicked body.</summary>
    private void HandleDoubleClick(Point screenPos)
    {
        if (_renderer == null) return;

        var renderState = _renderer.RenderState;
        int hitIndex = SelectionHelper.Raycast(
            (float)screenPos.X, (float)screenPos.Y,
            (float)ActualWidth, (float)ActualHeight,
            _renderer.Camera, renderState.Bodies, renderState.BodyCount);

        if (hitIndex >= 0)
        {
            var body = renderState.Bodies[hitIndex];
            float flyDistance = MathF.Max(body.Radius * 4f, 2f);
            _renderer.Camera.FlyTo(body.Position, flyDistance);

            // Also select the body
            _renderer.SelectedInstanceIndex = hitIndex;
            ViewModel?.SelectBodyById(body.Id);
        }
    }

    /// <summary>Right click: cancel placement or deselect.</summary>
    private void HandleRightClick()
    {
        if (ViewModel == null) return;

        if (ViewModel.IsPlacingObject)
        {
            ViewModel.CancelPlacement();
        }
        else
        {
            _renderer!.SelectedInstanceIndex = -1;
            ViewModel.DeselectBody();
        }
        UpdateCursorForMode();
    }

    /// <summary>Focuses the camera on the currently selected body (F key).</summary>
    private void FocusOnSelectedBody()
    {
        if (_renderer == null) return;

        int selectedIdx = _renderer.SelectedInstanceIndex;
        var renderState = _renderer.RenderState;

        if (selectedIdx >= 0 && selectedIdx < renderState.BodyCount)
        {
            var body = renderState.Bodies[selectedIdx];
            float flyDist = MathF.Max(body.Radius * 4f, 2f);
            _renderer.Camera.FlyTo(body.Position, flyDist);
        }
    }

    /// <summary>
    /// Converts a screen position to a point on the XZ plane (Y=0) using proper ray-plane intersection.
    /// </summary>
    private (float x, float y, float z) ScreenToWorldXZPlane(Point screenPos)
    {
        if (_renderer == null) return (0, 0, 0);

        float viewW = (float)ActualWidth;
        float viewH = (float)ActualHeight;

        if (viewW < 1 || viewH < 1) return (0, 0, 0);

        var cam = _renderer.Camera;
        float aspect = viewW / viewH;
        var view = cam.GetViewMatrix();
        var projection = cam.GetProjectionMatrix(aspect);

        if (!Matrix4x4.Invert(view * projection, out var invVP))
            return (0, 0, 0);

        float ndcX = (float)(2.0 * screenPos.X / viewW - 1.0);
        float ndcY = (float)(1.0 - 2.0 * screenPos.Y / viewH);

        var nearPoint = Vector4.Transform(new Vector4(ndcX, ndcY, -1f, 1f), invVP);
        var farPoint = Vector4.Transform(new Vector4(ndcX, ndcY, 1f, 1f), invVP);

        if (nearPoint.W == 0 || farPoint.W == 0)
            return (0, 0, 0);

        var rayOrigin = new Vector3(nearPoint.X, nearPoint.Y, nearPoint.Z) / nearPoint.W;
        var rayFar = new Vector3(farPoint.X, farPoint.Y, farPoint.Z) / farPoint.W;
        var rayDir = Vector3.Normalize(rayFar - rayOrigin);

        // Intersect ray with Y=0 plane
        if (MathF.Abs(rayDir.Y) < 0.0001f)
            return (rayOrigin.X, 0, rayOrigin.Z);

        float t = -rayOrigin.Y / rayDir.Y;
        if (t < 0) t = 10f; // Camera below plane; use a default distance

        var hitPoint = rayOrigin + rayDir * t;
        return (hitPoint.X, 0f, hitPoint.Z);
    }

    private void UpdateCursorForMode()
    {
        if (ViewModel == null)
        {
            Cursor = Cursors.Arrow;
            return;
        }

        Cursor = ViewModel.CurrentMode switch
        {
            UiMode.AddPlacement => Cursors.Cross,
            UiMode.Edit => Cursors.Hand,
            _ => Cursors.Arrow
        };
    }
}
