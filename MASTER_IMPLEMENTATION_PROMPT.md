# MASTER IMPLEMENTATION PROMPT — Celestial Mechanics Desktop

> **Usage**: Copy ONE phase at a time and give it as a prompt. Each phase is self-contained.
> **Project**: `c:\sharon\projects\celestial-mechanics-desktop`
> **Stack**: WPF (.NET 8) · Silk.NET OpenGL · CommunityToolkit.Mvvm 8.4 · GLSL 330

---

# ══════════════════════════════════════════════════════════════
# PHASE 1 — SIMULATION RUNTIME INTEGRATION (P0 Critical)
# ══════════════════════════════════════════════════════════════

## PROMPT — PHASE 1

You are working on `c:\sharon\projects\celestial-mechanics-desktop`, a WPF .NET 8 desktop application for N-body gravitational simulation. The UI shell is complete but the simulation doesn't actually run yet — new projects open to an empty black viewport. Your task is to make the simulation functional end-to-end.

### CONTEXT — What Already Exists

**SimulationService** (`src/CelestialMechanics.Desktop/Services/SimulationService.cs`, 186 lines):
- Thread-safe wrapper around `SimulationEngine` with dedicated sim thread via `StartSimThread()` running at ~1kHz
- Key methods: `Play()`, `Pause()`, `Step()`, `ResetScene()`, `AddBody(PhysicsBody)`, `RemoveBody(int)`, `SetIntegrator(string)`, `ApplyConfig(Action<PhysicsConfig>)`
- `WithEngineLock(Action<SimulationEngine>)` for thread-safe engine access
- Atomic metric snapshots: `LastState`, `LastSimTime`, `LastPhysicsTimeMs`, `LastSimState`
- `TimeScale` property (0.1–10.0) via `Interlocked`
- `StateUpdated` event fired at ~30Hz from sim thread

**SimulationEngine** (`src/CelestialMechanics.Simulation/SimulationEngine.cs`, 239 lines):
- `SetBodies(PhysicsBody[])`, `AddBody(PhysicsBody)`, `RemoveBody(int)`
- `Start()`, `Pause()`, `Stop()`, `StepOnce()`, `Update(double frameTime)`
- `State` → `EngineState` enum: `Stopped`, `Running`, `Paused`
- `CurrentState` → `SimulationState` with: `TotalEnergy`, `KineticEnergy`, `PotentialEnergy`, `EnergyDrift`, `TotalMomentum` (Vec3d), `BodyCount`
- `Config` → `PhysicsConfig`, `ApplyConfig()`, `SetIntegrator("Euler"|"Verlet"|"RK4")`

**PhysicsBody** (struct-like, from `CelestialMechanics.Physics.Types`):
```csharp
new PhysicsBody(int id, double mass, Vec3d position, Vec3d velocity, BodyType type)
{ Radius, IsActive, IsCollidable, GravityStrength, GravityRange }
```

**BodyType enum**: `Star, Planet, GasGiant, RockyPlanet, Moon, Asteroid, NeutronStar, BlackHole, Comet, Custom`

**MainWindowViewModel** (`src/CelestialMechanics.Desktop/ViewModels/MainWindowViewModel.cs`, 707 lines):
- Owns `_simService` (SimulationService), `_sceneService` (SceneService), `_renderer` (GLRenderer), `_projectService`
- Has `_uiTimer` (DispatcherTimer at 50ms/20Hz) calling `OnUiTimerTick()`
- `OnUiTimerTick()` reads `_simService.LastState`, updates `FpsText`, `BodyCountText`, `PhysicsTimeText`, `RenderTimeText`
- Has `CreateProjectInternal(name, path)` and `OpenProjectInternal(path)` methods
- Has `PlaceObjectAt(x, y, z)` that creates PhysicsBody and calls `_simService.AddBody()`
- Has observable properties: `SimStateText`, `SimTimeText`

**RenderLoop** (`Infrastructure/RenderLoop.cs`, 145 lines): 60fps dedicated thread, calls `renderer.UpdateFromSimulation(engine)` under engine lock

**GLRenderer** (`CelestialMechanics.Renderer/GLRenderer.cs`, 217 lines):
- `UpdateFromSimulation(SimulationEngine engine)` reads engine.Bodies, updates sphere instances, records trails
- 5-pass pipeline: starfield → grid → trails → bodies (procedural shaders) → velocity arrows

**SceneService** (`Services/SceneService.cs`, 98 lines):
- `PopulateFromSimulation(SimulationService)` — creates SceneNode for each body
- `RepopulateFromSimulation(SimulationService)` — clears and rebuilds
- `RegisterBodyNode(Guid nodeId, int bodyId)` — bidirectional mapping

