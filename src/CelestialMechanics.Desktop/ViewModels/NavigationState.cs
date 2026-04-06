namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// Navigation state machine for the startup/project flow.
/// Controls which modal overlay is displayed before entering the simulation IDE.
/// </summary>
public enum NavigationState
{
    /// <summary>Initial screen — choose Simulation or Observation mode.</summary>
    ModeSelection,

    /// <summary>Simulation hub — New Project, File, Projects, Settings.</summary>
    SimulationMenu,

    /// <summary>Create a new project — name + location form.</summary>
    NewProject,

    /// <summary>Browse and open saved projects.</summary>
    ProjectsList,

    /// <summary>File operations — Save, Save As, Import, Share, Preferences.</summary>
    FileMenu,

    /// <summary>Full-screen simulation IDE is active.</summary>
    SimulationIDE
}
