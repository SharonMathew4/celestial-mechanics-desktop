using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CelestialMechanics.AppCore.Scene;
using CelestialMechanics.Desktop.Models;
using CelestialMechanics.Desktop.Services;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Scene Outliner panel.
/// Maintains a flat list of bodies synced with the SceneGraph.
/// </summary>
public sealed partial class SceneOutlinerViewModel : ObservableObject, IDisposable
{
    private readonly SceneService _sceneService;
    private readonly SimulationService _simService;
    private readonly Dispatcher _dispatcher;
    private bool _suppressSelectionSync;

    public ObservableCollection<SceneNodeItem> Items { get; } = new();

    [ObservableProperty]
    private SceneNodeItem? _selectedItem;

    /// <summary>Raised when a body is selected in the outliner (passes nodeId).</summary>
    public event Action<Guid>? BodySelected;

    /// <summary>Raised when user requests deletion of a body (passes bodyId).</summary>
    public event Action<int>? DeleteRequested;

    public SceneOutlinerViewModel(SceneService sceneService, SimulationService simService, Dispatcher dispatcher)
    {
        _sceneService = sceneService;
        _simService = simService;
        _dispatcher = dispatcher;

        sceneService.Scene.Graph.NodeAdded += OnNodeAdded;
        sceneService.Scene.Graph.NodeRemoved += OnNodeRemoved;
        sceneService.SelectionManager.OnSelectionChanged += OnExternalSelectionChanged;
    }

    partial void OnSelectedItemChanged(SceneNodeItem? value)
    {
        if (_suppressSelectionSync || value == null) return;
        _sceneService.SelectionManager.Select(value.NodeId);
        BodySelected?.Invoke(value.NodeId);
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedItem == null) return;
        var bodyId = _sceneService.GetBodyIdForNode(SelectedItem.NodeId);
        if (bodyId.HasValue)
            DeleteRequested?.Invoke(bodyId.Value);
    }

    /// <summary>
    /// Fully rebuilds the Items list from the current SceneGraph state.
    /// </summary>
    public void Refresh()
    {
        Items.Clear();
        foreach (var node in _sceneService.Scene.Graph.Root.Children)
        {
            Items.Add(CreateItem(node));
        }
    }

    private SceneNodeItem CreateItem(SceneNode node)
    {
        // Parse body type from node name (format: "{BodyType} {Id}")
        var bodyType = ParseBodyTypeFromName(node.Name);
        return new SceneNodeItem
        {
            NodeId = node.Id,
            Name = node.Name,
            TypeLabel = bodyType.ToString(),
            IconGlyph = SceneNodeItem.GetIconForBodyType(bodyType)
        };
    }

    private static BodyType ParseBodyTypeFromName(string name)
    {
        var typePart = name.Split(' ')[0];
        return Enum.TryParse<BodyType>(typePart, out var bt) ? bt : BodyType.Star;
    }

    private void OnNodeAdded(SceneNode node)
    {
        if (node.NodeType != NodeType.Entity) return;
        _dispatcher.Invoke(() => Items.Add(CreateItem(node)));
    }

    private void OnNodeRemoved(Guid nodeId)
    {
        _dispatcher.Invoke(() =>
        {
            var item = Items.FirstOrDefault(i => i.NodeId == nodeId);
            if (item != null)
            {
                if (SelectedItem == item)
                    SelectedItem = null;
                Items.Remove(item);
            }
        });
    }

    private void OnExternalSelectionChanged(IReadOnlyList<Guid> selection)
    {
        _dispatcher.Invoke(() =>
        {
            if (selection.Count == 0)
            {
                _suppressSelectionSync = true;
                SelectedItem = null;
                _suppressSelectionSync = false;
                return;
            }

            var nodeId = selection[0];
            var item = Items.FirstOrDefault(i => i.NodeId == nodeId);
            if (item != null && item != SelectedItem)
            {
                _suppressSelectionSync = true;
                SelectedItem = item;
                _suppressSelectionSync = false;
            }
        });
    }

    /// <summary>
    /// Programmatically sets the outliner selection to match the given nodeId.
    /// Pass null to clear the selection. Suppresses circular selection sync.
    /// </summary>
    public void SetSelectedNodeId(Guid? nodeId)
    {
        _dispatcher.Invoke(() =>
        {
            _suppressSelectionSync = true;
            SelectedItem = nodeId.HasValue
                ? Items.FirstOrDefault(i => i.NodeId == nodeId.Value)
                : null;
            _suppressSelectionSync = false;
        });
    }

    public void Dispose()
    {
        _sceneService.Scene.Graph.NodeAdded -= OnNodeAdded;
        _sceneService.Scene.Graph.NodeRemoved -= OnNodeRemoved;
        _sceneService.SelectionManager.OnSelectionChanged -= OnExternalSelectionChanged;
    }
}
