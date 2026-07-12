namespace CelestialMechanics.Observation.Core;

/// <summary>
/// Module registration entry point for Observation Mode.
/// Provides a central point for future dependency injection
/// registration when the mode is loaded.
/// </summary>
public static class ObservationModule
{
    /// <summary>
    /// Module name used for logging and diagnostics.
    /// </summary>
    public const string ModuleName = "CelestialMechanics.Observation";

    /// <summary>
    /// Module version string.
    /// </summary>
    public const string ModuleVersion = "0.1.0-alpha";

    /// <summary>
    /// Returns true if the Observation Mode module is available.
    /// Always returns true once the assembly is loaded.
    /// </summary>
    public static bool IsAvailable => true;

    /// <summary>
    /// Creates a new bootstrap instance for launching Observation Mode.
    /// </summary>
    public static ObservationBootstrap CreateBootstrap()
    {
        return new ObservationBootstrap();
    }
}
