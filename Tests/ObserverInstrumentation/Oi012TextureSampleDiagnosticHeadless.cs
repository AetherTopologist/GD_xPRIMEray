using Godot;
using System;
using System.Globalization;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Math;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Resources;
using XPrimeRay.ObserverInstrumentation.Runtime;
using NumericsVector3 = System.Numerics.Vector3;

public partial class Oi012TextureSampleDiagnosticHeadless : Node
{
    private const string ProbeName = "texture_probe";
    private const int MinimumValidSamples = 64;
    private const int TimeoutFrames = 300;
    private const int TextureWidth = 16;
    private const int TextureHeight = 16;
    private const int MaxRaySlots = 4096;
    private const int UpscaleFactor = 16;
    private const string SourceOutputRelPath = "res://output/observer_instrumentation/oi_012_texture_sample_diagnostic.png";
    private const string UpscaledOutputRelPath = "res://output/observer_instrumentation/oi_012_texture_sample_diagnostic_upscaled.png";

    private readonly InstrumentObservation[] _surfaceByRay = new InstrumentObservation[MaxRaySlots];
    private readonly bool[] _hasSurfaceByRay = new bool[MaxRaySlots];

    private GrinFilmCamera _film;
    private byte[] _textureBytes;
    private TextureAssetId _assetId;
    private ulong _lastSequence;
    private int _frames;
    private int _surfaceObservations;
    private int _textureObservations;
    private int _samples;
    private int _mismatches;
    private int _unresolved;
    private int _texelOutOfBounds;
    private int _missingSurface;
    private int _invalidUv;
    private int _unexpectedInstrument;
    private int _maxChannelDelta;
    private long _sumChannelDelta;
    private int _channelComparisons;
    private bool _overflow;
    private int _dropped;
    private bool _finished;
    private string _firstMismatch;
    private string _sourcePngPath;
    private string _upscaledPngPath;
    private int _writtenPixels;
    private int _uniquePixels;
    private int _transparentPixels;
    private int _magentaPixels;
    private int _grayPixels;

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
                1,
                1,
                fullRegion,
                out InstrumentTargetMetadata metadata))
        {
            Finish(false, "metadata construction failed");
            return;
        }

        _assetId = new TextureAssetId("oi_012_cpu_source_texture");
        _textureBytes = BuildTextureBytes(TextureWidth, TextureHeight);
        var provenance = new TextureProvenance
        {
            AssetId = _assetId,
            SourceDescription = "OI-012 deterministic CPU source",
            LoadedAt = DateTimeOffset.UnixEpoch,
            Width = TextureWidth,
            Height = TextureHeight,
            Format = TexturePixelFormat.Rgba8,
            ColorSpace = ColorSpace.Linear
        };
        if (!TextureSnapshot.TryCreate(provenance, _textureBytes, out TextureSnapshot snapshot))
        {
            Finish(false, "texture snapshot construction failed");
            return;
        }

        var binding = new TextureResourceBinding
        {
            AssetId = _assetId,
            SlotId = TextureSlotId.BaseColor,
            SamplingPolicy = TextureSamplingPolicy.NearestClamp,
            UvChannel = 0
        };
        string bindingError = null;
        string resourceError = null;
        if (!TextureBindingCatalog.TryCreate(
                new[] { (ProbeName, binding) },
                out TextureBindingCatalog bindings,
                out bindingError) ||
            !TextureResourceSnapshot.TryBuild(
                new[] { snapshot },
                out TextureResourceSnapshot resources,
                out resourceError))
        {
            Finish(false, $"texture catalog construction failed binding={bindingError ?? "ok"} resource={resourceError ?? "ok"}");
            return;
        }

        var configuration = new ObserverInstrumentationConfiguration
        {
            EnabledMask = ObserverInstrumentMask.SurfaceUv | ObserverInstrumentMask.TextureSample,
            Catalog = new InstrumentMetadataCatalog(metadata),
            TextureBindings = bindings,
            TextureResources = resources
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
            ReadOnlySpan<TextureSampleObservation> textureSamples =
                _film.GetInstrumentationTextureSamplesForTesting();

            ProcessFrame(observations, textureSamples);
            _overflow |= _film.InstrumentationOverflowForTesting;
            _dropped += _film.InstrumentationDroppedObservationCountForTesting;

            if (_samples >= MinimumValidSamples)
            {
                bool parityPass =
                    _mismatches == 0 &&
                    _unresolved == 0 &&
                    _texelOutOfBounds == 0 &&
                    _missingSurface == 0 &&
                    _invalidUv == 0 &&
                    _unexpectedInstrument == 0 &&
                    !_overflow &&
                    _dropped == 0;
                if (!parityPass)
                {
                    Finish(false, "oi-011 parity gate failed");
                    return;
                }

                ReadOnlySpan<int> pxSpan = _film.GetDebugRayPixelXForTesting();
                ReadOnlySpan<int> pySpan = _film.GetDebugRayPixelYForTesting();
                string pngReason = WriteDiagnosticPng(observations, textureSamples, pxSpan, pySpan);
                if (pngReason != null)
                {
                    Finish(false, pngReason);
                    return;
                }

                Finish(true, "acceptance met");
                return;
            }
        }

        if (_frames >= TimeoutFrames)
            Finish(false, "timeout waiting for texture samples");
    }

    private void ProcessFrame(
        ReadOnlySpan<InstrumentObservation> observations,
        ReadOnlySpan<TextureSampleObservation> textureSamples)
    {
        Array.Clear(_hasSurfaceByRay, 0, _hasSurfaceByRay.Length);

        for (int i = 0; i < observations.Length; i++)
        {
            ref readonly InstrumentObservation observation = ref observations[i];
            switch (observation.Instrument)
            {
                case InstrumentKind.SurfaceUv:
                    _surfaceObservations++;
                    if ((uint)observation.RayIndex < (uint)_surfaceByRay.Length)
                    {
                        _surfaceByRay[observation.RayIndex] = observation;
                        _hasSurfaceByRay[observation.RayIndex] = true;
                    }
                    break;
                case InstrumentKind.TextureSample:
                    _textureObservations++;
                    if (observation.ColliderName == ProbeName &&
                        observation.DiagnosticState == InstrumentDiagnosticState.DiagnosticUnresolved)
                    {
                        _unresolved++;
                    }
                    break;
                default:
                    _unexpectedInstrument++;
                    break;
            }
        }

        for (int i = 0; i < textureSamples.Length; i++)
        {
            ref readonly TextureSampleObservation sample = ref textureSamples[i];
            if (sample.DiagnosticState != InstrumentDiagnosticState.RegionSampled)
            {
                _unresolved++;
                continue;
            }

            if ((uint)sample.RayIndex >= (uint)_surfaceByRay.Length ||
                !_hasSurfaceByRay[sample.RayIndex])
            {
                RecordMismatch(sample, "missing surface observation", -1, -1, default, default);
                _missingSurface++;
                continue;
            }

            InstrumentObservation surface = _surfaceByRay[sample.RayIndex];
            if (surface.ColliderName != ProbeName ||
                surface.DiagnosticState != InstrumentDiagnosticState.RegionSampled ||
                !surface.HasUv ||
                !float.IsFinite(surface.Uv.X) ||
                !float.IsFinite(surface.Uv.Y))
            {
                _invalidUv++;
                continue;
            }

            if (sample.AssetId != _assetId ||
                sample.SlotId != TextureSlotId.BaseColor ||
                sample.MipLevel != 0 ||
                sample.SamplingPolicy != TextureSamplingPolicy.NearestClamp)
            {
                RecordMismatch(sample, "unexpected texture identity or policy", -1, -1, default, default);
                continue;
            }

            if (!TryReferenceSample(
                    surface.Uv.X,
                    surface.Uv.Y,
                    out int expectedX,
                    out int expectedY,
                    out SampledColor expectedColor,
                    out byte expectedR,
                    out byte expectedG,
                    out byte expectedB,
                    out byte expectedA))
            {
                _invalidUv++;
                continue;
            }

            bool texelMismatch = sample.TexelX != expectedX || sample.TexelY != expectedY;
            if (sample.TexelX < 0 || sample.TexelX >= TextureWidth ||
                sample.TexelY < 0 || sample.TexelY >= TextureHeight)
            {
                _texelOutOfBounds++;
            }

            byte actualR = ToByte(sample.Color.R);
            byte actualG = ToByte(sample.Color.G);
            byte actualB = ToByte(sample.Color.B);
            byte actualA = ToByte(sample.Color.A);
            int dr = Math.Abs(actualR - expectedR);
            int dg = Math.Abs(actualG - expectedG);
            int db = Math.Abs(actualB - expectedB);
            int da = Math.Abs(actualA - expectedA);
            _maxChannelDelta = Math.Max(_maxChannelDelta, Math.Max(Math.Max(dr, dg), Math.Max(db, da)));
            _sumChannelDelta += dr + dg + db + da;
            _channelComparisons += 4;

            bool colorMismatch =
                actualR != expectedR ||
                actualG != expectedG ||
                actualB != expectedB ||
                actualA != expectedA;

            if (texelMismatch || colorMismatch)
            {
                RecordMismatch(sample, "sample mismatch", expectedX, expectedY, expectedColor, sample.Color);
                continue;
            }

            _samples++;
        }
    }

    private string WriteDiagnosticPng(
        ReadOnlySpan<InstrumentObservation> observations,
        ReadOnlySpan<TextureSampleObservation> textureSamples,
        ReadOnlySpan<int> pxSpan,
        ReadOnlySpan<int> pySpan)
    {
        int filmW = _film.Width;
        int filmH = _film.Height;
        if (filmW <= 0 || filmH <= 0)
            return $"invalid film dimensions filmW={filmW} filmH={filmH}";
        if (pxSpan.Length != pySpan.Length)
            return $"coord-span-length-mismatch px={pxSpan.Length} py={pySpan.Length}";

        int maxRayIndex = -1;
        for (int i = 0; i < observations.Length; i++)
            if (observations[i].RayIndex > maxRayIndex)
                maxRayIndex = observations[i].RayIndex;
        for (int i = 0; i < textureSamples.Length; i++)
            if (textureSamples[i].RayIndex > maxRayIndex)
                maxRayIndex = textureSamples[i].RayIndex;

        if (maxRayIndex >= pxSpan.Length)
            return $"coord-span-too-short coordCount={pxSpan.Length} maxRayIndex={maxRayIndex}";

        Image img = Image.CreateEmpty(filmW, filmH, false, Image.Format.Rgba8);
        img.Fill(new Color(0f, 0f, 0f, 0f));

        var written = new bool[filmW * filmH];
        _writtenPixels = 0;
        _uniquePixels = 0;
        _magentaPixels = 0;
        _grayPixels = 0;

        for (int i = 0; i < textureSamples.Length; i++)
        {
            ref readonly TextureSampleObservation sample = ref textureSamples[i];
            if (sample.DiagnosticState != InstrumentDiagnosticState.RegionSampled)
                return $"unresolved texture sample before png rayIndex={sample.RayIndex} state={sample.DiagnosticState}";
            if ((uint)sample.RayIndex >= (uint)pxSpan.Length)
                return $"RayIndex {sample.RayIndex} out of coord span [0,{pxSpan.Length})";

            int px = pxSpan[sample.RayIndex];
            int py = pySpan[sample.RayIndex];
            if (px < 0 || px >= filmW || py < 0 || py >= filmH)
                return $"pixel coord out of film bounds rayIndex={sample.RayIndex} px={px} py={py} filmW={filmW} filmH={filmH}";

            if (!TryConvertColor(sample.Color, out Color color, out string colorReason))
                return $"invalid sampled color rayIndex={sample.RayIndex} reason={colorReason}";

            img.SetPixel(px, py, color);
            _writtenPixels++;
            int pixelIndex = py * filmW + px;
            if (!written[pixelIndex])
            {
                written[pixelIndex] = true;
                _uniquePixels++;
            }
        }

        _transparentPixels = 0;
        for (int y = 0; y < filmH; y++)
        {
            for (int x = 0; x < filmW; x++)
            {
                if (!written[y * filmW + x])
                    _transparentPixels++;
            }
        }

        string dir = ProjectSettings.GlobalizePath("res://output/observer_instrumentation");
        DirAccess.MakeDirRecursiveAbsolute(dir);
        _sourcePngPath = ProjectSettings.GlobalizePath(SourceOutputRelPath);
        Error saveErr = img.SavePng(_sourcePngPath);
        if (saveErr != Error.Ok)
            return $"source SavePng failed err={saveErr} path={_sourcePngPath}";

        int upW = filmW * UpscaleFactor;
        int upH = filmH * UpscaleFactor;
        Image upImg = Image.CreateEmpty(upW, upH, false, Image.Format.Rgba8);
        for (int oy = 0; oy < upH; oy++)
        {
            int sy = oy / UpscaleFactor;
            for (int ox = 0; ox < upW; ox++)
                upImg.SetPixel(ox, oy, img.GetPixel(ox / UpscaleFactor, sy));
        }

        _upscaledPngPath = ProjectSettings.GlobalizePath(UpscaledOutputRelPath);
        Error upErr = upImg.SavePng(_upscaledPngPath);
        if (upErr != Error.Ok)
            return $"upscaled SavePng failed err={upErr} path={_upscaledPngPath}";

        GD.Print(
            $"[OI-012] PNG source={_sourcePngPath} upscaled={_upscaledPngPath} " +
            $"sourceSize={filmW}x{filmH} upscaledSize={upW}x{upH} scale={UpscaleFactor}x " +
            $"writtenPixels={_writtenPixels} uniquePixels={_uniquePixels} transparent={_transparentPixels} " +
            $"magenta={_magentaPixels} gray={_grayPixels}");

        return null;
    }

    private static byte[] BuildTextureBytes(int width, int height)
    {
        var bytes = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int offset = (y * width + x) * 4;
                bytes[offset + 0] = (byte)((x * 17 + y * 3) & 0xff);
                bytes[offset + 1] = (byte)((x * 5 + y * 29) & 0xff);
                bytes[offset + 2] = (byte)((x * 11 + y * 13 + 37) & 0xff);
                bytes[offset + 3] = (byte)(255 - ((x * 7 + y * 19) & 0x3f));
            }
        }
        return bytes;
    }

    private bool TryReferenceSample(
        float u,
        float v,
        out int texelX,
        out int texelY,
        out SampledColor color,
        out byte r,
        out byte g,
        out byte b,
        out byte a)
    {
        texelX = 0;
        texelY = 0;
        color = default;
        r = g = b = a = 0;
        if (!float.IsFinite(u) || !float.IsFinite(v))
            return false;

        float cu = Math.Clamp(u, 0f, 1f);
        float cv = Math.Clamp(v, 0f, 1f);
        texelX = Math.Clamp((int)(cu * TextureWidth), 0, TextureWidth - 1);
        texelY = Math.Clamp((int)(cv * TextureHeight), 0, TextureHeight - 1);
        int offset = (texelY * TextureWidth + texelX) * 4;
        r = _textureBytes[offset + 0];
        g = _textureBytes[offset + 1];
        b = _textureBytes[offset + 2];
        a = _textureBytes[offset + 3];
        const float inv = 1f / 255f;
        color = new SampledColor(r * inv, g * inv, b * inv, a * inv);
        return true;
    }

    private void RecordMismatch(
        in TextureSampleObservation sample,
        string reason,
        int expectedX,
        int expectedY,
        SampledColor expected,
        SampledColor actual)
    {
        _mismatches++;
        if (_firstMismatch != null)
            return;

        _firstMismatch =
            $"reason={reason} rayIndex={sample.RayIndex} " +
            $"expectedTexel=({expectedX},{expectedY}) actualTexel=({sample.TexelX},{sample.TexelY}) " +
            $"expectedRgba={FormatColor(expected)} actualRgba={FormatColor(actual)} " +
            $"asset={sample.AssetId.Tag ?? "<null>"} slot={sample.SlotId.Name ?? "<null>"} " +
            $"policy={sample.SamplingPolicy}";
    }

    private static bool TryConvertColor(SampledColor sampled, out Color color, out string reason)
    {
        color = default;
        reason = null;
        if (!float.IsFinite(sampled.R) ||
            !float.IsFinite(sampled.G) ||
            !float.IsFinite(sampled.B) ||
            !float.IsFinite(sampled.A))
        {
            reason = "non-finite channel";
            return false;
        }
        if (sampled.R < 0f || sampled.R > 1f ||
            sampled.G < 0f || sampled.G > 1f ||
            sampled.B < 0f || sampled.B > 1f ||
            sampled.A < 0f || sampled.A > 1f)
        {
            reason = "channel outside [0,1]";
            return false;
        }

        color = new Color(sampled.R, sampled.G, sampled.B, sampled.A);
        return true;
    }

    private static byte ToByte(float value)
    {
        if (!float.IsFinite(value))
            return 0;
        return (byte)Math.Clamp((int)MathF.Round(value * 255f), 0, 255);
    }

    private static string FormatColor(SampledColor color) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"({color.R:F6},{color.G:F6},{color.B:F6},{color.A:F6})");

    private void Finish(bool pass, string reason)
    {
        if (_finished)
            return;
        _finished = true;

        double meanChannelDelta = _channelComparisons > 0
            ? (double)_sumChannelDelta / _channelComparisons
            : 0.0;

        string message =
            $"[OI-012] {(pass ? "PASS" : "FAIL")} reason={reason} " +
            $"samples={_samples} mismatches={_mismatches} unresolved={_unresolved} " +
            $"texelOutOfBounds={_texelOutOfBounds} " +
            $"maxChannelDelta={_maxChannelDelta} " +
            $"meanChannelDelta={meanChannelDelta.ToString("F6", CultureInfo.InvariantCulture)} " +
            $"overflow={(_overflow ? 1 : 0)} dropped={_dropped} " +
            $"surface={_surfaceObservations} textureObservations={_textureObservations} " +
            $"writtenPixels={_writtenPixels} uniquePixels={_uniquePixels} " +
            $"transparent={_transparentPixels} magenta={_magentaPixels} gray={_grayPixels} " +
            $"sourcePng={_sourcePngPath ?? "<none>"} upscaledPng={_upscaledPngPath ?? "<none>"}";

        if (_firstMismatch != null)
            message += $" firstMismatch=[{_firstMismatch}]";

        GD.Print(message);
        GetTree().Quit(pass ? 0 : 1);
    }
}
