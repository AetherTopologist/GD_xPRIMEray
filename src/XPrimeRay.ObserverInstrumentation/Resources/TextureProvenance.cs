namespace XPrimeRay.ObserverInstrumentation.Resources;

// Describes the color space of the raw decoded bytes stored in a TextureSnapshot.
public enum ColorSpace
{
    Unknown = 0,
    Linear  = 1,
    Srgb    = 2,
}

public sealed class TextureProvenance
{
    public TextureAssetId     AssetId           { get; init; }
    public string             SourceDescription { get; init; } = string.Empty;
    public DateTimeOffset     LoadedAt          { get; init; }
    public int                Width             { get; init; }
    public int                Height            { get; init; }
    public TexturePixelFormat Format            { get; init; }
    // ColorSpace describes what the stored bytes represent (not what TryGetPixel returns).
    public ColorSpace         ColorSpace        { get; init; }

    public bool IsValid =>
        AssetId.IsValid &&
        Width  > 0 &&
        Height > 0 &&
        Format     != TexturePixelFormat.Unknown &&
        ColorSpace != ColorSpace.Unknown;
}
