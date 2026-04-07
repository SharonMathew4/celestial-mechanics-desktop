using CelestialMechanics.Physics.SoA;

namespace CelestialMechanics.Physics.Solvers;

/// <summary>
/// Compatibility wrapper that now delegates to the native CUDA backend.
/// </summary>
public sealed class CudaPhysicsBackend : IPhysicsComputeBackend, IDisposable
{
    private readonly NativeGpuPhysicsBackend _inner = new();

    /// <inheritdoc/>
    public void ComputeForces(BodySoA bodies, double softening)
    {
        _inner.ComputeForces(bodies, softening);
    }

    public void Dispose()
    {
        _inner.Dispose();
    }
}
