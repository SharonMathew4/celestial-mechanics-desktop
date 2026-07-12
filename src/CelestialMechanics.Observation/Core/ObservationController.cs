using System;
using CelestialMechanics.Observation.Camera;
using CelestialMechanics.Observation.Scene;
using CelestialMechanics.Observation.Services;
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

        if (Camera is ObservationCamera obsCamera)
        {
            obsCamera.Update(deltaTime);
        }

        Scene?.Update(deltaTime);
    }
}
