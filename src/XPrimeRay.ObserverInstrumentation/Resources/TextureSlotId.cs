namespace XPrimeRay.ObserverInstrumentation.Resources;

public readonly record struct TextureSlotId(string Name)
{
    public bool IsValid => !string.IsNullOrEmpty(Name);
    public static readonly TextureSlotId None      = new(string.Empty);
    public static readonly TextureSlotId BaseColor = new("base_color");
}
