namespace CelestialMechanics.Observation.Services;

/// <summary>
/// Abstraction for the Observation Mode local database.
/// Provides access to cached catalog data, user annotations,
/// and session persistence via SQLite (future implementation).
/// </summary>
public interface IObservationDatabase
{
    /// <summary>
    /// Whether the database connection is open and ready.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Opens or creates the database at the specified path.
    /// </summary>
    Task ConnectAsync(string databasePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the database connection.
    /// </summary>
    Task DisconnectAsync();
}
