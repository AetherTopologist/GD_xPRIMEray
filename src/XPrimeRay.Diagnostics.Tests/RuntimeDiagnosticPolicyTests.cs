using XPrimeRay.Diagnostics;

namespace XPrimeRay.Diagnostics.Tests;

internal static class RuntimeDiagnosticPolicyTests
{
    public static void Run()
    {
        VerbosityOrdering();
        TerminalErrorsAllowedAtAllLevels();
        SummaryRejectsNoisierMessages();
        FrameAndRegionBoundaries();
        PixelTraceRequiresSelectedIndices();
        TransportTraceRequiresSecondaryOptIn();
        DisabledLogsDoNotInvokeFormattingHelper();
        LiveAggregateEmitsAtMostOncePerInterval();
        CountersResetOnlyAfterSuccessfulFlush();
        RepeatedWatchdogEventsAggregate();
        SnapshotEvidenceEmitsOnceWithGuard();
        OneShotFlagNotConsumedBelowVerbosity();
        ModeAwareDefaultsAndExplicitOverride();
        SemanticCountersUnavailableAreExplicit();
    }

    private static void VerbosityOrdering()
    {
        Assert.True(DiagnosticVerbosity.Summary < DiagnosticVerbosity.Frame, "summary before frame");
        Assert.True(DiagnosticVerbosity.Frame < DiagnosticVerbosity.Region, "frame before region");
        Assert.True(DiagnosticVerbosity.Region < DiagnosticVerbosity.PixelTrace, "region before pixel");
        Assert.True(DiagnosticVerbosity.PixelTrace < DiagnosticVerbosity.TransportTrace, "pixel before transport");
    }

    private static void TerminalErrorsAllowedAtAllLevels()
    {
        var policy = new RuntimeDiagnosticPolicy(DiagnosticVerbosity.Off);
        Assert.True(policy.Allows(DiagnosticVerbosity.TransportTrace, DiagnosticCategory.Error), "error category");
        Assert.True(policy.Allows(DiagnosticVerbosity.TransportTrace, terminal: true), "terminal flag");
    }

    private static void SummaryRejectsNoisierMessages()
    {
        var policy = new RuntimeDiagnosticPolicy(DiagnosticVerbosity.Summary);
        Assert.True(policy.Allows(DiagnosticVerbosity.Summary), "summary allowed");
        Assert.False(policy.Allows(DiagnosticVerbosity.Frame), "frame rejected");
        Assert.False(policy.Allows(DiagnosticVerbosity.Region), "region rejected");
        Assert.False(policy.Allows(DiagnosticVerbosity.PixelTrace), "pixel rejected");
        Assert.False(policy.Allows(DiagnosticVerbosity.TransportTrace), "transport rejected");
    }

    private static void FrameAndRegionBoundaries()
    {
        var frame = new RuntimeDiagnosticPolicy(DiagnosticVerbosity.Frame);
        Assert.True(frame.Allows(DiagnosticVerbosity.Frame), "frame allows frame");
        Assert.False(frame.Allows(DiagnosticVerbosity.Region), "frame rejects region");
        var region = new RuntimeDiagnosticPolicy(DiagnosticVerbosity.Region);
        Assert.True(region.Allows(DiagnosticVerbosity.Region), "region allows region");
        Assert.False(region.Allows(DiagnosticVerbosity.PixelTrace), "region rejects pixel");
    }

    private static void PixelTraceRequiresSelectedIndices()
    {
        var policy = new RuntimeDiagnosticPolicy(DiagnosticVerbosity.PixelTrace, selectedPixelTraceLimit: 2);
        Assert.False(policy.AllowsSelectedPixelTrace(0), "no selected pixels rejected");
        Assert.True(policy.AllowsSelectedPixelTrace(2), "selected pixels allowed");
        Assert.False(policy.AllowsSelectedPixelTrace(3), "selected pixel cap enforced");
    }

    private static void TransportTraceRequiresSecondaryOptIn()
    {
        var noOptIn = new RuntimeDiagnosticPolicy(DiagnosticVerbosity.TransportTrace);
        Assert.False(noOptIn.Allows(DiagnosticVerbosity.TransportTrace, DiagnosticCategory.Transport), "transport opt-in required");
        var optIn = new RuntimeDiagnosticPolicy(DiagnosticVerbosity.TransportTrace, transportTraceOptIn: true);
        Assert.True(optIn.Allows(DiagnosticVerbosity.TransportTrace, DiagnosticCategory.Transport), "transport opt-in allowed");
    }

    private static void DisabledLogsDoNotInvokeFormattingHelper()
    {
        var policy = new RuntimeDiagnosticPolicy(DiagnosticVerbosity.Off);
        bool called = false;
        if (policy.Allows(DiagnosticVerbosity.Frame))
        {
            called = true;
        }
        Assert.False(called, "format helper would not be called");
    }

