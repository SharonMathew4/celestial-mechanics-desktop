using HelixToolkit.Wpf.SharpDX;

namespace CelestialMechanics.Desktop.Services;

public sealed class CameraManager
{
    public ProjectionCamera CreateDefaultCamera()
    {
        return new PerspectiveCamera
        {
            Position = new System.Windows.Media.Media3D.Point3D(0, 15, 35),
            LookDirection = new System.Windows.Media.Media3D.Vector3D(0, -10, -35),
            UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0),
            FarPlaneDistance = 1_000_000,
            NearPlaneDistance = 0.001,
            FieldOfView = 45
        };
    }

    public double ToLogZoom(double linearValue)
    {
        var clamped = System.Math.Clamp(linearValue, 0.1, 1_000_000.0);
        return System.Math.Log10(clamped);
    }
}
