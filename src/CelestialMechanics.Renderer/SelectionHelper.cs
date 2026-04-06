using System.Numerics;

namespace CelestialMechanics.Renderer;

/// <summary>
/// Provides ray-sphere intersection tests for body selection via mouse picking.
/// </summary>
public static class SelectionHelper
{
    /// <summary>
    /// Casts a ray from a screen position through the camera and tests intersection
    /// with all active bodies. Returns the index (into the RenderBody array) of the
    /// nearest hit body, or -1 if nothing was hit.
    /// </summary>
    public static int Raycast(
        float screenX, float screenY,
        float viewportWidth, float viewportHeight,
        Camera camera, RenderBody[] bodies, int bodyCount)
    {
        if (bodyCount == 0 || viewportWidth < 1 || viewportHeight < 1)
            return -1;

        float aspect = viewportWidth / viewportHeight;
        var view = camera.GetViewMatrix();
        var projection = camera.GetProjectionMatrix(aspect);

        // Build inverse view-projection
        if (!Matrix4x4.Invert(view * projection, out var invVP))
            return -1;

        // Normalized device coordinates
        float ndcX = 2f * screenX / viewportWidth - 1f;
        float ndcY = 1f - 2f * screenY / viewportHeight;

        // Unproject near and far points
        var nearPoint = Vector4.Transform(new Vector4(ndcX, ndcY, -1f, 1f), invVP);
        var farPoint = Vector4.Transform(new Vector4(ndcX, ndcY, 1f, 1f), invVP);

        if (nearPoint.W == 0 || farPoint.W == 0)
            return -1;

        var rayOrigin = new Vector3(nearPoint.X, nearPoint.Y, nearPoint.Z) / nearPoint.W;
        var rayFar = new Vector3(farPoint.X, farPoint.Y, farPoint.Z) / farPoint.W;
        var rayDir = Vector3.Normalize(rayFar - rayOrigin);

        int bestIndex = -1;
        float bestDist = float.MaxValue;

        for (int i = 0; i < bodyCount; i++)
        {
            ref var body = ref bodies[i];

            // Expand the hit radius slightly for easier selection of small bodies
            float hitRadius = MathF.Max(body.Radius * 1.2f, 0.05f);

            float t = RaySphereIntersect(rayOrigin, rayDir, body.Position, hitRadius);
            if (t >= 0 && t < bestDist)
            {
                bestDist = t;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    /// <summary>
    /// Returns distance along ray to the nearest intersection with a sphere, or -1 if no hit.
    /// </summary>
    private static float RaySphereIntersect(Vector3 origin, Vector3 dir, Vector3 center, float radius)
    {
        var oc = origin - center;
        float a = Vector3.Dot(dir, dir);
        float b = 2f * Vector3.Dot(oc, dir);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4f * a * c;

        if (discriminant < 0)
            return -1f;

        float sqrtD = MathF.Sqrt(discriminant);
        float t0 = (-b - sqrtD) / (2f * a);
        float t1 = (-b + sqrtD) / (2f * a);

        if (t0 >= 0) return t0;
        if (t1 >= 0) return t1;
        return -1f;
    }
}
