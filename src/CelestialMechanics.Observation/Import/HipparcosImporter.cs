using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CelestialMechanics.Observation.Database;
using Microsoft.Data.Sqlite;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Concrete importer for the Hipparcos star catalog.
/// Implements validation, conversion, database insertion via Repository, and progress tracking.
/// </summary>
public sealed class HipparcosImporter : ImporterBase
{
    private readonly AstronomicalObjectRepository _repository;
    private static readonly byte[] MagicBytes = "HIPB"u8.ToArray();
    private const ushort ExpectedVersion = 1;

    /// <inheritdoc />
    public override string CatalogName => "Hipparcos";

    public HipparcosImporter(AstronomicalObjectRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc />
    public override async Task<ImportResult> ImportAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        var result = new ImportResult { CatalogName = CatalogName, Success = false };
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var stream = job.DataSource.OpenRead();
            using var reader = new BinaryReader(stream, Encoding.ASCII);

            // Read header
            byte[] magic = reader.ReadBytes(4);
            if (magic.Length < 4 || !magic.SequenceEqual(MagicBytes))
            {
                throw new ImportException("Invalid magic bytes in Hipparcos binary catalog.");
            }

            ushort version = reader.ReadUInt16();
            if (version != ExpectedVersion)
            {
                throw new ImportException($"Unsupported catalog version: {version}. Expected {ExpectedVersion}.");
            }

            int totalCount = reader.ReadInt32();
            if (totalCount < 0)
            {
                throw new ImportException($"Malformed record count in catalog: {totalCount}");
            }

            long imported = 0;
            long skipped = 0;
            long validationErrors = 0;
            byte[] spectralBuffer = new byte[12];

            int batchSize = job.Settings.BatchSize;
            int currentBatchCount = 0;

            // Log start of import
            result.Warnings.Add($"Import started for {CatalogName} catalog. Total records expected: {totalCount}");

            // Execute all writes inside transaction batches
            for (int i = 0; i < totalCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Read individual record parameters
                int rawId = reader.ReadInt32();
                double ra = reader.ReadDouble();
                double dec = reader.ReadDouble();
                float parallax = reader.ReadSingle();
                float mag = reader.ReadSingle();
                float pmRa = reader.ReadSingle();
                float pmDec = reader.ReadSingle();

                int bytesRead = reader.Read(spectralBuffer, 0, 12);
                string spectralType = Encoding.ASCII.GetString(spectralBuffer, 0, bytesRead).TrimEnd('\0', ' ');

                // 1. Validation phase
                bool isValid = true;
                string? errorMsg = null;

                if (rawId < 0)
                {
                    isValid = false;
                    errorMsg = $"Record {i}: ID cannot be negative. ID={rawId}.";
                }
                else if (ra < 0.0 || ra > 360.0 || double.IsNaN(ra))
                {
                    isValid = false;
                    errorMsg = $"Record ID {rawId}: Invalid Right Ascension (RA) coordinate: {ra}. Must be in [0, 360].";
                }
                else if (dec < -90.0 || dec > 90.0 || double.IsNaN(dec))
                {
                    isValid = false;
                    errorMsg = $"Record ID {rawId}: Invalid Declination (Dec) coordinate: {dec}. Must be in [-90, 90].";
                }
                else if (parallax < 0.0f || float.IsNaN(parallax))
                {
                    isValid = false;
                    errorMsg = $"Record ID {rawId}: Invalid negative or NaN parallax: {parallax}.";
                }
                else if (mag < -30.0f || mag > 40.0f || float.IsNaN(mag))
                {
                    isValid = false;
                    errorMsg = $"Record ID {rawId}: Malformed magnitude: {mag}.";
                }

                if (!isValid)
                {
                    validationErrors++;
                    skipped++;
                    result.Errors.Add(errorMsg!);
                    continue;
                }

                // 2. Conversion Phase
                // Star object-centric ID: e.g. "HIP_101"
                string objectId = $"HIP_{rawId}";
                // Distance in parsecs: 1000 / parallax (if parallax > 0)
                double distancePc = parallax > 0.0f ? 1000.0 / parallax : 1_000_000.0;

                // 3. Storage Phase (committing asynchronously)
                await _repository.InsertOrReplaceObjectAsync(
                    objectId,
                    $"HIP {rawId}",
                    "Star",
                    ra,
                    dec,
                    distancePc,
                    mag
                );

                await _repository.InsertOrReplaceReferenceAsync(
                    CatalogName,
                    rawId.ToString(),
                    objectId
                );

                await _repository.InsertOrReplaceStellarMetadataAsync(
                    objectId,
                    parallax,
                    pmRa,
                    pmDec,
                    spectralType
                );

                imported++;
                currentBatchCount++;

                // Trigger batch updates and progress notification
                if (currentBatchCount >= batchSize || i == totalCount - 1)
                {
                    currentBatchCount = 0;
                    if (job.ProgressCallback != null)
                    {
                        var elapsed = stopwatch.Elapsed;
                        var remaining = EstimateRemaining(elapsed, i + 1, totalCount);
                        var progress = new ImportProgress(
                            Path.GetFileName(job.DataSource.FilePath),
                            imported,
                            skipped,
                            validationErrors,
                            elapsed,
                            remaining
                        );
                        job.ProgressCallback(progress);
                    }
                }
            }

            stopwatch.Stop();
            result.Success = true;
            result.ImportedCount = imported;
            result.SkippedCount = skipped;
            result.Duration = stopwatch.Elapsed;
            result.Warnings.Add($"Import completed successfully in {result.Duration.TotalSeconds:F2} seconds. Imported: {imported}, Skipped: {skipped}, Validation Errors: {validationErrors}.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            result.Success = false;
            result.Errors.Add($"Fatal error during Hipparcos import: {ex.Message}");
            throw new ImportException($"Hipparcos importer failed: {ex.Message}", ex);
        }

        return result;
    }
}
