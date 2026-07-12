namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Handles record-level validation before database insertion.
/// </summary>
/// <typeparam name="T">The model type representing parsed catalog records.</typeparam>
public interface IImportValidator<in T>
{
    /// <summary>
    /// Validates an individual record.
    /// </summary>
    /// <param name="record">The record instance to validate.</param>
    /// <param name="errorMessage">Output detailing validation issue if validation fails.</param>
    /// <returns>True if the record is valid; otherwise false.</returns>
    bool Validate(T record, out string? errorMessage);
}
