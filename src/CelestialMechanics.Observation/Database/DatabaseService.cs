using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CelestialMechanics.Observation.Services;
using Microsoft.Data.Sqlite;

namespace CelestialMechanics.Observation.Database;

/// <summary>
/// Implements <see cref="IObservationDatabase"/> using SQLite for local storage,
/// catalog metadata index caches, and annotations persistence.
/// </summary>
public sealed class DatabaseService : IObservationDatabase, IDisposable
{
    private SqliteConnection? _connection;

    /// <inheritdoc />
    public bool IsConnected => _connection != null && _connection.State == System.Data.ConnectionState.Open;

    /// <summary>
    /// Gets the current open SQLite connection.
    /// </summary>
    public SqliteConnection GetConnection()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("SQLite database connection is not open.");
        }
        return _connection!;
    }

    /// <inheritdoc />
    public async Task ConnectAsync(string databasePath, CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return;

        if (databasePath == null) throw new ArgumentNullException(nameof(databasePath));

        // Create directory structure if needed
        var dir = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var connString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        _connection = new SqliteConnection(connString);
        
        // Open connection on background thread
        await Task.Run(async () => await _connection.OpenAsync(cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
    public async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                await _connection.CloseAsync();
            }
            _connection.Dispose();
            _connection = null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _connection?.Dispose();
        _connection = null;
    }
}
