using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Abstractions;

namespace XPrimeRay.ObserverInstrumentation.Instruments;

internal static class InstrumentObservationFactory
{
    public static bool TryClassifyInput(
        InstrumentKind instrument,
        in InstrumentContext context,
        bool catalogAvailable,
        bool metadataFound,
        out InstrumentObservation observation)
    {
        if (context.HitKind != InstrumentHitKind.SurfaceHit)
        {
            observation = Create(instrument, in context, InstrumentDiagnosticState.TransportClassNotSurfaceHit);
            return false;
        }

        if (!catalogAvailable || string.IsNullOrEmpty(context.ColliderName))
        {
            observation = Create(instrument, in context, InstrumentDiagnosticState.DiagnosticUnresolved);
            return false;
        }

        if (!metadataFound)
        {
            observation = Create(instrument, in context, InstrumentDiagnosticState.OtherGeometryHit);
            return false;
        }

        observation = default;
        return true;
    }

    public static InstrumentObservation Create(
        InstrumentKind instrument,
        in InstrumentContext context,
        InstrumentDiagnosticState state,
        Vector2 uv = default,
        bool hasUv = false,
        bool sampleValue = false) =>
        new(context.RayIndex, instrument, state, context.ColliderName, uv, hasUv, sampleValue);
}
