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
}
