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
    public int StepsPerRay { get; init; }
    public int FieldSampleCount { get; init; }
    public float MeanBendMagnitude { get; init; }
    public float MaxBendMagnitude { get; init; }
    public bool HasInvalidValues { get; init; }
    public string Note { get; init; } = "";
    public IReadOnlyList<RayMetric> RayMetrics { get; init; } = Array.Empty<RayMetric>();

    public static TransportResult FromArtifactFixture(FixtureDefinition fixture, string note)
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
            StepsPerRay = fixture.Transport.MaxStepsPerRay,
            Note = note,
            RayMetrics = Array.Empty<RayMetric>(),
        };
    }
}
