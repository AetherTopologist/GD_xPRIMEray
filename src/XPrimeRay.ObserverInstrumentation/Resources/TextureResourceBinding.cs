namespace XPrimeRay.ObserverInstrumentation.Resources;

public sealed class TextureResourceBinding
{
    public TextureAssetId        AssetId        { get; init; }
    // SlotId identifies which material slot this binding targets (e.g. "base_color").
    // Allows multiple bindings per collider — one per slot.
    public TextureSlotId         SlotId         { get; init; }
    public TextureSamplingPolicy SamplingPolicy { get; init; }
    // Only UvChannel 0 is supported. Any other value causes IsValid to return false (fail closed).
    public int                   UvChannel      { get; init; }

    public bool IsValid => AssetId.IsValid && SlotId.IsValid && UvChannel == 0;
}
