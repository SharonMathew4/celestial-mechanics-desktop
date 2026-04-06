using CelestialMechanics.AppCore.Scene;
using CelestialMechanics.Desktop.Services;

namespace CelestialMechanics.Desktop.Services;

/// <summary>
/// Owns the AppCore Scene (SceneGraph) and SelectionManager.
/// Maintains a bidirectional mapping between SceneNode.Id (Guid) and
/// PhysicsBody.Id (int) so ViewModels can work with scene-graph identity
/// while the engine uses integer body IDs.
/// </summary>
public sealed class SceneService : IDisposable
{
    private readonly Dictionary<Guid, int> _nodeToBodyId = new();
    private readonly Dictionary<int, Guid> _bodyIdToNodeId = new();

    public Scene Scene { get; }
    public SelectionManager SelectionManager { get; }

    public SceneService()
    {
        Scene = new Scene("Default");
        SelectionManager = new SelectionManager();
        SelectionManager.BindSceneGraph(Scene.Graph);
        Scene.Graph.NodeRemoved += OnNodeRemoved;
    }

    /// <summary>
    /// Walks engine.Bodies and creates a SceneNode for each active body.
    /// Call after adding bodies to the simulation.
    /// </summary>
    public void PopulateFromSimulation(SimulationService simService)
    {
        simService.WithEngineLock(engine =>
        {
            if (engine.Bodies == null) return;
            foreach (var body in engine.Bodies)
            {
                if (!body.IsActive) continue;
                string name = $"{body.Type} {body.Id}";
                var node = new SceneNode(name, NodeType.Entity);
                Scene.Graph.AddNode(Guid.Empty, node);
                _nodeToBodyId[node.Id] = body.Id;
                _bodyIdToNodeId[body.Id] = node.Id;
            }
        });
    }

    /// <summary>
    /// Clears the scene graph and rebuilds it from the current engine state.
    /// Call after SimulationService.ResetScene().
    /// </summary>
    public void RepopulateFromSimulation(SimulationService simService)
    {
        SelectionManager.Clear();

        // Remove all top-level nodes (and their subtrees)
        var roots = Scene.Graph.Root.Children.ToList();
        foreach (var child in roots)
            Scene.Graph.RemoveNode(child.Id);

        _nodeToBodyId.Clear();
        _bodyIdToNodeId.Clear();

        PopulateFromSimulation(simService);
    }

    /// <summary>Returns the PhysicsBody.Id for the given SceneNode, or null if unmapped.</summary>
    public int? GetBodyIdForNode(Guid nodeId)
        => _nodeToBodyId.TryGetValue(nodeId, out int id) ? id : null;

    /// <summary>Returns the SceneNode.Id for the given PhysicsBody.Id, or null if unmapped.</summary>
    public Guid? GetNodeIdForBody(int bodyId)
        => _bodyIdToNodeId.TryGetValue(bodyId, out Guid id) ? id : null;

    /// <summary>Registers a mapping between a SceneNode and a PhysicsBody.</summary>
    public void RegisterBodyNode(Guid nodeId, int bodyId)
    {
        _nodeToBodyId[nodeId] = bodyId;
        _bodyIdToNodeId[bodyId] = nodeId;
    }

    private void OnNodeRemoved(Guid nodeId)
    {
        if (_nodeToBodyId.TryGetValue(nodeId, out int bodyId))
        {
            _nodeToBodyId.Remove(nodeId);
            _bodyIdToNodeId.Remove(bodyId);
        }
    }

    public void Dispose()
    {
        Scene.Graph.NodeRemoved -= OnNodeRemoved;
        Scene.Graph.Dispose();
    }
}
