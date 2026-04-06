# CelestialMechanics.Desktop — UI Planning

WPF desktop front-end for the CelestialMechanics N-body gravitational simulation engine.

---

## 1. Current Status

### Implemented

| Component | File(s) | Details |
|---|---|---|
| WPF Application Shell | `App.xaml`, `App.xaml.cs` | Startup wired to `MainWindow`; loads `SpaceDarkTheme` |
| Main Window | `Views/MainWindow.xaml`, `.cs` | 3-row layout: toolbar, viewport, status bar |
| Toolbar | `Views/MainWindow.xaml` (Row 0) | Play/Pause, Step, Reset, integrator combo (Verlet/Euler/RK4), grid/arrow toggles, sim-time label |
| 3D Viewport | `Views/ViewportPanel.xaml`, `.cs` | OpenGL child window via `HwndHost`; mouse orbit/pan/zoom |
| Status Bar | `Views/MainWindow.xaml` (Row 2) | FPS, body count, physics time, render time, total energy |
| OpenGL Host | `Infrastructure/OpenGLHost.cs` | Creates native HWND, WGL context, DPI-aware; exposes `Silk.NET.OpenGL.GL` |
| Render Loop | `Infrastructure/RenderLoop.cs` | Dedicated render thread, 60 FPS pacing, WGL context ownership, FPS tracking |
| Win32 Interop | `Infrastructure/Win32Interop.cs` | P/Invoke declarations for window, pixel format, and WGL APIs |
| Simulation Service | `Services/SimulationService.cs` | Thread-safe engine wrapper; dedicated sim thread (~1 kHz); atomic metric snapshots via `Interlocked`; `WithEngineLock` callback for safe reads |
| Theme — Colors | `Themes/Colors.xaml` | 20+ named colors: dark navy backgrounds, cyan/purple accents, status green/yellow/red |
| Theme — Controls | `Themes/SpaceDarkTheme.xaml` | Styled templates for Button, ToggleButton, ComboBox, TextBox, StatusBar, ToolBar, etc. |
| Post-build Shaders | `.csproj` | Copies `*.vert`/`*.frag` from Renderer project to output |

### Placeholder / Stub

| Item | Notes |
|---|---|
| `App.xaml.cs` `OnStartup` | Empty override — no DI, no service registration |
| `ViewModels/` directory | Exists but contains no files |
| `Converters/` directory | Exists but contains no files |

### Not Yet Started

| Feature | AppCore API Available |
|---|---|
| Scene outliner (tree view) | `SceneGraph`, `SceneNode`, `NodeAdded`/`NodeRemoved`/`NodeMoved` events |
| Body inspector / property editor | `PhysicsComponent` (Mass, Position, Velocity, Radius, Density) |
| Simulation settings panel | `PhysicsConfig` (30+ configurable properties) |
| Body creation dialog | `Entity`, `PhysicsComponent`, `CelestialMechanics.Data` factory presets |
| Save / Load project | `ProjectSerializer.SaveProject()`, `ProjectDeserializer.LoadProject()` |
| Snapshot timeline / scrubber | `SnapshotManager` (capture, restore, index access) |
| Viewport body selection | `SelectionManager` (Select, MultiSelect, events) |
| Event log | `EventBus.Subscribe()` for supernova, merge, and other simulation events |
| MVVM data-binding layer | `CommunityToolkit.Mvvm` is referenced but unused |
| Dependency injection | No IoC container configured |

---

## 2. UI Requirements

Requirements derived from the public API surface of `CelestialMechanics.AppCore`, `Simulation`, and `Physics`.

### 2.1 Simulation Control

| Requirement | API Entry Point | Status |
|---|---|---|
| Play / Pause | `SimulationService.Play()` / `.Pause()` | Done |
| Single Step | `SimulationService.Step()` | Done |
| Reset Scene | `SimulationService.ResetScene()` | Done |
| Integrator Picker | `SimulationService.SetIntegrator(name)` | Done |
| Time-Scale Slider | `SimulationManager.Time.TimeScale` (double, default 1.0) | Needed |
| Engine State Indicator | `SimulationService.LastState` (Running/Paused/Stopped) | Done |

### 2.2 Scene Outliner

Display the `SceneGraph` as a hierarchical tree.

