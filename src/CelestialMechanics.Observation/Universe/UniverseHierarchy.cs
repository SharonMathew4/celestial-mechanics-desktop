using CelestialMechanics.Observation.Objects;

namespace CelestialMechanics.Observation.Universe;

/// <summary>
/// Manages parent-child hierarchy relationships between <see cref="CelestialBody"/> instances.
/// Provides efficient lookup for children, ancestors, and descendants.
/// </summary>
public sealed class UniverseHierarchy
{
    private readonly Dictionary<string, HashSet<string>> _childrenMap = new();
    private readonly Dictionary<string, string?> _parentMap = new();

    /// <summary>
    /// Registers a body in the hierarchy (with no parent initially).
    /// </summary>
    /// <param name="bodyId">The unique ID of the body.</param>
    public void Register(string bodyId)
    {
        if (bodyId == null) throw new ArgumentNullException(nameof(bodyId));
        if (!_parentMap.ContainsKey(bodyId))
        {
            _parentMap[bodyId] = null;
        }
        if (!_childrenMap.ContainsKey(bodyId))
        {
            _childrenMap[bodyId] = new HashSet<string>();
        }
    }

    /// <summary>
    /// Removes a body from the hierarchy entirely.
    /// </summary>
    /// <param name="bodyId">The unique ID of the body to remove.</param>
    public void Remove(string bodyId)
    {
        if (bodyId == null) return;

        // Remove from parent's children set
        if (_parentMap.TryGetValue(bodyId, out var parentId) && parentId != null)
        {
            if (_childrenMap.TryGetValue(parentId, out var siblings))
            {
                siblings.Remove(bodyId);
            }
        }

        // Orphan all children
        if (_childrenMap.TryGetValue(bodyId, out var children))
        {
            foreach (var childId in children)
            {
                if (_parentMap.ContainsKey(childId))
                {
                    _parentMap[childId] = null;
                }
            }
        }

        _parentMap.Remove(bodyId);
        _childrenMap.Remove(bodyId);
    }

    /// <summary>
    /// Sets the parent of a body. Pass null to make the body a root.
    /// </summary>
    /// <param name="childId">The child body ID.</param>
    /// <param name="parentId">The parent body ID, or null for root.</param>
    public void SetParent(string childId, string? parentId)
    {
        if (childId == null) throw new ArgumentNullException(nameof(childId));

        // Remove from old parent
        if (_parentMap.TryGetValue(childId, out var oldParentId) && oldParentId != null)
        {
            if (_childrenMap.TryGetValue(oldParentId, out var oldSiblings))
            {
                oldSiblings.Remove(childId);
            }
        }

        // Set new parent
        _parentMap[childId] = parentId;

        // Add to new parent's children set
        if (parentId != null)
        {
            if (!_childrenMap.TryGetValue(parentId, out var newSiblings))
            {
                newSiblings = new HashSet<string>();
                _childrenMap[parentId] = newSiblings;
            }
            newSiblings.Add(childId);
        }
    }

    /// <summary>
    /// Gets the parent ID of a body, or null if it has no parent.
    /// </summary>
    public string? GetParent(string bodyId)
    {
        return _parentMap.TryGetValue(bodyId, out var parentId) ? parentId : null;
    }

    /// <summary>
    /// Gets the IDs of all direct children of a body.
    /// </summary>
    public IReadOnlyCollection<string> GetChildren(string bodyId)
    {
        return _childrenMap.TryGetValue(bodyId, out var children)
            ? children
            : Array.Empty<string>();
    }

    /// <summary>
    /// Gets all root body IDs (bodies with no parent).
    /// </summary>
    public IReadOnlyList<string> GetRoots()
    {
        var roots = new List<string>();
        foreach (var kvp in _parentMap)
        {
            if (kvp.Value == null)
            {
                roots.Add(kvp.Key);
            }
        }
        return roots;
    }

    /// <summary>
    /// Gets all ancestor IDs of a body, from immediate parent to root.
    /// </summary>
    public IReadOnlyList<string> GetAncestors(string bodyId)
    {
        var ancestors = new List<string>();
        var current = GetParent(bodyId);
        var visited = new HashSet<string>();

        while (current != null && visited.Add(current))
        {
            ancestors.Add(current);
            current = GetParent(current);
        }

        return ancestors;
    }

    /// <summary>
    /// Gets all descendant IDs of a body (recursive).
    /// </summary>
    public IReadOnlyList<string> GetDescendants(string bodyId)
    {
        var descendants = new List<string>();
        CollectDescendants(bodyId, descendants);
        return descendants;
    }

    private void CollectDescendants(string bodyId, List<string> result)
    {
        if (!_childrenMap.TryGetValue(bodyId, out var children))
            return;

        foreach (var childId in children)
        {
            result.Add(childId);
            CollectDescendants(childId, result);
        }
    }

    /// <summary>
    /// Removes all entries from the hierarchy.
    /// </summary>
    public void Clear()
    {
        _parentMap.Clear();
        _childrenMap.Clear();
    }
}
