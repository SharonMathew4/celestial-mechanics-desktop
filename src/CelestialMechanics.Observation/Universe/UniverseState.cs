namespace CelestialMechanics.Observation.Universe;

/// <summary>
/// Represents the lifecycle state of the universe management layer.
/// </summary>
public enum UniverseState
{
    /// <summary>The universe manager has not been initialized.</summary>
    Uninitialized,

    /// <summary>The universe manager is currently initializing.</summary>
    Initializing,

    /// <summary>The universe manager is active and operational.</summary>
    Active,

    /// <summary>The universe manager is paused.</summary>
    Paused,

    /// <summary>The universe manager is shutting down.</summary>
    ShuttingDown
}
