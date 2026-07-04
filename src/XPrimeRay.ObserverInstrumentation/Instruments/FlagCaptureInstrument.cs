using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Math;
using XPrimeRay.ObserverInstrumentation.Metadata;

namespace XPrimeRay.ObserverInstrumentation.Instruments;

public sealed class FlagCaptureInstrument : IObserverInstrument
{
    public InstrumentKind Kind => InstrumentKind.FlagCapture;
    public ObserverInstrumentMask Feature => ObserverInstrumentMask.FlagCapture;

    public void Observe(
        in InstrumentContext context,
        bool catalogAvailable,
        bool metadataFound,
        in InstrumentTargetMetadata metadata,
        out InstrumentObservation observation)
    {
        if (!InstrumentObservationFactory.TryClassifyInput(
                Kind, in context, catalogAvailable, metadataFound, out observation))
        {
            return;
        }

        if (!metadata.IsValid ||
            !SphericalUvMath.TryFromNormal(context.HitPosition - metadata.Center, out var uv))
        {
            observation = InstrumentObservationFactory.Create(
                Kind, in context, InstrumentDiagnosticState.DiagnosticUnresolved);
            return;
        }

        bool revealed = metadata.RevealRegion.Contains(uv.X, uv.Y);
        observation = InstrumentObservationFactory.Create(
            Kind,
            in context,
            revealed ? InstrumentDiagnosticState.RegionSampled : InstrumentDiagnosticState.ProbeHitOutsideRegion,
            uv,
            hasUv: true,
            sampleValue: revealed);
    }
}
