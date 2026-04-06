using System.Numerics;

namespace CelestialMechanics.Renderer;

public class Camera
{
    public float Yaw { get; set; } = -90f;
    public float Pitch { get; set; } = 20f;
    public float Distance { get; set; } = 10f;
    public Vector3 Target { get; set; } = Vector3.Zero;
    public float NearPlane { get; set; } = 0.01f;
    public float FarPlane { get; set; } = 10000f;
    public float Fov { get; set; } = 60f;

    private float _smoothYaw;
    private float _smoothPitch;
    private float _smoothDistance;
    private float _smoothFov;
    private Vector3 _smoothTarget;
    private const float Damping = 8.0f;

    // ── Fly-to animation state ──────────────────────────────────
    private bool _flyToActive;
    private Vector3 _flyToTarget;
    private float _flyToDistance;
    private float _flyToProgress;
    private const float FlyToDuration = 0.8f; // seconds

    // ── Default state for Home reset ────────────────────────────
    private const float DefaultYaw = -90f;
    private const float DefaultPitch = 20f;
    private const float DefaultDistance = 10f;
    private const float DefaultFov = 60f;

    public Camera()
    {
        _smoothYaw = Yaw;
        _smoothPitch = Pitch;
        _smoothDistance = Distance;
        _smoothTarget = Target;
        _smoothFov = Fov;
    }

    public Vector3 Position
    {
        get
        {
            float yawRad = MathF.PI / 180f * _smoothYaw;
            float pitchRad = MathF.PI / 180f * _smoothPitch;

            float x = _smoothDistance * MathF.Cos(pitchRad) * MathF.Cos(yawRad);
            float y = _smoothDistance * MathF.Sin(pitchRad);
            float z = _smoothDistance * MathF.Cos(pitchRad) * MathF.Sin(yawRad);

            return _smoothTarget + new Vector3(x, y, z);
        }
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, _smoothTarget, Vector3.UnitY);
    }

    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        float fovRad = _smoothFov * MathF.PI / 180f;
        return Matrix4x4.CreatePerspectiveFieldOfView(fovRad, aspectRatio, NearPlane, FarPlane);
    }

    // ── Mouse controls ──────────────────────────────────────────

    public void ProcessMouseOrbit(float deltaX, float deltaY)
    {
        CancelFlyTo();
        Yaw += deltaX * 0.3f;
        Pitch -= deltaY * 0.3f;
        Pitch = MathF.Max(-89f, MathF.Min(89f, Pitch));
    }

    public void ProcessMousePan(float deltaX, float deltaY)
    {
        CancelFlyTo();
        float yawRad = MathF.PI / 180f * _smoothYaw;
        float pitchRad = MathF.PI / 180f * _smoothPitch;

        // Right vector (perpendicular to view direction in XZ plane)
        Vector3 right = new Vector3(-MathF.Sin(yawRad), 0, MathF.Cos(yawRad));

        // Up vector (perpendicular to both view and right)
        Vector3 forward = Vector3.Normalize(_smoothTarget - Position);
        Vector3 up = Vector3.Normalize(Vector3.Cross(right, forward));

        float panSpeed = _smoothDistance * 0.002f;
        Target -= right * deltaX * panSpeed;
        Target += up * deltaY * panSpeed;
    }

    public void ProcessMouseZoom(float scrollDelta)
    {
        CancelFlyTo();
        Distance *= MathF.Pow(0.9f, scrollDelta);
        Distance = MathF.Max(0.1f, MathF.Min(10000f, Distance));
    }

    // ── Keyboard controls (WASD + QE for up/down) ───────────────

    public void ProcessKeyboard(CameraMovement movement, float deltaTime)
    {
        CancelFlyTo();
        float speed = _smoothDistance * 1.5f * deltaTime;

        float yawRad = MathF.PI / 180f * _smoothYaw;
        Vector3 right = new Vector3(-MathF.Sin(yawRad), 0, MathF.Cos(yawRad));
        Vector3 forward = new Vector3(-MathF.Cos(yawRad), 0, -MathF.Sin(yawRad));

        switch (movement)
        {
            case CameraMovement.Forward:
                Target += forward * speed;
                break;
            case CameraMovement.Backward:
                Target -= forward * speed;
                break;
            case CameraMovement.Left:
                Target -= right * speed;
                break;
            case CameraMovement.Right:
                Target += right * speed;
                break;
            case CameraMovement.Up:
                Target += Vector3.UnitY * speed;
                break;
            case CameraMovement.Down:
                Target -= Vector3.UnitY * speed;
                break;
        }
    }

    // ── FOV zoom (Shift+Scroll) ─────────────────────────────────

    public void ProcessFovZoom(float delta)
    {
        Fov -= delta * 2f;
        Fov = MathF.Max(10f, MathF.Min(120f, Fov));
    }

    // ── Focus / Fly-to ──────────────────────────────────────────

    public void FocusOn(Vector3 position)
    {
        Target = position;
        Distance = 5f;
    }

    /// <summary>
    /// Smoothly fly the camera to focus on a target position.
    /// The camera will animate to the target over FlyToDuration seconds.
    /// </summary>
    public void FlyTo(Vector3 target, float distance = 0)
    {
        _flyToTarget = target;
        _flyToDistance = distance > 0 ? distance : MathF.Max(_smoothDistance * 0.5f, 2f);
        _flyToProgress = 0;
        _flyToActive = true;
    }

    /// <summary>
    /// Resets the camera to its default position (Home key).
    /// </summary>
    public void ResetToDefault()
    {
        CancelFlyTo();
        Yaw = DefaultYaw;
        Pitch = DefaultPitch;
        Distance = DefaultDistance;
        Target = Vector3.Zero;
        Fov = DefaultFov;
    }

    // ── Frame update ────────────────────────────────────────────

    public void Update(float deltaTime)
    {
        // Handle fly-to animation
        if (_flyToActive)
        {
            _flyToProgress += deltaTime / FlyToDuration;
            if (_flyToProgress >= 1f)
            {
                _flyToProgress = 1f;
                _flyToActive = false;
                Target = _flyToTarget;
                Distance = _flyToDistance;
            }
            else
            {
                // Smooth ease-in-out interpolation
                float t = EaseInOut(_flyToProgress);
                Target = Vector3.Lerp(Target, _flyToTarget, t * 0.15f);
                Distance = Lerp(Distance, _flyToDistance, t * 0.15f);
            }
        }

        // Smooth interpolation for all camera parameters
        float smoothT = 1f - MathF.Exp(-Damping * deltaTime);
        _smoothYaw = Lerp(_smoothYaw, Yaw, smoothT);
        _smoothPitch = Lerp(_smoothPitch, Pitch, smoothT);
        _smoothDistance = Lerp(_smoothDistance, Distance, smoothT);
        _smoothTarget = Vector3.Lerp(_smoothTarget, Target, smoothT);
        _smoothFov = Lerp(_smoothFov, Fov, smoothT);
    }

    private void CancelFlyTo()
    {
        _flyToActive = false;
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    private static float EaseInOut(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
    }
}

public enum CameraMovement
{
    Forward,
    Backward,
    Left,
    Right,
    Up,
    Down
}
