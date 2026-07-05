using Godot;
using System;
using System.Collections.Generic;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Math;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Runtime;
using NumericsVector3 = System.Numerics.Vector3;

// OI-006 Checker Diagnostic PNG
// Extends the OI-005 checker observation chain and converts the observation
// records into a headless diagnostic PNG.
//
// Coordinate safety guarantee:
//   Before writing any pixel, the coord spans (GetDebugRayPixelX/YForTesting)
//   are validated: their length must be >= maxRayIndex+1 across all observations,
//   and no observation RayIndex may fall outside the spans. Failure is loud.
public partial class Oi006CheckerDiagnosticHeadless : Node
{
    private const string ProbeName = "uv_probe";
    private const int MinimumValidPairs = 16;
    private const int TimeoutFrames = 300;
    private const int CheckerTilesU = 8;
    private const int CheckerTilesV = 4;
    private const float UvAgreementTolerance = 1e-6f;
    private const string OutputRelPath = "res://output/observer_instrumentation/oi_006_checker_diagnostic.png";

    private GrinFilmCamera _film;
    private ulong _lastSequence;
    private int _frames;

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

                if (pass)
                {
                    // Take the coord spans and observation snapshot at the same tick
                    // before _dbgHits is overwritten by the next frame.
                    ReadOnlySpan<int> pxSpan = _film.GetDebugRayPixelXForTesting();
                    ReadOnlySpan<int> pySpan = _film.GetDebugRayPixelYForTesting();
                    string pngFailReason = WriteDiagnosticPng(observations, pxSpan, pySpan);
                    if (pngFailReason != null)
                    {
                        Finish(false, pngFailReason);
                        return;
                    }
                }

                Finish(pass, pass ? "acceptance met" : "acceptance failed");
                return;
            }
        }

        if (_frames >= TimeoutFrames)
            Finish(false, "timeout waiting for valid observation pairs");
    }

    private void ProcessFrame(ReadOnlySpan<InstrumentObservation> observations)
    {
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

            if (checkerObs.HasUv)
            {
                float du = MathF.Abs(u - checkerObs.Uv.X);
                float dv = MathF.Abs(v - checkerObs.Uv.Y);
                if (du > UvAgreementTolerance || dv > UvAgreementTolerance)
                    _uvMismatchCount++;
            }

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

        foreach (int rayIndex in checkerByRay.Keys)
        {
            if (!surfaceByRay.ContainsKey(rayIndex))
                _missingSurfaceCount++;
        }
    }

    // Returns null on success, or an error reason string on failure.
    // Observations and coord spans must be from the same _Process tick.
    private string WriteDiagnosticPng(
        ReadOnlySpan<InstrumentObservation> observations,
        ReadOnlySpan<int> pxSpan,
        ReadOnlySpan<int> pySpan)
    {
        int filmW = _film.Width;
        int filmH = _film.Height;
        int coordCount = pxSpan.Length;

        if (pxSpan.Length != pySpan.Length)
            return $"coord-span-length-mismatch px={pxSpan.Length} py={pySpan.Length}";

        // Find the maximum RayIndex referenced by any observation.
        int maxRayIndex = -1;
        for (int i = 0; i < observations.Length; i++)
            if (observations[i].RayIndex > maxRayIndex)
                maxRayIndex = observations[i].RayIndex;

        // Every referenced RayIndex must have a coord entry.
        // coordCount == _dbgRayCount by construction; maxRayIndex+1 <= coordCount must hold.
        if (maxRayIndex >= coordCount)
            return $"coord-span-too-short coordCount={coordCount} maxRayIndex={maxRayIndex}";

        // Validate all individual RayIndex values.
        for (int i = 0; i < observations.Length; i++)
        {
            int ri = observations[i].RayIndex;
            if (ri < 0 || ri >= coordCount)
                return $"RayIndex {ri} out of coord span [0,{coordCount})";
        }

        // Build lookup: rayIndex → (surfaceUv obs, checkerProbe obs)
        var surfaceByRay = new Dictionary<int, InstrumentObservation>();
        var checkerByRay = new Dictionary<int, InstrumentObservation>();
        for (int i = 0; i < observations.Length; i++)
        {
            ref readonly InstrumentObservation obs = ref observations[i];
            if (obs.Instrument == InstrumentKind.SurfaceUv)
                surfaceByRay[obs.RayIndex] = obs;
            else if (obs.Instrument == InstrumentKind.CheckerProbe)
                checkerByRay[obs.RayIndex] = obs;
        }

        // Allocate image; default fill = transparent (non-surface or unregistered ray).
        Image img = Image.CreateEmpty(filmW, filmH, false, Image.Format.Rgba8);
        img.Fill(new Color(0f, 0f, 0f, 0f));

        // Paint each debug ray slot.
        for (int ri = 0; ri < coordCount; ri++)
        {
            int px = pxSpan[ri];
            int py = pySpan[ri];

            if (px < 0 || px >= filmW || py < 0 || py >= filmH)
                return $"pixel coord out of film bounds rayIndex={ri} px={px} py={py} filmW={filmW} filmH={filmH}";

            Color color;
            bool hasSurface = surfaceByRay.TryGetValue(ri, out InstrumentObservation surfObs);
            bool hasChecker = checkerByRay.TryGetValue(ri, out InstrumentObservation chkObs);

            if (!hasSurface && !hasChecker)
            {
                // Non-surface ray (miss, absorbed, non-probe hit) — gray
                color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else if (hasSurface &&
                     surfObs.ColliderName == ProbeName &&
                     surfObs.DiagnosticState == InstrumentDiagnosticState.DiagnosticUnresolved)
            {
                // Probe hit but UV unresolved — magenta
                color = new Color(1f, 0f, 1f, 1f);
            }
            else if (hasChecker &&
                     chkObs.DiagnosticState == InstrumentDiagnosticState.RegionSampled)
            {
                // Resolved checker observation — black (dark) or white (light)
                color = chkObs.SampleValue
                    ? new Color(0f, 0f, 0f, 1f)
                    : new Color(1f, 1f, 1f, 1f);
            }
            else if (hasSurface &&
                     surfObs.ColliderName == ProbeName &&
                     surfObs.DiagnosticState == InstrumentDiagnosticState.RegionSampled)
            {
                // Probe surface hit with UV but checker missing — magenta
                color = new Color(1f, 0f, 1f, 1f);
            }
            else
            {
                // Other geometry hit — gray
                color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            img.SetPixel(px, py, color);
        }

        string dir = ProjectSettings.GlobalizePath("res://output/observer_instrumentation");
        DirAccess.MakeDirRecursiveAbsolute(dir);
        string globalPath = ProjectSettings.GlobalizePath(OutputRelPath);
        Error saveErr = img.SavePng(globalPath);
        if (saveErr != Error.Ok)
            return $"SavePng failed err={saveErr} path={globalPath}";

        GD.Print($"[OI-006] PNG written: {globalPath} ({filmW}x{filmH}) coordCount={coordCount} maxRayIndex={maxRayIndex}");
        return null;
    }

    private void Finish(bool pass, string reason)
    {
        if (_finished)
            return;
        _finished = true;

        GD.Print(
            $"[OI-006] {(pass ? "PASS" : "FAIL")} reason={reason} " +
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
