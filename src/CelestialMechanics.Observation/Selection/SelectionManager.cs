using CelestialMechanics.Observation.Events;
using CelestialMechanics.Observation.Objects;
using CelestialMechanics.Observation.Services;
using CelestialMechanics.Observation.Universe;

namespace CelestialMechanics.Observation.Selection;

/// <summary>
/// Manages the currently selected <see cref="CelestialBody"/> and provides
/// selection change notifications. Implements <see cref="ISelectionService"/>
/// for compatibility with the existing service abstraction layer.
/// </summary>
public sealed class SelectionManager : ISelectionService
{
    private readonly UniverseManager _universeManager;
    private readonly EventBus _eventBus;
    private CelestialBody? _selectedBody;

    /// <summary>
    /// Gets the currently selected celestial body, or null if nothing is selected.
    /// </summary>
    public CelestialBody? SelectedBody => _selectedBody;

    /// <inheritdoc />
    public string? SelectedObjectId => _selectedBody?.Id;

    /// <inheritdoc />
    public bool HasSelection => _selectedBody != null;

    /// <summary>
    /// Raised when the selected celestial body changes.
    /// Parameter is the newly selected body (or null if cleared).
    /// </summary>
    public event Action<CelestialBody?>? BodySelectionChanged;

    /// <inheritdoc />
    public event Action<string?>? SelectionChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionManager"/> class.
    /// </summary>
    /// <param name="universeManager">The universe manager for ID-based lookups.</param>
    /// <param name="eventBus">The event bus for publishing selection events.</param>
    public SelectionManager(UniverseManager universeManager, EventBus eventBus)
    {
        _universeManager = universeManager ?? throw new ArgumentNullException(nameof(universeManager));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    /// <summary>
    /// Selects the specified celestial body.
    /// </summary>
    /// <param name="body">The body to select.</param>
    public void Select(CelestialBody body)
    {
        if (body == null) throw new ArgumentNullException(nameof(body));
        if (ReferenceEquals(_selectedBody, body))
            return;

        _selectedBody = body;
        RaiseSelectionChanged();
    }

    /// <summary>
    /// Selects a celestial body by its unique ID.
    /// </summary>
    /// <param name="objectId">The unique ID of the body to select.</param>
    public void SelectById(string objectId)
    {
        if (objectId == null) throw new ArgumentNullException(nameof(objectId));

        var body = _universeManager.GetById(objectId);
        if (body != null)
        {
            Select(body);
        }
    }

    /// <inheritdoc />
    void ISelectionService.Select(string objectId)
    {
        SelectById(objectId);
    }

    /// <inheritdoc />
    public void ClearSelection()
    {
        if (_selectedBody == null)
            return;

        _selectedBody = null;
        RaiseSelectionChanged();
    }

    private void RaiseSelectionChanged()
    {
        BodySelectionChanged?.Invoke(_selectedBody);
        SelectionChanged?.Invoke(_selectedBody?.Id);
        _eventBus.Publish(new UniverseEventArgs(UniverseEvent.SelectionChanged, _selectedBody));
    }
}
