using System;
using Microsoft.Extensions.DependencyInjection;
using CelestialMechanics.Observation.Services;
using CelestialMechanics.Observation.Camera;
using CelestialMechanics.Observation.Rendering;
using CelestialMechanics.Observation.Scene;
using CelestialMechanics.Observation.Catalog;
using CelestialMechanics.Observation.Database;
using CelestialMechanics.Observation.World;
using CelestialMechanics.Observation.Import;
using CelestialMechanics.Observation.Resources;
using CelestialMechanics.Observation.Events;
using CelestialMechanics.Observation.Objects;
using CelestialMechanics.Observation.Search;
using CelestialMechanics.Observation.Selection;
using CelestialMechanics.Observation.Time;
using CelestialMechanics.Observation.Universe;

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
        services.AddSingleton<AstronomicalObjectRepository>();

        // 2. Camera, Render Engine & Settings
        services.AddSingleton<ObservationCamera>();
        services.AddSingleton<ICameraService>(sp => sp.GetRequiredService<ObservationCamera>());
        services.AddSingleton<ObservationRenderer>();
        services.AddSingleton<IRenderService>(sp => sp.GetRequiredService<ObservationRenderer>());
        services.AddSingleton<RenderEngine>();
        services.AddSingleton<RendererManager>();
        services.AddTransient<RenderQueue>();
        services.AddSingleton<RenderSettings>();

        // 3. Scene, World & Raycasting Picking
        services.AddSingleton<SceneManager>();
        services.AddSingleton<WorldManager>();
        services.AddSingleton<ScenePicker>();

        // 4. Central Resource Management
        services.AddSingleton<ResourceLoader>();

        // 5. Catalog Management & Providers
        services.AddSingleton<CatalogService>();
        services.AddSingleton<ICatalogService>(sp => sp.GetRequiredService<CatalogService>());
        services.AddSingleton<StarProvider>();
        services.AddSingleton<PlanetProvider>();
        services.AddSingleton<GalaxyProvider>();
        services.AddSingleton<NebulaProvider>();
        services.AddSingleton<SpacecraftProvider>();
        
        // 6. Ingestion Pipeline
        services.AddSingleton<ImportManager>();
        services.AddTransient<HipparcosImporter>();
        services.AddTransient<GaiaImporter>();
        services.AddTransient<SpiceImporter>();
        services.AddTransient<MessierImporter>();
        services.AddTransient<NgcImporter>();
        services.AddTransient<SimbadImporter>();
        services.AddTransient<ExoplanetImporter>();
        services.AddTransient<MpcImporter>();

        // 7. Renderers
        services.AddSingleton<StarRenderer>();
        services.AddSingleton<PlanetRenderer>();
        services.AddSingleton<OrbitRenderer>();
        services.AddSingleton<GridRenderer>();
        services.AddSingleton<LabelRenderer>();
        services.AddSingleton<SkyRenderer>();

        // 8. Placeholder Service Adaptors (Navigation, Layers remain as placeholders)
        services.AddSingleton<INavigationService, ObservationNavigationService>();
        services.AddSingleton<ILayerService, ObservationLayerService>();

        // 9. Phase 5 — Universe Core Services
        services.AddSingleton<EventBus>();
        services.AddSingleton<UniverseHierarchy>();
        services.AddSingleton<UniverseManager>();
        services.AddSingleton<TimeManager>();
        services.AddSingleton<ITimeService>(sp => sp.GetRequiredService<TimeManager>());
        services.AddSingleton<SelectionManager>();
        services.AddSingleton<ISelectionService>(sp => sp.GetRequiredService<SelectionManager>());
        services.AddSingleton<SearchService>();
        services.AddSingleton<CelestialBodyFactory>();
        services.AddSingleton<CameraBehaviorController>();

        // 10. Central Controller
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
        
        catalogService.RegisterProvider(_serviceProvider.GetRequiredService<StarProvider>());
        catalogService.RegisterProvider(_serviceProvider.GetRequiredService<PlanetProvider>());
        catalogService.RegisterProvider(_serviceProvider.GetRequiredService<GalaxyProvider>());
        catalogService.RegisterProvider(_serviceProvider.GetRequiredService<NebulaProvider>());
        catalogService.RegisterProvider(_serviceProvider.GetRequiredService<SpacecraftProvider>());

        // Register default importers to ImportManager
        var importManager = _serviceProvider.GetRequiredService<ImportManager>();
        importManager.RegisterImporter(_serviceProvider.GetRequiredService<HipparcosImporter>());
        importManager.RegisterImporter(_serviceProvider.GetRequiredService<GaiaImporter>());
        importManager.RegisterImporter(_serviceProvider.GetRequiredService<SpiceImporter>());
        importManager.RegisterImporter(_serviceProvider.GetRequiredService<MessierImporter>());
        importManager.RegisterImporter(_serviceProvider.GetRequiredService<NgcImporter>());
        importManager.RegisterImporter(_serviceProvider.GetRequiredService<SimbadImporter>());
        importManager.RegisterImporter(_serviceProvider.GetRequiredService<ExoplanetImporter>());
        importManager.RegisterImporter(_serviceProvider.GetRequiredService<MpcImporter>());

        // Register default renderers to RendererManager
        var rendererManager = _serviceProvider.GetRequiredService<RendererManager>();
        rendererManager.RegisterRenderer(_serviceProvider.GetRequiredService<StarRenderer>());
        rendererManager.RegisterRenderer(_serviceProvider.GetRequiredService<PlanetRenderer>());
        rendererManager.RegisterRenderer(_serviceProvider.GetRequiredService<OrbitRenderer>());
        rendererManager.RegisterRenderer(_serviceProvider.GetRequiredService<GridRenderer>());
        rendererManager.RegisterRenderer(_serviceProvider.GetRequiredService<LabelRenderer>());
        rendererManager.RegisterRenderer(_serviceProvider.GetRequiredService<SkyRenderer>());

        // Initialize Universe Manager
        var universeManager = _serviceProvider.GetRequiredService<UniverseManager>();
        universeManager.Initialize();

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
