namespace XPrimeRay.ObservationLayer;

public readonly struct ObservationChannelDependency
{
    public ObservationChannelDependency(ObservationChannelId channelId, string reason)
    {
        if (!channelId.IsValid)
        {
            throw new ArgumentException("Channel dependency requires a valid channel id.", nameof(channelId));
        }

        ChannelId = channelId;
        Reason = string.IsNullOrWhiteSpace(reason) ? "unspecified" : reason.Trim();
    }

    public ObservationChannelId ChannelId { get; }
    public string Reason { get; }
}