- Bind a `TreeView` to `SceneGraph.Root.Children` recursively
- Each node shows `SceneNode.Name` and an icon based on `NodeType` (Folder / Entity / Composite)
- Support add, rename, delete, and drag-drop reparent via `SceneGraph.AddNode`, `RemoveNode`, `MoveNode`
- Listen to `NodeAdded`, `NodeRemoved`, `NodeMoved` events for live updates
- Clicking a node calls `SelectionManager.Select(node.LinkedEntityId)`
- Multi-select via Ctrl+click mapped to `SelectionManager.AddToSelection`

### 2.3 Body Inspector

Edit the selected entity's components in a side panel.

- **Identity**: `Entity.Tag` (string), `Entity.Id` (Guid, read-only)
- **Physics** (from `PhysicsComponent`):
  - Mass (kg, double, editable)
  - Position X/Y/Z (m, Vec3d, editable)
  - Velocity X/Y/Z (m/s, Vec3d, editable)
  - Acceleration X/Y/Z (m/s^2, Vec3d, read-only)
  - Radius (m, double, editable)
  - Density (kg/m^3, double, editable)
  - IsCollidable (bool toggle)
- **Stellar** (from `StellarEvolutionComponent`, if present):
  - Luminosity, Temperature, Age, FuelFraction (read-only)
- **Relativistic** (from `RelativisticComponent`, if present):
  - SchwarzschildRadius (read-only)
- Two-way bindings update the live simulation when paused; read-only while running

### 2.4 Simulation Settings Panel

Expose `PhysicsConfig` properties grouped by category.

**Core**
- `TimeStep` — numeric input (default 0.001)
- `SofteningEpsilon` — numeric input (default 1e-4)
- `DeterministicMode` — checkbox (default true)

**Force Solver**
- `UseBarnesHut` — checkbox (default false)
- `Theta` — slider 0.0-1.5 (default 0.5, only enabled when Barnes-Hut is on)
- `SofteningMode` — dropdown (Constant / Plummer)
- `UseParallelComputation` — checkbox
- `UseSimd` — checkbox

**Collisions**
- `EnableCollisions` — checkbox
- `UseAdaptiveTimestep` — checkbox
- `MinDt` / `MaxDt` — numeric inputs (only when adaptive is on)

**Relativistic Physics**
- `EnablePostNewtonian` — checkbox
- `EnableGravitationalLensing` — checkbox
- `EnableAccretionDisks` — checkbox
- `EnableGravitationalWaves` — checkbox
- `EnableJetEmission` — checkbox
- `MaxAccretionParticles` — numeric (default 5000)
- `GravitationalWaveObserverDistance` — numeric (default 1000)

### 2.5 Body Creation

Dialog or flyout panel to add new bodies to the simulation.

- **Preset Templates** from `CelestialMechanics.Data`:
  - Star (various masses), Planet (Earth-like, gas giant), Asteroid, Black Hole, Neutron Star
  - Composite: Binary System, Planetary System
- **Custom Entry**: manual mass, position, velocity, radius, body type
- On confirm: create `Entity` + `PhysicsComponent`, add to `SimulationManager`, add `SceneNode` to `SceneGraph`

### 2.6 Save / Load Project

- **Save**: `Microsoft.Win32.SaveFileDialog` with `.cesim` filter, calls `ProjectSerializer.SaveProject(path, scene, manager)` — must pause simulation first
- **Load**: `Microsoft.Win32.OpenFileDialog`, calls `ProjectDeserializer.LoadProject(path)` — check `ProjectLoadResult.Success`, show warnings if any
- Toolbar buttons or File menu: New, Open, Save, Save As

### 2.7 Snapshot Timeline

Horizontal slider/scrubber at the bottom of the viewport.

- `SnapshotManager.CaptureSnapshot(manager)` at configurable interval (every N steps)
- Slider range = `0 .. SnapshotManager.SnapshotCount - 1`
- Dragging slider calls `SnapshotManager.RestoreSnapshot(snapshot, manager)` to scrub time
- Display `SimulationSnapshot.SimulationTime` and `StepIndex` as labels
- Play-from-here button resumes simulation from the restored state

### 2.8 Viewport Selection

