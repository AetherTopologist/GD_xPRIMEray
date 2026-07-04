using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Instruments;
using XPrimeRay.ObserverInstrumentation.Metadata;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class InstrumentTests
{
    public static void Run()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        InstrumentContext sampled = Stage1BTestData.Surface();

        var surface = new SurfaceUvInstrument();
        surface.Observe(in sampled, true, true, in metadata, out InstrumentObservation uv);
        AssertState(InstrumentDiagnosticState.RegionSampled, uv, "surface UV sampled");
        TestAssert.True(uv.HasUv, "surface observation includes UV");
        TestAssert.NearlyEqual(0.5f, uv.Uv.X, 1e-6f, "surface U");

        var checker = new CheckerProbeInstrument();
        checker.Observe(in sampled, true, true, in metadata, out InstrumentObservation checkedProbe);
        AssertState(InstrumentDiagnosticState.RegionSampled, checkedProbe, "checker sampled");

        var flag = new FlagCaptureInstrument();
        flag.Observe(in sampled, true, true, in metadata, out InstrumentObservation revealed);
        AssertState(InstrumentDiagnosticState.RegionSampled, revealed, "flag region sampled");
        TestAssert.True(revealed.SampleValue, "flag reveal mask true");

        InstrumentTargetMetadata outsideMetadata = Stage1BTestData.Metadata(0.1f, 0.2f);
        flag.Observe(in sampled, true, true, in outsideMetadata, out InstrumentObservation outside);
        AssertState(InstrumentDiagnosticState.ProbeHitOutsideRegion, outside, "outside reveal region");
        TestAssert.False(outside.SampleValue, "outside reveal mask false");

        InstrumentContext other = Stage1BTestData.Surface(colliderName: "other_geometry");
        flag.Observe(in other, true, false, in metadata, out InstrumentObservation otherGeometry);
        AssertState(InstrumentDiagnosticState.OtherGeometryHit, otherGeometry, "other geometry");

        var nonSurface = new InstrumentContext(8, InstrumentHitKind.NonSurfaceHit, Vector3.Zero, Vector3.Zero, null);
        flag.Observe(in nonSurface, true, false, in metadata, out InstrumentObservation transport);
        AssertState(InstrumentDiagnosticState.TransportClassNotSurfaceHit, transport, "non-surface hit");

        flag.Observe(in sampled, false, false, in metadata, out InstrumentObservation missingCatalog);
        AssertState(InstrumentDiagnosticState.DiagnosticUnresolved, missingCatalog, "missing catalog fails closed");

        InstrumentTargetMetadata invalid = default;
        flag.Observe(in sampled, true, true, in invalid, out InstrumentObservation invalidMetadata);
        AssertState(InstrumentDiagnosticState.DiagnosticUnresolved, invalidMetadata, "invalid metadata fails closed");
    }

    private static void AssertState(
        InstrumentDiagnosticState expected,
        InstrumentObservation observation,
        string message) => TestAssert.Equal(expected, observation.DiagnosticState, message);
}
