using System.Collections.Generic;

namespace CelestialMechanics.Observation.Scene;

/// <summary>
/// Defines the contract for all nodes within the Observation Mode Scene Graph.
/// Enables hierarchical object management and parent-child updates.
/// </summary>
public interface ISceneNode
{
    /// <summary>
    /// Unique identifier for the scene node.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// User-friendly name of the scene node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The local transform of this node relative to its parent.
    /// </summary>
    Transform Transform { get; }

    /// <summary>
    /// Gets or sets the parent node of this node.
    /// </summary>
    ISceneNode? Parent { get; set; }

    /// <summary>
    /// Gets the read-only collection of child nodes.
    /// </summary>
    IReadOnlyList<ISceneNode> Children { get; }

    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    void AddChild(ISceneNode child);

    /// <summary>
    /// Removes a child node from this node.
    /// </summary>
    bool RemoveChild(ISceneNode child);

    /// <summary>
    /// Updates this node and propagates the update to its children.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame, in seconds.</param>
    void Update(float deltaTime);
}
