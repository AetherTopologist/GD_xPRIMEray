using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Instruments;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Runtime;

namespace XPrimeRay.ObserverInstrumentation.Tests;

// Lifecycle tests for ObserverInstrumentationSession.
// No Godot types. Covers every state the Godot adapter drives, in a pure context.
internal static class Stage1CLifecycleTests
{
    public static void Run()
    {
        RunDisabledMaskZeroWork();
        RunEnabledMaskProducesObservations();
        RunFrameClearAndReuse();
        RunOverflowReporting();
        RunFrameAndIncrementalOverflowMatch();
        RunCatalogNullProducesDiagnosticUnresolved();
        RunUnknownColliderProducesOtherGeometryHit();
        RunSurfaceUvOnlyFrameSequence();
    }

    private static void RunDisabledMaskZeroWork()
    {
        ObserverInstrumentationSession session = ObserverInstrumentationSession.Create(
            ObserverInstrumentationConfiguration.Disabled, maxHitsCapacity: 16);

        TestAssert.False(session.IsEnabled, "disabled: IsEnabled false");

        InstrumentContext ctx = SurfaceHit(0, "probe");
        session.RunFrame(new[] { ctx });

        TestAssert.Equal(0, session.LastObservationCount, "disabled: zero observations");
        TestAssert.False(session.OverflowOccurred, "disabled: no overflow");
        TestAssert.Equal(0, session.DroppedObservationCount, "disabled: zero dropped");
        TestAssert.Equal(0, session.FrameBuffer.Count, "disabled: buffer empty");
    }

    private static void RunEnabledMaskProducesObservations()
    {
        ObserverInstrumentationSession session = AllEnabled(maxHitsCapacity: 16);

        TestAssert.True(session.IsEnabled, "enabled: IsEnabled true");

        session.RunFrame(new[] { SurfaceHit(0, "probe") });

        // 4 instruments x 1 hit = 4 observations after adding the texture-sample seam.
        TestAssert.Equal(4, session.LastObservationCount, "enabled: all four instruments observe surface hit");
        TestAssert.False(session.OverflowOccurred, "enabled: no overflow within capacity");
        TestAssert.Equal(0, session.DroppedObservationCount, "enabled: zero dropped");
    }

    private static void RunFrameClearAndReuse()
    {
        ObserverInstrumentationSession session = AllEnabled(maxHitsCapacity: 16);

        session.RunFrame(new[] { SurfaceHit(0, "probe") });
        TestAssert.Equal(4, session.LastObservationCount, "reuse: first frame 4 observations");

        // Second frame must start fresh, not accumulate.
        session.RunFrame(new[] { SurfaceHit(1, "probe") });
        TestAssert.Equal(4, session.LastObservationCount, "reuse: second frame still 4, not 8");
        TestAssert.Equal(4, session.FrameBuffer.Count, "reuse: buffer count equals LastObservationCount");
    }

    private static void RunOverflowReporting()
    {
        // maxHitsCapacity=1 → buffer holds 4 slots (4 instruments x 1 hit).
        // Two hits require 8 slots → overflow fires on the second Evaluate call.
        ObserverInstrumentationSession session = AllEnabled(maxHitsCapacity: 1);

        TestAssert.Equal(4, session.ObservationCapacity, "overflow: capacity is 4 (4 instruments x 1 hit)");

        session.RunFrame(new[] { SurfaceHit(0, "probe"), SurfaceHit(1, "probe") });

        TestAssert.True(session.OverflowOccurred, "overflow: OverflowOccurred set on buffer saturation");
        TestAssert.True(session.DroppedObservationCount > 0, "overflow: DroppedObservationCount positive");
        TestAssert.Equal(4, session.FrameBuffer.Count, "overflow: buffer fills to capacity and stops");
    }

    private static void RunFrameAndIncrementalOverflowMatch()
    {
        InstrumentContext[] contexts =
        {
            SurfaceHit(0, "probe"),
            SurfaceHit(1, "probe"),
            SurfaceHit(2, "probe")
        };

        ObserverInstrumentationSession batch = AllEnabled(maxHitsCapacity: 1);
        batch.RunFrame(contexts);

        ObserverInstrumentationSession incremental = AllEnabled(maxHitsCapacity: 1);
        incremental.BeginFrame();
        for (int i = 0; i < contexts.Length; i++)
        {
            if (!incremental.ProcessContext(in contexts[i]))
                break;
        }
        incremental.EndFrame();

        TestAssert.True(batch.OverflowOccurred, "batch overflow: overflow reported");
        TestAssert.True(incremental.OverflowOccurred, "incremental overflow: overflow reported");
        TestAssert.Equal(
            incremental.DroppedObservationCount,
            batch.DroppedObservationCount,
            "batch overflow: dropped count matches incremental break behavior");
        TestAssert.Equal(1, batch.DroppedObservationCount, "batch overflow: stops after first dropped context");
        TestAssert.Equal(
            incremental.LastObservationCount,
            batch.LastObservationCount,
            "batch overflow: observation count matches incremental");
        TestAssert.Equal(4, batch.LastObservationCount, "batch overflow: buffer remains at capacity");
        TestAssert.Equal(
            incremental.FrameSequence,
            batch.FrameSequence,
            "batch overflow: frame sequence matches incremental");
    }

