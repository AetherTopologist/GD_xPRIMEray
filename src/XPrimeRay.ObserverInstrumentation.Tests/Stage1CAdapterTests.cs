using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Instruments;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Runtime;

namespace XPrimeRay.ObserverInstrumentation.Tests;

// Tests for Stage 1C: HitContextMapper (the pure classification logic the adapter delegates to).
// The Godot-aware adapter itself cannot be tested here because RayBeamRenderer.HitPayload
// depends on Godot types. HitContextMapper is the extractable, testable core.
internal static class Stage1CAdapterTests
{
    public static void Run()
    {
        RunHitClassifierTests();
        RunColliderNameNormalizerTests();
        RunBuildContextTests();
        RunDisabledMaskProducesZeroWork();
        RunSurfaceHitWithMatchingMetadata();
        RunMissProducesTransportClassNotSurfaceHit();
        RunNonSurfaceHitProducesTransportClassNotSurfaceHit();
        RunSurfaceHitWithNoMetadataCatalog();
        RunSurfaceHitWithUnknownCollider();
    }

    private static void RunHitClassifierTests()
    {
        TestAssert.Equal(
            InstrumentHitKind.SurfaceHit,
            HitContextMapper.ClassifyHit(valid: true, absorbed: 0),
            "valid+not-absorbed → SurfaceHit");

        TestAssert.Equal(
            InstrumentHitKind.SurfaceHit,
            HitContextMapper.ClassifyHit(valid: true, absorbed: 1),
            "valid overrides absorbed flag → SurfaceHit");

        TestAssert.Equal(
            InstrumentHitKind.NonSurfaceHit,
            HitContextMapper.ClassifyHit(valid: false, absorbed: 1),
            "invalid+absorbed → NonSurfaceHit (inner-radius absorption)");

        TestAssert.Equal(
            InstrumentHitKind.Miss,
            HitContextMapper.ClassifyHit(valid: false, absorbed: 0),
            "invalid+not-absorbed → Miss (step exhaustion or geometry miss)");
    }

    private static void RunColliderNameNormalizerTests()
    {
        TestAssert.Equal(null, HitContextMapper.NormalizeColliderName("<none>"), "<none> sentinel → null");
        TestAssert.Equal(null, HitContextMapper.NormalizeColliderName(""), "empty string → null");
        TestAssert.Equal(null, HitContextMapper.NormalizeColliderName(null), "null → null");
        TestAssert.Equal("probe_sphere", HitContextMapper.NormalizeColliderName("probe_sphere"), "real name preserved");
        TestAssert.Equal("SphereA", HitContextMapper.NormalizeColliderName("SphereA"), "mixed-case name preserved");
    }

    private static void RunBuildContextTests()
    {
        InstrumentContext ctx = HitContextMapper.BuildContext(
            rayIndex: 3,
            valid: true,
            absorbed: 0,
            position: new Vector3(1f, 0f, 0f),
            normal: new Vector3(1f, 0f, 0f),
            colliderName: "probe");

        TestAssert.Equal(3, ctx.RayIndex, "BuildContext preserves rayIndex");
        TestAssert.Equal(InstrumentHitKind.SurfaceHit, ctx.HitKind, "BuildContext classifies valid → SurfaceHit");
        TestAssert.Equal("probe", ctx.ColliderName, "BuildContext preserves real colliderName");

        InstrumentContext missCtx = HitContextMapper.BuildContext(
            rayIndex: 7,
            valid: false,
            absorbed: 0,
            position: Vector3.Zero,
            normal: Vector3.Zero,
            colliderName: "<none>");

        TestAssert.Equal(7, missCtx.RayIndex, "miss preserves rayIndex");
        TestAssert.Equal(InstrumentHitKind.Miss, missCtx.HitKind, "miss classifies correctly");
        TestAssert.Equal(null, missCtx.ColliderName, "miss normalizes <none> → null");
    }

    private static void RunDisabledMaskProducesZeroWork()
    {
        var registry = new InstrumentRegistry(
            new FlagCaptureInstrument(),
            new SurfaceUvInstrument(),
            new CheckerProbeInstrument());
        // Default mask is None — no Seal needed to check; Seal before Evaluate.
        registry.Seal();

        var buffer = new InstrumentFrameBuffer(registry.Count * 4);

        InstrumentContext surfaceHit = HitContextMapper.BuildContext(
            0, true, 0, Vector3.UnitX, Vector3.UnitX, Stage1BTestData.ProbeName);
        InstrumentContext miss = HitContextMapper.BuildContext(
            1, false, 0, Vector3.Zero, Vector3.Zero, "<none>");

        TestAssert.True(
            registry.Evaluate(in surfaceHit, null, buffer),
            "disabled registry: surface hit evaluate succeeds");
        TestAssert.Equal(0, buffer.Count, "disabled registry: surface hit produces zero observations");

        buffer.Clear();
        TestAssert.True(
            registry.Evaluate(in miss, null, buffer),
            "disabled registry: miss evaluate succeeds");
        TestAssert.Equal(0, buffer.Count, "disabled registry: miss produces zero observations");
    }

    private static void RunSurfaceHitWithMatchingMetadata()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        var catalog = new InstrumentMetadataCatalog(metadata);
        var registry = AllInstrumentsEnabled();
        var buffer = new InstrumentFrameBuffer(registry.Count * 4);

