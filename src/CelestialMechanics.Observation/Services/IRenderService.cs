namespace CelestialMechanics.Observation.Services;

/// <summary>
/// Abstraction for the Observation Mode rendering pipeline.
/// Manages the OpenGL context lifecycle, frame rendering,
/// and viewport configuration.
/// </summary>
public interface IRenderService
{
    /// <summary>
    /// Whether the render context has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Current frames per second.
    /// </summary>
    double Fps { get; }

    /// <summary>
    /// Whether to render coordinate axes for debugging.
    /// </summary>
    bool ShowCoordinateAxes { get; set; }

    /// <summary>
    /// Initializes the rendering pipeline.
    /// </summary>
    void Initialize(int viewportWidth, int viewportHeight);

    /// <summary>
    /// Resizes the rendering viewport.
    /// </summary>
    void Resize(int width, int height);

    /// <summary>
    /// Renders a single frame.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame, in seconds.</param>
    void RenderFrame(float deltaTime);

    /// <summary>
    /// Shuts down the rendering pipeline and releases GPU resources.
    /// </summary>
    void Shutdown();
}
