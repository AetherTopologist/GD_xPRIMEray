namespace XPrimeRay.ObserverInstrumentation.Abstractions;

public readonly record struct FilmFrameIdentity(
    int    FilmWidth,
    int    FilmHeight,
    ulong  FrameSequence,
    string SceneTag)
{
    public bool IsValid => FilmWidth > 0 && FilmHeight > 0;
    public static readonly FilmFrameIdentity None = new(0, 0, 0, string.Empty);
}
