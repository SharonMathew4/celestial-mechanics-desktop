using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace CelestialMechanics.Observation.Database;

/// <summary>
/// Prepares and executes the initial SQL schema script for the SQLite catalog database.
/// Schema designed around normalized astronomical objects rather than raw catalog files.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Creates the standard catalog index tables and annotations schema if they do not exist.
    /// </summary>
    public static async Task InitializeSchemaAsync(DatabaseService databaseService)
    {
        var connection = databaseService.GetConnection();

        // 1. Normalized base table for all astronomical objects
        var createAstronomicalObjectsTable = @"
            CREATE TABLE IF NOT EXISTS AstronomicalObjects (
                Id TEXT PRIMARY KEY,
                Name TEXT,
                ObjectType TEXT NOT NULL,
                RightAscension REAL NOT NULL,
                Declination REAL NOT NULL,
                Distance REAL NOT NULL,
                Magnitude REAL NOT NULL
            );";

        // 2. Many-to-one cross references mapping catalogs to database objects
        var createCatalogReferencesTable = @"
            CREATE TABLE IF NOT EXISTS CatalogReferences (
                CatalogName TEXT NOT NULL,
                CatalogObjectId TEXT NOT NULL,
                ObjectId TEXT NOT NULL,
                PRIMARY KEY (CatalogName, CatalogObjectId),
                FOREIGN KEY (ObjectId) REFERENCES AstronomicalObjects (Id) ON DELETE CASCADE
            );";

        // 3. Specialized stellar properties
        var createStellarMetadataTable = @"
            CREATE TABLE IF NOT EXISTS StellarMetadata (
                ObjectId TEXT PRIMARY KEY,
                Parallax REAL NOT NULL,
                ProperMotionRa REAL NOT NULL,
                ProperMotionDec REAL NOT NULL,
                SpectralType TEXT,
                FOREIGN KEY (ObjectId) REFERENCES AstronomicalObjects (Id) ON DELETE CASCADE
            );";

        // Legacy/compatibility table: Stars
        var createStarsTable = @"
            CREATE TABLE IF NOT EXISTS Stars (
                Id INTEGER PRIMARY KEY,
                RightAscension REAL NOT NULL,
                Declination REAL NOT NULL,
                Parallax REAL NOT NULL,
                Magnitude REAL NOT NULL,
                ProperMotionRa REAL NOT NULL,
                ProperMotionDec REAL NOT NULL,
                SpectralType TEXT
            );";

        var createAnnotationsTable = @"
            CREATE TABLE IF NOT EXISTS Annotations (
                Id TEXT PRIMARY KEY,
                ObjectId TEXT NOT NULL,
                AnnotationText TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );";

        using (var cmd = new SqliteCommand(createAstronomicalObjectsTable, connection))
            await cmd.ExecuteNonQueryAsync();

        using (var cmd = new SqliteCommand(createCatalogReferencesTable, connection))
            await cmd.ExecuteNonQueryAsync();

        using (var cmd = new SqliteCommand(createStellarMetadataTable, connection))
            await cmd.ExecuteNonQueryAsync();

        using (var cmd = new SqliteCommand(createStarsTable, connection))
            await cmd.ExecuteNonQueryAsync();

        using (var cmd = new SqliteCommand(createAnnotationsTable, connection))
            await cmd.ExecuteNonQueryAsync();
    }
}
