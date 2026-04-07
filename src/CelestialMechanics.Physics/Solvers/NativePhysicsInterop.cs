using System.Runtime.InteropServices;

namespace CelestialMechanics.Physics.Solvers;

internal static partial class NativePhysicsInterop
{
    [LibraryImport("celestial_engine", EntryPoint = "celestial_init")]
    internal static partial int Init(int maxParticles);

    [LibraryImport("celestial_engine", EntryPoint = "celestial_shutdown")]
    internal static partial void Shutdown();

    [LibraryImport("celestial_engine", EntryPoint = "celestial_set_particles")]
    internal static partial void SetParticles(
        double[] posX, double[] posY, double[] posZ,
        double[] velX, double[] velY, double[] velZ,
        double[] accX, double[] accY, double[] accZ,
        double[] mass, double[] radius,
        byte[] isActive, int count);

    [LibraryImport("celestial_engine", EntryPoint = "celestial_compute_forces")]
    internal static partial void ComputeForces(double softening);

    [LibraryImport("celestial_engine", EntryPoint = "celestial_set_compute_mode")]
    internal static partial void SetComputeMode(int mode);

    [LibraryImport("celestial_engine", EntryPoint = "celestial_get_accelerations")]
    internal static partial void GetAccelerations(
        double[] outAccX, double[] outAccY, double[] outAccZ, int count);

    [LibraryImport("celestial_engine", EntryPoint = "celestial_get_last_error_message", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint GetLastErrorMessagePtr();

    internal static string? GetLastErrorMessage()
    {
        nint ptr = GetLastErrorMessagePtr();
        return ptr == nint.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }
}
