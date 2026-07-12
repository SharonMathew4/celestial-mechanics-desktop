using System;
using System.Collections.Generic;

namespace CelestialMechanics.Observation.Scene;

/// <summary>
/// Concrete base implementation of <see cref="ISceneNode"/> representing
/// a generic node in the hierarchical Scene Graph.
/// </summary>
public class SceneNode : ISceneNode
{
    private readonly List<ISceneNode> _children = new();
    private ISceneNode? _parent;

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string NodeType { get; set; } = "Generic";

    /// <inheritdoc />
    public Transform Transform { get; } = new();

    /// <inheritdoc />
    public ISceneNode? Parent
    {
        get => _parent;
        set => _parent = value;
    }

    /// <inheritdoc />
    public IReadOnlyList<ISceneNode> Children => _children;

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneNode"/> class.
    /// </summary>
    public SceneNode(string id, string name)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <inheritdoc />
    public virtual void AddChild(ISceneNode child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (child.Parent != null)
        {
            child.Parent.RemoveChild(child);
        }
        child.Parent = this;
        _children.Add(child);
    }

    /// <inheritdoc />
    public virtual bool RemoveChild(ISceneNode child)
    {
        if (child == null) return false;
        if (_children.Remove(child))
        {
            child.Parent = null;
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public virtual void Update(float deltaTime)
    {
        // Propagate updates down the tree, using reverse order in case modifications happen
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            if (i < _children.Count)
            {
                _children[i].Update(deltaTime);
            }
        }
    }
}
