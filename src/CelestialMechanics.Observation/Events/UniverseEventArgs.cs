namespace CelestialMechanics.Observation.Events;

/// <summary>
/// Event arguments for universe-level events published through <see cref="EventBus"/>.
/// </summary>
public class UniverseEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of universe event.
    /// </summary>
    public UniverseEvent EventType { get; }

    /// <summary>
    /// Gets an optional payload carrying event-specific data.
    /// </summary>
    public object? Payload { get; }

    /// <summary>
    /// Gets the UTC timestamp when the event was raised.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UniverseEventArgs"/> class.
    /// </summary>
    /// <param name="eventType">The type of universe event.</param>
    /// <param name="payload">Optional event-specific data.</param>
    public UniverseEventArgs(UniverseEvent eventType, object? payload = null)
    {
        EventType = eventType;
        Payload = payload;
        Timestamp = DateTime.UtcNow;
    }
}
