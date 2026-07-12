using System;
using Microsoft.Extensions.DependencyInjection;
using CelestialMechanics.Observation.Services;
using CelestialMechanics.Observation.Camera;
using CelestialMechanics.Observation.Rendering;
using CelestialMechanics.Observation.Scene;
using CelestialMechanics.Observation.Catalog;
using CelestialMechanics.Observation.Database;
using CelestialMechanics.Observation.World;

namespace CelestialMechanics.Observation.Core;

/// <summary>
/// Bootstraps Observation Mode by setting up the Dependency Injection container
/// and registering all required services.
/// </summary>
public sealed class ObservationBootstrap
{
    private readonly IServiceProvider _serviceProvider;
    private bool _isInitialized;

    /// <summary>
    /// Gets the Dependency Injection service provider for this session.
    /// </summary>
    public IServiceProvider ServiceProvider => _serviceProvider;

    /// <summary>
    /// Gets the active controller.
    /// </summary>
    public ObservationController Controller => _serviceProvider.GetRequiredService<ObservationController>();

    /// <summary>
    /// Whether initialization has completed successfully.
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservationBootstrap"/> class.
    /// Sets up all DI container service descriptors.
    /// </summary>
    public ObservationBootstrap()
    {
        var services = new ServiceCollection();

        // 1. Data/Database Infrastructure
        services.AddSingleton<DatabaseService>();
        services.AddSingleton<IObservationDatabase>(sp => sp.GetRequiredService<DatabaseService>());
        services.AddSingleton<ObservationCatalog>();
        services.AddSingleton<IObservationCatalog>(sp => sp.GetRequiredService<ObservationCatalog>());

        // 2. Camera & Render Services
        services.AddSingleton<ObservationCamera>();
        services.AddSingleton<ICameraService>(sp => sp.GetRequiredService<ObservationCamera>());
        services.AddSingleton<ObservationRenderer>();
        services.AddSingleton<IRenderService>(sp => sp.GetRequiredService<ObservationRenderer>());

        // 3. Scene & World Systems
        services.AddSingleton<SceneManager>();
        services.AddSingleton<WorldManager>();

        // 4. Catalog Management
        services.AddSingleton<CatalogService>();
        services.AddSingleton<ICatalogService>(sp => sp.GetRequiredService<CatalogService>());
        
        // 5. Placeholder Service Adaptors
        services.AddSingleton<INavigationService, ObservationNavigationService>();
        services.AddSingleton<ITimeService, ObservationTimeService>();
        services.AddSingleton<ILayerService, ObservationLayerService>();
        services.AddSingleton<ISelectionService, ObservationSelectionService>();

        // 6. Central Controller
        services.AddSingleton<ObservationController>();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Initializes all Observation Mode subsystems.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
            return;

        // Register default providers to CatalogService
        var catalogService = _serviceProvider.GetRequiredService<ICatalogService>();
        var catalog = _serviceProvider.GetRequiredService<ObservationCatalog>();
        
        catalogService.RegisterProvider(new StarProvider(catalog));
        catalogService.RegisterProvider(new PlanetProvider());
        catalogService.RegisterProvider(new GalaxyProvider());
        catalogService.RegisterProvider(new NebulaProvider());
        catalogService.RegisterProvider(new SpacecraftProvider());

        Controller.Initialize(_serviceProvider);
        _isInitialized = true;
    }

    /// <summary>
    /// Shuts down all Observation Mode subsystems.
    /// </summary>
    public void Shutdown()
    {
        if (!_isInitialized)
            return;

        Controller.Shutdown();
        (_serviceProvider as IDisposable)?.Dispose();
        _isInitialized = false;
    }
}
