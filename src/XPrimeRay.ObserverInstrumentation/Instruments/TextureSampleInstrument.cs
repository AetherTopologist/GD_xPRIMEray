using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Math;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Resources;

namespace XPrimeRay.ObserverInstrumentation.Instruments;

public sealed class TextureSampleInstrument : IObserverInstrument
{
    private readonly TextureBindingCatalog? _bindings;
    private readonly TextureResourceSnapshot? _resources;

    public TextureSampleInstrument(
        TextureBindingCatalog? bindings,
        TextureResourceSnapshot? resources)
    {
        _bindings = bindings;
        _resources = resources;
    }

    public InstrumentKind Kind => InstrumentKind.TextureSample;
    public ObserverInstrumentMask Feature => ObserverInstrumentMask.TextureSample;

    public void Observe(
        in InstrumentContext context,
        bool catalogAvailable,
        bool metadataFound,
        in InstrumentTargetMetadata metadata,
        out InstrumentObservation observation)
    {
        ObserveWithExtensions(
            in context,
            catalogAvailable,
            metadataFound,
            in metadata,
            out observation,
            out _,
            out _);
    }

    public void ObserveWithExtensions(
        in InstrumentContext context,
        bool catalogAvailable,
        bool metadataFound,
        in InstrumentTargetMetadata metadata,
        out InstrumentObservation observation,
        out TextureSampleObservation textureObservation,
        out bool hasTextureObservation)
    {
        textureObservation = default;
        hasTextureObservation = false;

        if (!InstrumentObservationFactory.TryClassifyInput(
                Kind, in context, catalogAvailable, metadataFound, out observation))
        {
            return;
        }

        if (!metadata.IsValid ||
            !SphericalUvMath.TryFromNormal(context.HitPosition - metadata.Center, out var uv))
        {
            observation = InstrumentObservationFactory.Create(
                Kind, in context, InstrumentDiagnosticState.DiagnosticUnresolved);
            return;
        }

        if (_bindings is null ||
            !_bindings.TryGetBinding(context.ColliderName!, TextureSlotId.BaseColor, out TextureResourceBinding? binding) ||
            binding is null)
        {
            observation = InstrumentObservationFactory.Create(
                Kind, in context, InstrumentDiagnosticState.DiagnosticUnresolved, uv, hasUv: true);
            return;
        }

        if (!IsSupportedPolicy(binding.SamplingPolicy) ||
            _resources is null ||
            !_resources.TryGetTexture(binding.AssetId, out TextureSnapshot? texture) ||
            texture is null ||
            !TryComputeTexel(uv.X, uv.Y, texture.Width, texture.Height, binding.SamplingPolicy, out int texelX, out int texelY) ||
            !texture.TryGetPixelRaw(texelX, texelY, out SampledColor color))
        {
            observation = InstrumentObservationFactory.Create(
                Kind, in context, InstrumentDiagnosticState.DiagnosticUnresolved, uv, hasUv: true);
            return;
        }

        textureObservation = new TextureSampleObservation(
            context.RayIndex,
            InstrumentDiagnosticState.RegionSampled,
            color,
            binding.AssetId,
            binding.SlotId,
            texelX,
            texelY,
            mipLevel: 0,
            binding.SamplingPolicy);
        hasTextureObservation = true;
        observation = InstrumentObservationFactory.Create(
            Kind, in context, InstrumentDiagnosticState.RegionSampled, uv, hasUv: true);
    }

    private static bool IsSupportedPolicy(TextureSamplingPolicy policy) =>
        policy.FilterMode == TextureFilterMode.Nearest &&
        policy.ColorInterpretation == ColorInterpretation.Raw;

    private static bool TryComputeTexel(
        float u,
        float v,
        int width,
        int height,
        TextureSamplingPolicy policy,
        out int texelX,
        out int texelY)
    {
        texelX = 0;
        texelY = 0;
        if (!float.IsFinite(u) || !float.IsFinite(v) || width <= 0 || height <= 0)
        {
            return false;
        }

        float wrappedU = ApplyWrap(u, policy.WrapMode);
        float wrappedV = ApplyWrap(v, policy.WrapMode);
        if (!float.IsFinite(wrappedU) || !float.IsFinite(wrappedV))
        {
            return false;
        }

        texelX = System.Math.Clamp((int)(wrappedU * width), 0, width - 1);
        texelY = System.Math.Clamp((int)(wrappedV * height), 0, height - 1);
        return true;
    }

    private static float ApplyWrap(float t, TextureWrapMode mode) => mode switch
    {
        TextureWrapMode.Clamp => System.Math.Clamp(t, 0f, 1f),
        TextureWrapMode.Repeat => t - MathF.Floor(t),
        TextureWrapMode.MirroredRepeat => ApplyMirroredRepeat(t),
        _ => float.NaN,
    };

    private static float ApplyMirroredRepeat(float t)
    {
        float s = t - 2f * MathF.Floor(t * 0.5f);
        return s > 1f ? 2f - s : s;
    }
}
