namespace CelestialMechanics.Observation.Core;

/// <summary>
/// Configuration settings for an Observation Mode session.
/// Holds default values that can be overridden by user preferences
/// or saved workspace state in future phases.
/// </summary>
public sealed class ObservationConfiguration
{
    /// <summary>
    /// Default field of view in degrees for the observation camera.
    /// </summary>
    public float DefaultFov { get; set; } = 60.0f;

    /// <summary>
    /// Near clipping plane distance.
    /// </summary>
    public float NearPlane { get; set; } = 0.01f;

    /// <summary>
    /// Far clipping plane distance.
    /// </summary>
    public float FarPlane { get; set; } = 1_000_000.0f;

    /// <summary>
    /// Mouse look sensitivity multiplier.
    /// </summary>
    public float MouseSensitivity { get; set; } = 0.25f;

    /// <summary>
    /// Zoom sensitivity multiplier for scroll wheel.
    /// </summary>
    public float ZoomSensitivity { get; set; } = 0.1f;

    /// <summary>
    /// Whether to render the coordinate axes helper.
    /// </summary>
    public bool ShowCoordinateAxes { get; set; } = true;

    /// <summary>
    /// Background clear color (RGBA, 0–1 range).
    /// Default: pure black.
    /// </summary>
    public float[] ClearColor { get; set; } = [0.0f, 0.0f, 0.0f, 1.0f];

    /// <summary>
    /// Window title suffix for the Observation window.
    /// </summary>
    public string WindowTitle { get; set; } = "Celestial Mechanics — Observation Mode";
}
