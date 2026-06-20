namespace XPrimeRay.Core.Fixtures;

public sealed record FixtureDefinition
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public RayGridDefinition RayGrid { get; init; } = new();
    public TransportDefinition Transport { get; init; } = new();
    public ValidationDefinition Validation { get; init; } = new();
}

public sealed record RayGridDefinition
{
    public int Width { get; init; }
    public int Height { get; init; }
}

public sealed record TransportDefinition
{
    public string Mode { get; init; } = "";
    public int MaxStepsPerRay { get; init; }
}

public sealed record ValidationDefinition
{
    public bool RequireHermeticClosure { get; init; }
    public int MaxMisses { get; init; }
}
