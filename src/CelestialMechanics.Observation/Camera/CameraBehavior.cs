namespace CelestialMechanics.Observation.Camera;

/// <summary>
/// Defines the available camera behavior modes.
/// </summary>
public enum CameraBehavior
{
    /// <summary>Free camera with full manual control.</summary>
    Free,

    /// <summary>Camera focuses on a specific object, keeping it centered.</summary>
    FocusObject,

    /// <summary>Camera orbits around a specific object.</summary>
    OrbitObject,

    /// <summary>Camera follows a specific object from behind.</summary>
    FollowObject
}
