namespace XPrimeRay.Core.Comparison;

public sealed record DifferenceSummary
{
    public int CountCompared { get; init; }
    public double MaxAbsDifference { get; init; }
    public double MeanAbsDifference { get; init; }
    public int NonZeroCount { get; init; }
}
