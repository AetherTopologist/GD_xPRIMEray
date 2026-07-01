namespace XPrimeRay.Core.Comparison;

public static class RetainedSnapshotComparison
{
    public static DifferencePacket Build(
        RetainedSnapshotDescriptor left,
        RetainedSnapshotDescriptor right,
        string requestedChannel,
        DateTimeOffset generatedUtc)
    {
        return DifferencePacketBuilder.BuildRetainedComparison(
            left,
            right,
            requestedChannel,
            generatedUtc);
    }
}
