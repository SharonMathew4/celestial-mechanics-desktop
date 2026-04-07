namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// Represents the phase of the two-step body placement workflow.
/// </summary>
public enum PlacementPhase
{
    /// <summary>No placement active.</summary>
    Inactive,

    /// <summary>User is choosing where to place the body (ghost follows cursor).</summary>
    ChoosingPosition,

    /// <summary>Position confirmed. User is setting the velocity vector direction &amp; magnitude.</summary>
    ChoosingVelocity,
}
