using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Instruments;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Runtime;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class SurfaceUvInstrumentTests
{
    private const float Tolerance = 1e-6f;

    public static void Run()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        var instrument = new SurfaceUvInstrument();

        InstrumentObservation positiveX = Observe(instrument, metadata, Vector3.UnitX, 0);
        AssertUv(positiveX, 0.5f, 0.5f, "+X");

        InstrumentObservation positiveZ = Observe(instrument, metadata, Vector3.UnitZ, 1);
        AssertUv(positiveZ, 0.75f, 0.5f, "+Z");

        InstrumentObservation negativeZ = Observe(instrument, metadata, -Vector3.UnitZ, 2);
        AssertUv(negativeZ, 0.25f, 0.5f, "-Z");

        InstrumentObservation northPole = Observe(instrument, metadata, Vector3.UnitY, 3);
        AssertUv(northPole, 0.5f, 0f, "north pole");

        InstrumentObservation southPole = Observe(instrument, metadata, -Vector3.UnitY, 4);
        AssertUv(southPole, 0.5f, 1f, "south pole");

        const float seamOffset = 0.0001f;
        InstrumentObservation upperSeam = Observe(
            instrument, metadata, new Vector3(-1f, 0f, seamOffset), 5);
        InstrumentObservation lowerSeam = Observe(
            instrument, metadata, new Vector3(-1f, 0f, -seamOffset), 6);
        TestAssert.True(upperSeam.Uv.X > 0.99f && upperSeam.Uv.X < 1f, "upper seam remains below one");
        TestAssert.True(lowerSeam.Uv.X >= 0f && lowerSeam.Uv.X < 0.01f, "lower seam wraps above zero");

        InstrumentObservation exactSeam = Observe(instrument, metadata, -Vector3.UnitX, 7);
        AssertUv(exactSeam, 0f, 0.5f, "exact seam wraps to zero");

        InstrumentObservation[] observations =
        {
            upperSeam,
            lowerSeam,
            northPole,
            southPole,
            new(
                8,
                InstrumentKind.CheckerProbe,
                InstrumentDiagnosticState.RegionSampled,
                Stage1BTestData.ProbeName,
                new Vector2(0.2f, 0.2f),
                hasUv: true,
                sampleValue: false),
            new(
                9,
                InstrumentKind.SurfaceUv,
                InstrumentDiagnosticState.TransportClassNotSurfaceHit,
                null,
                default,
                hasUv: false,
                sampleValue: false),
            new(
                10,
                InstrumentKind.SurfaceUv,
                InstrumentDiagnosticState.OtherGeometryHit,
                "wall",
                default,
                hasUv: false,
                sampleValue: false),
            new(
                11,
                InstrumentKind.SurfaceUv,
                InstrumentDiagnosticState.DiagnosticUnresolved,
                null,
                default,
                hasUv: false,
                sampleValue: false)
        };
        var coverage = new UvCoverageDiagnostic();
        coverage.Evaluate(observations);

        TestAssert.Equal(7, coverage.ObservationCount, "coverage counts SurfaceUv observations only");
        TestAssert.Equal(4, coverage.ValidUvCount, "coverage valid UV count");
        TestAssert.Equal(1, coverage.NonSurfaceCount, "coverage non-surface diagnostic count");
        TestAssert.Equal(1, coverage.OtherGeometryCount, "coverage other-geometry diagnostic count");
        TestAssert.Equal(1, coverage.DiagnosticUnresolvedCount, "coverage unresolved diagnostic count");
        TestAssert.Equal(1, coverage.LowerSeamBandCount, "coverage lower seam count");
        TestAssert.Equal(1, coverage.UpperSeamBandCount, "coverage upper seam count");
        TestAssert.True(coverage.CoversBothSeamBands, "coverage reports both seam bands");
        TestAssert.Equal(1, coverage.NorthPoleCount, "coverage north pole count");
        TestAssert.Equal(1, coverage.SouthPoleCount, "coverage south pole count");
        TestAssert.NearlyEqual(0f, coverage.MinimumV, Tolerance, "coverage minimum V");
        TestAssert.NearlyEqual(1f, coverage.MaximumV, Tolerance, "coverage maximum V");

        Console.WriteLine(
            $"  UV coverage: valid={coverage.ValidUvCount}/{coverage.ObservationCount} " +
            $"u=[{coverage.MinimumU:F6},{coverage.MaximumU:F6}] " +
            $"v=[{coverage.MinimumV:F6},{coverage.MaximumV:F6}] " +
            $"seam={coverage.LowerSeamBandCount}/{coverage.UpperSeamBandCount} " +
            $"poles={coverage.NorthPoleCount}/{coverage.SouthPoleCount} " +
            $"unresolved={coverage.DiagnosticUnresolvedCount}");

        coverage.Evaluate(ReadOnlySpan<InstrumentObservation>.Empty);
        TestAssert.False(coverage.HasCoverage, "empty evaluation resets coverage");
        TestAssert.Equal(0, coverage.ObservationCount, "empty evaluation prevents stale diagnostics");
    }

    private static InstrumentObservation Observe(
        SurfaceUvInstrument instrument,
        InstrumentTargetMetadata metadata,
        Vector3 position,
        int rayIndex)
    {
        var context = new InstrumentContext(
            rayIndex,
            InstrumentHitKind.SurfaceHit,
            position,
            Vector3.Normalize(position),
            Stage1BTestData.ProbeName);
        instrument.Observe(in context, true, true, in metadata, out InstrumentObservation observation);
        TestAssert.Equal(InstrumentDiagnosticState.RegionSampled, observation.DiagnosticState, $"ray {rayIndex} sampled");
        TestAssert.True(observation.HasUv, $"ray {rayIndex} has UV");
        return observation;
    }

    private static void AssertUv(
        InstrumentObservation observation,
        float expectedU,
        float expectedV,
        string label)
    {
        TestAssert.NearlyEqual(expectedU, observation.Uv.X, Tolerance, $"{label} U");
        TestAssert.NearlyEqual(expectedV, observation.Uv.Y, Tolerance, $"{label} V");
    }
}
