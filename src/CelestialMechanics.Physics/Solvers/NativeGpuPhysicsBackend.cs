using CelestialMechanics.Physics.SoA;

namespace CelestialMechanics.Physics.Solvers;

/// <summary>
/// GPU backend backed by the native celestial_engine CUDA implementation.
/// If native initialization fails, this backend throws so callers can surface
/// the failure instead of silently degrading.
/// </summary>
public sealed class NativeGpuPhysicsBackend : IPhysicsComputeBackend, IDisposable
{
    private readonly object _sync = new();
    private bool _initialized;
    private int _capacity;

    public void ComputeForces(BodySoA bodies, double softening)
    {
        int count = bodies.Count;
        if (count <= 0)
            return;

        lock (_sync)
        {
            EnsureInitialized(count);

            byte[] activeFlags = count <= 0 ? Array.Empty<byte>() : new byte[count];
            for (int i = 0; i < count; i++)
                activeFlags[i] = bodies.IsActive[i] ? (byte)1 : (byte)0;

            NativePhysicsInterop.SetParticles(
                bodies.PosX, bodies.PosY, bodies.PosZ,
                bodies.VelX, bodies.VelY, bodies.VelZ,
                bodies.AccX, bodies.AccY, bodies.AccZ,
                bodies.Mass, bodies.Radius,
                activeFlags, count);

            NativePhysicsInterop.ComputeForces(softening);
            NativePhysicsInterop.GetAccelerations(bodies.AccX, bodies.AccY, bodies.AccZ, count);
        }
    }

    private void EnsureInitialized(int requiredCount)
    {
        if (_initialized && requiredCount <= _capacity)
            return;

        if (_initialized)
        {
            NativePhysicsInterop.Shutdown();
            _initialized = false;
            _capacity = 0;
        }

        int rc = NativePhysicsInterop.Init(System.Math.Max(requiredCount, 16));
        if (rc != 0)
        {
            string details = NativePhysicsInterop.GetLastErrorMessage() ?? "unknown native initialization error";
            throw new InvalidOperationException($"Failed to initialize native GPU backend (code {rc}): {details}");
        }

        // 2 = GPU_BruteForce (native API contract)
        NativePhysicsInterop.SetComputeMode(2);
        _initialized = true;
        _capacity = System.Math.Max(requiredCount, 16);
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_initialized)
            {
                NativePhysicsInterop.Shutdown();
                _initialized = false;
                _capacity = 0;
            }
        }
    }
}