    private static void RunCatalogNullProducesDiagnosticUnresolved()
    {
        // No catalog in configuration: instruments cannot resolve metadata.
        var config = new ObserverInstrumentationConfiguration
        {
            EnabledMask = ObserverInstrumentMask.All,
            Catalog = null
        };
        ObserverInstrumentationSession session = ObserverInstrumentationSession.Create(config, maxHitsCapacity: 16);

        session.RunFrame(new[] { SurfaceHit(0, "probe") });

        TestAssert.Equal(4, session.LastObservationCount, "null catalog: four observations");
        for (int i = 0; i < session.FrameBuffer.Count; i++)
        {
            session.FrameBuffer.TryGet(i, out InstrumentObservation obs);
            TestAssert.Equal(
                InstrumentDiagnosticState.DiagnosticUnresolved,
                obs.DiagnosticState,
                $"null catalog: observation {i} is DiagnosticUnresolved");
        }
    }

    private static void RunUnknownColliderProducesOtherGeometryHit()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        var config = new ObserverInstrumentationConfiguration
        {
            EnabledMask = ObserverInstrumentMask.All,
            Catalog = new InstrumentMetadataCatalog(metadata)
        };
        ObserverInstrumentationSession session = ObserverInstrumentationSession.Create(config, maxHitsCapacity: 16);

        // "wall_panel" is not registered in the catalog.
        InstrumentContext ctx = HitContextMapper.BuildContext(
            0, true, 0, Vector3.UnitY, Vector3.UnitY, "wall_panel");
        session.RunFrame(new[] { ctx });

        TestAssert.Equal(4, session.LastObservationCount, "unknown collider: four observations");
        for (int i = 0; i < session.FrameBuffer.Count; i++)
        {
            session.FrameBuffer.TryGet(i, out InstrumentObservation obs);
            TestAssert.Equal(
                InstrumentDiagnosticState.OtherGeometryHit,
                obs.DiagnosticState,
                $"unknown collider: observation {i} is OtherGeometryHit");
        }
    }

    private static void RunSurfaceUvOnlyFrameSequence()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        var config = new ObserverInstrumentationConfiguration
        {
            EnabledMask = ObserverInstrumentMask.SurfaceUv,
            Catalog = new InstrumentMetadataCatalog(metadata)
        };
        ObserverInstrumentationSession session = ObserverInstrumentationSession.Create(config, maxHitsCapacity: 4);

        TestAssert.Equal(0UL, session.FrameSequence, "surface-only sequence starts at zero");
        session.RunFrame(new[] { SurfaceHit(0, Stage1BTestData.ProbeName) });

        TestAssert.Equal(1UL, session.FrameSequence, "surface-only sequence advances once");
        TestAssert.Equal(1, session.LastObservationCount, "surface-only emits one observation per hit");
        TestAssert.True(session.FrameBuffer.TryGet(0, out InstrumentObservation observation), "surface-only observation exists");
        TestAssert.Equal(InstrumentKind.SurfaceUv, observation.Instrument, "surface-only kind");
        TestAssert.Equal(InstrumentDiagnosticState.RegionSampled, observation.DiagnosticState, "surface-only sampled");
        TestAssert.True(observation.HasUv, "surface-only includes UV");

        session.RunFrame(ReadOnlySpan<InstrumentContext>.Empty);
        TestAssert.Equal(2UL, session.FrameSequence, "empty frame still advances sequence");
        TestAssert.Equal(0, session.LastObservationCount, "empty frame clears prior observations");
    }

    // Helpers

    private static ObserverInstrumentationSession AllEnabled(int maxHitsCapacity) =>
        ObserverInstrumentationSession.Create(
            new ObserverInstrumentationConfiguration { EnabledMask = ObserverInstrumentMask.All },
            maxHitsCapacity);

    private static InstrumentContext SurfaceHit(int rayIndex, string colliderName) =>
        HitContextMapper.BuildContext(
            rayIndex, valid: true, absorbed: 0,
            Vector3.UnitX, Vector3.UnitX, colliderName);
}
