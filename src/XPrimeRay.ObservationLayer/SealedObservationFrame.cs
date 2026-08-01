namespace XPrimeRay.ObservationLayer;

public sealed class SealedObservationFrame : ITransportObservationSource
{
    private readonly Dictionary<ObservationChannelId, ISealedObservationChannel> _channels;
    private readonly ObservationChannelDescriptor[] _descriptors;

    public SealedObservationFrame(
        ObservationFrameDescriptor descriptor,
        IEnumerable<ISealedObservationChannel> channels)
    {
        ArgumentNullException.ThrowIfNull(channels);
        Descriptor = descriptor;
        var dict = new Dictionary<ObservationChannelId, ISealedObservationChannel>();
        foreach (ISealedObservationChannel channel in channels)
        {
            ArgumentNullException.ThrowIfNull(channel);
            // Invariant: channel generations are equal to the owning sealed frame generation.
            // Future zero-copy leased frames may use the same invariant to detect stale views.
            if (channel.Generation != descriptor.Generation)
            {
                throw new ArgumentException("Channel.Generation must equal Frame.Generation.");
            }

            if (!descriptor.IsComplete && channel.Descriptor.Domain == ObservationDomain.DensePixelPlane)
            {
                throw new ArgumentException("Dense semantic evidence channels require a complete sealed frame.");
            }

            if (!dict.TryAdd(channel.Descriptor.Id, channel))
            {
                throw new ArgumentException($"Duplicate observation channel id '{channel.Descriptor.Id}'.");
            }
        }

        _channels = dict;
        _descriptors = dict.Values.Select(channel => channel.Descriptor).ToArray();
    }

    public ObservationFrameDescriptor Descriptor { get; }
    public ObservationFrameDescriptor FrameDescriptor => Descriptor;
    public bool IsComplete => Descriptor.IsComplete;
    public int Generation => Descriptor.Generation;
    public IReadOnlyList<ObservationChannelDescriptor> Channels => _descriptors;

    public IReadOnlyList<ObservationChannelDescriptor> EnumerateChannels() => _descriptors;

    public bool TryGetChannel<T>(
        ObservationChannelId channelId,
        out SealedObservationChannel<T>? channel,
        out ObservationValidity validity)
    {
        channel = null;
        if (!channelId.IsValid)
        {
            validity = ObservationValidity.MissingChannel;
            return false;
        }

        if (!_channels.TryGetValue(channelId, out ISealedObservationChannel? stored))
        {
            validity = ObservationValidity.MissingChannel;
            return false;
        }

        if (stored is not SealedObservationChannel<T> typed)
        {
            validity = ObservationValidity.TypeMismatch;
            return false;
        }

        if (!Descriptor.IsComplete && stored.Descriptor.Domain == ObservationDomain.DensePixelPlane)
        {
            validity = ObservationValidity.RequiresCompleteFrame;
            return false;
        }

        channel = typed;
        validity = ObservationValidity.Valid;
        return true;
    }
}
