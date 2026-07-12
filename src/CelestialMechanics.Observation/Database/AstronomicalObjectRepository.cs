using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using CelestialMechanics.Observation.Catalog;
using Microsoft.Data.Sqlite;

namespace CelestialMechanics.Observation.Database;

/// <summary>
/// Repository class managing database access for <c>AstronomicalObjects</c> and their metadata.
/// Encapsulates all SQL execution, protecting upper-level modules from direct database exposure.
/// </summary>
public sealed class AstronomicalObjectRepository : Repository<object>
{
    public AstronomicalObjectRepository(DatabaseService databaseService) : base(databaseService)
    {
    }

    /// <summary>
    /// Executes database changes within a single transaction.
    /// </summary>
    public async Task RunInTransactionAsync(Func<SqliteTransaction, Task> action)
    {
        var conn = Connection;
        using var transaction = conn.BeginTransaction();
        try
        {
            await action(transaction);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Inserts or replaces a base astronomical object.
    /// </summary>
    public async Task InsertOrReplaceObjectAsync(
        string id, 
        string? name, 
        string objectType, 
        double ra, 
        double dec, 
        double distance, 
        double magnitude,
        SqliteTransaction? transaction = null)
    {
        var sql = @"
            INSERT OR REPLACE INTO AstronomicalObjects (Id, Name, ObjectType, RightAscension, Declination, Distance, Magnitude)
            VALUES ($id, $name, $objectType, $ra, $dec, $distance, $mag);";

        using var cmd = new SqliteCommand(sql, Connection, transaction);
        cmd.Parameters.AddWithValue("$id", id);
        cmd.Parameters.AddWithValue("$name", (object?)name ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$objectType", objectType);
        cmd.Parameters.AddWithValue("$ra", ra);
        cmd.Parameters.AddWithValue("$dec", dec);
        cmd.Parameters.AddWithValue("$distance", distance);
        cmd.Parameters.AddWithValue("$mag", magnitude);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Inserts or replaces a catalog reference mapping.
    /// </summary>
    public async Task InsertOrReplaceReferenceAsync(
        string catalogName, 
        string catalogObjectId, 
        string objectId,
        SqliteTransaction? transaction = null)
    {
        var sql = @"
            INSERT OR REPLACE INTO CatalogReferences (CatalogName, CatalogObjectId, ObjectId)
            VALUES ($catalogName, $catalogObjectId, $objectId);";

        using var cmd = new SqliteCommand(sql, Connection, transaction);
        cmd.Parameters.AddWithValue("$catalogName", catalogName);
        cmd.Parameters.AddWithValue("$catalogObjectId", catalogObjectId);
        cmd.Parameters.AddWithValue("$objectId", objectId);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Inserts or replaces specialized stellar metadata.
    /// </summary>
    public async Task InsertOrReplaceStellarMetadataAsync(
        string objectId, 
        double parallax, 
        double pmRa, 
        double pmDec, 
        string? spectralType,
        SqliteTransaction? transaction = null)
    {
        var sql = @"
            INSERT OR REPLACE INTO StellarMetadata (ObjectId, Parallax, ProperMotionRa, ProperMotionDec, SpectralType)
            VALUES ($objectId, $parallax, $pmRa, $pmDec, $spectralType);";

        using var cmd = new SqliteCommand(sql, Connection, transaction);
        cmd.Parameters.AddWithValue("$objectId", objectId);
        cmd.Parameters.AddWithValue("$parallax", parallax);
        cmd.Parameters.AddWithValue("$pmRa", pmRa);
        cmd.Parameters.AddWithValue("$pmDec", pmDec);
        cmd.Parameters.AddWithValue("$spectralType", (object?)spectralType ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Retrieves all stars registered under a specific catalog.
    /// </summary>
    public async Task<List<StarEntry>> GetStarsAsync(string catalogName)
    {
        var sql = @"
            SELECT ref.CatalogObjectId, ao.RightAscension, ao.Declination, sm.Parallax, ao.Magnitude, sm.ProperMotionRa, sm.ProperMotionDec, sm.SpectralType
            FROM AstronomicalObjects ao
            JOIN StellarMetadata sm ON ao.Id = sm.ObjectId
            JOIN CatalogReferences ref ON ao.Id = ref.ObjectId
            WHERE ao.ObjectType = 'Star' AND ref.CatalogName = $catalogName;";

        var stars = new List<StarEntry>();
        using var cmd = new SqliteCommand(sql, Connection);
        cmd.Parameters.AddWithValue("$catalogName", catalogName);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var rawId = reader.GetString(0);
            if (!int.TryParse(rawId, out int id))
            {
                // Fallback hash if ID is non-integer
                id = rawId.GetHashCode();
            }
            var ra = reader.GetDouble(1);
            var dec = reader.GetDouble(2);
            var parallax = (float)reader.GetDouble(3);
            var mag = (float)reader.GetDouble(4);
            var pmRa = (float)reader.GetDouble(5);
            var pmDec = (float)reader.GetDouble(6);
            var spectralType = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);

            stars.Add(new StarEntry(id, ra, dec, parallax, mag, pmRa, pmDec, spectralType));
        }

        return stars;
    }
}
