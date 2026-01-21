using System.Numerics;

namespace Lithium.Core.Extensions;

public static class MathExtensions
{
    /// <summary>
    /// Clamps the value between a minimum and maximum.
    /// </summary>
    public static T Clamp<T>(this T value, T min, T max) where T : INumber<T>
    {
        return T.Clamp(value, min, max);
    }

    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    public static float Lerp(this float start, float end, float t)
    {
        return start + (end - start) * float.Clamp(t, 0f, 1f);
    }

    /// <summary>
    /// Linearly interpolates between two values without clamping t.
    /// </summary>
    public static float LerpUnclamped(this float start, float end, float t)
    {
        return start + (end - start) * t;
    }

    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    public static double Lerp(this double start, double end, double t)
    {
        return start + (end - start) * double.Clamp(t, 0.0, 1.0);
    }

    /// <summary>
    /// Linearly interpolates between two values without clamping t.
    /// </summary>
    public static double LerpUnclamped(this double start, double end, double t)
    {
        return start + (end - start) * t;
    }

    /// <summary>
    /// Calculates the percentage of the value relative to the total.
    /// </summary>
    public static double PercentOf<T>(this T value, T total) where T : INumber<T>
    {
        if (total == T.Zero) return 0.0;
        return double.CreateChecked(value) * 100.0 / double.CreateChecked(total);
    }
    
    /// <summary>
    /// Remaps a value from one range to another.
    /// </summary>
    public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        var t = (value - fromMin) / (fromMax - fromMin);
        return toMin + t * (toMax - toMin);
    }

    /// <summary>
    /// Wraps an angle in radians to the range [-PI, PI].
    /// </summary>
    public static float WrapAngle(this float angle)
    {
        angle %= MathF.Tau;
        if (angle <= -MathF.PI)
        {
            angle += MathF.Tau;
        }
        else if (angle > MathF.PI)
        {
            angle -= MathF.Tau;
        }
        return angle;
    }
    
    /// <summary>
    /// Calculates the shortest difference between two angles in radians.
    /// </summary>
    public static float ShortAngleDistance(this float from, float to)
    {
        var distance = (to - from) % MathF.Tau;
        return (2.0f * distance) % MathF.Tau - distance;
    }
    
    /// <summary>
    /// Linearly interpolates between two angles in radians, taking the shortest path.
    /// </summary>
    public static float LerpAngle(this float from, float to, float t)
    {
        return from + from.ShortAngleDistance(to) * t;
    }
}
