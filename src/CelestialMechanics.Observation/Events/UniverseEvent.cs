namespace CelestialMechanics.Observation.Events;

/// <summary>
/// Defines the types of events raised within the universe management layer.
/// Used by <see cref="EventBus"/> for publish-subscribe routing.
/// </summary>
public enum UniverseEvent
{
    /// <summary>
    /// A new celestial body was created and registered.
    /// </summary>
    ObjectCreated,

    /// <summary>
    /// A celestial body was removed from the universe.
    /// </summary>
    ObjectRemoved,

    /// <summary>
    /// The currently selected celestial body changed.
    /// </summary>
    SelectionChanged,

    /// <summary>
    /// The simulation time was advanced or changed.
    /// </summary>
    TimeChanged,

    /// <summary>
    /// The universe state was updated (e.g. hierarchy change, sync).
    /// </summary>
    UniverseUpdated
}
