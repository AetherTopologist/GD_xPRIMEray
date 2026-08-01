namespace XPrimeRay.ObservationLayer;

public sealed class SealedObservationChannel<T> : ISealedObservationChannel
{
    private readonly T[] _values;

    public SealedObservationChannel(
        ObservationChannelDescriptor descriptor,
        int generation,
        ReadOnlySpan<T> values)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (generation < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(generation));
        }

        ObservationDataType expected = ObservationTypeMap.GetDataType(typeof(T), descriptor.DataType);
        if (expected != descriptor.DataType)
        {
            throw new ArgumentException("Channel descriptor data type does not match the channel value type.", nameof(descriptor));
        }

        Descriptor = descriptor;
        Generation = generation;
        _values = values.ToArray();
    }

    public ObservationChannelDescriptor Descriptor { get; }
    public int Generation { get; }
    public int Count => _values.Length;
    public ObservationDataType DataType => Descriptor.DataType;
    public ReadOnlyMemory<T> Values => _values;
    public ReadOnlySpan<T> Span => _values;
}

internal static class ObservationTypeMap
{
    public static ObservationDataType GetDataType(Type type, ObservationDataType descriptorType = ObservationDataType.Unknown)
    {
        if (type == typeof(byte))
        {
            return descriptorType == ObservationDataType.ProbeOutcomeCode
                ? ObservationDataType.ProbeOutcomeCode
                : ObservationDataType.UInt8;
        }
        if (type == typeof(ushort)) return ObservationDataType.UInt16;
        if (type == typeof(int)) return ObservationDataType.Int32;
        if (type == typeof(float)) return ObservationDataType.Float32;
        if (type == typeof(Float2)) return ObservationDataType.Float2;
        if (type == typeof(Float3)) return ObservationDataType.Float3;
        if (type == typeof(bool)) return ObservationDataType.Boolean;
        if (type == typeof(PortableInstrumentObservationRecord)) return ObservationDataType.InstrumentObservationRecord;
        if (type == typeof(TransportEventRecord)) return ObservationDataType.TransportEventRecord;
        return ObservationDataType.Unknown;
    }
}
