namespace XPrimeRay.Core.Fixtures;

public sealed record FixtureDefinition
{
    public string Name { get; init; } = "";
    public string? ComparisonIdentity { get; init; }
    public string Description { get; init; } = "";
    public RayGridDefinition RayGrid { get; init; } = new();
    public TransportDefinition Transport { get; init; } = new();
    public ObserverDefinition? Observer { get; init; }
    public List<FieldDefinition> Fields { get; init; } = new();
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
    public float StepSize { get; init; }
}

public sealed record ObserverDefinition
{
    public float[] Origin { get; init; } = Array.Empty<float>();
    public float[] Forward { get; init; } = Array.Empty<float>();
    public float[] Up { get; init; } = Array.Empty<float>();
    public float FovDegrees { get; init; }
}

public sealed record FieldDefinition
{
    public string Type { get; init; } = "";
    public float[] Center { get; init; } = Array.Empty<float>();
    public float RadiusOuter { get; init; }
    public float Amplitude { get; init; }
    public string CurveType { get; init; } = "";
    public float Gamma { get; init; } = 1f;
}

public sealed record ValidationDefinition
{
    public bool RequireHermeticClosure { get; init; }
    public int MinFieldSamples { get; init; }
    public float MinMeanBend { get; init; }
    public int MaxMisses { get; init; }
}
