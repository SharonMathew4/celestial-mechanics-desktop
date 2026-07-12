using System;
using CelestialMechanics.Math;

namespace CelestialMechanics.Observation.Scene;

/// <summary>
/// Representation of a 3D ray for object selection and intersection picking.
/// </summary>
public readonly struct Ray
{
    public Vec3d Origin { get; }
    public Vec3d Direction { get; }

    public Ray(Vec3d origin, Vec3d direction)
    {
        Origin = origin;
        Direction = direction.Normalized();
    }

    /// <summary>
    /// Checks intersection of this ray with a sphere.
    /// </summary>
    /// <param name="center">Sphere center.</param>
    /// <param name="radius">Sphere radius.</param>
    /// <param name="distance">Distance along ray to intersection point.</param>
    /// <returns>True if the ray intersects the sphere; otherwise false.</returns>
    public bool IntersectsSphere(Vec3d center, double radius, out double distance)
    {
        distance = 0.0;
        Vec3d m = Origin - center;
        double b = Vec3d.Dot(m, Direction);
        double c = Vec3d.Dot(m, m) - radius * radius;

        // Exit if ray origin is outside sphere and ray points away from sphere
        if (c > 0.0 && b > 0.0)
            return false;

        double discriminant = b * b - c;

        // A negative discriminant means the ray misses the sphere
        if (discriminant < 0.0)
            return false;

        // Ray intersects sphere, find closest hit point
        distance = -b - System.Math.Sqrt(discriminant);

        // If distance is negative, ray started inside the sphere
        if (distance < 0.0)
            distance = 0.0;

        return true;
    }
}

/// <summary>
/// Service to cast picking rays against the Scene Graph to determine selected nodes.
/// </summary>
public sealed class ScenePicker
{
    private readonly SceneManager _sceneManager;

    public ScenePicker(SceneManager sceneManager)
    {
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
    }

    /// <summary>
    /// Casts a ray and returns the closest intersecting SceneNode.
    /// </summary>
    public ISceneNode? PickNode(Ray ray, double boundingSphereRadius = 5.0)
    {
        ISceneNode? closestNode = null;
        double minDistance = double.MaxValue;

        // Traverse scene tree to find closest intersection
        TraverseAndPick(_sceneManager.Root, ray, boundingSphereRadius, ref closestNode, ref minDistance);

        return closestNode;
    }

    private void TraverseAndPick(
        ISceneNode current, 
        Ray ray, 
        double radius, 
        ref ISceneNode? closestNode, 
        ref double minDistance)
    {
        // Don't pick root
        if (current != _sceneManager.Root)
        {
            var pos = current.Transform.Position;

            if (ray.IntersectsSphere(pos, radius, out double distance))
            {
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestNode = current;
                }
            }
        }

        foreach (var child in current.Children)
        {
            TraverseAndPick(child, ray, radius, ref closestNode, ref minDistance);
        }
    }
}
