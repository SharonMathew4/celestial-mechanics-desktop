using CelestialMechanics.Observation.Services;

namespace CelestialMechanics.Observation.Rendering;

/// <summary>
/// Renderer for Observation Mode implementing <see cref="IRenderService"/>.
/// Tracks viewport dimensions and clear color state.
/// </summary>
public sealed class ObservationRenderer : IRenderService
{
    /// <summary>
    /// Viewport width in pixels.
    /// </summary>
    public int ViewportWidth { get; private set; } = 1;

    /// <summary>
    /// Viewport height in pixels.
    /// </summary>
    public int ViewportHeight { get; private set; } = 1;

    /// <inheritdoc />
    public bool IsInitialized { get; private set; }

    /// <inheritdoc />
    public double Fps { get; set; }

    /// <summary>
    /// Whether to render coordinate axes for debugging.
    /// </summary>
    public bool ShowCoordinateAxes { get; set; } = true;

    /// <summary>
    /// Background clear color (RGBA, 0–1 range).
    /// </summary>
    public float[] ClearColor { get; set; } = [0.0f, 0.0f, 0.0f, 1.0f];

    /// <inheritdoc />
    public void Initialize(int viewportWidth, int viewportHeight)
    {
        ViewportWidth = viewportWidth > 0 ? viewportWidth : 1;
        ViewportHeight = viewportHeight > 0 ? viewportHeight : 1;
        IsInitialized = true;
    }

    /// <inheritdoc />
    public void Resize(int width, int height)
    {
        ViewportWidth = width > 0 ? width : 1;
        ViewportHeight = height > 0 ? height : 1;
    }

    /// <inheritdoc />
    public void RenderFrame(float deltaTime)
    {
        // OpenGL rendering logic placeholder
    }

    /// <summary>
    /// Aspect ratio of the current viewport.
    /// </summary>
    public float AspectRatio => (float)ViewportWidth / ViewportHeight;

    /// <inheritdoc />
    public void Shutdown()
    {
        IsInitialized = false;
    }
}