### TASK — Step by Step

**Step 1**: Create `src/CelestialMechanics.Desktop/Services/DefaultSceneFactory.cs` — static class with `CreateSolarSystem()` returning `PhysicsBody[]` containing:
- Sun: id=1, mass=1.0, pos=origin, vel=zero, type=Star, radius=0.05
- Earth: id=2, mass=3e-6, pos=(1,0,0), vel=(0,0,6.2832), type=Planet, radius=0.015
- Jupiter: id=3, mass=9.5e-4, pos=(5.2,0,0), vel=(0,0,2.755), type=GasGiant, radius=0.035
- Mars: id=4, mass=3.2e-7, pos=(1.524,0,0), vel=(0,0,5.089), type=RockyPlanet, radius=0.01
- All with: IsActive=true, IsCollidable=true, GravityStrength=60, GravityRange=100
- Units: solar masses, AU, AU/TU where circular velocity = 2π/√r

**Step 2**: Add to `SimulationService.cs`:
```csharp
public void LoadBodies(PhysicsBody[] bodies) { lock (_engineLock) _engine.SetBodies(bodies); }
public PhysicsBody[] GetBodies() { lock (_engineLock) return _engine.Bodies.ToArray(); }
```

**Step 3**: In `MainWindowViewModel.CreateProjectInternal()`, after project directory creation, add:
```csharp
var bodies = DefaultSceneFactory.CreateSolarSystem();
_simService.LoadBodies(bodies);
_sceneService.RepopulateFromSimulation(_simService);
SceneOutlinerVm.RefreshFromScene();
_simService.StartSimThread();
_renderer.ClearTrails();
```

**Step 4**: Do the same in `OpenProjectInternal()` after loading saved bodies.

**Step 5**: Add observable properties if missing: `[ObservableProperty] string _totalEnergyText = "E: --"`, `[ObservableProperty] string _momentumText = "P: --"`. In `OnUiTimerTick()`, read `_simService.LastSimState` and update `SimTimeText`, `TotalEnergyText`, `MomentumText`.

**Step 6**: In `MainWindow.xaml` StatusBar, add TextBlocks bound to `SimTimeText`, `TotalEnergyText`, `MomentumText`.

**Step 7**: Ensure Simulate mode button calls `_simService.Play()`, and leaving Simulate mode calls `_simService.Pause()`.

**Step 8**: Reset command should: pause, reset scene, reload defaults, repopulate scene, clear trails.

### VERIFICATION
1. `dotnet build src/CelestialMechanics.Desktop` — 0 errors
2. Run → New Project → 4 bodies visible (Sun, Earth, Jupiter, Mars)
3. Simulate mode → bodies orbit with trails
4. Status bar: live FPS, bodies, physics ms, energy, sim time
5. Pause/Step/Reset all work
6. Commit: `feat: wire simulation runtime - default scene, live metrics, play/pause/reset`

---

# ══════════════════════════════════════════════════════════════
# PHASE 2 — SNAPSHOT TIMELINE & EVENT LOG
# ══════════════════════════════════════════════════════════════

## PROMPT — PHASE 2

You are working on `c:\sharon\projects\celestial-mechanics-desktop`. Phase 1 is complete — simulation runs, bodies orbit, metrics display. Now add snapshot time-scrubbing and an event log panel.

### CONTEXT — Backend APIs

**SnapshotManager** (`src/CelestialMechanics.AppCore/Snapshot/SnapshotManager.cs`):
- `CaptureSnapshot(SimulationEngine engine)` → stores snapshot
- `RestoreSnapshot(int index, SimulationEngine engine)` → restores state
- `SnapshotCount` (int), `GetSnapshot(int index)` → `SimulationSnapshot` with `.SimulationTime`, `.StepIndex`, `.Bodies`

**EventBus** (`src/CelestialMechanics.Simulation/EventBus.cs`):
- `Subscribe(Action<SimulationEvent>)`, `Unsubscribe(...)`, `Publish(...)`, `Clear()`
- `SimulationEvent`: `Type` (string), `Message` (string), `Time` (double)

**Event Types** in `src/CelestialMechanics.Simulation/Events/`: `SupernovaEvent`, `MergerEvent`, `CollapseEvent`

**UI Patterns**: ViewModels use `[ObservableProperty]`/`[RelayCommand]` from CommunityToolkit.Mvvm. Panels are UserControls in `Views/Panels/` with DockPanel, `PanelHeaderBorderStyle`/`PanelHeaderTextStyle`. Colors: `BackgroundSecondary` #121829, `CyanAccent` #4FC3F7, `TextPrimary` #E8EAF0

