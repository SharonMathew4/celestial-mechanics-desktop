using System;
using CelestialMechanics.Observation.Camera;
using CelestialMechanics.Observation.Scene;
using CelestialMechanics.Observation.Services;
using CelestialMechanics.Observation.Time;
using CelestialMechanics.Observation.Universe;
using Microsoft.Extensions.DependencyInjection;

namespace CelestialMechanics.Observation.Core;

/// <summary>
/// Central controller managing the Observation Mode lifecycle.
/// Resolves and coordinates the camera, renderer, and scene subsystems through DI.
/// </summary>
public sealed class ObservationController
{
    private IServiceProvider? _serviceProvider;
    private bool _isRunning;

    /// <summary>
    /// Gets the resolved camera service.
    /// </summary>
    public ICameraService? Camera => _serviceProvider?.GetService<ICameraService>();

    /// <summary>
    /// Gets the resolved renderer service.
    /// </summary>
    public IRenderService? Renderer => _serviceProvider?.GetService<IRenderService>();

    /// <summary>
    /// Gets the resolved scene manager.
    /// </summary>
    public SceneManager? Scene => _serviceProvider?.GetService<SceneManager>();

    /// <summary>
    /// Gets the resolved time manager.
    /// </summary>
    public TimeManager? TimeManager => _serviceProvider?.GetService<TimeManager>();

    /// <summary>
    /// Gets the resolved universe manager.
    /// </summary>
    public UniverseManager? UniverseManager => _serviceProvider?.GetService<UniverseManager>();

    /// <summary>
    /// Gets the resolved camera behavior controller.
    /// </summary>
    public CameraBehaviorController? CameraBehavior => _serviceProvider?.GetService<CameraBehaviorController>();

    /// <summary>
    /// Whether the controller is currently running.
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Initializes the controller with the registered service provider.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider.</param>
    public void Initialize(IServiceProvider serviceProvider)
    {
        if (_isRunning)
            return;

        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _isRunning = true;
    }

    /// <summary>
    /// Shuts down the controller and releases resources.
    /// </summary>
    public void Shutdown()
    {
        if (!_isRunning)
            return;

        _serviceProvider = null;
        _isRunning = false;
    }

    /// <summary>
    /// Called each frame to update the camera, scene graph, and sub-systems.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame, in seconds.</param>
    public void Update(float deltaTime)
    {
        if (!_isRunning)
            return;

        // 1. Advance simulation time
        TimeManager?.Tick(deltaTime);

        // 2. Update camera (use behavior controller if available, otherwise raw camera)
        if (CameraBehavior != null)
        {
            CameraBehavior.Update(deltaTime);
        }
        else if (Camera is ObservationCamera obsCamera)
        {
            obsCamera.Update(deltaTime);
        }

        // 3. Update scene graph
        Scene?.Update(deltaTime);
    }
}
