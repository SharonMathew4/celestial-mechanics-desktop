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
    private bool _rightMouseDragged;
    private Point _lastRightClickPos;

    // Edit mode state: track if we're dragging a body
    private bool _isDraggingBody;
    private int _draggedBodyIndex = -1;

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

    // ── Keyboard Polling (WASD / QE + Arrow Keys) ────────────────────

    private void OnKeyboardPoll(object? sender, EventArgs e)
    {
        if (_renderer == null) return;

        // Keyboard input requires focus
        if (IsFocused || IsKeyboardFocusWithin)
        {
            const float dt = 0.016f; // ~60fps
            bool shiftHeld = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            // WASD controls (classic 3D viewport)
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

            // Arrow key controls (per spec)
            if (Keyboard.IsKeyDown(Key.Up))
                _renderer.Camera.ProcessKeyboard(shiftHeld ? CameraMovement.Up : CameraMovement.Forward, dt);
            if (Keyboard.IsKeyDown(Key.Down))
                _renderer.Camera.ProcessKeyboard(shiftHeld ? CameraMovement.Down : CameraMovement.Backward, dt);
            if (Keyboard.IsKeyDown(Key.Left))
                _renderer.Camera.ProcessKeyboard(CameraMovement.Left, dt);
            if (Keyboard.IsKeyDown(Key.Right))
                _renderer.Camera.ProcessKeyboard(CameraMovement.Right, dt);
        }

        // Ghost placement polling — works even when HwndHost swallows MouseMove
        PollPlacementGhostPosition();
    }

    /// <summary>
    /// Polls the current mouse position and updates the ghost body or velocity
    /// endpoint during placement mode. This is needed because the native OpenGL
    /// HWND child absorbs WM_MOUSEMOVE and WPF's MouseMove routed event doesn't
    /// fire on this control when no mouse button is pressed.
    /// </summary>
    private void PollPlacementGhostPosition()
    {
        if (ViewModel == null || !ViewModel.IsPlacingObject || _renderer == null) return;

        // Mouse.GetPosition uses Win32 GetCursorPos internally — works regardless of HwndHost
        var pos = Mouse.GetPosition(this);

        // Only update if cursor is within the viewport bounds
        if (pos.X < 0 || pos.Y < 0 || pos.X > ActualWidth || pos.Y > ActualHeight) return;

        var (worldX, worldY, worldZ) = ScreenToWorldXZPlane(pos);

        if (ViewModel.PlacementPhase == PlacementPhase.ChoosingPosition)
        {
            ViewModel.UpdateGhostPosition(worldX, worldY, worldZ);
            if (Cursor != Cursors.Cross)
                Cursor = Cursors.Cross;
        }
        else if (ViewModel.PlacementPhase == PlacementPhase.ChoosingVelocity)
        {
            ViewModel.UpdateVelocityEndpoint(worldX, worldY, worldZ);
            if (Cursor != Cursors.ScrollAll)
                Cursor = Cursors.ScrollAll;
        }
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

            case Key.Space:
                // During velocity selection, Space places with zero velocity
                if (ViewModel?.PlacementPhase == PlacementPhase.ChoosingVelocity)
                {
                    ViewModel.PlaceWithZeroVelocity();
                    Cursor = Cursors.Cross; // Back to position cursor
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                // Cancel placement
                if (ViewModel?.IsPlacingObject == true)
                {
                    ViewModel.CancelPlacement();
                    UpdateCursorForMode();
                    e.Handled = true;
                }
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

            // In Edit mode, check if clicking on a body to start dragging
            if (ViewModel != null && ViewModel.CurrentMode == UiMode.Edit)
            {
                var hitIndex = HitTestBody(e.GetPosition(this));
                if (hitIndex >= 0)
                {
                    _isDraggingBody = true;
                    _draggedBodyIndex = hitIndex;
                    _renderer.SelectedInstanceIndex = hitIndex;
                    Cursor = Cursors.SizeAll;
                }
            }

            CaptureMouse();
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            _rightMouseDown = true;
            _rightMouseDragged = false;
            _lastRightClickPos = e.GetPosition(this);
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
            // Reset Edit mode dragging state
            if (_isDraggingBody)
            {
                _isDraggingBody = false;
                _draggedBodyIndex = -1;
                UpdateCursorForMode();
            }

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
            // Only handle as a click (context menu / deselect) if user didn't drag
            if (!_rightMouseDragged)
            {
                HandleRightClick(_lastRightClickPos);
            }
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

        // ── Two-step placement: update ghost or velocity endpoint ────
        if (ViewModel != null && ViewModel.IsPlacingObject)
        {
            var (worldX, worldY, worldZ) = ScreenToWorldXZPlane(pos);

            if (ViewModel.PlacementPhase == PlacementPhase.ChoosingPosition)
            {
                ViewModel.UpdateGhostPosition(worldX, worldY, worldZ);
            }
            else if (ViewModel.PlacementPhase == PlacementPhase.ChoosingVelocity)
            {
                ViewModel.UpdateVelocityEndpoint(worldX, worldY, worldZ);
            }
        }

        // Middle mouse drag: always PAN (Blender/Unity convention)
        if (_middleMouseDown)
        {
            _renderer.Camera.ProcessMousePan(deltaX, deltaY);
        }

        // Right mouse drag: ORBIT camera
        if (_rightMouseDown)
        {
            if (System.Math.Abs(deltaX) > 2 || System.Math.Abs(deltaY) > 2)
            {
                _rightMouseDragged = true;
            }
            if (_rightMouseDragged)
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

            // In Edit mode with a body selected, drag moves the body
            if (_leftMouseDragged && ViewModel != null && ViewModel.CurrentMode == UiMode.Edit)
            {
                if (_isDraggingBody && _draggedBodyIndex >= 0 && _simService != null)
                {
                    var (dx, dz) = ScreenDeltaToWorldDelta(deltaX, deltaY);
                    _simService.OffsetBodyPosition(_draggedBodyIndex, dx, 0, dz);
                }
            }
            // In Idle or Simulate mode, left-drag orbits the camera
            else if (_leftMouseDragged && ViewModel != null &&
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
            // Left-click finalizes: confirm velocity and place the body
            if (ViewModel.PlacementPhase == PlacementPhase.ChoosingVelocity)
            {
                ViewModel.ConfirmVelocityAndPlace();
                Cursor = Cursors.Cross; // Back to position cursor
            }
            // Left-click during ChoosingPosition is a no-op (use right-click to fix position)
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

    /// <summary>
    /// Right click (no drag): during placement fixes position or cancels;
    /// otherwise opens context menu or deselects.
    /// </summary>
    private void HandleRightClick(Point screenPos)
    {
        if (ViewModel == null || _renderer == null) return;

        if (ViewModel.IsPlacingObject)
        {
            if (ViewModel.PlacementPhase == PlacementPhase.ChoosingPosition)
            {
                // Right-click during position selection: fix ghost position
                ViewModel.ConfirmPosition();
                Cursor = Cursors.ScrollAll; // Change cursor for velocity phase
            }
            else if (ViewModel.PlacementPhase == PlacementPhase.ChoosingVelocity)
            {
                // Right-click during velocity selection: cancel back to position phase
                ViewModel.CancelVelocityPhase();
                Cursor = Cursors.Cross;
            }
            return;
        }

        // Raycast to check if a body was clicked
        var renderState = _renderer.RenderState;
        int hitIndex = SelectionHelper.Raycast(
            (float)screenPos.X, (float)screenPos.Y,
            (float)ActualWidth, (float)ActualHeight,
            _renderer.Camera, renderState.Bodies, renderState.BodyCount);

        if (hitIndex >= 0)
        {
            // Select the body and open inspector
            _renderer.SelectedInstanceIndex = hitIndex;
            int bodyId = renderState.Bodies[hitIndex].Id;
            ViewModel.SelectBodyById(bodyId);
            ViewModel.RightPanelTabIndex = 0; // Switch to Inspector
            ViewModel.ShowInspector = true;

            // Show context menu at click position
            ShowBodyContextMenu(screenPos, bodyId);
        }
        else
        {
            // Clicked empty space: deselect
            _renderer.SelectedInstanceIndex = -1;
            ViewModel.DeselectBody();
        }
        UpdateCursorForMode();
    }

    /// <summary>Shows a themed context menu for the selected body.</summary>
    private void ShowBodyContextMenu(Point screenPos, int bodyId)
    {
        if (ViewModel == null) return;

        var menu = new ContextMenu
        {
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x12, 0x18, 0x29)),
            BorderBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x2A, 0x30, 0x48)),
            BorderThickness = new Thickness(1),
            Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xE8, 0xEA, 0xF0)),
        };

        var inspectItem = new MenuItem { Header = "🔍  Inspect Properties" };
        inspectItem.Click += (_, _) => { ViewModel.RightPanelTabIndex = 0; ViewModel.ShowInspector = true; };

        var focusItem = new MenuItem { Header = "🎯  Focus Camera  (F)" };
        focusItem.Click += (_, _) => FocusOnSelectedBody();

        var editOrbitItem = new MenuItem { Header = "🔄  Edit Orbit" };
        editOrbitItem.Click += (_, _) => 
        {
            ViewModel.CurrentMode = UiMode.Edit;
            ViewModel.RightPanelTabIndex = 0;
            ViewModel.ShowInspector = true;
        };

        var deleteItem = new MenuItem { Header = "❌  Delete  (Del)" };
        deleteItem.Click += (_, _) => ViewModel.DeleteSelectedBodyCommand.Execute(null);

        var copyPosItem = new MenuItem { Header = "📋  Copy Position" };
        copyPosItem.Click += (_, _) =>
        {
            var rs = _renderer!.RenderState;
            int idx = _renderer.SelectedInstanceIndex;
            if (idx >= 0 && idx < rs.BodyCount)
            {
                var p = rs.Bodies[idx].Position;
                Clipboard.SetText($"({p.X:F4}, {p.Y:F4}, {p.Z:F4})");
            }
        };

        var setReferenceItem = new MenuItem { Header = "🌐  Set as Reference Frame" };
        setReferenceItem.Click += (_, _) => ViewModel.SetReferenceFrameCommand.Execute(bodyId);

        menu.Items.Add(inspectItem);
        menu.Items.Add(focusItem);
        menu.Items.Add(editOrbitItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(deleteItem);
        menu.Items.Add(copyPosItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(setReferenceItem);

        menu.IsOpen = true;
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

    /// <summary>
    /// Converts a screen-space delta (pixels) to a world-space delta on the XZ plane,
    /// based on current camera orientation and distance.
    /// </summary>
    private (float dx, float dz) ScreenDeltaToWorldDelta(float screenDeltaX, float screenDeltaY)
    {
        if (_renderer == null) return (0, 0);

        var cam = _renderer.Camera;
        float viewW = (float)ActualWidth;
        float viewH = (float)ActualHeight;
        if (viewW < 1 || viewH < 1) return (0, 0);

        // Get camera's right and up vectors in world space
        var view = cam.GetViewMatrix();
        var camRight = new Vector3(view.M11, view.M21, view.M31);
        var camUp = new Vector3(view.M12, view.M22, view.M32);

        // Scale factor based on camera distance (approximate world units per pixel)
        float scale = cam.Distance * 0.002f;

        // Move in camera's right direction for X movement, and forward (projected) for Y
        // We want movement on the XZ plane, so project camUp onto XZ
        var camForwardXZ = new Vector3(-view.M13, 0, -view.M33);
        if (camForwardXZ.LengthSquared() > 0.001f)
            camForwardXZ = Vector3.Normalize(camForwardXZ);
        else
            camForwardXZ = new Vector3(0, 0, -1);

        // Project camRight onto XZ plane for horizontal movement
        var camRightXZ = new Vector3(camRight.X, 0, camRight.Z);
        if (camRightXZ.LengthSquared() > 0.001f)
            camRightXZ = Vector3.Normalize(camRightXZ);
        else
            camRightXZ = new Vector3(1, 0, 0);

        // Convert screen delta to world movement
        var worldDelta = camRightXZ * screenDeltaX * scale + camForwardXZ * (-screenDeltaY) * scale;
        return (worldDelta.X, worldDelta.Z);
    }

    /// <summary>
    /// Performs a raycast to find which body (if any) is under the given screen position.
    /// Returns the body index or -1 if no body hit.
    /// </summary>
    private int HitTestBody(Point screenPos)
    {
        if (_renderer == null) return -1;

        var renderState = _renderer.RenderState;
        return SelectionHelper.Raycast(
            (float)screenPos.X, (float)screenPos.Y,
            (float)ActualWidth, (float)ActualHeight,
            _renderer.Camera, renderState.Bodies, renderState.BodyCount);
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
            UiMode.AddPlacement => ViewModel.PlacementPhase == PlacementPhase.ChoosingVelocity
                ? Cursors.ScrollAll
                : Cursors.Cross,
            UiMode.Edit => Cursors.Hand,
            _ => Cursors.Arrow
        };
    }
}
