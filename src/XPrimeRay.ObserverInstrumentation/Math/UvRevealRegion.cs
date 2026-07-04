namespace XPrimeRay.ObserverInstrumentation.Math;

public readonly struct UvRevealRegion
{
    private UvRevealRegion(float uMin, float uMax, float vMin, float vMax)
    {
        _isValid = true;
        UMin = uMin;
        UMax = uMax;
        VMin = vMin;
        VMax = vMax;
    }

    public float UMin { get; }
    public float UMax { get; }
    public float VMin { get; }
    public float VMax { get; }
    public bool IsValid => _isValid;
    public bool CrossesUSeam => UMin > UMax;

    private readonly bool _isValid;

    public static bool TryCreate(
        float uMin,
        float uMax,
        float vMin,
        float vMax,
        out UvRevealRegion region)
    {
        region = default;
        if (!AreFinite(uMin, uMax, vMin, vMax) ||
            !IsUnitInterval(uMin) ||
            !IsUnitInterval(uMax) ||
            !IsUnitInterval(vMin) ||
            !IsUnitInterval(vMax) ||
            uMin == uMax ||
            vMin >= vMax)
        {
            return false;
        }

        region = new UvRevealRegion(uMin, uMax, vMin, vMax);
        return true;
    }

    public bool Contains(float u, float v)
    {
        if (!_isValid || !float.IsFinite(u) || !float.IsFinite(v) || v < 0f || v > 1f)
        {
            return false;
        }

        float wrappedU = u == 1f ? 1f : SphericalUvMath.Wrap01(u);
        bool withinU = CrossesUSeam
            ? wrappedU >= UMin || wrappedU <= UMax
            : wrappedU >= UMin && wrappedU <= UMax;
        return withinU && v >= VMin && v <= VMax;
    }

    private static bool AreFinite(float a, float b, float c, float d) =>
        float.IsFinite(a) && float.IsFinite(b) && float.IsFinite(c) && float.IsFinite(d);

    private static bool IsUnitInterval(float value) => value >= 0f && value <= 1f;
}
