using CelestialMechanics.Observation.Objects;
using CelestialMechanics.Observation.Universe;

namespace CelestialMechanics.Observation.Search;

/// <summary>
/// Provides search capabilities over the registered celestial body collection.
/// Supports find by name (case-insensitive substring), find by ID, and find by type.
/// No UI implementation — returns data models only.
/// </summary>
public sealed class SearchService
{
    private readonly UniverseManager _universeManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchService"/> class.
    /// </summary>
    /// <param name="universeManager">The universe manager containing all registered bodies.</param>
    public SearchService(UniverseManager universeManager)
    {
        _universeManager = universeManager ?? throw new ArgumentNullException(nameof(universeManager));
    }

    /// <summary>
    /// Finds all celestial bodies whose name contains the given search term.
    /// </summary>
    /// <param name="name">The search term (case-insensitive substring match).</param>
    /// <returns>A list of matching celestial bodies.</returns>
    public IReadOnlyList<CelestialBody> FindByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Array.Empty<CelestialBody>();

        var results = new List<CelestialBody>();
        foreach (var body in _universeManager.GetAll())
        {
            if (body.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(body);
            }

            // Also search catalog references
            if (results.Count == 0 || !results.Contains(body))
            {
                foreach (var kvp in body.CatalogReferences)
                {
                    if (kvp.Value.Contains(name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!results.Contains(body))
                        {
                            results.Add(body);
                        }
                        break;
                    }
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Finds a celestial body by its exact unique ID.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <returns>The matching celestial body, or null if not found.</returns>
    public CelestialBody? FindById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return _universeManager.GetById(id);
    }

    /// <summary>
    /// Finds all celestial bodies of a specific type.
    /// </summary>
    /// <param name="type">The celestial body type to filter by.</param>
    /// <returns>A list of matching celestial bodies.</returns>
    public IReadOnlyList<CelestialBody> FindByType(CelestialBodyType type)
    {
        return _universeManager.GetByType(type);
    }
}
