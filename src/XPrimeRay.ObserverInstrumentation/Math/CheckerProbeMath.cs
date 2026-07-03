namespace XPrimeRay.ObserverInstrumentation.Math;

public static class CheckerProbeMath
{
    public static bool TryIsDark(
        float u,
        float v,
        int tilesU,
        int tilesV,
        out bool isDark)
    {
        isDark = false;
        if (!float.IsFinite(u) || !float.IsFinite(v) || tilesU <= 0 || tilesV <= 0)
        {
            return false;
        }

        float wrappedU = SphericalUvMath.Wrap01(u);
        float clampedV = System.Math.Clamp(v, 0f, 1f);
        if (clampedV >= 1f)
        {
            clampedV = float.BitDecrement(1f);
        }

        int tileU = System.Math.Min((int)MathF.Floor(wrappedU * tilesU), tilesU - 1);
        int tileV = System.Math.Min((int)MathF.Floor(clampedV * tilesV), tilesV - 1);
        isDark = ((tileU + tileV) & 1) != 0;
        return true;
    }
}
