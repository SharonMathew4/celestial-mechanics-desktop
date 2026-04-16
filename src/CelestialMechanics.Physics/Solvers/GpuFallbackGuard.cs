using CelestialMechanics.Physics.SoA;

namespace CelestialMechanics.Physics.Solvers;

/// <summary>
/// Wraps a GPU physics backend with automatic CPU fallback on failure.
///
/// If the GPU backend throws any exception during ComputeForces(),
/// this guard transparently switches to a CPU fallback backend
/// for all subsequent calls. A diagnostic message is logged once.
///
/// This ensures the simulation never crashes due to GPU issues
/// (driver bugs, missing GL 4.3 support, context loss, etc.).
/// </summary>
public sealed class GpuFallbackGuard : IPhysicsComputeBackend
{
    private readonly IPhysicsComputeBackend _gpuBackend;
    private readonly IPhysicsComputeBackend _cpuFallback;
    private bool _gpuFailed;
    private string? _failureReason;

    /// <summary>True if the GPU backend has failed and fallback is active.</summary>
    public bool IsUsingFallback => _gpuFailed;

    /// <summary>Reason for GPU fallback, or null if GPU is healthy.</summary>
    public string? FallbackReason => _failureReason;

    /// <summary>Name of the currently active backend for diagnostics.</summary>
    public string ActiveBackendName => _gpuFailed
        ? $"Fallback:{_cpuFallback.GetType().Name}"
        : _gpuBackend.GetType().Name;

    public GpuFallbackGuard(IPhysicsComputeBackend gpuBackend, IPhysicsComputeBackend cpuFallback)
    {
        _gpuBackend = gpuBackend ?? throw new ArgumentNullException(nameof(gpuBackend));
        _cpuFallback = cpuFallback ?? throw new ArgumentNullException(nameof(cpuFallback));
    }

    /// <inheritdoc/>
    public void ComputeForces(BodySoA bodies, double softening)
    {
        if (_gpuFailed)
        {
            _cpuFallback.ComputeForces(bodies, softening);
            return;
        }

        try
        {
            _gpuBackend.ComputeForces(bodies, softening);
        }
        catch (Exception ex)
        {
            _gpuFailed = true;
            _failureReason = $"GPU backend failed: {ex.GetType().Name}: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[GpuFallbackGuard] {_failureReason}");
            System.Diagnostics.Debug.WriteLine($"[GpuFallbackGuard] Falling back to {_cpuFallback.GetType().Name}");

            // Execute this frame on CPU so simulation doesn't stall
            _cpuFallback.ComputeForces(bodies, softening);
        }
    }

    /// <summary>
    /// Reset the guard to attempt GPU usage again.
    /// Call this after resolving the underlying GPU issue.
    /// </summary>
    public void ResetFallback()
    {
        _gpuFailed = false;
        _failureReason = null;
    }
}
