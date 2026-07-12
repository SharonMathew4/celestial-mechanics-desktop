namespace CelestialMechanics.Observation.Scene;

/// <summary>
/// Placeholder scene graph for Observation Mode.
/// In future phases, this will manage an octree-based spatial
/// partitioning structure for astronomical-scale object management.
/// </summary>
public sealed class ObservationScene
{
    /// <summary>
    /// Total number of objects currently in the scene.
    /// </summary>
    public int ObjectCount { get; private set; }

    /// <summary>
    /// Whether the scene has been initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Initializes the scene graph.
    /// </summary>
    public void Initialize()
    {
        ObjectCount = 0;
        IsInitialized = true;
    }

    /// <summary>
    /// Clears all objects from the scene.
    /// </summary>
    public void Clear()
    {
        ObjectCount = 0;
    }

    /// <summary>
    /// Shuts down the scene graph.
    /// </summary>
    public void Shutdown()
    {
        Clear();
        IsInitialized = false;
    }
}
