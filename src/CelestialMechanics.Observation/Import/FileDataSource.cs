using System;
using System.IO;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// A file-based implementation of <see cref="IDataSource"/>.
/// </summary>
public sealed class FileDataSource : IDataSource
{
    /// <inheritdoc />
    public string FilePath { get; }

    public FileDataSource(string filePath)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    /// <inheritdoc />
    public Stream OpenRead()
    {
        if (!File.Exists(FilePath))
        {
            throw new FileNotFoundException($"Import source file not found: {FilePath}", FilePath);
        }
        return new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
