namespace CelestialMechanics.Physics.Types;

public enum BodyType
{
    Star,
    Planet,
    GasGiant,
    RockyPlanet,
    Moon,
    Asteroid,
    NeutronStar,
    BlackHole,
    Comet,
    Custom,

    // ── Phase 8: Galaxy simulation body types ─────────────────────────────
    /// <summary>Disk-population particle in a spiral/elliptical galaxy.</summary>
    GalaxyDiskParticle,
    /// <summary>Bulge-population particle (central stellar overdensity).</summary>
    GalaxyBulgeParticle,
    /// <summary>Dark-matter halo particle (non-luminous, gravitational only).</summary>
    GalaxyHaloParticle,
    /// <summary>Interstellar dust cloud (JWST deep orange/red rendering).</summary>
    DustCloud,
    /// <summary>Young star-forming cluster (JWST electric blue/cyan rendering).</summary>
    YoungStarCluster,
    /// <summary>H-II ionised hydrogen region (JWST magenta/pink rendering).</summary>
    HIIRegion
}
