using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Instruments;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Runtime;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class RegistryAndBufferTests
{
    public static void Run()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        var catalog = new InstrumentMetadataCatalog(metadata);
        var registry = new InstrumentRegistry(
            new FlagCaptureInstrument(),
            new SurfaceUvInstrument(),
            new CheckerProbeInstrument());
        var buffer = new InstrumentFrameBuffer(3);
        InstrumentContext context = Stage1BTestData.Surface();

        registry.Seal();
        TestAssert.Equal(ObserverInstrumentMask.None, registry.EnabledMask, "features default off");
        TestAssert.True(registry.Evaluate(in context, catalog, buffer), "disabled evaluation succeeds");
        TestAssert.Equal(0, buffer.Count, "disabled registry emits nothing");

        var enabled = new InstrumentRegistry(
            new FlagCaptureInstrument(),
            new SurfaceUvInstrument(),
            new CheckerProbeInstrument());
        enabled.SetEnabledMask(ObserverInstrumentMask.All);
        enabled.Seal();
        TestAssert.True(enabled.Evaluate(in context, catalog, buffer), "enabled evaluation fits buffer");
        TestAssert.Equal(3, buffer.Count, "three observations emitted");
        AssertKind(buffer, 0, InstrumentKind.FlagCapture);
        AssertKind(buffer, 1, InstrumentKind.SurfaceUv);
        AssertKind(buffer, 2, InstrumentKind.CheckerProbe);

        buffer.Clear();
        TestAssert.Equal(0, buffer.Count, "clear resets count");
        TestAssert.False(buffer.TryGet(0, out _), "cleared data is inaccessible");
        TestAssert.Equal(3, buffer.Capacity, "clear preserves allocation capacity");

        var one = new InstrumentRegistry(new SurfaceUvInstrument());
        one.SetEnabledMask(ObserverInstrumentMask.SurfaceUv);
        one.Seal();
        TestAssert.True(one.Evaluate(in context, catalog, buffer), "buffer reuses storage");
        TestAssert.Equal(1, buffer.Count, "reuse exposes only current frame data");
        TestAssert.False(buffer.TryGet(1, out _), "stale slot remains inaccessible");
    }

    private static void AssertKind(InstrumentFrameBuffer buffer, int index, InstrumentKind expected)
    {
        TestAssert.True(buffer.TryGet(index, out InstrumentObservation observation), $"slot {index} exists");
        TestAssert.Equal(expected, observation.Instrument, $"slot {index} preserves registry order");
    }
}
