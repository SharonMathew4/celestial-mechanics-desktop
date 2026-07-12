using System;
using System.Numerics;
using CelestialMechanics.Observation.Services;

namespace CelestialMechanics.Observation.Camera;

/// <summary>
/// Free-flight camera for Observation Mode implementing <see cref="ICameraService"/>.
/// Supports mouse look (yaw/pitch), mouse panning, scroll zoom, and keyboard strafe controls.
/// </summary>
public sealed class ObservationCamera : ICameraService
{
    // ── Target state (set by input) ─────────────────────────────────
    private float _yaw = -90.0f;     // Degrees, facing -Z initially
    private float _pitch = 0.0f;     // Degrees
    private float _distance = 50.0f;  // Distance from target (zoom level)
    private Vector3 _target = Vector3.Zero;

    // ── Smoothed state (interpolated each frame) ────────────────────
    private float _smoothYaw;
    private float _smoothPitch;
    private float _smoothDistance;
    private Vector3 _smoothTarget = Vector3.Zero;

    // ── Configuration ───────────────────────────────────────────────
    private const float Damping = 8.0f;
    private const float MinPitch = -89.0f;
    private const float MaxPitch = 89.0f;
    private const float MinDistance = 0.1f;
    private const float MaxDistance = 500_000.0f;

    /// <inheritdoc />
    public float FieldOfView { get; set; } = 60.0f;

    /// <summary>
    /// Near clipping plane.
    /// </summary>
    public float NearPlane { get; set; } = 0.01f;

    /// <summary>
    /// Far clipping plane.
    /// </summary>
    public float FarPlane { get; set; } = 1_000_000.0f;

    /// <summary>
    /// Mouse look sensitivity multiplier.
    /// </summary>
    public float MouseSensitivity { get; set; } = 0.25f;

    /// <summary>
    /// Zoom sensitivity multiplier.
    /// </summary>
    public float ZoomSensitivity { get; set; } = 0.1f;

    /// <inheritdoc />
    public float Yaw => _smoothYaw;

    /// <inheritdoc />
    public float Pitch => _smoothPitch;

    /// <inheritdoc />
    public float Distance => _smoothDistance;

    /// <inheritdoc />
    public Vector3 Target
    {
        get => _target;
        set => _target = value;
    }

    /// <inheritdoc />
    public float MovementSpeed { get; set; } = 50.0f;

    /// <inheritdoc />
    public bool ContinuousForward { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservationCamera"/> class.
    /// </summary>
    public ObservationCamera()
    {
        _smoothYaw = _yaw;
        _smoothPitch = _pitch;
        _smoothDistance = _distance;
    }

    /// <inheritdoc />
    public Vector3 Position
    {
        get
        {
            float yawRad = MathF.PI / 180.0f * _smoothYaw;
            float pitchRad = MathF.PI / 180.0f * _smoothPitch;

            float x = _smoothDistance * MathF.Cos(pitchRad) * MathF.Cos(yawRad);
            float y = _smoothDistance * MathF.Sin(pitchRad);
            float z = _smoothDistance * MathF.Cos(pitchRad) * MathF.Sin(yawRad);

            return _smoothTarget + new Vector3(x, y, z);
        }
    }

    /// <inheritdoc />
    public Vector3 Forward
    {
        get
        {
            float yawRad = MathF.PI / 180.0f * _smoothYaw;
            float pitchRad = MathF.PI / 180.0f * _smoothPitch;

            float x = MathF.Cos(pitchRad) * MathF.Cos(yawRad);
            float y = MathF.Sin(pitchRad);
            float z = MathF.Cos(pitchRad) * MathF.Sin(yawRad);

            return -Vector3.Normalize(new Vector3(x, y, z));
        }
    }

    /// <summary>
    /// Local right unit vector.
    /// </summary>
    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));

    /// <summary>
    /// Local up unit vector.
    /// </summary>
    public Vector3 Up => Vector3.Normalize(Vector3.Cross(Right, Forward));

    /// <summary>
    /// Constructs the view matrix.
    /// </summary>
    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, _smoothTarget, Vector3.UnitY);
    }

    /// <summary>
    /// Constructs the projection matrix.
    /// </summary>
    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        float fovRad = FieldOfView * MathF.PI / 180.0f;
        return Matrix4x4.CreatePerspectiveFieldOfView(fovRad, aspectRatio, NearPlane, FarPlane);
    }

    /// <inheritdoc />
    public void Look(float deltaX, float deltaY)
    {
        ProcessMouseLook(deltaX, deltaY);
    }

    /// <summary>
    /// Applies mouse look input.
    /// </summary>
    public void ProcessMouseLook(float deltaX, float deltaY)
    {
        _yaw += deltaX * MouseSensitivity;
        _pitch -= deltaY * MouseSensitivity;
        _pitch = MathF.Max(MinPitch, MathF.Min(MaxPitch, _pitch));
    }

    /// <inheritdoc />
    public void Zoom(float delta)
    {
        ProcessZoom(delta);
    }

    /// <summary>
    /// Applies zoom input.
    /// </summary>
    public void ProcessZoom(float scrollDelta)
    {
        _distance *= MathF.Pow(0.9f, scrollDelta * ZoomSensitivity * 10.0f);
        _distance = MathF.Max(MinDistance, MathF.Min(MaxDistance, _distance));
    }

    /// <inheritdoc />
    public void Pan(float deltaX, float deltaY)
    {
        float factor = _smoothDistance * 0.002f;
        _target += -Right * deltaX * factor + Up * deltaY * factor;
    }

    /// <inheritdoc />
    public void MoveHorizontal(float direction, float deltaTime)
    {
        _target += Right * direction * MovementSpeed * deltaTime;
    }

    /// <inheritdoc />
    public void MoveVertical(float direction, float deltaTime)
    {
        _target += Up * direction * MovementSpeed * deltaTime;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _yaw = -90.0f;
        _pitch = 0.0f;
        _distance = 50.0f;
        _target = Vector3.Zero;
    }

    /// <summary>
    /// Smoothly interpolates toward target states each frame.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame, in seconds.</param>
    public void Update(float deltaTime)
    {
        if (ContinuousForward)
        {
            _target += Forward * MovementSpeed * deltaTime;
        }

        float t = 1.0f - MathF.Exp(-Damping * deltaTime);
        _smoothYaw = Lerp(_smoothYaw, _yaw, t);
        _smoothPitch = Lerp(_smoothPitch, _pitch, t);
        _smoothDistance = Lerp(_smoothDistance, _distance, t);
        _smoothTarget = Vector3.Lerp(_smoothTarget, _target, t);
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
