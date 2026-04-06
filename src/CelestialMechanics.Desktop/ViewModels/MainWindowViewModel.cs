using System.IO;
using System.Text.Json;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CelestialMechanics.Desktop.Models;
using CelestialMechanics.Desktop.Services;
using CelestialMechanics.Math;
using CelestialMechanics.Physics.Types;
using CelestialMechanics.Renderer;
using CelestialMechanics.Simulation;

namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// Top-level ViewModel for MainWindow.
/// Manages the multi-stage navigation flow and the simulation IDE workspace.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly SimulationService _simService;
    private readonly SceneService _sceneService;
    private readonly GLRenderer _renderer;
    private readonly ProjectService _projectService;
    private readonly Dispatcher _dispatcher;
    private readonly DispatcherTimer _uiTimer;

    // ── Service Accessors (for code-behind viewport initialization) ──

    public SimulationService SimService => _simService;
    public SceneService SceneService => _sceneService;
    public GLRenderer Renderer => _renderer;

    // ── Child ViewModels ─────────────────────────────────────────────

    public ModeSelectionViewModel ModeSelectionVm { get; }
    public SimulationMenuViewModel SimulationMenuVm { get; }
    public NewProjectViewModel NewProjectVm { get; }
    public ProjectsListViewModel ProjectsListVm { get; }
    public FileMenuViewModel FileMenuVm { get; }

    // Phase 4: IDE Panel ViewModels
    public SceneOutlinerViewModel SceneOutlinerVm { get; }
    public BodyInspectorViewModel BodyInspectorVm { get; }
    public SimulationSettingsViewModel SimulationSettingsVm { get; }

    // ── Navigation ───────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsModalVisible))]
    [NotifyPropertyChangedFor(nameof(IsIdeActive))]
    [NotifyPropertyChangedFor(nameof(IsStatusBarVisible))]
    [NotifyPropertyChangedFor(nameof(IsControlBarVisible))]
    private NavigationState _navState = NavigationState.ModeSelection;

    /// <summary>True when any modal overlay should be visible.</summary>
    public bool IsModalVisible => NavState != NavigationState.SimulationIDE;

    /// <summary>True when the full simulation IDE workspace is active.</summary>
    public bool IsIdeActive => NavState == NavigationState.SimulationIDE;

    // ── Current Project ──────────────────────────────────────────────

    [ObservableProperty]
    private ProjectInfo? _currentProject;

    [ObservableProperty]
    private string _windowTitle = "Celestial Mechanics \u2014 Desktop";

    // ── UI Mode State Machine ────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAddMode))]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    [NotifyPropertyChangedFor(nameof(IsAnalyseMode))]
    [NotifyPropertyChangedFor(nameof(IsSimulateMode))]
    private UiMode _currentMode = UiMode.Idle;

    public bool IsAddMode => CurrentMode == UiMode.AddPlacement;
    public bool IsEditMode => CurrentMode == UiMode.Edit;
    public bool IsAnalyseMode => CurrentMode == UiMode.Analyse;
    public bool IsSimulateMode => CurrentMode == UiMode.Simulate;

    // ── Object Placement State ───────────────────────────────────────

    /// <summary>When true, the user is in placement mode with a ghost object on cursor.</summary>
    [ObservableProperty]
    private bool _isPlacingObject;

    /// <summary>The type of celestial body being placed.</summary>
    [ObservableProperty]
    private string _placementObjectType = string.Empty;

    /// <summary>The currently selected body type for placement.</summary>
    [ObservableProperty]
    private BodyType _selectedBodyType = BodyType.Star;

    /// <summary>All body types available for the palette.</summary>
    public IReadOnlyList<BodyType> AllBodyTypes { get; } =
        Enum.GetValues<BodyType>().ToList().AsReadOnly();

    // ── Time Scale (Time Flow Slider) ────────────────────────────────

    [ObservableProperty]
    private double _timeScale = 1.0;

    // ── Status Bar Metrics ───────────────────────────────────────────

    [ObservableProperty]
    private string _fpsText = "FPS: --";

    [ObservableProperty]
    private string _bodyCountText = "Bodies: 0";

    [ObservableProperty]
    private string _physicsTimeText = "Physics: -- ms";

    [ObservableProperty]
    private string _renderTimeText = "Render: -- ms";

    [ObservableProperty]
    private SimulationState _simulationState = SimulationState.Idle;

    [ObservableProperty]
    private string _simulationStateText = "Idle";

    // ── Toolbar Toggles ──────────────────────────────────────────────

    [ObservableProperty]
    private bool _showGrid = true;

    [ObservableProperty]
    private bool _showVelocityArrows;

    [ObservableProperty]
    private bool _showStarfield = true;

    [ObservableProperty]
    private bool _showTrails = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStatusBarVisible))]
    private bool _showStatusBar = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsControlBarVisible))]
    private bool _showControlBar = true;

    // ── Panel Visibility Toggles (Phase 4) ───────────────────────────

    [ObservableProperty]
    private bool _showSceneOutliner = true;

    [ObservableProperty]
    private bool _showInspector = true;

    /// <summary>
    /// Index of the selected tab in the right-panel TabControl.
    /// 0 = Inspector, 1 = Settings.
    /// </summary>
    [ObservableProperty]
    private int _rightPanelTabIndex;

    /// <summary>True when the compact status bar should be visible (IDE active + toggle on).</summary>
    public bool IsStatusBarVisible => IsIdeActive && ShowStatusBar;

    /// <summary>True when the floating control bar should be visible (IDE active + toggle on).</summary>
    public bool IsControlBarVisible => IsIdeActive && ShowControlBar;

    // ── Constructor ──────────────────────────────────────────────────

    public MainWindowViewModel(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;

        // 1. Create services
        _simService = new SimulationService();
        _renderer = new GLRenderer();
        _sceneService = new SceneService();
        _projectService = new ProjectService();

        // 2. Create child ViewModels — Navigation
        ModeSelectionVm = new ModeSelectionViewModel();
        SimulationMenuVm = new SimulationMenuViewModel();
        NewProjectVm = new NewProjectViewModel(_projectService);
        ProjectsListVm = new ProjectsListViewModel(_projectService);
        FileMenuVm = new FileMenuViewModel();

        // 3. Create child ViewModels — IDE Panels (Phase 4)
        SceneOutlinerVm = new SceneOutlinerViewModel(_sceneService, _simService, dispatcher);
        BodyInspectorVm = new BodyInspectorViewModel(_simService, _sceneService);
        SimulationSettingsVm = new SimulationSettingsViewModel(_simService);

        // 4. Wire navigation events
        WireNavigation();

        // 5. Wire IDE panel events
        WireIdePanels();

        // 6. UI refresh timer (20 Hz)
        _uiTimer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(50),
        };
        _uiTimer.Tick += OnUiTimerTick;
    }

    // ── Navigation Wiring ────────────────────────────────────────────

    private void WireNavigation()
    {
        // Mode Selection
        ModeSelectionVm.SimulationSelected += () => NavState = NavigationState.SimulationMenu;
        ModeSelectionVm.ExitRequested += () => System.Windows.Application.Current.Shutdown();

        // Simulation Menu
        SimulationMenuVm.NewProjectRequested += () =>
        {
            NewProjectVm.Reset();
            NavState = NavigationState.NewProject;
        };
        SimulationMenuVm.FileRequested += () => NavState = NavigationState.FileMenu;
        SimulationMenuVm.ProjectsRequested += () =>
        {
            ProjectsListVm.RefreshProjects();
            NavState = NavigationState.ProjectsList;
        };
        SimulationMenuVm.BackRequested += () => NavState = NavigationState.ModeSelection;

        // New Project
        NewProjectVm.ProjectCreated += OnProjectOpened;
        NewProjectVm.CancelRequested += () => NavState = NavigationState.SimulationMenu;

        // Projects List
        ProjectsListVm.ProjectOpened += OnProjectOpened;
        ProjectsListVm.CancelRequested += () => NavState = NavigationState.SimulationMenu;

        // File Menu
        FileMenuVm.BackRequested += () => NavState = NavigationState.SimulationMenu;
    }

    /// <summary>
    /// Wires events between IDE panel ViewModels (Phase 4).
    /// </summary>
    private void WireIdePanels()
    {
        // Outliner selection → Inspector load
        SceneOutlinerVm.BodySelected += nodeId => BodyInspectorVm.LoadBody(nodeId);

        // Outliner delete request → delete body
        SceneOutlinerVm.DeleteRequested += DeleteBody;

        // Settings panel close → switch back to Inspector tab
        SimulationSettingsVm.CloseRequested += () => RightPanelTabIndex = 0;
    }

    /// <summary>
    /// Common handler: project created or opened — enter the IDE.
    /// </summary>
    private void OnProjectOpened(ProjectInfo project)
    {
        CurrentProject = project;
        WindowTitle = $"Celestial Mechanics \u2014 {project.Name}";

        // Start simulation engine and UI timer
        _simService.StartSimThread();
        _uiTimer.Start();

        NavState = NavigationState.SimulationIDE;
        CurrentMode = UiMode.Idle;

        // Refresh outliner
        SceneOutlinerVm.Refresh();
    }

    // ═══════════════════════════════════════════════════════════════
    //  MENU COMMANDS
    // ═══════════════════════════════════════════════════════════════

    [RelayCommand]
    private void NewSimulation()
    {
        if (IsModalVisible)
            return;

        // Reset existing simulation to empty scene
        _simService.ResetScene();
        _sceneService.RepopulateFromSimulation(_simService);
        BodyInspectorVm.ClearSelection();
        CurrentMode = UiMode.Idle;
    }

    [RelayCommand]
    private void Open()
    {
        if (CurrentProject == null) return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Simulation State|simulation_state.json|All Files|*.*",
            InitialDirectory = CurrentProject.Path
        };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var json = File.ReadAllText(dialog.FileName);
            var state = JsonSerializer.Deserialize<SimulationSaveState>(json);
            if (state == null) return;

            _simService.WithEngineLock(engine =>
            {
                engine.Stop();

                var bodies = state.Bodies.Select(b => new PhysicsBody(
                    b.Id,
                    mass: b.Mass,
                    position: new Vec3d(b.PositionX, b.PositionY, b.PositionZ),
                    velocity: new Vec3d(b.VelocityX, b.VelocityY, b.VelocityZ),
                    type: b.Type)
                {
                    IsActive = b.IsActive,
                    IsCollidable = b.IsCollidable,
                }).ToArray();
                engine.SetBodies(bodies);

                // Apply saved config
                var c = state.Config;
                engine.Config.IntegratorName = c.IntegratorName;
                engine.Config.TimeStep = c.TimeStep;
                engine.Config.MinDt = c.MinDt;
                engine.Config.MaxDt = c.MaxDt;
                engine.Config.DeterministicMode = c.DeterministicMode;
                engine.Config.UseParallelComputation = c.UseParallelComputation;
                engine.Config.UseSimd = c.UseSimd;
                engine.Config.UseSoAPath = c.UseSoAPath;
                engine.Config.UseBarnesHut = c.UseBarnesHut;
                engine.Config.Theta = c.Theta;
                engine.Config.EnableCollisions = c.EnableCollisions;
                engine.Config.UseAdaptiveTimestep = c.UseAdaptiveTimestep;
                engine.Config.EnablePostNewtonian = c.EnablePostNewtonian;
                engine.Config.EnableGravitationalLensing = c.EnableGravitationalLensing;
                engine.Config.EnableAccretionDisks = c.EnableAccretionDisks;
                engine.Config.EnableGravitationalWaves = c.EnableGravitationalWaves;
                engine.Config.EnableJetEmission = c.EnableJetEmission;
                engine.Config.SofteningEpsilon = c.SofteningEpsilon;
                if (Enum.TryParse<SofteningMode>(c.SofteningMode, out var sm))
                    engine.Config.SofteningMode = sm;
                engine.ApplyConfig();
            });

            _simService.SetIntegrator(state.Config.IntegratorName);
            _sceneService.RepopulateFromSimulation(_simService);
            SceneOutlinerVm.Refresh();
            BodyInspectorVm.ClearSelection();
        }
        catch
        {
            // Silently ignore deserialization errors for now
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (CurrentProject == null) return;

        var statePath = Path.Combine(CurrentProject.Path, "simulation_state.json");

        _simService.WithEngineLock(engine =>
        {
            var state = new SimulationSaveState();

            if (engine.Bodies != null)
            {
                state.Bodies = engine.Bodies.Select(b => new BodySaveData
                {
                    Id = b.Id,
                    Mass = b.Mass,
                    Radius = b.Radius,
                    Density = b.Density,
                    PositionX = b.Position.X,
                    PositionY = b.Position.Y,
                    PositionZ = b.Position.Z,
                    VelocityX = b.Velocity.X,
                    VelocityY = b.Velocity.Y,
                    VelocityZ = b.Velocity.Z,
                    Type = b.Type,
                    IsActive = b.IsActive,
                    IsCollidable = b.IsCollidable,
                }).ToList();
            }

            var c = engine.Config;
            state.Config = new ConfigSaveData
            {
                IntegratorName = c.IntegratorName,
                TimeStep = c.TimeStep,
                MinDt = c.MinDt,
                MaxDt = c.MaxDt,
                DeterministicMode = c.DeterministicMode,
                UseParallelComputation = c.UseParallelComputation,
                UseSimd = c.UseSimd,
                UseSoAPath = c.UseSoAPath,
                UseBarnesHut = c.UseBarnesHut,
                Theta = c.Theta,
                EnableCollisions = c.EnableCollisions,
                UseAdaptiveTimestep = c.UseAdaptiveTimestep,
                EnablePostNewtonian = c.EnablePostNewtonian,
                EnableGravitationalLensing = c.EnableGravitationalLensing,
                EnableAccretionDisks = c.EnableAccretionDisks,
                EnableGravitationalWaves = c.EnableGravitationalWaves,
                EnableJetEmission = c.EnableJetEmission,
                SofteningEpsilon = c.SofteningEpsilon,
                SofteningMode = c.SofteningMode.ToString(),
            };

            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(statePath, json);
        });
    }

    [RelayCommand]
    private void Exit()
    {
        System.Windows.Application.Current.Shutdown();
    }

    // ═══════════════════════════════════════════════════════════════
    //  MODE COMMANDS (Bottom Control Bar — Left Group)
    // ═══════════════════════════════════════════════════════════════

    [RelayCommand]
    private void EnterAddMode()
    {
        if (CurrentMode == UiMode.AddPlacement)
        {
            CurrentMode = UiMode.Idle;
            IsPlacingObject = false;
            PlacementObjectType = string.Empty;
        }
        else
        {
            CurrentMode = UiMode.AddPlacement;
            IsPlacingObject = true;
            SelectedBodyType = BodyType.Star;
            PlacementObjectType = "Star";
        }
    }

    [RelayCommand]
    private void EnterSimulateMode()
    {
        CurrentMode = CurrentMode == UiMode.Simulate ? UiMode.Idle : UiMode.Simulate;
    }

    [RelayCommand]
    private void EnterEditMode()
    {
        CurrentMode = CurrentMode == UiMode.Edit ? UiMode.Idle : UiMode.Edit;
        IsPlacingObject = false;
    }

    [RelayCommand]
    private void EnterAnalyseMode()
    {
        CurrentMode = CurrentMode == UiMode.Analyse ? UiMode.Idle : UiMode.Analyse;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        if (RightPanelTabIndex == 1)
        {
            RightPanelTabIndex = 0; // Switch back to Inspector
        }
        else
        {
            RightPanelTabIndex = 1; // Switch to Settings tab
            ShowInspector = true;   // Ensure right panel is visible
        }
    }

    /// <summary>
    /// Changes the selected body type for placement mode.
    /// Called from the BodyTypePalette.
    /// </summary>
    public void SelectBodyType(BodyType type)
    {
        SelectedBodyType = type;
        PlacementObjectType = type.ToString();
    }

    /// <summary>
    /// Called by ViewportPanel when user left-clicks to place an object.
    /// </summary>
    public void PlaceObjectAt(float worldX, float worldY, float worldZ)
    {
        if (!IsPlacingObject) return;

        _simService.WithEngineLock(engine =>
        {
            int nextId = (engine.Bodies?.Length ?? 0) + 1;
            var body = new PhysicsBody(
                nextId,
                mass: DefaultMassForType(SelectedBodyType),
                position: new Vec3d(worldX, worldY, worldZ),
                velocity: Vec3d.Zero,
                type: SelectedBodyType);
            engine.AddBody(body);
        });
        _sceneService.RepopulateFromSimulation(_simService);
    }

    /// <summary>
    /// Called by ViewportPanel when user right-clicks to deselect/cancel placement.
    /// </summary>
    public void CancelPlacement()
    {
        if (CurrentMode == UiMode.AddPlacement)
        {
            IsPlacingObject = false;
            PlacementObjectType = string.Empty;
            CurrentMode = UiMode.Idle;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  BODY MANAGEMENT (Phase 4)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Deletes a body by its engine ID.</summary>
    private void DeleteBody(int bodyId)
    {
        _simService.RemoveBody(bodyId);
        _sceneService.RepopulateFromSimulation(_simService);
        BodyInspectorVm.ClearSelection();
    }

    /// <summary>
    /// Selects a body by its engine ID. Called from ViewportPanel on raycast hit.
    /// </summary>
    public void SelectBodyById(int bodyId)
    {
        var nodeId = _sceneService.GetNodeIdForBody(bodyId);
        if (nodeId.HasValue)
        {
            _sceneService.SelectionManager.Select(nodeId.Value);
            BodyInspectorVm.LoadBody(nodeId.Value);
            SceneOutlinerVm.SetSelectedNodeId(nodeId.Value);
        }
    }

    /// <summary>
    /// Deselects the current body. Called from ViewportPanel on empty-space click.
    /// </summary>
    public void DeselectBody()
    {
        _sceneService.SelectionManager.Clear();
        BodyInspectorVm.ClearSelection();
        SceneOutlinerVm.SetSelectedNodeId(null);
    }

    [RelayCommand]
    private void DeleteSelectedBody()
    {
        var selectedNodeId = _sceneService.SelectionManager.SelectedEntity;
        if (selectedNodeId == null) return;
        var bodyId = _sceneService.GetBodyIdForNode(selectedNodeId.Value);
        if (bodyId.HasValue)
            DeleteBody(bodyId.Value);
    }

    /// <summary>Returns a sensible default mass for each body type.</summary>
    private static double DefaultMassForType(BodyType type) => type switch
    {
        BodyType.Star => 1.0,
        BodyType.Planet => 0.001,
        BodyType.GasGiant => 0.01,
        BodyType.RockyPlanet => 0.0005,
        BodyType.Moon => 0.0001,
        BodyType.Asteroid => 0.00001,
        BodyType.NeutronStar => 2.0,
        BodyType.BlackHole => 10.0,
        BodyType.Comet => 0.000001,
        BodyType.Custom => 1.0,
        _ => 1.0,
    };

    // ═══════════════════════════════════════════════════════════════
    //  SIMULATION CONTROL COMMANDS (Bottom Control Bar — Right Group)
    // ═══════════════════════════════════════════════════════════════

    [RelayCommand]
    private void StartSimulation()
    {
        if (_simService.LastState != EngineState.Running)
        {
            _simService.StartSimThread(); // idempotent
            _simService.Play();
        }
    }

    [RelayCommand]
    private void PauseSimulation()
    {
        if (_simService.LastState == EngineState.Running)
        {
            _simService.Pause();
        }
    }

    [RelayCommand]
    private void ResetSimulation()
    {
        _simService.ResetScene();
        _sceneService.RepopulateFromSimulation(_simService);
        _renderer.ClearTrails();
        _renderer.SelectedInstanceIndex = -1;
        BodyInspectorVm.ClearSelection();
        CurrentMode = UiMode.Idle;
    }

    // ═══════════════════════════════════════════════════════════════
    //  PROPERTY CHANGE CALLBACKS
    // ═══════════════════════════════════════════════════════════════

    partial void OnTimeScaleChanged(double value)
    {
        _simService.TimeScale = value;
    }

    partial void OnShowGridChanged(bool value)
    {
        _renderer.ShowGrid = value;
    }

    partial void OnShowVelocityArrowsChanged(bool value)
    {
        _renderer.ShowVelocityArrows = value;
    }

    partial void OnShowStarfieldChanged(bool value)
    {
        _renderer.ShowStarfield = value;
    }

    partial void OnShowTrailsChanged(bool value)
    {
        _renderer.ShowTrails = value;
    }

    partial void OnRightPanelTabIndexChanged(int value)
    {
        if (value == 1)
        {
            SimulationSettingsVm.LoadFromEngine();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  UI TIMER (20 Hz status bar refresh)
    // ═══════════════════════════════════════════════════════════════

    private void OnUiTimerTick(object? sender, EventArgs e)
    {
        var state = _simService.LastState;

        SimulationState = state switch
        {
            EngineState.Running => SimulationState.Running,
            EngineState.Paused  => SimulationState.Paused,
            _                   => SimulationState.Idle
        };
        SimulationStateText = SimulationState.ToString();

        PhysicsTimeText = $"Physics: {_simService.LastPhysicsTimeMs:F1} ms";

        _simService.WithEngineLock(engine =>
        {
            BodyCountText = $"Bodies: {engine.Bodies?.Length ?? 0}";
        });
    }

    /// <summary>
    /// Called from code-behind to feed render-thread metrics into the ViewModel.
    /// </summary>
    public void UpdateRenderMetrics(double fps, double renderTimeMs)
    {
        FpsText = $"FPS: {fps:F0}";
        RenderTimeText = $"Render: {renderTimeMs:F1} ms";
    }

    // ═══════════════════════════════════════════════════════════════
    //  CLEANUP
    // ═══════════════════════════════════════════════════════════════

    public void Dispose()
    {
        _uiTimer.Stop();
        SceneOutlinerVm.Dispose();
        _simService.Dispose();
        _renderer.Dispose();
        _sceneService.Dispose();
    }
}