- Left-click in viewport → unproject screen coordinates to world ray
- Test ray against all active `PhysicsComponent.Position` / `Radius` spheres
- On hit: `SelectionManager.Select(entityId)` — highlight body with a selection ring or glow
- On miss: `SelectionManager.Clear()`
- Shift+click: `SelectionManager.AddToSelection(entityId)`
- `SelectionManager.OnSelectionChanged` event updates the inspector panel

### 2.9 Event Log

Scrollable list panel showing simulation events.

- Subscribe to `SimulationManager.EventBus` via `.Subscribe(handler)`
- Display `SimulationEvent.Type`, `.Message`, `.Time`
- Color-coded by type (supernova = red, merge = yellow, expansion = blue)
- Optional: filter by event type

### 2.10 Energy & Diagnostics

- **Status bar** (existing): FPS, body count, physics time ms, render time ms, total energy
- **Extended**: Total kinetic energy, total potential energy, momentum magnitude, angular momentum
- **Optional**: Real-time energy chart (rolling window) using custom canvas or OxyPlot

---

## 3. Suggestions

### 3.1 Architecture

- **MVVM Pattern**: Use `CommunityToolkit.Mvvm` (`ObservableObject`, `RelayCommand`, `[ObservableProperty]`) for all ViewModels. The NuGet package is already referenced in the `.csproj`.
- **Dependency Injection**: Register `SimulationService`, `ModeManager`, `SelectionManager`, etc. in `App.xaml.cs` `OnStartup`. Pass via constructor injection to ViewModels.
- **ViewModels to create**:

  | ViewModel | Responsibility |
  |---|---|
  | `MainViewModel` | Top-level state, engine state, toolbar commands, delegates to child VMs |
  | `SceneOutlinerViewModel` | Wraps `SceneGraph`, exposes `ObservableCollection<SceneNodeViewModel>` tree |
  | `BodyInspectorViewModel` | Wraps selected `Entity` + `PhysicsComponent`, two-way property editing |
  | `SimulationSettingsViewModel` | Wraps `PhysicsConfig`, grouped toggle/slider properties |
  | `TimelineViewModel` | Wraps `SnapshotManager`, slider position, capture/restore commands |
  | `EventLogViewModel` | Wraps `EventBus`, `ObservableCollection<SimulationEventItem>` |

- **Converters to create**:

  | Converter | Purpose |
  |---|---|
  | `BoolToVisibilityConverter` | Standard WPF pattern |
  | `Vec3dToStringConverter` | Display `Vec3d` as `(x, y, z)` |
  | `BodyTypeToIconConverter` | Map `BodyType` enum to icon path or glyph |
  | `EngineStateToColorConverter` | Running = green, Paused = yellow, Stopped = red |
  | `InverseBoolConverter` | Negate boolean for IsEnabled bindings |

### 3.2 Layout

Recommended 3-column dockable layout:

```
+------------------+---------------------------+-------------------+
|  Scene Outliner  |                           |  Body Inspector   |
|  (TreeView)      |        3D Viewport        |  (Properties)     |
|                  |        (OpenGL)           |                   |
|                  |                           |  Sim Settings     |
|                  |                           |  (Toggles/Sliders)|
+------------------+---------------------------+-------------------+
|                 Snapshot Timeline Slider                         |
+-----------------------------------------------------------------+
|  Toolbar: Play | Pause | Step | Reset | Integrator | Time Scale |
+-----------------------------------------------------------------+
|  Status Bar: FPS | Bodies | Physics ms | Render ms | Energy     |
+-----------------------------------------------------------------+
```

- Use `GridSplitter` between columns so users can resize panels
- Collapsible panels via toggle buttons in the toolbar
- Tab control in right panel to switch between Inspector and Settings

### 3.3 UX Recommendations

- **Keyboard Shortcuts**: Space = play/pause, R = reset, S = single step, Ctrl+S = save, Ctrl+O = open, Delete = remove selected body
- **Body Presets**: Quick-add buttons in toolbar or outliner context menu (Add Star, Add Planet, Add Binary)
- **Tooltips**: Show property units (kg, m, m/s) and valid ranges on inspector fields
- **Validation**: Clamp mass > 0, radius > 0, warn on extreme velocities (> 0.1c)
- **Undo/Redo**: optional but valuable; can leverage snapshot system for state-level undo
- **GPU Engine Mode Selector**: Dropdown for the 8 engine modes (CPU_BruteForce, CPU_BarnesHut, GPU_BruteForce, GPU_BarnesHut) x (Leapfrog, Yoshida4) when the native engine is loaded
- **Orbital Trails**: Extend existing `ShowArrowsCheck` concept; store per-body position history ring buffer, render as GL lines
- **Real-Time Energy Graph**: Small docked panel with rolling window line chart (total, kinetic, potential energy over simulation time)

