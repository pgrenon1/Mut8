namespace Mut8.Scripts.Utils;

public static class MathUtils
{
    public const float TOLERANCE = 0.00001f;
    
    public static bool IsEqualWithTolerance(this float value, float compareTo, float tolerance = TOLERANCE)
    {
        return Math.Abs(value - compareTo) <= tolerance;
    }
}