namespace XPrimeRay.ObserverInstrumentation.Resources;

public readonly record struct SampledColor(float R, float G, float B, float A)
{
    public static readonly SampledColor Black       = new(0f, 0f, 0f, 1f);
    public static readonly SampledColor White       = new(1f, 1f, 1f, 1f);
    public static readonly SampledColor Transparent = new(0f, 0f, 0f, 0f);
}
