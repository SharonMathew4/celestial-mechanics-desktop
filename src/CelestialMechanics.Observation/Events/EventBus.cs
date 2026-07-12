namespace CelestialMechanics.Observation.Events;

/// <summary>
/// Lightweight publish-subscribe event bus for loosely coupling universe subsystems.
/// Subscribers register for specific <see cref="UniverseEvent"/> types and receive
/// <see cref="UniverseEventArgs"/> when events are published.
/// </summary>
public sealed class EventBus
{
    private readonly Dictionary<UniverseEvent, List<Action<UniverseEventArgs>>> _handlers = new();
    private readonly object _lock = new();

    /// <summary>
    /// Subscribes a handler to a specific event type.
    /// </summary>
    /// <param name="eventType">The event type to subscribe to.</param>
    /// <param name="handler">The callback to invoke when the event is published.</param>
    public void Subscribe(UniverseEvent eventType, Action<UniverseEventArgs> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        lock (_lock)
        {
            if (!_handlers.TryGetValue(eventType, out var list))
            {
                list = new List<Action<UniverseEventArgs>>();
                _handlers[eventType] = list;
            }
            list.Add(handler);
        }
    }

    /// <summary>
    /// Unsubscribes a handler from a specific event type.
    /// </summary>
    /// <param name="eventType">The event type to unsubscribe from.</param>
    /// <param name="handler">The callback to remove.</param>
    public void Unsubscribe(UniverseEvent eventType, Action<UniverseEventArgs> handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        lock (_lock)
        {
            if (_handlers.TryGetValue(eventType, out var list))
            {
                list.Remove(handler);
            }
        }
    }

    /// <summary>
    /// Publishes an event to all subscribers of the specified event type.
    /// Handlers are invoked synchronously in registration order.
    /// </summary>
    /// <param name="args">The event arguments to deliver.</param>
    public void Publish(UniverseEventArgs args)
    {
        if (args == null) throw new ArgumentNullException(nameof(args));

        List<Action<UniverseEventArgs>>? snapshot;
        lock (_lock)
        {
            if (!_handlers.TryGetValue(args.EventType, out var list) || list.Count == 0)
                return;

            // Snapshot to avoid issues if handlers modify subscriptions
            snapshot = new List<Action<UniverseEventArgs>>(list);
        }

        foreach (var handler in snapshot)
        {
            handler(args);
        }
    }

    /// <summary>
    /// Removes all subscriptions for all event types.
    /// </summary>
    public void ClearAll()
    {
        lock (_lock)
        {
            _handlers.Clear();
        }
    }

    /// <summary>
    /// Gets the number of handlers registered for a specific event type.
    /// </summary>
    public int GetSubscriberCount(UniverseEvent eventType)
    {
        lock (_lock)
        {
            return _handlers.TryGetValue(eventType, out var list) ? list.Count : 0;
        }
    }
}
