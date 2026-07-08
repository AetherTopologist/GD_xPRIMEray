namespace XPrimeRay.ObserverInstrumentation.Resources;

public readonly record struct TextureAssetId(string Tag)
{
    public bool IsValid => !string.IsNullOrEmpty(Tag);
    public static readonly TextureAssetId None = new(string.Empty);
}
