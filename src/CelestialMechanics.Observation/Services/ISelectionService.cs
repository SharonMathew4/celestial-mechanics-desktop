namespace CelestialMechanics.Observation.Services;

/// <summary>
/// Abstraction for object selection within the observation viewport.
/// Manages the currently selected astronomical object and provides
/// selection/deselection notifications.
/// </summary>
public interface ISelectionService
{
    /// <summary>
    /// Identifier of the currently selected object, or null if nothing is selected.
    /// </summary>
    string? SelectedObjectId { get; }

    /// <summary>
    /// Whether an object is currently selected.
    /// </summary>
    bool HasSelection { get; }

    /// <summary>
    /// Selects the specified object.
    /// </summary>
    /// <param name="objectId">Catalog identifier of the object to select.</param>
    void Select(string objectId);

    /// <summary>
    /// Clears the current selection.
    /// </summary>
    void ClearSelection();

    /// <summary>
    /// Raised when the selection changes. Parameter is the new selection ID (or null).
    /// </summary>
    event Action<string?>? SelectionChanged;
}