    private static void LiveAggregateEmitsAtMostOncePerInterval()
    {
        var aggregate = new LiveDiagnosticAggregator(TimeSpan.FromSeconds(1));
        aggregate.Counters.Frames = 1;
        Assert.False(aggregate.TryFlush(TimeSpan.FromMilliseconds(0).Ticks, out _), "first sample initializes");
        Assert.False(aggregate.TryFlush(TimeSpan.FromMilliseconds(500).Ticks, out _), "too early rejected");
        Assert.True(aggregate.TryFlush(TimeSpan.FromMilliseconds(1000).Ticks, out LiveDiagnosticCounters snapshot), "interval flush");
        Assert.Equal(1L, snapshot.Frames, "flushed frames");
        Assert.False(aggregate.TryFlush(TimeSpan.FromMilliseconds(1500).Ticks, out _), "second early rejected");
    }

    private static void CountersResetOnlyAfterSuccessfulFlush()
    {
        var aggregate = new LiveDiagnosticAggregator(TimeSpan.FromSeconds(1));
        aggregate.Counters.RenderSteps = 3;
        aggregate.TryFlush(0, out _);
        aggregate.TryFlush(TimeSpan.FromMilliseconds(500).Ticks, out _);
        Assert.Equal(3L, aggregate.Counters.RenderSteps, "not reset before flush");
        Assert.True(aggregate.TryFlush(TimeSpan.FromMilliseconds(1000).Ticks, out _), "flush succeeds");
        Assert.Equal(0L, aggregate.Counters.RenderSteps, "reset after flush");
    }

    private static void RepeatedWatchdogEventsAggregate()
    {
        var aggregate = new LiveDiagnosticAggregator(TimeSpan.FromSeconds(1));
        aggregate.Counters.RecordWatchdog();
        aggregate.Counters.RecordWatchdog();
        aggregate.TryFlush(0, out _);
        Assert.True(aggregate.TryFlush(TimeSpan.FromSeconds(1).Ticks, out LiveDiagnosticCounters snapshot), "flush watchdogs");
        Assert.Equal(2L, snapshot.Watchdog, "watchdogs aggregate");
    }

    private static void SnapshotEvidenceEmitsOnceWithGuard()
    {
        bool emitted = false;
        int count = 0;
        var policy = RuntimeDiagnosticPolicy.SnapshotDefault;
        for (int i = 0; i < 2; i++)
        {
            if (!emitted && policy.Allows(DiagnosticVerbosity.Frame, DiagnosticCategory.Probe))
            {
                emitted = true;
                count++;
            }
        }
        Assert.Equal(1, count, "snapshot evidence once");
    }

    private static void OneShotFlagNotConsumedBelowVerbosity()
    {
        var gate = new OneShotDiagnosticGate();
        var summary = RuntimeDiagnosticPolicy.LiveDefault;
        Assert.False(gate.TryClaim(summary, DiagnosticVerbosity.Frame, DiagnosticCategory.Probe), "summary does not claim");
        Assert.False(gate.HasEmitted, "summary did not consume flag");
        var frame = RuntimeDiagnosticPolicy.SnapshotDefault;
        Assert.True(gate.TryClaim(frame, DiagnosticVerbosity.Frame, DiagnosticCategory.Probe), "frame claims");
        Assert.False(gate.TryClaim(frame, DiagnosticVerbosity.Frame, DiagnosticCategory.Probe), "second frame rejected");
    }

    private static void ModeAwareDefaultsAndExplicitOverride()
    {
        Assert.Equal(DiagnosticVerbosity.Summary, RuntimeDiagnosticPolicy.DefaultFor(DiagnosticRuntimeMode.Live), "auto live");
        Assert.Equal(DiagnosticVerbosity.Frame, RuntimeDiagnosticPolicy.DefaultFor(DiagnosticRuntimeMode.Snapshot), "auto snapshot");
        Assert.Equal(DiagnosticVerbosity.Frame, RuntimeDiagnosticPolicy.DefaultFor(DiagnosticRuntimeMode.HeadlessQualification), "auto headless");
        RuntimeDiagnosticPolicy explicitOff = RuntimeDiagnosticPolicy.Resolve(
            DiagnosticVerbosityPreference.Explicit,
            DiagnosticVerbosity.Off,
            DiagnosticRuntimeMode.Snapshot);
        Assert.Equal(DiagnosticVerbosity.Off, explicitOff.Verbosity, "explicit off");
        RuntimeDiagnosticPolicy explicitRegion = RuntimeDiagnosticPolicy.Resolve(
            DiagnosticVerbosityPreference.Explicit,
            DiagnosticVerbosity.Region,
            DiagnosticRuntimeMode.Live);
        Assert.Equal(DiagnosticVerbosity.Region, explicitRegion.Verbosity, "explicit region");
    }

    private static void SemanticCountersUnavailableAreExplicit()
    {
        var counters = new LiveDiagnosticCounters();
        Assert.False(counters.SemanticCountersAvailable, "semantic counters unavailable by default");
        counters.AddOutcomes(1, 2, 3, 4, 5, 6);
        Assert.True(counters.SemanticCountersAvailable, "semantic counters available after outcome add");
        Assert.Equal(1L, counters.Geometry, "geometry count");
    }
}
