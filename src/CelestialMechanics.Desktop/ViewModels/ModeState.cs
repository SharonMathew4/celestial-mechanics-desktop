namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// UI interaction mode for Simulation Mode.
/// Determines what user input actions do in the viewport.
/// </summary>
public enum UiMode
{
    /// <summary>Default state — no special interaction mode active.</summary>
    Idle,

    /// <summary>User is placing a new celestial body (Phase 4).</summary>
    AddPlacement,

    /// <summary>User is editing existing body properties (Phase 4).</summary>
    Edit,

    /// <summary>User is in analysis/observation mode (Phase 4).</summary>
    Analyse,

    /// <summary>User is running/controlling the simulation.</summary>
    Simulate
}

/// <summary>
/// Simulation lifecycle state displayed in the status bar.
/// Maps from the engine's internal EngineState.
/// Renamed from SimulationState to avoid collision with Physics.Types.SimulationState.
/// </summary>
public enum SimLifecycleState
{
    Idle,
    Running,
    Paused
}