        InstrumentContext ctx = HitContextMapper.BuildContext(
            rayIndex: 0,
            valid: true,
            absorbed: 0,
            position: Vector3.UnitX,
            normal: Vector3.UnitX,
            colliderName: Stage1BTestData.ProbeName);

        TestAssert.True(registry.Evaluate(in ctx, catalog, buffer), "surface hit with metadata: evaluate fits buffer");
        TestAssert.Equal(3, buffer.Count, "surface hit with matching metadata: all three instruments produce observations");
    }

    private static void RunMissProducesTransportClassNotSurfaceHit()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        var catalog = new InstrumentMetadataCatalog(metadata);
        var registry = AllInstrumentsEnabled();
        var buffer = new InstrumentFrameBuffer(registry.Count * 4);

        InstrumentContext ctx = HitContextMapper.BuildContext(
            rayIndex: 5,
            valid: false,
            absorbed: 0,
            position: Vector3.Zero,
            normal: Vector3.Zero,
            colliderName: "<none>");

        TestAssert.True(registry.Evaluate(in ctx, catalog, buffer), "miss evaluate fits buffer");
        TestAssert.Equal(3, buffer.Count, "miss: three observations produced");

        for (int i = 0; i < buffer.Count; i++)
        {
            TestAssert.True(buffer.TryGet(i, out InstrumentObservation obs), $"miss slot {i} accessible");
            TestAssert.Equal(
                InstrumentDiagnosticState.TransportClassNotSurfaceHit,
                obs.DiagnosticState,
                $"miss observation {i}: TransportClassNotSurfaceHit");
        }
    }

    private static void RunNonSurfaceHitProducesTransportClassNotSurfaceHit()
    {
        var registry = AllInstrumentsEnabled();
        var buffer = new InstrumentFrameBuffer(registry.Count * 4);

        // absorbed=1 with valid=false → NonSurfaceHit
        InstrumentContext ctx = HitContextMapper.BuildContext(
            rayIndex: 2,
            valid: false,
            absorbed: 1,
            position: Vector3.Zero,
            normal: Vector3.Zero,
            colliderName: null);

        TestAssert.True(registry.Evaluate(in ctx, null, buffer), "non-surface hit evaluate fits buffer");
        TestAssert.Equal(3, buffer.Count, "non-surface hit: three observations produced");

        for (int i = 0; i < buffer.Count; i++)
        {
            TestAssert.True(buffer.TryGet(i, out InstrumentObservation obs), $"non-surface slot {i} accessible");
            TestAssert.Equal(
                InstrumentDiagnosticState.TransportClassNotSurfaceHit,
                obs.DiagnosticState,
                $"non-surface observation {i}: TransportClassNotSurfaceHit");
        }
    }

    private static void RunSurfaceHitWithNoMetadataCatalog()
    {
        var registry = AllInstrumentsEnabled();
        var buffer = new InstrumentFrameBuffer(registry.Count * 4);

        InstrumentContext ctx = HitContextMapper.BuildContext(
            rayIndex: 1,
            valid: true,
            absorbed: 0,
            position: Vector3.UnitX,
            normal: Vector3.UnitX,
            colliderName: "some_collider");

        // catalog=null → DiagnosticUnresolved for all instruments
        TestAssert.True(registry.Evaluate(in ctx, null, buffer), "surface hit no catalog: evaluate fits buffer");
        TestAssert.Equal(3, buffer.Count, "surface hit no catalog: three observations produced");

        for (int i = 0; i < buffer.Count; i++)
        {
            TestAssert.True(buffer.TryGet(i, out InstrumentObservation obs), $"no-catalog slot {i} accessible");
            TestAssert.Equal(
                InstrumentDiagnosticState.DiagnosticUnresolved,
                obs.DiagnosticState,
                $"no-catalog observation {i}: DiagnosticUnresolved");
        }
    }

    private static void RunSurfaceHitWithUnknownCollider()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        var catalog = new InstrumentMetadataCatalog(metadata);
        var registry = AllInstrumentsEnabled();
        var buffer = new InstrumentFrameBuffer(registry.Count * 4);

        // A real surface hit but the collider is not in the catalog → OtherGeometryHit
        InstrumentContext ctx = HitContextMapper.BuildContext(
            rayIndex: 4,
            valid: true,
            absorbed: 0,
            position: Vector3.UnitY,
            normal: Vector3.UnitY,
            colliderName: "wall_mesh");

        TestAssert.True(registry.Evaluate(in ctx, catalog, buffer), "unknown collider evaluate fits buffer");
        TestAssert.Equal(3, buffer.Count, "unknown collider: three observations produced");

        for (int i = 0; i < buffer.Count; i++)
        {
            TestAssert.True(buffer.TryGet(i, out InstrumentObservation obs), $"unknown-collider slot {i} accessible");
            TestAssert.Equal(
                InstrumentDiagnosticState.OtherGeometryHit,
                obs.DiagnosticState,
                $"unknown-collider observation {i}: OtherGeometryHit");
        }
    }

    private static InstrumentRegistry AllInstrumentsEnabled()
    {
        var registry = new InstrumentRegistry(
            new FlagCaptureInstrument(),
            new SurfaceUvInstrument(),
            new CheckerProbeInstrument());
        registry.SetEnabledMask(ObserverInstrumentMask.All);
        registry.Seal();
        return registry;
    }
}
