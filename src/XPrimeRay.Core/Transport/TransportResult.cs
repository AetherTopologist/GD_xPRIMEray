using XPrimeRay.Core.Fixtures;

namespace XPrimeRay.Core.Transport;

public sealed record TransportResult
{
    public string FixtureName { get; init; } = "";
    public string Mode { get; init; } = "";
    public int Width { get; init; }
    public int Height { get; init; }
    public int Rays { get; init; }
    public int Hits { get; init; }
    public int Misses { get; init; }
    public int MaxStepsPerRay { get; init; }

    public static TransportResult FromFixture(FixtureDefinition fixture)
    {
        var rays = checked(fixture.RayGrid.Width * fixture.RayGrid.Height);
        return new TransportResult
        {
            FixtureName = fixture.Name,
            Mode = fixture.Transport.Mode,
            Width = fixture.RayGrid.Width,
            Height = fixture.RayGrid.Height,
            Rays = rays,
            Hits = 0,
            Misses = rays,
            MaxStepsPerRay = fixture.Transport.MaxStepsPerRay,
        };
    }
}