### 3.4 Performance

- UI refresh at 20 Hz (current `DispatcherTimer` at 50ms) is appropriate; do not increase
- Render thread at 60 FPS via `RenderLoop` — already well-optimized
- Simulation thread at ~1 kHz — do not block with UI operations
- For large body counts (>10k), virtualize the scene outliner `TreeView` with `VirtualizingStackPanel`
- Batch property change notifications when restoring snapshots to avoid UI thrash

---

## 4. UI Implementation Plan

### Phase A — MVVM Foundation

**Goal**: Establish the MVVM infrastructure so all subsequent phases use bindings.

**Files to create / modify**:
- `ViewModels/ViewModelBase.cs` — base class inheriting `ObservableObject`
- `ViewModels/MainViewModel.cs` — top-level ViewModel with engine state and commands
- `App.xaml.cs` — initialize services and wire `MainWindow.DataContext`
- `Views/MainWindow.xaml` — convert event handlers to `{Binding}` commands

**Key deliverables**:
- `MainViewModel` replaces code-behind logic in `MainWindow.xaml.cs`
- Toolbar commands (PlayPause, Step, Reset) become `RelayCommand` instances
- Status bar fields bound to observable properties updated by timer

---

### Phase B — Panel Layout

**Goal**: Restructure MainWindow from single-viewport to 3-panel layout.

**Files to modify**:
- `Views/MainWindow.xaml` — add left/right columns with `GridSplitter`
- `Views/SceneOutlinerPanel.xaml` (new) — TreeView placeholder
- `Views/InspectorPanel.xaml` (new) — TabControl with Inspector + Settings tabs

**Key deliverables**:
- Resizable 3-column grid: outliner (220px default) | viewport (star) | inspector (280px default)
- Panel visibility toggle buttons in toolbar
- GridSplitters between columns

---

### Phase C — Scene Outliner

**Goal**: Display and manipulate the scene hierarchy.

**Files to create**:
- `ViewModels/SceneOutlinerViewModel.cs`
- `ViewModels/SceneNodeViewModel.cs` — wraps `SceneNode`, exposes children as `ObservableCollection`
- `Views/SceneOutlinerPanel.xaml` — `TreeView` with `HierarchicalDataTemplate`

**Key deliverables**:
- TreeView bound to `SceneGraph` root children
- Click-to-select syncs with `SelectionManager`
- Context menu: Add Body, Add Folder, Rename, Delete
- Live updates on `NodeAdded` / `NodeRemoved` / `NodeMoved` events

---

### Phase D — Body Inspector

**Goal**: View and edit selected body properties.

**Files to create**:
- `ViewModels/BodyInspectorViewModel.cs`
- `Views/InspectorPanel.xaml` — property grid layout with labels + text boxes

**Key deliverables**:
- Reacts to `SelectionManager.OnSelectionChanged`
- Displays: Tag, Mass, Position (X/Y/Z), Velocity (X/Y/Z), Radius, Density, BodyType
- Two-way editable when simulation is paused; read-only when running
- Converters: `Vec3dToStringConverter`

---

### Phase E — Simulation Settings

**Goal**: Expose `PhysicsConfig` for user tuning.

**Files to create**:
- `ViewModels/SimulationSettingsViewModel.cs`
- Add Settings tab content in `Views/InspectorPanel.xaml`

**Key deliverables**:
- Grouped sections: Core, Force Solver, Collisions, Relativistic
- Checkboxes for boolean flags, sliders for theta/timestep, numeric inputs for limits
- Conditional visibility (e.g., Theta slider only when `UseBarnesHut == true`)

---

### Phase F — Body Creation

**Goal**: Add new bodies via dialog or flyout.

**Files to create**:
- `ViewModels/AddBodyViewModel.cs`
- `Views/AddBodyDialog.xaml` — modal dialog

