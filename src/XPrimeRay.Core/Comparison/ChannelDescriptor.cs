namespace XPrimeRay.Core.Comparison;

public sealed record ChannelDescriptor
{
    public string Id { get; init; } = "";
    public string Producer { get; init; } = "core";
    public string Kind { get; init; } = "measurement";
    public string DataType { get; init; } = "unknown";
    public string Units { get; init; } = "unknown";
    public string Representation { get; init; } = "unknown";
    public string Description { get; init; } = "";
}

public readonly record struct ChannelComparisonContext(
    bool SameObserverBasis,
    bool SameFixtureIdentity,
    bool SameCoordinateGrid);
