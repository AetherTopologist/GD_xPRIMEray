namespace XPrimeRay.Core.Comparison;

public sealed record ChannelCompatibility
{
    public string RuleId { get; init; } = "missing_channel_declaration";
    public DifferenceStatus Status { get; init; } = DifferenceStatus.Unknown;
    public bool TransformRequired { get; init; }
    public string Reason { get; init; } = "Channel compatibility is not declared.";
    public string[] RequiredConditions { get; init; } = Array.Empty<string>();
}
