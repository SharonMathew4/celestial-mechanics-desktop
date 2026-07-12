namespace CelestialMechanics.Observation.Services;

/// <summary>
/// Abstraction for managing visibility layers in the observation viewport.
/// Layers control which categories of objects and overlays are rendered
/// (e.g., constellations, grid lines, star classes, nebulae).
/// </summary>
public interface ILayerService
{
    /// <summary>
    /// Gets all available layer names.
    /// </summary>
    IReadOnlyList<string> AvailableLayers { get; }

    /// <summary>
    /// Checks whether a specific layer is currently visible.
    /// </summary>
    bool IsLayerVisible(string layerName);

    /// <summary>
    /// Sets the visibility of a specific layer.
    /// </summary>
    void SetLayerVisibility(string layerName, bool visible);

    /// <summary>
    /// Toggles the visibility of a specific layer.
    /// </summary>
    void ToggleLayer(string layerName);

    /// <summary>
    /// Raised when any layer's visibility changes.
    /// </summary>
    event Action<string, bool>? LayerVisibilityChanged;
}
