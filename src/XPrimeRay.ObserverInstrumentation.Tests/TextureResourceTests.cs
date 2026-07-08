using System;
using XPrimeRay.ObserverInstrumentation.Resources;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class TextureResourceTests
{
    // --- helpers ---

    private static TextureProvenance MakeProvenance(
        TextureAssetId assetId, int w, int h,
        TexturePixelFormat fmt   = TexturePixelFormat.Rgba8,
        ColorSpace         space = ColorSpace.Linear) =>
        new()
        {
            AssetId           = assetId,
            SourceDescription = "test",
            LoadedAt          = DateTimeOffset.UtcNow,
            Width             = w,
            Height            = h,
            Format            = fmt,
            ColorSpace        = space,
        };

    // Builds an Rgba8/Linear snapshot; all pixels set to the given RGBA bytes.
    private static TextureSnapshot MakeRgba8(TextureAssetId id, int w, int h, byte r, byte g, byte b, byte a)
    {
        var prov   = MakeProvenance(id, w, h);
        var pixels = new byte[w * h * 4];
        for (int i = 0; i < w * h; i++)
        {
            pixels[i * 4 + 0] = r;
            pixels[i * 4 + 1] = g;
            pixels[i * 4 + 2] = b;
            pixels[i * 4 + 3] = a;
        }
        TextureSnapshot.TryCreate(prov, pixels, out TextureSnapshot? snap);
        return snap!;
    }

    // Builds a 2×1 Rgba8/Linear snapshot: pixel 0 = left, pixel 1 = right.
    private static TextureSnapshot Make2x1(
        TextureAssetId id,
        (byte r, byte g, byte b, byte a) left,
        (byte r, byte g, byte b, byte a) right)
    {
        var prov   = MakeProvenance(id, 2, 1);
        var pixels = new byte[]
        {
            left.r,  left.g,  left.b,  left.a,
            right.r, right.g, right.b, right.a,
        };
        TextureSnapshot.TryCreate(prov, pixels, out TextureSnapshot? snap);
        return snap!;
    }

    private static TextureResourceBinding MakeBinding(TextureAssetId id, TextureSlotId slot) =>
        new()
        {
            AssetId        = id,
            SlotId         = slot,
            SamplingPolicy = TextureSamplingPolicy.NearestClamp,
            UvChannel      = 0,
        };

    private static readonly TextureAssetId IdA  = new("tex_a");
    private static readonly TextureAssetId IdB  = new("tex_b");
    private static readonly TextureSlotId  Base = TextureSlotId.BaseColor;
    private static readonly TextureSlotId  Norm = new("normal");

    // --- test suite entry point ---

    public static void Run()
    {
        ProvenanceIsValid();
        ProvenanceColorSpaceRequired();
        SnapshotTryCreate_Success();
        SnapshotTryCreate_WrongPixelCount();
        SnapshotTryCreate_NullProvenance();
        TryGetPixel_NearestClamp_Center();
        TryGetPixel_NonFiniteUv();
        TryGetPixel_UnsupportedFilterMode();
        TryGetPixel_UnsupportedColorInterpretation();
        TryGetPixel_RepeatWrap();
        TryGetPixel_MirroredRepeat();
        BindingCatalog_SingleSlotLookup();
        BindingCatalog_MultiSlotSameCollider();
        BindingCatalog_DuplicateSlotRejected();
        BindingCatalog_BlankNameRejected();
        BindingIsValid_UnsupportedUvChannel();
        ResourceSnapshot_TryGetTexture();
        ResourceSnapshot_DuplicateRejected();
        ResourceSnapshot_EmptyHasCount0();
        ResourceSnapshot_InvalidAssetId();
    }

    // --- provenance ---

    private static void ProvenanceIsValid()
    {
        TestAssert.True(MakeProvenance(IdA, 4, 4).IsValid,  "valid provenance must be IsValid");
        TestAssert.False(MakeProvenance(IdA, 0, 4).IsValid,  "zero Width must be invalid");
        TestAssert.False(MakeProvenance(IdA, 4, 0).IsValid,  "zero Height must be invalid");
        TestAssert.False(MakeProvenance(TextureAssetId.None, 4, 4).IsValid, "empty AssetId must be invalid");

        var unknownFmt = new TextureProvenance
        { AssetId = IdA, Width = 4, Height = 4, ColorSpace = ColorSpace.Linear };
        TestAssert.False(unknownFmt.IsValid, "Unknown format must be invalid");
    }

    private static void ProvenanceColorSpaceRequired()
    {
        // Default ColorSpace is Unknown → invalid.
        var noSpace = new TextureProvenance
        { AssetId = IdA, Width = 4, Height = 4, Format = TexturePixelFormat.Rgba8 };
        TestAssert.False(noSpace.IsValid, "Unknown ColorSpace must be invalid");

        TestAssert.True(MakeProvenance(IdA, 4, 4, TexturePixelFormat.Rgba8, ColorSpace.Linear).IsValid,
            "Linear ColorSpace must be valid");
        TestAssert.True(MakeProvenance(IdA, 4, 4, TexturePixelFormat.Rgba8, ColorSpace.Srgb).IsValid,
            "Srgb ColorSpace must be valid");
    }

    // --- snapshot creation ---

    private static void SnapshotTryCreate_Success()
    {
        var prov   = MakeProvenance(IdA, 2, 2);
        var pixels = new byte[2 * 2 * 4];
        bool ok    = TextureSnapshot.TryCreate(prov, pixels, out TextureSnapshot? snap);
        TestAssert.True(ok,         "valid provenance + correct pixel count must succeed");
        TestAssert.True(snap != null, "snapshot must be non-null on success");
        TestAssert.Equal(2, snap!.Width,            "snapshot width");
        TestAssert.Equal(2, snap.Height,             "snapshot height");
        TestAssert.Equal(ColorSpace.Linear, snap.ColorSpace, "snapshot color space");
    }

    private static void SnapshotTryCreate_WrongPixelCount()
    {
        var prov = MakeProvenance(IdA, 2, 2);
        TestAssert.False(TextureSnapshot.TryCreate(prov, new byte[3], out _),
            "wrong pixel buffer length must return false");
    }

    private static void SnapshotTryCreate_NullProvenance()
    {
        TestAssert.False(TextureSnapshot.TryCreate(null, new byte[4], out _),
            "null provenance must return false");
    }

    // --- TryGetPixel ---

    private static void TryGetPixel_NearestClamp_Center()
    {
        var snap = MakeRgba8(IdA, 1, 1, 255, 255, 255, 255);

        bool ok = snap.TryGetPixel(0.5f, 0.5f, TextureSamplingPolicy.NearestClamp, out SampledColor c);
        TestAssert.True(ok, "valid UV on valid snapshot must return true");
        TestAssert.Equal(SampledColor.White, c, "1x1 white pixel at center");

        bool ok2 = snap.TryGetPixel(0f, 0f, TextureSamplingPolicy.NearestClamp, out SampledColor c2);
        TestAssert.True(ok2, "corner (0,0) must succeed");
        TestAssert.Equal(SampledColor.White, c2, "1x1 white pixel at corner");
    }

    private static void TryGetPixel_NonFiniteUv()
    {
        var snap = MakeRgba8(IdA, 1, 1, 128, 128, 128, 255);
        TestAssert.False(snap.TryGetPixel(float.NaN,              0.5f, TextureSamplingPolicy.NearestClamp, out _), "NaN u");
        TestAssert.False(snap.TryGetPixel(0.5f,              float.NaN, TextureSamplingPolicy.NearestClamp, out _), "NaN v");
        TestAssert.False(snap.TryGetPixel(float.PositiveInfinity, 0.5f, TextureSamplingPolicy.NearestClamp, out _), "Inf u");
        TestAssert.False(snap.TryGetPixel(0.5f, float.NegativeInfinity, TextureSamplingPolicy.NearestClamp, out _), "Inf v");
    }

    private static void TryGetPixel_UnsupportedFilterMode()
    {
        var snap      = MakeRgba8(IdA, 1, 1, 128, 128, 128, 255);
        var badPolicy = new TextureSamplingPolicy(TextureWrapMode.Clamp, (TextureFilterMode)99, ColorInterpretation.Raw);
        TestAssert.False(snap.TryGetPixel(0.5f, 0.5f, badPolicy, out _),
            "unsupported FilterMode must return false (fail closed)");
    }

    private static void TryGetPixel_UnsupportedColorInterpretation()
    {
        var snap = MakeRgba8(IdA, 1, 1, 128, 128, 128, 255);

        var linearPolicy  = new TextureSamplingPolicy(TextureWrapMode.Clamp, TextureFilterMode.Nearest, ColorInterpretation.Linear);
        var srgbPolicy    = new TextureSamplingPolicy(TextureWrapMode.Clamp, TextureFilterMode.Nearest, ColorInterpretation.SrgbToLinear);
        var unknownPolicy = new TextureSamplingPolicy(TextureWrapMode.Clamp, TextureFilterMode.Nearest, (ColorInterpretation)99);

        TestAssert.False(snap.TryGetPixel(0.5f, 0.5f, linearPolicy,  out _), "Linear interpretation must fail closed");
        TestAssert.False(snap.TryGetPixel(0.5f, 0.5f, srgbPolicy,    out _), "SrgbToLinear interpretation must fail closed");
        TestAssert.False(snap.TryGetPixel(0.5f, 0.5f, unknownPolicy, out _), "Unknown cast interpretation must fail closed");
    }

    private static void TryGetPixel_RepeatWrap()
    {
        // 2×1: pixel 0 = red (255,0,0,255), pixel 1 = blue (0,0,255,255)
        var snap        = Make2x1(IdA, (255, 0, 0, 255), (0, 0, 255, 255));
        const float inv = 1f / 255f;
        var red  = new SampledColor(255 * inv, 0, 0, 255 * inv);
        var blue = new SampledColor(0, 0, 255 * inv, 255 * inv);

        // u=1.25 → Repeat → frac=0.25 → px=int(0.25*2)=0 → red
        bool ok1 = snap.TryGetPixel(1.25f, 0.5f, TextureSamplingPolicy.NearestRepeat, out SampledColor c1);
        TestAssert.True(ok1, "repeat u=1.25 must succeed");
        TestAssert.Equal(red, c1, "repeat u=1.25 must map to pixel 0 (red)");

        // u=1.75 → Repeat → frac=0.75 → px=int(0.75*2)=1 → blue
        bool ok2 = snap.TryGetPixel(1.75f, 0.5f, TextureSamplingPolicy.NearestRepeat, out SampledColor c2);
        TestAssert.True(ok2, "repeat u=1.75 must succeed");
        TestAssert.Equal(blue, c2, "repeat u=1.75 must map to pixel 1 (blue)");
    }

    private static void TryGetPixel_MirroredRepeat()
    {
        // 2×1: pixel 0 = red, pixel 1 = blue
        var snap        = Make2x1(IdA, (255, 0, 0, 255), (0, 0, 255, 255));
        const float inv = 1f / 255f;
        var blue   = new SampledColor(0, 0, 255 * inv, 255 * inv);
        var policy = new TextureSamplingPolicy(TextureWrapMode.MirroredRepeat, TextureFilterMode.Nearest, ColorInterpretation.Raw);

        // u=1.3 → s=1.3-2*floor(0.65)=1.3 → >1 → 2-1.3=0.7 → px=int(0.7*2)=1 → blue
        bool ok = snap.TryGetPixel(1.3f, 0.5f, policy, out SampledColor c);
        TestAssert.True(ok, "mirrored repeat u=1.3 must succeed");
        TestAssert.Equal(blue, c, "u=1.3 mirrored to 0.7 must yield pixel 1 (blue)");
    }

    // --- binding catalog ---

    private static void BindingCatalog_SingleSlotLookup()
    {
        var binding = MakeBinding(IdA, Base);
        bool built  = TextureBindingCatalog.TryCreate(
            new[] { ("sphere", binding) }, out TextureBindingCatalog? cat, out _);
        TestAssert.True(built, "single valid binding must build");
        TestAssert.True(cat != null, "catalog must be non-null on success");
        TestAssert.Equal(1, cat!.Count, "count must be 1");

        bool hit = cat.TryGetBinding("sphere", Base, out TextureResourceBinding? result);
        TestAssert.True(hit, "registered (collider, slot) must be found");
        TestAssert.Equal(IdA, result!.AssetId, "binding asset id must match");

        TestAssert.False(cat.TryGetBinding("sphere", Norm, out _), "unregistered slot must not be found");
        TestAssert.False(cat.TryGetBinding("unknown", Base, out _), "unregistered collider must not be found");
    }

    private static void BindingCatalog_MultiSlotSameCollider()
    {
        // Two different slots on the same collider are both legal.
        var bBase = MakeBinding(IdA, Base);
        var bNorm = MakeBinding(IdB, Norm);
        bool built = TextureBindingCatalog.TryCreate(
            new[] { ("sphere", bBase), ("sphere", bNorm) },
            out TextureBindingCatalog? cat, out _);
        TestAssert.True(built, "two different slots on same collider must be accepted");
        TestAssert.Equal(2, cat!.Count, "count must be 2 for two distinct slots");
        TestAssert.True(cat.TryGetBinding("sphere", Base, out _), "base_color slot must be found");
        TestAssert.True(cat.TryGetBinding("sphere", Norm, out _), "normal slot must be found");
    }

    private static void BindingCatalog_DuplicateSlotRejected()
    {
        var binding = MakeBinding(IdA, Base);
        bool built  = TextureBindingCatalog.TryCreate(
            new[] { ("sphere", binding), ("sphere", binding) },
            out _, out string? reason);
        TestAssert.False(built, "duplicate (collider, slot) must be rejected");
        TestAssert.True(!string.IsNullOrEmpty(reason), "error reason must be non-empty on rejection");
    }

    private static void BindingCatalog_BlankNameRejected()
    {
        bool built = TextureBindingCatalog.TryCreate(
            new[] { ("", MakeBinding(IdA, Base)) }, out _, out _);
        TestAssert.False(built, "blank collider name must be rejected");
    }

    private static void BindingIsValid_UnsupportedUvChannel()
    {
        var binding = new TextureResourceBinding
        { AssetId = IdA, SlotId = Base, SamplingPolicy = TextureSamplingPolicy.NearestClamp, UvChannel = 1 };
        TestAssert.False(binding.IsValid, "UvChannel != 0 must be invalid (fail closed)");
    }

    // --- resource snapshot ---

    private static void ResourceSnapshot_TryGetTexture()
    {
        var snap  = MakeRgba8(IdA, 1, 1, 0, 0, 0, 255);
        bool built = TextureResourceSnapshot.TryBuild(new[] { snap }, out TextureResourceSnapshot? res, out _);
        TestAssert.True(built, "single snapshot must build");
        TestAssert.Equal(1, res!.Count, "count must be 1");

        TestAssert.True(res.TryGetTexture(IdA, out TextureSnapshot? found), "registered AssetId must be found");
        TestAssert.True(found != null, "found texture must be non-null");
        TestAssert.False(res.TryGetTexture(IdB, out _), "unregistered AssetId must not be found");
    }

    private static void ResourceSnapshot_DuplicateRejected()
    {
        var snap1 = MakeRgba8(IdA, 1, 1, 0,   0, 0, 255);
        var snap2 = MakeRgba8(IdA, 1, 1, 255, 0, 0, 255); // same AssetId
        bool built = TextureResourceSnapshot.TryBuild(new[] { snap1, snap2 }, out _, out string? reason);
        TestAssert.False(built, "duplicate AssetId must be rejected");
        TestAssert.True(!string.IsNullOrEmpty(reason), "error reason must be non-empty on rejection");
    }

    private static void ResourceSnapshot_EmptyHasCount0()
    {
        TestAssert.Equal(0, TextureResourceSnapshot.Empty.Count, "Empty.Count must be 0");
        TestAssert.False(TextureResourceSnapshot.Empty.TryGetTexture(IdA, out _),
            "Empty resolver must return false for any id");
    }

    private static void ResourceSnapshot_InvalidAssetId()
    {
        var snap = MakeRgba8(IdA, 1, 1, 0, 0, 0, 255);
        TextureResourceSnapshot.TryBuild(new[] { snap }, out TextureResourceSnapshot? res, out _);
        TestAssert.False(res!.TryGetTexture(TextureAssetId.None, out _),
            "None AssetId must return false");
    }
}
