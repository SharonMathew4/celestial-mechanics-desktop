using CelestialMechanics.Observation.Events;
using CelestialMechanics.Observation.Objects;
using CelestialMechanics.Observation.Scene;
using CelestialMechanics.Observation.Universe;

namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Factory for creating <see cref="CelestialBody"/> instances with matching
/// <see cref="SceneNode"/> objects. Automatically registers created bodies
/// with the <see cref="UniverseManager"/> and adds corresponding SceneNodes
/// to the <see cref="SceneManager"/>.
/// </summary>
public sealed class CelestialBodyFactory
{
    private readonly UniverseManager _universeManager;
    private readonly SceneManager _sceneManager;
    private readonly EventBus _eventBus;

    /// <summary>
    /// Initializes a new instance of the <see cref="CelestialBodyFactory"/> class.
    /// </summary>
    public CelestialBodyFactory(
        UniverseManager universeManager,
        SceneManager sceneManager,
        EventBus eventBus)
    {
        _universeManager = universeManager ?? throw new ArgumentNullException(nameof(universeManager));
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    /// <summary>
    /// Creates a Star, registers it, and creates a matching SceneNode.
    /// </summary>
    public Star CreateStar(string name, string? id = null)
    {
        var body = new Star(id, name);
        RegisterAndCreateNode(body);
        return body;
    }

    /// <summary>
    /// Creates a Planet, registers it, and creates a matching SceneNode.
    /// </summary>
    public Planet CreatePlanet(string name, string? id = null)
    {
        var body = new Planet(id, name);
        RegisterAndCreateNode(body);
        return body;
    }

    /// <summary>
    /// Creates a Moon, registers it, and creates a matching SceneNode.
    /// </summary>
    public Moon CreateMoon(string name, string? id = null)
    {
        var body = new Moon(id, name);
        RegisterAndCreateNode(body);
        return body;
    }

    /// <summary>
    /// Creates an Asteroid, registers it, and creates a matching SceneNode.
    /// </summary>
    public Asteroid CreateAsteroid(string name, string? id = null)
    {
        var body = new Asteroid(id, name);
        RegisterAndCreateNode(body);
        return body;
    }

    /// <summary>
    /// Creates a Comet, registers it, and creates a matching SceneNode.
    /// </summary>
    public Comet CreateComet(string name, string? id = null)
    {
        var body = new Comet(id, name);
        RegisterAndCreateNode(body);
        return body;
    }

    /// <summary>
    /// Creates a Galaxy, registers it, and creates a matching SceneNode.
    /// </summary>
    public Galaxy CreateGalaxy(string name, string? id = null)
    {
        var body = new Galaxy(id, name);
        RegisterAndCreateNode(body);
        return body;
    }

    /// <summary>
    /// Creates a Nebula, registers it, and creates a matching SceneNode.
    /// </summary>
    public Nebula CreateNebula(string name, string? id = null)
    {
        var body = new Nebula(id, name);
        RegisterAndCreateNode(body);
        return body;
    }

    /// <summary>
    /// Creates a BlackHole, registers it, and creates a matching SceneNode.
    /// </summary>
    public BlackHole CreateBlackHole(string name, string? id = null)
    {
        var body = new BlackHole(id, name);
        RegisterAndCreateNode(body);
        return body;
    }

    /// <summary>
    /// Creates a Spacecraft, registers it, and creates a matching SceneNode.
    /// </summary>
    public Spacecraft CreateSpacecraft(string name, string? id = null)
    {
        var body = new Spacecraft(id, name);
        RegisterAndCreateNode(body);
        return body;
    }

    /// <summary>
    /// Creates a SceneNode for an existing body and adds it to the scene graph.
    /// </summary>
    /// <param name="body">The celestial body to create a scene node for.</param>
    /// <returns>The created scene node.</returns>
    public SceneNode CreateSceneNodeForBody(CelestialBody body)
    {
        if (body == null) throw new ArgumentNullException(nameof(body));

        var node = new SceneNode(body.Id, body.Name)
        {
            NodeType = body.ObjectType.ToString()
        };
        node.Transform.Position = body.Position;
        node.Transform.Rotation = body.Rotation;

        // Try to place under parent's scene node
        if (body.Parent != null)
        {
            var parentNode = _sceneManager.FindNode(body.Parent.Id);
            if (parentNode != null)
            {
                parentNode.AddChild(node);
                return node;
            }
        }

        _sceneManager.Root.AddChild(node);
        return node;
    }

    private void RegisterAndCreateNode(CelestialBody body)
    {
        _universeManager.Register(body);
        CreateSceneNodeForBody(body);
    }
}
