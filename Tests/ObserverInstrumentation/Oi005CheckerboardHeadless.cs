using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Math;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Runtime;
using NumericsVector3 = System.Numerics.Vector3;

// OI-005 Checkerboard — verifies the derived observation chain:
//   ValidatedHit → SurfaceUvInstrument → CheckerProbeInstrument
//
// Critical chain verification: for each sampled ray, the checker parity is computed
// independently from the SurfaceUv observation's UV, then compared against the
// CheckerProbe observation's SampleValue. Mismatch = chain break.
public partial class Oi005CheckerboardHeadless : Node
{
    private const string ProbeName = "uv_probe";
    private const int MinimumValidPairs = 16;
    private const int TimeoutFrames = 300;
    private const int CheckerTilesU = 8;
    private const int CheckerTilesV = 4;
    private const float UvAgreementTolerance = 1e-6f;

    private GrinFilmCamera _film;
    private ulong _lastSequence;
    private int _frames;

    // Accumulated diagnostic counters
    private int _surfaceObservationCount;
    private int _checkerObservationCount;
    private int _flagObservationCount;
    private int _pairedCount;
    private int _missingCheckerCount;
    private int _missingSurfaceCount;
    private int _lightCount;
    private int _darkCount;
    private int _uvMismatchCount;
    private int _checkerMismatchCount;
    private int _invalidUvCount;
    private int _unresolvedValidProbeCount;
    private bool _overflow;
    private int _dropped;
    private bool _finished;

    public override void _Ready()
    {
        _film = GetNodeOrNull<GrinFilmCamera>("GrinFilmCamera");
        if (_film == null)
        {
            Finish(false, "missing GrinFilmCamera");
            return;
        }

        _film.NeedColliderNames = true;

        if (!UvRevealRegion.TryCreate(0f, 1f, 0f, 1f, out UvRevealRegion fullRegion) ||
            !InstrumentTargetMetadata.TryCreate(
                ProbeName,
                NumericsVector3.Zero,
                CheckerTilesU,
                CheckerTilesV,
                fullRegion,
                out InstrumentTargetMetadata metadata))
        {
            Finish(false, "metadata construction failed");
            return;
        }

        var configuration = new ObserverInstrumentationConfiguration
        {
            EnabledMask = ObserverInstrumentMask.SurfaceUv | ObserverInstrumentMask.CheckerProbe,
            Catalog = new InstrumentMetadataCatalog(metadata)
        };
        _film.ApplyInstrumentationConfiguration(configuration);
    }

    public override void _Process(double delta)
    {
        if (_finished || _film == null)
            return;

        _frames++;
        ulong sequence = _film.InstrumentationFrameSequenceForTesting;
        if (sequence != 0 && sequence != _lastSequence)
        {
            _lastSequence = sequence;
            ReadOnlySpan<InstrumentObservation> observations =
                _film.GetInstrumentationObservationsForTesting();
            ProcessFrame(observations);
            _overflow |= _film.InstrumentationOverflowForTesting;
            _dropped += _film.InstrumentationDroppedObservationCountForTesting;

            if (_pairedCount >= MinimumValidPairs)
            {
                bool pass =
                    _checkerMismatchCount == 0 &&
                    _uvMismatchCount == 0 &&
                    _unresolvedValidProbeCount == 0 &&
                    _flagObservationCount == 0 &&
                    _invalidUvCount == 0 &&
                    _lightCount > 0 &&
                    _darkCount > 0 &&
                    !_overflow &&
                    _dropped == 0;
                Finish(pass, pass ? "acceptance met" : "acceptance failed");
                return;
            }
        }

        if (_frames >= TimeoutFrames)
            Finish(false, "timeout waiting for valid observation pairs");
    }

    private void ProcessFrame(ReadOnlySpan<InstrumentObservation> observations)
    {
        // Index by RayIndex within this frame.
        var surfaceByRay = new Dictionary<int, InstrumentObservation>();
        var checkerByRay = new Dictionary<int, InstrumentObservation>();

        for (int i = 0; i < observations.Length; i++)
        {
            ref readonly InstrumentObservation obs = ref observations[i];
            switch (obs.Instrument)
            {
                case InstrumentKind.SurfaceUv:
                    _surfaceObservationCount++;
                    surfaceByRay[obs.RayIndex] = obs;
                    break;
                case InstrumentKind.CheckerProbe:
                    _checkerObservationCount++;
                    checkerByRay[obs.RayIndex] = obs;
                    break;
                case InstrumentKind.FlagCapture:
                    _flagObservationCount++;
                    break;
            }
        }

        // Pair each fully-sampled probe SurfaceUv observation with its CheckerProbe counterpart.
        foreach (var (rayIndex, uvObs) in surfaceByRay)
        {
            if (uvObs.ColliderName != ProbeName)
                continue;

            if (uvObs.DiagnosticState == InstrumentDiagnosticState.DiagnosticUnresolved)
            {
                _unresolvedValidProbeCount++;
                continue;
            }
            if (uvObs.DiagnosticState != InstrumentDiagnosticState.RegionSampled || !uvObs.HasUv)
                continue;

            float u = uvObs.Uv.X;
            float v = uvObs.Uv.Y;
            if (!float.IsFinite(u) || !float.IsFinite(v) || u < 0f || u >= 1f || v < 0f || v > 1f)
            {
                _invalidUvCount++;
                continue;
            }

            if (!checkerByRay.TryGetValue(rayIndex, out InstrumentObservation checkerObs))
            {
                _missingCheckerCount++;
                continue;
            }
            if (checkerObs.DiagnosticState != InstrumentDiagnosticState.RegionSampled)
                continue;

            // UV agreement: both instruments compute UV from the same context; values must agree.
            if (checkerObs.HasUv)
            {
                float du = MathF.Abs(u - checkerObs.Uv.X);
                float dv = MathF.Abs(v - checkerObs.Uv.Y);
                if (du > UvAgreementTolerance || dv > UvAgreementTolerance)
                    _uvMismatchCount++;
            }

            // Independent chain verification: derive checker from SurfaceUv UV, not from position.
            if (!CheckerProbeMath.TryIsDark(u, v, CheckerTilesU, CheckerTilesV, out bool expectedDark))
            {
                _invalidUvCount++;
                continue;
            }

            if (expectedDark != checkerObs.SampleValue)
                _checkerMismatchCount++;

            _pairedCount++;
            if (checkerObs.SampleValue) _darkCount++;
            else _lightCount++;
        }

        // Track orphaned checker observations (CheckerProbe without a SurfaceUv partner).
        foreach (int rayIndex in checkerByRay.Keys)
        {
            if (!surfaceByRay.ContainsKey(rayIndex))
                _missingSurfaceCount++;
        }
    }

    private void Finish(bool pass, string reason)
    {
        if (_finished)
            return;
        _finished = true;

        GD.Print(
            $"[OI-005] {(pass ? "PASS" : "FAIL")} reason={reason} " +
            $"surface={_surfaceObservationCount} checker={_checkerObservationCount} " +
            $"paired={_pairedCount} " +
            $"missingChecker={_missingCheckerCount} missingSurface={_missingSurfaceCount} " +
            $"light={_lightCount} dark={_darkCount} " +
            $"uvMismatch={_uvMismatchCount} checkerMismatch={_checkerMismatchCount} " +
            $"invalidUv={_invalidUvCount} unresolvedProbe={_unresolvedValidProbeCount} " +
            $"flag={_flagObservationCount} " +
            $"overflow={(_overflow ? 1 : 0)} dropped={_dropped}");
        GetTree().Quit(pass ? 0 : 1);
    }
}
