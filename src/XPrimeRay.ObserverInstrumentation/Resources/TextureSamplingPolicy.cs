namespace XPrimeRay.ObserverInstrumentation.Resources;

public enum TextureWrapMode
{
    Clamp          = 0,
    Repeat         = 1,
    MirroredRepeat = 2,
}

// Only Nearest is supported. Any other value passed to TryGetPixel returns false (fail closed).
public enum TextureFilterMode
{
    Nearest = 0,
}

// Describes what TryGetPixel does with the decoded byte values.
// Only Raw is supported initially; other values fail closed.
public enum ColorInterpretation
{
    Raw         = 0,  // byte / 255f — no color-space conversion
    Linear      = 1,  // unsupported; TryGetPixel returns false
    SrgbToLinear = 2, // unsupported; TryGetPixel returns false
}

public readonly record struct TextureSamplingPolicy(
    TextureWrapMode      WrapMode,
    TextureFilterMode    FilterMode,
    ColorInterpretation  ColorInterpretation)
{
    public static readonly TextureSamplingPolicy NearestClamp = new(
        TextureWrapMode.Clamp,  TextureFilterMode.Nearest, ColorInterpretation.Raw);
    public static readonly TextureSamplingPolicy NearestRepeat = new(
        TextureWrapMode.Repeat, TextureFilterMode.Nearest, ColorInterpretation.Raw);
}
