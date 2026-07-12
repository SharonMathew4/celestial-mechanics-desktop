using CelestialMechanics.Observation.Events;
using CelestialMechanics.Observation.Objects;
using CelestialMechanics.Observation.Scene;

namespace CelestialMechanics.Observation.Universe;

/// <summary>
/// Central registry and management layer for all <see cref="CelestialBody"/> instances
/// in the observation universe. Maintains the hierarchy, provides lookup operations,
/// and synchronizes with the <see cref="SceneManager"/> scene graph.
/// </summary>
public sealed class UniverseManager
{
    private readonly Dictionary<string, CelestialBody> _bodies = new();
    private readonly UniverseHierarchy _hierarchy;
    private readonly EventBus _eventBus;

    /// <summary>
    /// Gets the universe hierarchy manager.
    /// </summary>
    public UniverseHierarchy Hierarchy => _hierarchy;

    /// <summary>
    /// Gets the current state of the universe manager.
    /// </summary>
    public UniverseState State { get; private set; } = UniverseState.Uninitialized;

    /// <summary>
    /// Gets the total number of registered celestial bodies.
    /// </summary>
    public int Count => _bodies.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="UniverseManager"/> class.
    /// </summary>
    /// <param name="hierarchy">The hierarchy manager for parent-child relationships.</param>
    /// <param name="eventBus">The event bus for publishing universe events.</param>
    public UniverseManager(UniverseHierarchy hierarchy, EventBus eventBus)
    {
        _hierarchy = hierarchy ?? throw new ArgumentNullException(nameof(hierarchy));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    /// <summary>
    /// Initializes the universe manager and transitions to the Active state.
    /// </summary>
    public void Initialize()
    {
        if (State != UniverseState.Uninitialized)
            return;

        State = UniverseState.Initializing;
        State = UniverseState.Active;
    }

    /// <summary>
    /// Registers a celestial body in the universe.
    /// </summary>
    /// <param name="body">The celestial body to register.</param>
    /// <exception cref="InvalidOperationException">Thrown if a body with the same ID already exists.</exception>
    public void Register(CelestialBody body)
    {
        if (body == null) throw new ArgumentNullException(nameof(body));
        if (_bodies.ContainsKey(body.Id))
            throw new InvalidOperationException($"A celestial body with ID '{body.Id}' is already registered.");

        _bodies[body.Id] = body;
        _hierarchy.Register(body.Id);

        // Sync parent relationship if set
        if (body.Parent != null)
        {
            _hierarchy.SetParent(body.Id, body.Parent.Id);
        }

        _eventBus.Publish(new UniverseEventArgs(UniverseEvent.ObjectCreated, body));
    }

    /// <summary>
    /// Removes a celestial body from the universe.
    /// </summary>
    /// <param name="bodyId">The unique ID of the body to remove.</param>
    /// <returns>True if the body was found and removed; otherwise false.</returns>
    public bool Remove(string bodyId)
    {
        if (bodyId == null) return false;
        if (!_bodies.TryGetValue(bodyId, out var body))
            return false;

        // Remove from parent's children collection
        body.Parent?.RemoveChild(body);

        _hierarchy.Remove(bodyId);
        _bodies.Remove(bodyId);

        _eventBus.Publish(new UniverseEventArgs(UniverseEvent.ObjectRemoved, body));
        return true;
    }

    /// <summary>
    /// Gets a celestial body by its unique ID.
    /// </summary>
    /// <param name="id">The unique ID.</param>
    /// <returns>The celestial body, or null if not found.</returns>
    public CelestialBody? GetById(string id)
    {
        if (id == null) return null;
        return _bodies.TryGetValue(id, out var body) ? body : null;
    }

    /// <summary>
    /// Gets all registered celestial bodies.
    /// </summary>
    public IReadOnlyCollection<CelestialBody> GetAll()
    {
        return _bodies.Values;
    }

    /// <summary>
    /// Gets all celestial bodies of a specific type.
    /// </summary>
    /// <param name="type">The celestial body type to filter by.</param>
    /// <returns>A list of matching bodies.</returns>
    public IReadOnlyList<CelestialBody> GetByType(CelestialBodyType type)
    {
        var result = new List<CelestialBody>();
        foreach (var body in _bodies.Values)
        {
            if (body.ObjectType == type)
            {
                result.Add(body);
            }
        }
        return result;
    }

    /// <summary>
    /// Synchronizes the universe model to the scene graph by creating
    /// SceneNodes for bodies that don't have corresponding nodes and
    /// removing orphaned nodes.
    /// </summary>
    /// <param name="sceneManager">The scene manager to synchronize with.</param>
    public void SyncToSceneGraph(SceneManager sceneManager)
    {
        if (sceneManager == null) throw new ArgumentNullException(nameof(sceneManager));

        // Build set of existing scene node IDs
        var existingNodeIds = new HashSet<string>();
        CollectNodeIds(sceneManager.Root, existingNodeIds);

        // Add SceneNodes for bodies that don't have one
        foreach (var body in _bodies.Values)
        {
            if (!existingNodeIds.Contains(body.Id))
            {
                var node = new SceneNode(body.Id, body.Name)
                {
                    NodeType = body.ObjectType.ToString()
                };
                node.Transform.Position = body.Position;
                node.Transform.Rotation = body.Rotation;

                // Try to parent under the body's parent scene node
                if (body.Parent != null)
                {
                    var parentNode = sceneManager.FindNode(body.Parent.Id);
                    if (parentNode != null)
                    {
                        parentNode.AddChild(node);
                    }
                    else
                    {
                        sceneManager.Root.AddChild(node);
                    }
                }
                else
                {
                    sceneManager.Root.AddChild(node);
                }
            }
            else
            {
                // Update existing node's transform from body
                var existingNode = sceneManager.FindNode(body.Id);
                if (existingNode != null)
                {
                    existingNode.Transform.Position = body.Position;
                    existingNode.Transform.Rotation = body.Rotation;
                }
            }
        }

        _eventBus.Publish(new UniverseEventArgs(UniverseEvent.UniverseUpdated));
    }

    /// <summary>
    /// Shuts down the universe manager and clears all bodies.
    /// </summary>
    public void Shutdown()
    {
        State = UniverseState.ShuttingDown;
        _bodies.Clear();
        _hierarchy.Clear();
        State = UniverseState.Uninitialized;
    }

    private static void CollectNodeIds(ISceneNode node, HashSet<string> ids)
    {
        ids.Add(node.Id);
        foreach (var child in node.Children)
        {
            CollectNodeIds(child, ids);
        }
    }
}
