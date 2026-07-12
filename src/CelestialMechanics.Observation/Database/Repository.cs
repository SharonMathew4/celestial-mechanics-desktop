using System;
using Microsoft.Data.Sqlite;

namespace CelestialMechanics.Observation.Database;

/// <summary>
/// Abstract base class for SQLite repositories providing access to the database connection.
/// </summary>
/// <typeparam name="T">The model type managed by the repository.</typeparam>
public abstract class Repository<T>
{
    /// <summary>
    /// Gets the underlying database service instance.
    /// </summary>
    protected DatabaseService DatabaseService { get; }

    /// <summary>
    /// Gets the current open SQLite connection for executing queries.
    /// </summary>
    protected SqliteConnection Connection => DatabaseService.GetConnection();

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T}"/> class.
    /// </summary>
    protected Repository(DatabaseService databaseService)
    {
        DatabaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
    }
}
