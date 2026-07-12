using System.IO;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Abstraction representing an external data source to read catalog records from.
/// </summary>
public interface IDataSource
{
    /// <summary>
    /// Gets the logical path or URI identifier of the source file.
    /// </summary>
    string FilePath { get; }

    /// <summary>
    /// Opens a read stream to retrieve data content.
    /// </summary>
    Stream OpenRead();
}
