namespace XPrimeRay.ObservationLayer;

public interface ITransportObservationSource
{
    ObservationFrameDescriptor FrameDescriptor { get; }
    bool IsComplete { get; }
    int Generation { get; }

    IReadOnlyList<ObservationChannelDescriptor> EnumerateChannels();

    bool TryGetChannel<T>(
        ObservationChannelId channelId,
        out SealedObservationChannel<T>? channel,
        out ObservationValidity validity);
}
