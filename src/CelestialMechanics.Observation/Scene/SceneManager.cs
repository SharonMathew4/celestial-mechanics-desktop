using System;
using System.Collections.Generic;

namespace CelestialMechanics.Observation.Scene;

/// <summary>
/// Manages the tree structure of the Scene Graph, handling updates, traversals,
/// node lookup, and tracking selection changes.
/// </summary>
public sealed class SceneManager
{
    /// <summary>
    /// Gets the root node of the scene graph.
    /// </summary>
    public ISceneNode Root { get; } = new SceneNode("root", "Root Node");

    private ISceneNode? _selectedNode;

    /// <summary>
    /// Gets or sets the currently selected node.
    /// </summary>
    public ISceneNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (!ReferenceEquals(_selectedNode, value))
            {
                _selectedNode = value;
                SelectionChanged?.Invoke(value);
            }
        }
    }

    /// <summary>
    /// Raised when the selected node changes.
    /// </summary>
    public event Action<ISceneNode?>? SelectionChanged;

    /// <summary>
    /// Performs the update traversal of the entire scene graph.
    /// </summary>
    public void Update(float deltaTime)
    {
        Root.Update(deltaTime);
    }

    /// <summary>
    /// Searches for a node by its unique identifier.
    /// </summary>
    public ISceneNode? FindNode(string id)
    {
        if (id == null) return null;
        return FindNodeRecursive(Root, id);
    }

    private static ISceneNode? FindNodeRecursive(ISceneNode current, string id)
    {
        if (current.Id == id)
        {
            return current;
        }

        foreach (var child in current.Children)
        {
            var found = FindNodeRecursive(child, id);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
