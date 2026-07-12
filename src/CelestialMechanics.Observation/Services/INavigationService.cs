namespace CelestialMechanics.Observation.Services;

/// <summary>
/// Abstraction for navigating to specific astronomical objects
/// or coordinates within the observation scene.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Whether a navigation animation is currently in progress.
    /// </summary>
    bool IsNavigating { get; }

    /// <summary>
    /// Navigates the camera to focus on the specified object.
    /// </summary>
    /// <param name="objectId">Catalog identifier of the target object.</param>
    Task NavigateToAsync(string objectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Navigates to a specific sky coordinate (Right Ascension / Declination).
    /// </summary>
    /// <param name="rightAscensionDeg">Right ascension in degrees.</param>
    /// <param name="declinationDeg">Declination in degrees.</param>
    Task NavigateToCoordinateAsync(double rightAscensionDeg, double declinationDeg,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels any in-progress navigation.
    /// </summary>
    void CancelNavigation();
}