### TASK — Timeline

Create `ViewModels/TimelineViewModel.cs`: properties `SnapshotCount`, `CurrentIndex` (two-way slider), `SimTimeLabel`, `StepLabel`, `IsScrubbing`. Auto-capture every 100 steps via `RecordIfNeeded()`. On slider change → pause sim, restore snapshot, update labels. `PlayFromHere` command.

Create `Views/Panels/TimelinePanel.xaml`: 32px height horizontal bar. Slider + step/time labels + play button. Uses `SpaceSliderStyle`, `ControlBarBackground`.

Modify `MainWindow.xaml`: add TimelinePanel row between viewport and control bar.
Modify `MainWindowViewModel.cs`: own TimelineViewModel, call `RecordIfNeeded()` from timer.

### TASK — Event Log

Create `Models/EventLogItem.cs`: record with Type, Message, Time, ColorHex.

Create `ViewModels/EventLogViewModel.cs`: `ObservableCollection<EventLogItem>`, subscribe to EventBus, marshal to UI thread, cap 500 entries, color mapping (Supernova=#EF5350, Merger=#FFA726, Collision=#4FC3F7, System=#8892B0). Filter toggles, ClearCommand.

Create `Views/Panels/EventLogPanel.xaml`: PanelHeader "EVENT LOG", ListView with VirtualizingStackPanel, colored items, filter toggles, clear button.

Modify `MainWindow.xaml`: add collapsible bottom panel (150px), toggle via View menu.

### VERIFICATION
1. Build succeeds
2. Run sim 10s → timeline shows snapshots, dragging scrubs time
3. "Play from here" resumes simulation
4. Collisions produce event log entries with colors
5. Commit: `feat: add snapshot timeline and event log panel`

---

# ══════════════════════════════════════════════════════════════
# PHASE 3 — ENERGY CHART & ENHANCED INSPECTOR
# ══════════════════════════════════════════════════════════════

## PROMPT — PHASE 3

You are working on `c:\sharon\projects\celestial-mechanics-desktop`. Phases 1-2 complete. Add real-time energy chart and extend body inspector.

### CONTEXT
`SimulationState` has: `KineticEnergy`, `PotentialEnergy`, `TotalEnergy`, `EnergyDrift`, `TotalMomentum`, `BodyCount`. Existing inspector has GroupBoxes: IDENTITY, TRANSFORM, PHYSICAL, SIMULATION using `InspectorGroupBoxStyle`.

### TASK — Energy Chart (Custom Canvas, NO external packages)

Create `ViewModels/EnergyChartViewModel.cs`: 200-point ring buffers for KE/PE/TE. `RecordDataPoint(SimulationState)` at 20Hz. `DriftPercent`, `DriftColorHex` (green<0.01%, yellow<1%, red>1%).

Create `Views/Panels/EnergyChartPanel.xaml + .cs`: 200px panel, PanelHeader "ENERGY MONITOR". Override `OnRender()` to draw 3 polylines on Canvas (KE=cyan #4FC3F7, PE=orange #FFB74D, TE=yellow #FFA726). Auto-scale Y-axis. Drift badge. Legend bar. Togglable via View menu.

Wire into MainWindowViewModel timer. Add as third tab in right panel (Inspector|Settings|Energy).

### TASK — Enhanced Inspector

Modify `BodyInspectorPanel.xaml`: add ACCELERATION GroupBox (read-only XYZ, E4 format), STELLAR GroupBox (Luminosity, Temperature, Age, FuelFraction — visible only for Star/NeutronStar). Add unit labels after fields.

Modify `BodyInspectorViewModel.cs`: add acceleration properties, stellar properties, `HasStellarData` bool.

### VERIFICATION
1. Energy chart shows 3 lines updating real-time, drift badge changes color
2. Inspector shows acceleration, stellar sections for stars only
3. Commit: `feat: add energy chart and enhanced inspector`

---

# ══════════════════════════════════════════════════════════════
# PHASE 4 — BODY CREATION DIALOG & HIERARCHICAL OUTLINER
# ══════════════════════════════════════════════════════════════

## PROMPT — PHASE 4

Phases 1-3 complete. Add professional body creation dialog and hierarchical scene tree.

### TASK — Add Body Dialog

Create `ViewModels/AddBodyViewModel.cs`: Presets collection (Sun/Earth/Jupiter/Mars/Moon/Asteroid/NeutronStar/BlackHole/Binary/Cluster), custom form (Name, Type, Mass, Position XYZ, Velocity XYZ, Radius), validation, `BodyCreated` event.

Create `Views/Modals/AddBodyDialog.xaml`: 500×550 modal, same styling as existing modals. TabControl (PRESETS tab with WrapPanel cards, CUSTOM tab with form). Cancel + "Add to Scene" buttons.

Wire into MainWindowViewModel with `IsAddBodyDialogVisible` property.

### TASK — Hierarchical Scene Outliner

Replace ListBox in `SceneOutlinerPanel.xaml` with TreeView + `HierarchicalDataTemplate`. Context menu: Add Body, Add Folder, Rename, Delete.

Create `ViewModels/SceneNodeViewModel.cs`: wraps SceneNode, `ObservableCollection<SceneNodeViewModel> Children`, reacts to SceneGraph events.

Modify `SceneOutlinerViewModel.cs`: tree structure, AddFolderCommand, multi-select.

### VERIFICATION
1. Add Body dialog works with presets and custom entry
2. Scene outliner shows tree, folders group bodies, context menu works
3. Commit: `feat: add body creation dialog and hierarchical outliner`

---

# ══════════════════════════════════════════════════════════════
# PHASE 5 — UNDO/REDO, VALIDATION & CONVERTERS
# ══════════════════════════════════════════════════════════════

## PROMPT — PHASE 5

Phases 1-4 complete. Add undo/redo, input validation, utility converters.

### TASK — Undo/Redo

Create `Services/UndoRedoService.cs`: uses SnapshotManager, two stacks (undo max 50, redo), `RecordState()`, `Undo()`, `Redo()`, `CanUndo`/`CanRedo` properties.

Wire into MainWindowViewModel: Ctrl+Z/Ctrl+Y InputBindings, record before body add/remove/settings apply.

### TASK — Validation

Create `ValidationRules/PositiveDoubleRule.cs` (mass>0, radius>0). Apply to inspector TextBox bindings. Red border + tooltip on failure.

### TASK — Converters

Create in `Converters/`: `EngineStateToColorConverter` (Running=#66BB6A, Paused=#FFA726, Stopped=#EF5350), `InverseBoolConverter`, `BodyTypeToIconConverter` (Star→★, Planet→● etc).

### VERIFICATION
1. Ctrl+Z undoes body addition, Ctrl+Y redoes
2. Negative mass shows red border, apply disabled
3. Correct state colors
4. Commit: `feat: add undo/redo, validation, converters`

---

# ══════════════════════════════════════════════════════════════
# PHASE 6 — POLISH, DI & ADVANCED RENDERING
# ══════════════════════════════════════════════════════════════

## PROMPT — PHASE 6

Phases 1-5 complete. Final phase: DI container, modal animations, keyboard overlay, optional accretion disks.

### TASKS

**DI**: Add `Microsoft.Extensions.DependencyInjection`. In `App.xaml.cs OnStartup`: register all services as singletons, ViewModels as transient. Resolve MainWindow DataContext from container.

**Animations**: In `Theme.xaml`: add `ModalFadeIn` Storyboard (opacity 0→1, 200ms, QuadraticEase), `ModalSlideUp` (translateY 30→0 + opacity, 250ms). Apply via EventTrigger Loaded on modal Borders.

**Keyboard Overlay**: Create `Views/KeyboardShortcutsOverlay.xaml` — full-window semi-transparent overlay (80% opacity), centered card (600×500) showing shortcuts grouped by Camera/Simulation/Editing/File. Toggle with `?` key, dismiss with Escape.

**Advanced Rendering** (optional): Add accretion disk particle ring shader for BlackHole bodies. Create `accretion.vert/frag`, add Pass 6 in GLRenderer.

### VERIFICATION
1. DI resolves all services, app starts correctly
2. Modals animate smoothly
3. `?` shows shortcut overlay, Escape dismisses
4. Final commits, `dotnet build` and `dotnet test` pass
5. Commit: `feat: add DI, animations, keyboard overlay`

---

# POST-IMPLEMENTATION CHECKLIST

After all 6 phases:
- [ ] `dotnet build` — 0 errors
- [ ] `dotnet test` — all tests pass
- [ ] New project → default scene with orbiting bodies
- [ ] All sim controls work (Play/Pause/Step/Reset)
- [ ] Timeline scrubbing forward/backward
- [ ] Event log captures events with colors
- [ ] Energy chart with 3 real-time traces
- [ ] Inspector shows stellar data for stars
- [ ] Add Body dialog with presets + custom
- [ ] Hierarchical scene tree with folders
- [ ] Undo/Redo for body add/remove
- [ ] Input validation on numeric fields
- [ ] Modal animations smooth
- [ ] Keyboard overlay accessible
- [ ] 6 clean feature commits in git
