using System;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Exception thrown during pipeline execution errors.
/// </summary>
public sealed class ImportException : Exception
{
    public ImportException(string message) : base(message)
    {
    }

    public ImportException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
