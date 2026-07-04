using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Math;
using XPrimeRay.ObserverInstrumentation.Metadata;

namespace XPrimeRay.ObserverInstrumentation.Instruments;

public sealed class CheckerProbeInstrument : IObserverInstrument
{
    public InstrumentKind Kind => InstrumentKind.CheckerProbe;
    public ObserverInstrumentMask Feature => ObserverInstrumentMask.CheckerProbe;

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
            !SphericalUvMath.TryFromNormal(context.HitPosition - metadata.Center, out var uv) ||
            !CheckerProbeMath.TryIsDark(
                uv.X, uv.Y, metadata.CheckerTilesU, metadata.CheckerTilesV, out bool isDark))
        {
            observation = InstrumentObservationFactory.Create(
                Kind, in context, InstrumentDiagnosticState.DiagnosticUnresolved);
            return;
        }

        observation = InstrumentObservationFactory.Create(
            Kind, in context, InstrumentDiagnosticState.RegionSampled, uv, hasUv: true, sampleValue: isDark);
    }
}
