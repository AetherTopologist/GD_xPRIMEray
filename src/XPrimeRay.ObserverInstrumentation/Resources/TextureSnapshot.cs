using SystemMath = System.Math;

namespace XPrimeRay.ObserverInstrumentation.Resources;

public sealed class TextureSnapshot
{
    private readonly byte[] _pixels;

    private TextureSnapshot(TextureProvenance provenance, byte[] pixels)
    {
        Provenance = provenance;
        _pixels    = pixels;
    }

    public TextureAssetId     AssetId    => Provenance.AssetId;
    public TextureProvenance  Provenance { get; }
    public int                Width      => Provenance.Width;
    public int                Height     => Provenance.Height;
    public TexturePixelFormat Format     => Provenance.Format;
    public ColorSpace         ColorSpace => Provenance.ColorSpace;

    public static bool TryCreate(
        TextureProvenance? provenance,
        byte[]? pixels,
        out TextureSnapshot? snapshot)
    {
        snapshot = null;
        if (provenance == null || !provenance.IsValid) return false;
        if (pixels == null) return false;
        int bpp = BytesPerPixel(provenance.Format);
        if (bpp <= 0) return false;
        int expected = provenance.Width * provenance.Height * bpp;
        if (pixels.Length != expected) return false;

        var copy = new byte[pixels.Length];
        pixels.AsSpan().CopyTo(copy);
        snapshot = new TextureSnapshot(provenance, copy);
        return true;
    }

    // Hot-path sampling — pure math, zero alloc, no Godot types.
    public bool TryGetPixel(float u, float v, TextureSamplingPolicy policy, out SampledColor color)
    {
        color = default;
        if (!float.IsFinite(u) || !float.IsFinite(v)) return false;
        if (policy.FilterMode != TextureFilterMode.Nearest) return false;
        if (policy.ColorInterpretation != ColorInterpretation.Raw) return false;

        float wu = ApplyWrap(u, policy.WrapMode);
        float wv = ApplyWrap(v, policy.WrapMode);
        if (!float.IsFinite(wu) || !float.IsFinite(wv)) return false;

        int px = SystemMath.Clamp((int)(wu * Width),  0, Width  - 1);
        int py = SystemMath.Clamp((int)(wv * Height), 0, Height - 1);
        return TryGetPixelRaw(px, py, out color);
    }

    internal bool TryGetPixelRaw(int x, int y, out SampledColor color)
    {
        color = default;
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        int bpp    = BytesPerPixel(Format);
        int offset = (y * Width + x) * bpp;
        const float inv = 1f / 255f;
        if (Format == TexturePixelFormat.Rgba8)
        {
            color = new SampledColor(
                _pixels[offset]     * inv,
                _pixels[offset + 1] * inv,
                _pixels[offset + 2] * inv,
                _pixels[offset + 3] * inv);
            return true;
        }
        if (Format == TexturePixelFormat.Rgb8)
        {
            color = new SampledColor(
                _pixels[offset]     * inv,
                _pixels[offset + 1] * inv,
                _pixels[offset + 2] * inv,
                1f);
            return true;
        }
        return false;
    }

    private static float ApplyWrap(float t, TextureWrapMode mode) => mode switch
    {
        TextureWrapMode.Clamp          => SystemMath.Clamp(t, 0f, 1f),
        TextureWrapMode.Repeat         => t - MathF.Floor(t),
        TextureWrapMode.MirroredRepeat => ApplyMirroredRepeat(t),
        _                              => float.NaN,
    };

    private static float ApplyMirroredRepeat(float t)
    {
        float s = t - 2f * MathF.Floor(t * 0.5f);
        return s > 1f ? 2f - s : s;
    }

    private static int BytesPerPixel(TexturePixelFormat format) => format switch
    {
        TexturePixelFormat.Rgba8 => 4,
        TexturePixelFormat.Rgb8  => 3,
        _                        => 0,
    };
}
