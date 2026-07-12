using System;
using System.Threading;
using System.Threading.Tasks;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Placeholder importer for Gaia DR3.
/// </summary>
public sealed class GaiaImporter : ImporterBase
{
    /// <inheritdoc />
    public override string CatalogName => "Gaia DR3";

    /// <inheritdoc />
    public override Task<ImportResult> ImportAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ImportResult
        {
            CatalogName = CatalogName,
            Success = true,
            ImportedCount = 0,
            SkippedCount = 0,
            Duration = TimeSpan.Zero
        });
    }
}

/// <summary>
/// Placeholder importer for JPL SPICE.
/// </summary>
public sealed class SpiceImporter : ImporterBase
{
    /// <inheritdoc />
    public override string CatalogName => "JPL SPICE";

    /// <inheritdoc />
    public override Task<ImportResult> ImportAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ImportResult
        {
            CatalogName = CatalogName,
            Success = true,
            ImportedCount = 0,
            SkippedCount = 0,
            Duration = TimeSpan.Zero
        });
    }
}

/// <summary>
/// Placeholder importer for Messier Catalog.
/// </summary>
public sealed class MessierImporter : ImporterBase
{
    /// <inheritdoc />
    public override string CatalogName => "Messier";

    /// <inheritdoc />
    public override Task<ImportResult> ImportAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ImportResult
        {
            CatalogName = CatalogName,
            Success = true,
            ImportedCount = 0,
            SkippedCount = 0,
            Duration = TimeSpan.Zero
        });
    }
}

/// <summary>
/// Placeholder importer for NGC.
/// </summary>
public sealed class NgcImporter : ImporterBase
{
    /// <inheritdoc />
    public override string CatalogName => "NGC";

    /// <inheritdoc />
    public override Task<ImportResult> ImportAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ImportResult
        {
            CatalogName = CatalogName,
            Success = true,
            ImportedCount = 0,
            SkippedCount = 0,
            Duration = TimeSpan.Zero
        });
    }
}

/// <summary>
/// Placeholder importer for SIMBAD.
/// </summary>
public sealed class SimbadImporter : ImporterBase
{
    /// <inheritdoc />
    public override string CatalogName => "SIMBAD";

    /// <inheritdoc />
    public override Task<ImportResult> ImportAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ImportResult
        {
            CatalogName = CatalogName,
            Success = true,
            ImportedCount = 0,
            SkippedCount = 0,
            Duration = TimeSpan.Zero
        });
    }
}

/// <summary>
/// Placeholder importer for NASA Exoplanet Archive.
/// </summary>
public sealed class ExoplanetImporter : ImporterBase
{
    /// <inheritdoc />
    public override string CatalogName => "NASA Exoplanet Archive";

    /// <inheritdoc />
    public override Task<ImportResult> ImportAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ImportResult
        {
            CatalogName = CatalogName,
            Success = true,
            ImportedCount = 0,
            SkippedCount = 0,
            Duration = TimeSpan.Zero
        });
    }
}

/// <summary>
/// Placeholder importer for Minor Planet Center.
/// </summary>
public sealed class MpcImporter : ImporterBase
{
    /// <inheritdoc />
    public override string CatalogName => "Minor Planet Center";

    /// <inheritdoc />
    public override Task<ImportResult> ImportAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ImportResult
        {
            CatalogName = CatalogName,
            Success = true,
            ImportedCount = 0,
            SkippedCount = 0,
            Duration = TimeSpan.Zero
        });
    }
}
