using System.Numerics;

namespace CelestialMechanics.Observation.Services;

/// <summary>
/// Abstraction for camera control in Observation Mode.
/// Supports free-flight camera operations including look, pan, and zoom.
/// </summary>
public interface ICameraService
{
    /// <summary>
    /// Current camera position in world space.
    /// </summary>
    Vector3 Position { get; }

    /// <summary>
    /// Current camera target (look-at point).
    /// </summary>
    Vector3 Target { get; set; }

    /// <summary>
    /// Current camera forward direction.
    /// </summary>
    Vector3 Forward { get; }

    /// <summary>
    /// Current field of view in degrees.
    /// </summary>
    float FieldOfView { get; set; }

    /// <summary>
    /// Camera movement speed.
    /// </summary>
    float MovementSpeed { get; set; }

    /// <summary>
    /// Whether continuous forward movement is enabled.
    /// </summary>
    bool ContinuousForward { get; set; }

    /// <summary>
    /// Current distance from target.
    /// </summary>
    float Distance { get; }

    /// <summary>
    /// Current yaw in degrees.
    /// </summary>
    float Yaw { get; }

    /// <summary>
    /// Current pitch in degrees.
    /// </summary>
    float Pitch { get; }

    /// <summary>
    /// Applies mouse look rotation (yaw/pitch delta).
    /// </summary>
    void Look(float deltaX, float deltaY);

    /// <summary>
    /// Applies zoom (scroll delta).
    /// </summary>
    void Zoom(float delta);

    /// <summary>
    /// Pans the camera target along its local axes.
    /// </summary>
    void Pan(float deltaX, float deltaY);

    /// <summary>
    /// Moves the camera horizontally (strafe left/right).
    /// </summary>
    void MoveHorizontal(float direction, float deltaTime);

    /// <summary>
    /// Moves the camera vertically (strafe up/down).
    /// </summary>
    void MoveVertical(float direction, float deltaTime);

    /// <summary>
    /// Resets the camera to its default orientation and position.
    /// </summary>
    void Reset();
}

