using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace CelestialMechanics.Observation.Database;

/// <summary>
/// Prepares and executes the initial SQL schema script for the SQLite catalog database.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Creates the standard catalog index tables and annotations schema if they do not exist.
    /// </summary>
    public static async Task InitializeSchemaAsync(DatabaseService databaseService)
    {
        var connection = databaseService.GetConnection();

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

        using var cmd1 = new SqliteCommand(createStarsTable, connection);
        await cmd1.ExecuteNonQueryAsync();

        using var cmd2 = new SqliteCommand(createAnnotationsTable, connection);
        await cmd2.ExecuteNonQueryAsync();
    }
}
