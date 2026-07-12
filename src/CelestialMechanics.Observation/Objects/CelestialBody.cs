using CelestialMechanics.Math;

namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Abstract base class for all celestial bodies in the scientific object model.
/// Contains common physical, positional, and hierarchical properties shared
/// across all body types. Rendering logic is intentionally excluded.
/// </summary>
public abstract class CelestialBody
{
    private readonly List<CelestialBody> _children = new();
    private CelestialBody? _parent;

    /// <summary>
    /// Gets the globally unique identifier for this body.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets or sets the display name of this body.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the categorized type of this body.
    /// </summary>
    public CelestialBodyType ObjectType { get; }

    // ── Physical Properties ─────────────────────────────────────────

    /// <summary>
    /// Gets or sets the mass of the body in kilograms.
    /// </summary>
    public double Mass { get; set; }

    /// <summary>
    /// Gets or sets the mean radius of the body in meters.
    /// </summary>
    public double Radius { get; set; }

    /// <summary>
    /// Gets or sets the mean density of the body in kg/m³.
    /// </summary>
    public double Density { get; set; }

    /// <summary>
    /// Gets or sets the effective surface temperature in Kelvin.
    /// </summary>
    public double Temperature { get; set; }

    // ── Photometric Properties ──────────────────────────────────────

    /// <summary>
    /// Gets or sets the apparent magnitude.
    /// </summary>
    public double Magnitude { get; set; }

    /// <summary>
    /// Gets or sets the luminosity relative to the Sun (L☉).
    /// </summary>
    public double Luminosity { get; set; }

    /// <summary>
    /// Gets or sets the normalized RGB color representation.
    /// Components should be in the range [0, 1].
    /// </summary>
    public Vec3d Color { get; set; } = new(1.0, 1.0, 1.0);

    // ── Spatial Properties ──────────────────────────────────────────

    /// <summary>
    /// Gets or sets the position in the current coordinate frame (meters or AU depending on context).
    /// </summary>
    public Vec3d Position { get; set; } = Vec3d.Zero;

    /// <summary>
    /// Gets or sets the velocity vector in m/s.
    /// </summary>
    public Vec3d Velocity { get; set; } = Vec3d.Zero;

    /// <summary>
    /// Gets or sets the rotation orientation.
    /// </summary>
    public Quaterniond Rotation { get; set; } = Quaterniond.Identity;

    // ── Hierarchy ───────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the parent body (e.g. a planet's star).
    /// </summary>
    public CelestialBody? Parent
    {
        get => _parent;
        internal set => _parent = value;
    }

    /// <summary>
    /// Gets the read-only collection of child bodies.
    /// </summary>
    public IReadOnlyList<CelestialBody> Children => _children;

    // ── Catalog References ──────────────────────────────────────────

    /// <summary>
    /// Gets the dictionary of catalog identifiers mapped by catalog name.
    /// Example: { "HIP": "27989", "HD": "39801", "Bayer": "α Ori" }
    /// </summary>
    public Dictionary<string, string> CatalogReferences { get; } = new(StringComparer.OrdinalIgnoreCase);

    // ── Constructor ─────────────────────────────────────────────────

    /// <summary>
    /// Initializes a new instance of the <see cref="CelestialBody"/> class.
    /// </summary>
    /// <param name="id">Unique identifier. If null, a new GUID is generated.</param>
    /// <param name="name">Display name of the body.</param>
    /// <param name="objectType">The categorized type of the body.</param>
    protected CelestialBody(string? id, string name, CelestialBodyType objectType)
    {
        Id = id ?? Guid.NewGuid().ToString("N");
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ObjectType = objectType;
    }

    // ── Hierarchy Methods ───────────────────────────────────────────

    /// <summary>
    /// Adds a child body to this body's hierarchy.
    /// </summary>
    /// <param name="child">The child body to add.</param>
    public void AddChild(CelestialBody child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (ReferenceEquals(child, this))
            throw new InvalidOperationException("A celestial body cannot be its own child.");

        // Remove from previous parent
        child._parent?.RemoveChild(child);

        child._parent = this;
        _children.Add(child);
    }

    /// <summary>
    /// Removes a child body from this body's hierarchy.
    /// </summary>
    /// <param name="child">The child body to remove.</param>
    /// <returns>True if the child was found and removed; otherwise false.</returns>
    public bool RemoveChild(CelestialBody child)
    {
        if (child == null) return false;
        if (_children.Remove(child))
        {
            child._parent = null;
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => $"{ObjectType}: {Name} [{Id}]";
}
