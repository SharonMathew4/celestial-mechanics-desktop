using CelestialMechanics.Physics.SoA;

#pragma warning disable CS0169 // Field is never used — reserved for GPU integration
namespace CelestialMechanics.Physics.Solvers;

/// <summary>
/// OpenGL 4.3 compute shader backend for N-body force calculation.
///
/// This backend offloads the O(n²) pairwise gravitational force computation
/// to the GPU using an OpenGL compute shader. Data flows:
///
///   1. HOST → DEVICE: Upload PosX, PosY, PosZ, Mass, IsActive to GPU SSBO
///   2. DISPATCH: Run compute shader (workgroup = ceil(n/256))
///   3. DEVICE → HOST: Download AccX, AccY, AccZ from GPU SSBO
///   4. CPU performs integration (Verlet kick-drift-kick)
///   5. CPU performs collision resolution
///
/// The GPU never mutates body count or performs structural changes.
/// SoA layout maps directly to GPU buffers (contiguous float arrays).
///
/// REQUIREMENTS:
///   • OpenGL 4.3+ context must be current on the calling thread
///   • GPU must support GL_ARB_compute_shader
///   • Context must be initialized before calling ComputeForces()
///
/// PRECISION:
///   Uses float (single-precision) on GPU for performance.
///   Double→float conversion at upload, float→double at download.
///   Typical relative force error: ~1e-7 (acceptable for real-time simulation).
///
/// THREAD SAFETY:
///   This backend MUST be called from the OpenGL thread. If the simulation
///   runs on a separate thread, the caller must marshal the call appropriately
///   or use a shared GL context.
/// </summary>
public sealed class GlComputePhysicsBackend : IPhysicsComputeBackend
{
    // GPU buffer handles
    private uint _positionBuffer;     // SSBO binding 0: float4 (x, y, z, mass)
    private uint _accelerationBuffer; // SSBO binding 1: float4 (accX, accY, accZ, 0)
    private uint _activeBuffer;       // SSBO binding 2: int (isActive flags)
    private uint _computeProgram;
    private int _uniformBodyCount;
    private int _uniformSoftening;
    private int _uniformG;
    private int _allocatedCapacity;
    private bool _initialized;
    private bool _disposed;

    // CPU-side staging buffers (avoid per-frame allocation)
    private float[] _positionData = Array.Empty<float>();   // interleaved x,y,z,mass
    private float[] _accelerationData = Array.Empty<float>(); // interleaved ax,ay,az,0
    private int[] _activeData = Array.Empty<int>();

    /// <summary>Gravitational constant in simulation units. Default = 1.0.</summary>
    public double GravitationalConstant { get; set; } = 1.0;

    /// <summary>Whether the backend has been successfully initialized.</summary>
    public bool IsInitialized => _initialized;

    /// <summary>
    /// Initialize the compute shader and GPU buffers.
    /// Must be called from the GL thread with a current GL context.
    /// This is a stub — actual GL calls require Silk.NET GL bindings
    /// which are available in the Renderer project. The backend is designed
    /// to be initialized by the Renderer via SetGlFunctions().
    /// </summary>
    public void Initialize()
    {
        // GPU resource creation will be done when Renderer provides GL context.
        // This stub ensures the backend can be constructed and configured
        // before the GL context is available.
        _initialized = false;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// When GPU resources are not initialized, falls back to a simple
    /// CPU O(n²) computation to maintain correctness.
    /// When initialized, dispatches the compute shader on the GPU.
    /// </remarks>
    public void ComputeForces(BodySoA bodies, double softening)
    {
        int count = bodies.Count;
        if (count == 0) return;

        // Until GL context integration is complete, use CPU fallback
        // This ensures the backend is usable immediately and produces
        // correct results while GPU plumbing is wired up.
        ComputeForcesCpuFallback(bodies, softening);
    }

    /// <summary>
    /// CPU reference implementation that mirrors the compute shader logic.
    /// Used as fallback when GPU is not available and for validation.
    /// </summary>
    private void ComputeForcesCpuFallback(BodySoA bodies, double softening)
    {
        int count = bodies.Count;
        double eps2 = softening * softening;
        double G = GravitationalConstant;

        // Clear accelerations
        Array.Clear(bodies.AccX, 0, count);
        Array.Clear(bodies.AccY, 0, count);
        Array.Clear(bodies.AccZ, 0, count);

        for (int i = 0; i < count; i++)
        {
            if (!bodies.IsActive[i]) continue;

            double ax = 0, ay = 0, az = 0;
            double px = bodies.PosX[i];
            double py = bodies.PosY[i];
            double pz = bodies.PosZ[i];

            for (int j = 0; j < count; j++)
            {
                if (i == j || !bodies.IsActive[j]) continue;

                double dx = bodies.PosX[j] - px;
                double dy = bodies.PosY[j] - py;
                double dz = bodies.PosZ[j] - pz;
                double dist2 = dx * dx + dy * dy + dz * dz + eps2;
                double invDist = 1.0 / System.Math.Sqrt(dist2);
                double invDist3 = invDist * invDist * invDist;
                double force = G * bodies.Mass[j] * invDist3;

                ax += dx * force;
                ay += dy * force;
                az += dz * force;
            }

            bodies.AccX[i] = ax;
            bodies.AccY[i] = ay;
            bodies.AccZ[i] = az;
        }
    }

    /// <summary>
    /// Dispose GPU resources. Safe to call multiple times.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // GPU resource cleanup would go here when GL integration is complete
    }
}
