using System.Numerics;

namespace Lithium.Server.Core.Extensions;

public static class VectorExtensions
{
    /// <summary>
    /// Rotates a vector around the Y axis.
    /// </summary>
    /// <param name="vector">The vector to rotate.</param>
    /// <param name="angleDegrees">The angle in degrees.</param>
    /// <param name="clockwise">True to rotate clockwise, false for counter-clockwise.</param>
    /// <returns>The rotated vector.</returns>
    public static Vector3 RotateY(this Vector3 vector, float angleDegrees, bool clockwise = true)
    {
        var radians = float.DegreesToRadians(angleDegrees);
        if (!clockwise)
        {
            radians = -radians;
        }

        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);

        var x = vector.X * cos - vector.Z * sin;
        var z = vector.X * sin + vector.Z * cos;

        return new Vector3(x, vector.Y, z);
    }

    /// <summary>
    /// Returns the squared distance to another vector (ignoring Y axis).
    /// </summary>
    public static float DistanceSquaredHorizontal(this Vector3 a, Vector3 b)
    {
        float dx = a.X - b.X;
        float dz = a.Z - b.Z;
        return dx * dx + dz * dz;
    }

    /// <summary>
    /// Returns the distance to another vector (ignoring Y axis).
    /// </summary>
    public static float DistanceHorizontal(this Vector3 a, Vector3 b)
    {
        return MathF.Sqrt(a.DistanceSquaredHorizontal(b));
    }
}
