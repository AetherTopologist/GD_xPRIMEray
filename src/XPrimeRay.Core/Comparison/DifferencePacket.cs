using System.Text.Json.Serialization;

namespace XPrimeRay.Core.Comparison;

public sealed record DifferencePacket
{
    public string Schema { get; init; } = "xprimeray.glowing_heart.difference_packet.v2.0";
    public string GeneratedUtc { get; init; } = "";
    public bool RuntimeExecuted { get; init; }
    public string ParityClaim { get; init; } = "NONE";
    public string ComparisonMode { get; init; } = "core_vs_core";
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ComparisonScope { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LeftRunId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RightRunId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LeftManifestPath { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RightManifestPath { get; init; }
    public DifferenceSnapshot LeftSnapshot { get; init; } = new();
    public DifferenceSnapshot RightSnapshot { get; init; } = new();
    public string LeftChannel { get; init; } = "unknown";
    public string RightChannel { get; init; } = "unknown";
    public string LeftRepresentation { get; init; } = "unknown";
    public string RightRepresentation { get; init; } = "unknown";
    public string ObserverBasis { get; init; } = "unknown";
    public string ComparisonBasis { get; init; } = "unknown";
    public string? CompatibilityRuleId { get; init; }
    public string? ChannelRegistryVersion { get; init; }
    public string? CompatibilityMatrixVersion { get; init; }
    public DifferenceStatus Status { get; init; } = DifferenceStatus.Unknown;
    public bool Comparable { get; init; }
    public bool TransformRequired { get; init; }
    public string Reason { get; init; } = "";
    public DifferenceSummary Summary { get; init; } = new();
    public string[] ClaimBoundary { get; init; } = Array.Empty<string>();

    public void Validate()
    {
        if (ParityClaim != "NONE")
        {
            throw new InvalidDataException("Difference packets must preserve parityClaim=NONE.");
        }

        if (RuntimeExecuted)
        {
            throw new InvalidDataException("Difference Packet v2.0 requires runtimeExecuted=false.");
        }

        if (ComparisonMode != "core_vs_core")
        {
            throw new InvalidDataException("Difference Packet v2.0 supports core_vs_core only.");
        }

        if (Status == DifferenceStatus.Comparable && !Comparable)
        {
            throw new InvalidDataException("Comparable status requires comparable=true.");
        }

        if (Status != DifferenceStatus.Comparable && Comparable)
        {
            throw new InvalidDataException("Only Comparable status may set comparable=true.");
        }

        if (Status == DifferenceStatus.RequiresTransform && !TransformRequired)
        {
            throw new InvalidDataException("RequiresTransform status requires transformRequired=true.");
        }

        if (Summary.CountCompared < 0 || Summary.NonZeroCount < 0)
        {
            throw new InvalidDataException("Difference counts cannot be negative.");
        }

        if (Summary.NonZeroCount > Summary.CountCompared)
        {
            throw new InvalidDataException("nonZeroCount cannot exceed countCompared.");
        }

        if (ClaimBoundary.Length == 0 || ClaimBoundary.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidDataException("Difference packets require non-empty claim boundary statements.");
        }
    }
}

public sealed record DifferenceSnapshot
{
    public string Id { get; init; } = "";
    public string FixtureId { get; init; } = "";
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FixtureComparisonIdentity { get; init; }
    public string FixturePath { get; init; } = "";
    public string ArtifactPath { get; init; } = "";
}
