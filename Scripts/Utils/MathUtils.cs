namespace Mut8.Scripts.Utils;

public static class MathUtils
{
    public const float TOLERANCE = 0.00001f;
    
    /// <summary>
    /// Checks if two float values are equal within a specified tolerance.
    /// </summary>
    /// <param name="value">The first float value.</param>
    /// <param name="compareTo">The second float value to compare against.</param>
    /// <param name="tolerance">The tolerance within which the two values are considered equal.</param>
    /// <returns>True if the values are equal within the specified tolerance; otherwise, false.</returns>
    public static bool IsEqualWithTolerance(this float value, float compareTo, float tolerance = TOLERANCE)
    {
        return Math.Abs(value - compareTo) <= tolerance;
    }
    
    /// <summary>
    /// Linearly interpolates between two values without clamping the result.
    /// </summary>
    /// <param name="a">Start value (at t=0)</param>
    /// <param name="b">End value (at t=1)</param>
    /// <param name="t">Interpolation factor (can be outside 0-1 range)</param>
    /// <returns>Interpolated value</returns>
    public static float LerpUnclamped(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}