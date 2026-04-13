namespace CelestialMechanics.Desktop.Services;

/// <summary>
/// Keeps lightweight mappings between simulation body IDs and scene node IDs.
/// </summary>
public sealed class SceneService : IDisposable
{
    private readonly Dictionary<Guid, int> _nodeToBody = new();
    private readonly Dictionary<int, Guid> _bodyToNode = new();

    public SelectionState SelectionManager { get; } = new();

    public event Action? SceneChanged;
    public event Action<Guid?>? SelectionChanged;

    public SceneService()
    {
        SelectionManager.SelectionChanged += id => SelectionChanged?.Invoke(id);
    }

    public void RepopulateFromSimulation(SimulationService simService)
    {
        _nodeToBody.Clear();
        _bodyToNode.Clear();

        simService.WithEngineLock(engine =>
        {
            if (engine.Bodies == null)
            {
                return;
            }

            foreach (var body in engine.Bodies)
            {
                var nodeId = BuildNodeId(body.Id);
                _nodeToBody[nodeId] = body.Id;
                _bodyToNode[body.Id] = nodeId;
            }
        });

        SceneChanged?.Invoke();
    }

    public int? GetBodyIdForNode(Guid nodeId)
    {
        return _nodeToBody.TryGetValue(nodeId, out var id) ? id : null;
    }

    public Guid? GetNodeIdForBody(int bodyId)
    {
        return _bodyToNode.TryGetValue(bodyId, out var nodeId) ? nodeId : null;
    }

    public void Dispose()
    {
        SelectionManager.Clear();
        _nodeToBody.Clear();
        _bodyToNode.Clear();
    }

    private static Guid BuildNodeId(int bodyId)
    {
        Span<byte> bytes = stackalloc byte[16];
        BitConverter.TryWriteBytes(bytes, bodyId);
        bytes[15] = 1;
        return new Guid(bytes);
    }
}

public sealed class SelectionState
{
    private Guid? _selectedEntity;

    public Guid? SelectedEntity => _selectedEntity;

    public event Action<Guid?>? SelectionChanged;

    public void Select(Guid nodeId)
    {
        _selectedEntity = nodeId;
        SelectionChanged?.Invoke(_selectedEntity);
    }

    public void Clear()
    {
        _selectedEntity = null;
        SelectionChanged?.Invoke(_selectedEntity);
    }
}