**Key deliverables**:
- Preset dropdown (Star, Planet, Gas Giant, Asteroid, Black Hole, Neutron Star, Binary System)
- Custom entry: mass, position, velocity, radius
- Confirm creates `Entity` + `PhysicsComponent`, adds to `SimulationManager` and `SceneGraph`

---

### Phase G — Save / Load

**Goal**: Persist and restore simulation projects.

**Files to modify**:
- `ViewModels/MainViewModel.cs` — add Save, SaveAs, Open, New commands
- `Views/MainWindow.xaml` — add File menu or toolbar buttons

**Key deliverables**:
- `SaveFileDialog` / `OpenFileDialog` with `.cesim` filter
- Pause simulation before save
- Load replaces current `SimulationManager` and `Scene`
- Display warnings from `ProjectLoadResult.Warnings`

---

### Phase H — Snapshot Timeline

**Goal**: Time-scrub through simulation history.

**Files to create**:
- `ViewModels/TimelineViewModel.cs`
- `Views/TimelinePanel.xaml` — horizontal slider + time labels

**Key deliverables**:
- Slider bound to `SnapshotManager.SnapshotCount`
- Dragging restores snapshot via `SnapshotManager.RestoreSnapshot`
- Labels show `SimulationTime` and `StepIndex`
- Configurable auto-capture interval

---

### Phase I — Viewport Selection

**Goal**: Click on bodies in the 3D viewport to select them.

**Files to modify**:
- `Views/ViewportPanel.xaml.cs` — add click handler for selection (distinct from orbit drag)
- `Services/SimulationService.cs` — add `HitTest(screenX, screenY, viewMatrix, projMatrix)` method

**Key deliverables**:
- Left-click (without drag) fires ray from camera through click point
- Ray-sphere intersection against all active bodies
- Nearest hit selected via `SelectionManager.Select`
- Visual highlight (glow ring or wireframe outline) on selected body in renderer

---

### Phase J — Advanced Features

**Goal**: Polish and extend with power-user features.

**Orbital Trails**
- Per-body position ring buffer (last N positions)
- Render as GL_LINE_STRIP in `GLRenderer`
- Toggle via toolbar checkbox

**Energy Graph**
- Dockable panel with rolling line chart
- Tracks total, kinetic, potential energy over simulation time
- Lightweight canvas-based renderer (avoid heavy charting library if possible)

**GPU Engine Mode Selector**
- Dropdown: CPU_BruteForce, CPU_BarnesHut, GPU_BruteForce, GPU_BarnesHut
- Integrator sub-selector: Leapfrog, Yoshida4
- Requires native engine DLL loaded

**Event Log Panel**
- Scrollable `ListBox` bound to `EventLogViewModel`
- Color-coded by event type
- Filter dropdown

**Keyboard Shortcuts**
- `InputBindings` in MainWindow: Space, R, S, Ctrl+S, Ctrl+O, Delete

---

## Appendix: Key API Types Reference

| Type | Namespace | Purpose |
|---|---|---|
| `ModeManager` | `AppCore.Modes` | Application mode switching |
| `SimulationMode` | `AppCore.Modes` | Active physics simulation mode |
| `SimulationManager` | `Simulation.Core` | ECS orchestrator, entity management |
| `Entity` | `Simulation.Core` | Component container with `Guid` ID |
| `PhysicsComponent` | `Simulation.Core` | Mass, position, velocity, radius |
| `PhysicsConfig` | `Physics.Types` | 30+ simulation parameters |
| `SceneGraph` | `AppCore.Scene` | Hierarchical node tree with events |
| `SceneNode` | `AppCore.Scene` | Tree node linked to entity |
| `SelectionManager` | `AppCore.Scene` | Multi-select state with events |
| `SnapshotManager` | `AppCore.Snapshot` | Time-recording and restore |
| `ProjectSerializer` | `AppCore.Serialization` | Save to `.cesim` |
| `ProjectDeserializer` | `AppCore.Serialization` | Load from `.cesim` |
| `EventBus` | `Simulation` | Pub/sub simulation events |
| `TimeManager` | `Simulation.Systems` | Simulation clock and time-scale |
| `BodyType` | `Physics.Types` | Star, Planet, BlackHole, etc. |
