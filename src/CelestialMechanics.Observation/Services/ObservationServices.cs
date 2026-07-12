namespace CelestialMechanics.Observation.Services;

/// <summary>
/// Provides centralized service registration for Observation Mode.
/// In future phases, this will wire up concrete implementations
/// of all service interfaces for dependency injection.
/// </summary>
public static class ObservationServices
{
    /// <summary>
    /// Gets all service interface types registered by Observation Mode.
    /// Useful for diagnostics and validation.
    /// </summary>
    public static IReadOnlyList<Type> ServiceInterfaces { get; } = new[]
    {
        typeof(IObservationCatalog),
        typeof(ICameraService),
        typeof(INavigationService),
        typeof(ITimeService),
        typeof(ILayerService),
        typeof(ISelectionService),
        typeof(IRenderService),
        typeof(IObservationDatabase),
    };

    /// <summary>
    /// Validates that all expected service interfaces are defined.
    /// Returns true if the module is structurally complete.
    /// </summary>
    public static bool ValidateServiceDefinitions()
    {
        foreach (var serviceType in ServiceInterfaces)
        {
            if (!serviceType.IsInterface)
                return false;
        }
        return true;
    }
}
