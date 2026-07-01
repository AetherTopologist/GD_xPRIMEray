namespace XPrimeRay.Core.Comparison;

public static class RetainedSnapshotComparison
{
    public static DifferencePacket Build(
        RetainedSnapshotDescriptor left,
        RetainedSnapshotDescriptor right,
        string leftRequestedChannel,
        string rightRequestedChannel,
        DateTimeOffset generatedUtc)
    {
        return DifferencePacketBuilder.BuildRetainedComparison(
            left,
            right,
            leftRequestedChannel,
            rightRequestedChannel,
            generatedUtc);
    }

    public static DifferencePacket Build(
        RetainedSnapshotDescriptor left,
        RetainedSnapshotDescriptor right,
        string requestedChannel,
        DateTimeOffset generatedUtc)
    {
        return Build(left, right, requestedChannel, requestedChannel, generatedUtc);
    }
}
