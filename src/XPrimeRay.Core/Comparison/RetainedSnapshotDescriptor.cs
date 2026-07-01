using XPrimeRay.Core.Transport;

namespace XPrimeRay.Core.Comparison;

public sealed record RetainedSnapshotDescriptor
{
    public string RunId { get; init; } = "";
    public string ManifestPath { get; init; } = "";
    public string SnapshotPath { get; init; } = "";
    public string FixtureId { get; init; } = "";
    public string FixtureComparisonIdentity { get; init; } = "";
    public string FixturePath { get; init; } = "";
    public string ObserverBasis { get; init; } = "";
    public string ChannelId { get; init; } = "unknown";
    public string Representation { get; init; } = "unknown";
    public IReadOnlyList<RayMetric> Samples { get; init; } = Array.Empty<RayMetric>();
}
