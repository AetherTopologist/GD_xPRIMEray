using System.Numerics;

namespace XPrimeRay.ObserverInstrumentation.Math;

public static class SphericalUvMath
{
    private const float MinimumLengthSquared = 1e-12f;
    private const float PoleRadiusSquared = 1e-12f;

    public static bool TryFromNormal(Vector3 normal, out Vector2 uv)
    {
        uv = default;
        if (!IsFinite(normal))
        {
            return false;
        }

        float lengthSquared = normal.LengthSquared();
        if (!float.IsFinite(lengthSquared) || lengthSquared <= MinimumLengthSquared)
        {
            return false;
        }

        Vector3 unit = normal / MathF.Sqrt(lengthSquared);
        float radialSquared = unit.X * unit.X + unit.Z * unit.Z;
        float u = radialSquared <= PoleRadiusSquared
            ? 0.5f
            : Wrap01(0.5f + MathF.Atan2(unit.Z, unit.X) / MathF.Tau);
        float v = 0.5f - MathF.Asin(System.Math.Clamp(unit.Y, -1f, 1f)) / MathF.PI;

        if (!float.IsFinite(u) || !float.IsFinite(v))
        {
            return false;
        }

        uv = new Vector2(u, System.Math.Clamp(v, 0f, 1f));
        return true;
    }

    public static float Wrap01(float value)
    {
        if (!float.IsFinite(value))
        {
            return float.NaN;
        }

        float wrapped = value - MathF.Floor(value);
        return wrapped >= 1f ? 0f : wrapped;
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) &&
        float.IsFinite(value.Y) &&
        float.IsFinite(value.Z);
}
