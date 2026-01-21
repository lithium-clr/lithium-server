using System.Numerics;

namespace Lithium.Core.Extensions;

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
        float radians = float.DegreesToRadians(angleDegrees);
        if (clockwise)
        {
            
        }
        else 
        {
            radians = -radians;
        }

        // Apply rotation matrix for Y axis (rotating X and Z)

        
        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);

        // Note: The Java code seems to treat "clockwise" as the positive rotation in its formula
        
        float x1, z1;
        if (clockwise)
        {
            x1 = vector.X * cos - vector.Z * sin;
            z1 = vector.X * sin + vector.Z * cos;
        }
        else
        {
            x1 = vector.X * cos + vector.Z * sin;
            z1 = -vector.X * sin + vector.Z * cos;
        }

        return new Vector3(x1, vector.Y, z1);
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
