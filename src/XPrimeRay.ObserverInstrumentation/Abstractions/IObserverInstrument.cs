using XPrimeRay.ObserverInstrumentation.Metadata;

namespace XPrimeRay.ObserverInstrumentation.Abstractions;

public interface IObserverInstrument
{
    InstrumentKind Kind { get; }
    ObserverInstrumentMask Feature { get; }

    void Observe(
        in InstrumentContext context,
        bool catalogAvailable,
        bool metadataFound,
        in InstrumentTargetMetadata metadata,
        out InstrumentObservation observation);

    void ObserveWithExtensions(
        in InstrumentContext context,
        bool catalogAvailable,
        bool metadataFound,
        in InstrumentTargetMetadata metadata,
        out InstrumentObservation observation,
        out TextureSampleObservation textureObservation,
        out bool hasTextureObservation)
    {
        Observe(in context, catalogAvailable, metadataFound, in metadata, out observation);
        textureObservation = default;
        hasTextureObservation = false;
    }
}
