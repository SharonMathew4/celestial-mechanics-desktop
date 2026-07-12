using System.Numerics;
using CelestialMechanics.Observation.Objects;

namespace CelestialMechanics.Observation.Camera;

/// <summary>
/// Extends <see cref="ObservationCamera"/> with high-level behavior modes
/// (focus, orbit, follow) for tracking celestial bodies. Uses composition
/// to reuse the existing camera implementation without inheritance.
/// </summary>
public sealed class CameraBehaviorController
{
    private readonly ObservationCamera _camera;

    /// <summary>
    /// Gets the current camera behavior mode.
    /// </summary>
    public CameraBehavior CurrentBehavior { get; private set; } = CameraBehavior.Free;

    /// <summary>
    /// Gets the celestial body the camera is currently tracking, if any.
    /// </summary>
    public CelestialBody? TrackedBody { get; private set; }

    /// <summary>
    /// Gets or sets the follow distance offset (used in FollowObject mode).
    /// </summary>
    public float FollowDistance { get; set; } = 100.0f;

    /// <summary>
    /// Gets or sets the interpolation speed for camera transitions.
    /// </summary>
    public float TransitionSpeed { get; set; } = 5.0f;

    /// <summary>
    /// Gets the underlying camera instance.
    /// </summary>
    public ObservationCamera Camera => _camera;

    /// <summary>
    /// Initializes a new instance of the <see cref="CameraBehaviorController"/> class.
    /// </summary>
    /// <param name="camera">The observation camera to control.</param>
    public CameraBehaviorController(ObservationCamera camera)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
    }

    /// <summary>
    /// Sets the camera behavior mode and optional tracking target.
    /// </summary>
    /// <param name="behavior">The desired camera behavior.</param>
    /// <param name="target">The celestial body to track. Required for non-Free modes.</param>
    public void SetBehavior(CameraBehavior behavior, CelestialBody? target = null)
    {
        CurrentBehavior = behavior;
        TrackedBody = target;

        if (behavior == CameraBehavior.Free)
        {
            TrackedBody = null;
        }
    }

    /// <summary>
    /// Frames the given celestial body in the viewport by adjusting
    /// the camera distance and target to fit the body.
    /// </summary>
    /// <param name="body">The celestial body to frame.</param>
    public void FrameSelection(CelestialBody body)
    {
        if (body == null) throw new ArgumentNullException(nameof(body));

        var targetPos = ToVector3(body.Position);
        _camera.Target = targetPos;

        // Set distance based on body radius (or a minimum if radius is very small)
        float frameDistance = (float)(body.Radius > 0 ? body.Radius * 3.0 : 50.0);
        // Use zoom to set distance proportionally
        _camera.Reset();
        _camera.Target = targetPos;

        CurrentBehavior = CameraBehavior.FocusObject;
        TrackedBody = body;
    }

    /// <summary>
    /// Initiates a camera transition to the specified celestial body's position.
    /// The camera target is set immediately; smooth interpolation occurs during Update.
    /// </summary>
    /// <param name="body">The celestial body to go to.</param>
    public void GoToObject(CelestialBody body)
    {
        if (body == null) throw new ArgumentNullException(nameof(body));

        var targetPos = ToVector3(body.Position);
        _camera.Target = targetPos;

        CurrentBehavior = CameraBehavior.FocusObject;
        TrackedBody = body;
    }

    /// <summary>
    /// Updates the camera behavior each frame. When tracking a body,
    /// the camera target is updated to follow the body's current position.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame, in seconds.</param>
    public void Update(float deltaTime)
    {
        if (TrackedBody == null || CurrentBehavior == CameraBehavior.Free)
        {
            _camera.Update(deltaTime);
            return;
        }

        var bodyPos = ToVector3(TrackedBody.Position);

        switch (CurrentBehavior)
        {
            case CameraBehavior.FocusObject:
                // Keep the camera looking at the body
                _camera.Target = Vector3.Lerp(_camera.Target, bodyPos,
                    1.0f - MathF.Exp(-TransitionSpeed * deltaTime));
                break;

            case CameraBehavior.OrbitObject:
                // Keep the target at the body position; camera orbits around it
                _camera.Target = bodyPos;
                break;

            case CameraBehavior.FollowObject:
                // Position camera behind the body at a fixed offset
                var behind = bodyPos - _camera.Forward * FollowDistance;
                _camera.Target = Vector3.Lerp(_camera.Target, behind,
                    1.0f - MathF.Exp(-TransitionSpeed * deltaTime));
                break;
        }

        _camera.Update(deltaTime);
    }

    private static Vector3 ToVector3(CelestialMechanics.Math.Vec3d v)
    {
        return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
    }
}
