namespace XPrimeRay.ObservationLayer;

public interface ISealedObservationChannel
{
    ObservationChannelDescriptor Descriptor { get; }
    int Generation { get; }
    int Count { get; }
    ObservationDataType DataType { get; }
}
