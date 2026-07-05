using Godot;
using System;
using System.Globalization;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Math;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Runtime;
using NumericsVector3 = System.Numerics.Vector3;

public partial class Oi001EquatorHeadless : Node
{
    private const string ProbeName = "uv_probe";
    private const int MinimumValidObservations = 16;
    private const int TimeoutFrames = 300;
    private const float EquatorHalfBand = 0.1f;

    private GrinFilmCamera _film;
    private ulong _lastSequence;
    private int _frames;
    private int _surfaceObservationCount;
    private int _validProbeCount;
    private int _inBandCount;
    private int _unresolvedProbeCount;
    private int _unresolvedAnyCount;
    private int _otherGeometryCount;
    private int _nonSurfaceCount;
    private int _otherInstrumentCount;
    private int _invalidUvCount;
    private double _vSum;
    private float _vMin = float.PositiveInfinity;
    private float _vMax = float.NegativeInfinity;
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
        if (!UvRevealRegion.TryCreate(0f, 1f, 0f, 1f, out UvRevealRegion inertRegion) ||
            !InstrumentTargetMetadata.TryCreate(
                ProbeName,
                NumericsVector3.Zero,
                1,
                1,
                inertRegion,
                out InstrumentTargetMetadata metadata))
        {
            Finish(false, "metadata construction failed");
            return;
        }

        var configuration = new ObserverInstrumentationConfiguration
        {
            EnabledMask = ObserverInstrumentMask.SurfaceUv,
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
            Accumulate(observations);
            _overflow |= _film.InstrumentationOverflowForTesting;
            _dropped += _film.InstrumentationDroppedObservationCountForTesting;

            if (_validProbeCount >= MinimumValidObservations)
            {
                double inBandFraction = (double)_inBandCount / _validProbeCount;
                bool pass =
                    _unresolvedProbeCount == 0 &&
                    _otherInstrumentCount == 0 &&
                    _invalidUvCount == 0 &&
                    !_overflow &&
                    _dropped == 0 &&
                    inBandFraction >= 0.9;
                Finish(pass, pass ? "acceptance met" : "acceptance failed");
                return;
            }
        }

        if (_frames >= TimeoutFrames)
            Finish(false, "timeout waiting for valid probe observations");
    }

    private void Accumulate(ReadOnlySpan<InstrumentObservation> observations)
    {
        for (int index = 0; index < observations.Length; index++)
        {
            ref readonly InstrumentObservation observation = ref observations[index];
            if (observation.Instrument != InstrumentKind.SurfaceUv)
            {
                _otherInstrumentCount++;
                continue;
            }

            if (_surfaceObservationCount == 0)
            {
                GD.Print(
                    $"[OI-001][FirstObservation] state={observation.DiagnosticState} " +
                    $"collider={observation.ColliderName ?? "<null>"} hasUv={(observation.HasUv ? 1 : 0)}");
            }
            _surfaceObservationCount++;
            if (observation.ColliderName == ProbeName &&
                observation.DiagnosticState == InstrumentDiagnosticState.DiagnosticUnresolved)
            {
                _unresolvedProbeCount++;
            }
            if (observation.DiagnosticState == InstrumentDiagnosticState.DiagnosticUnresolved)
                _unresolvedAnyCount++;
            else if (observation.DiagnosticState == InstrumentDiagnosticState.OtherGeometryHit)
                _otherGeometryCount++;
            else if (observation.DiagnosticState == InstrumentDiagnosticState.TransportClassNotSurfaceHit)
                _nonSurfaceCount++;

            if (observation.ColliderName != ProbeName ||
                observation.DiagnosticState != InstrumentDiagnosticState.RegionSampled ||
                !observation.HasUv)
            {
                continue;
            }

            float u = observation.Uv.X;
            float v = observation.Uv.Y;
            if (!float.IsFinite(u) || !float.IsFinite(v) || u < 0f || u >= 1f || v < 0f || v > 1f)
            {
                _invalidUvCount++;
                continue;
            }

            _validProbeCount++;
            _vSum += v;
            _vMin = MathF.Min(_vMin, v);
            _vMax = MathF.Max(_vMax, v);
            if (MathF.Abs(v - 0.5f) <= EquatorHalfBand)
                _inBandCount++;
        }
    }

    private void Finish(bool pass, string reason)
    {
        if (_finished)
            return;
        _finished = true;

        double meanV = _validProbeCount > 0 ? _vSum / _validProbeCount : double.NaN;
        double inBandPercent = _validProbeCount > 0
            ? 100.0 * _inBandCount / _validProbeCount
            : 0.0;
        string minV = _validProbeCount > 0 ? _vMin.ToString("F6", CultureInfo.InvariantCulture) : "na";
        string maxV = _validProbeCount > 0 ? _vMax.ToString("F6", CultureInfo.InvariantCulture) : "na";
        string mean = double.IsFinite(meanV) ? meanV.ToString("F6", CultureInfo.InvariantCulture) : "na";

        GD.Print(
            $"[OI-001] {(pass ? "PASS" : "FAIL")} reason={reason} " +
            $"surface={_surfaceObservationCount} valid={_validProbeCount} " +
            $"vMin={minV} vMax={maxV} vMean={mean} " +
            $"vBand={_inBandCount}/{_validProbeCount} ({inBandPercent:F2}%) " +
            $"unresolvedProbe={_unresolvedProbeCount} unresolvedAny={_unresolvedAnyCount} " +
            $"otherGeometry={_otherGeometryCount} nonSurface={_nonSurfaceCount} otherInstrument={_otherInstrumentCount} " +
            $"invalidUv={_invalidUvCount} overflow={(_overflow ? 1 : 0)} dropped={_dropped}");
        GetTree().Quit(pass ? 0 : 1);
    }
}
