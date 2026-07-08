using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Instruments;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Resources;
using XPrimeRay.ObserverInstrumentation.Runtime;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class TextureSampleObservationTests
{
    public static void Run()
    {
        DefaultObservation_DiagnosticState_IsUnresolved();
        ExistingInstruments_ObserveWithExtensions_HasTexture_False();
        FrameBuffer_ZeroTextureCapacity_TextureSamplesEmpty();
        FrameBuffer_TryAppendTextureSample_CountsCorrectly();
        FrameBuffer_TextureOverflow_DoesNotAffectPrimaryBuffer();
        FrameBuffer_Clear_ResetsBothCounts();
        TextureSampleObservation_AllFields_RoundTrip();
        TextureSampleInstrument_MissingPayload_FailsClosed();
        TextureSampleInstrument_ValidSample_EmitsColorAndTexel();
        TextureSampleInstrument_MissingBinding_FailsClosed();
        TextureSampleInstrument_MissingTexture_FailsClosed();
        TextureSampleInstrument_UnsupportedPolicy_FailsClosed();
        TextureSampleInstrument_NonFiniteUv_FailsClosed();
        TextureSampleInstrument_NonSurfaceHit_NoTypedPayload();
        TextureSampleInstrument_SessionTextureSpanCounts();
    }

    private static void DefaultObservation_DiagnosticState_IsUnresolved()
    {
        TextureSampleObservation observation = default;

        TestAssert.Equal(
            InstrumentDiagnosticState.DiagnosticUnresolved,
            observation.DiagnosticState,
            "texture default: diagnostic state defaults to unresolved");
        TestAssert.Equal(0, observation.RayIndex, "texture default: ray index defaults to zero");
        TestAssert.False(observation.AssetId.IsValid, "texture default: asset id defaults to invalid");
        TestAssert.False(observation.SlotId.IsValid, "texture default: slot id defaults to invalid");
    }

    private static void ExistingInstruments_ObserveWithExtensions_HasTexture_False()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        InstrumentContext context = Stage1BTestData.Surface();

        AssertDefaultExtension(new SurfaceUvInstrument(), in context, in metadata, "surface uv");
        AssertDefaultExtension(new CheckerProbeInstrument(), in context, in metadata, "checker");
        AssertDefaultExtension(new FlagCaptureInstrument(), in context, in metadata, "flag");
    }

    private static void AssertDefaultExtension(
        IObserverInstrument instrument,
        in InstrumentContext context,
        in InstrumentTargetMetadata metadata,
        string label)
    {
        instrument.ObserveWithExtensions(
            in context,
            catalogAvailable: true,
            metadataFound: true,
            in metadata,
            out InstrumentObservation observation,
            out TextureSampleObservation textureObservation,
            out bool hasTextureObservation);

        TestAssert.Equal(instrument.Kind, observation.Instrument, $"{label}: base observation kind");
        TestAssert.False(hasTextureObservation, $"{label}: no texture observation");
        TestAssert.Equal(default, textureObservation, $"{label}: texture observation default");
    }

    private static void FrameBuffer_ZeroTextureCapacity_TextureSamplesEmpty()
    {
        var buffer = new InstrumentFrameBuffer(capacity: 1);
        TextureSampleObservation sample = SampleObservation();

        TestAssert.Equal(0, buffer.TextureSampleCapacity, "zero texture capacity: capacity is zero");
        TestAssert.Equal(0, buffer.TextureSampleCount, "zero texture capacity: count is zero");
        TestAssert.Equal(0, buffer.TextureSamples.Length, "zero texture capacity: span is empty");
        TestAssert.False(
            buffer.TryAppendTextureSample(in sample),
            "zero texture capacity: append fails closed");
        TestAssert.Equal(0, buffer.TextureSampleCount, "zero texture capacity: failed append does not count");
    }

    private static void FrameBuffer_TryAppendTextureSample_CountsCorrectly()
    {
        var buffer = new InstrumentFrameBuffer(capacity: 1, textureCapacity: 2);
        TextureSampleObservation first = SampleObservation(rayIndex: 7);
        TextureSampleObservation second = SampleObservation(rayIndex: 8);

        TestAssert.True(buffer.TryAppendTextureSample(in first), "texture append: first fits");
        TestAssert.True(buffer.TryAppendTextureSample(in second), "texture append: second fits");
        TestAssert.Equal(2, buffer.TextureSampleCount, "texture append: count increments");

        ReadOnlySpan<TextureSampleObservation> samples = buffer.TextureSamples;
        TestAssert.Equal(2, samples.Length, "texture append: span length follows count");
        TestAssert.Equal(7, samples[0].RayIndex, "texture append: first preserved");
        TestAssert.Equal(8, samples[1].RayIndex, "texture append: second preserved");
    }

    private static void FrameBuffer_TextureOverflow_DoesNotAffectPrimaryBuffer()
    {
        var buffer = new InstrumentFrameBuffer(capacity: 2, textureCapacity: 1);
        InstrumentObservation baseObservation = new(
            1,
            InstrumentKind.TextureSample,
            InstrumentDiagnosticState.RegionSampled,
            "probe",
            new Vector2(0.25f, 0.75f),
            hasUv: true,
            sampleValue: false);
        TextureSampleObservation first = SampleObservation(rayIndex: 1);
        TextureSampleObservation second = SampleObservation(rayIndex: 2);

        TestAssert.True(buffer.TryAppend(in baseObservation), "texture overflow: primary append succeeds");
        TestAssert.True(buffer.TryAppendTextureSample(in first), "texture overflow: first texture append succeeds");
        TestAssert.False(buffer.TryAppendTextureSample(in second), "texture overflow: second texture append fails");

        TestAssert.Equal(1, buffer.Count, "texture overflow: primary count unaffected");
        TestAssert.Equal(1, buffer.TextureSampleCount, "texture overflow: texture count remains full");
    }

    private static void FrameBuffer_Clear_ResetsBothCounts()
    {
        var buffer = new InstrumentFrameBuffer(capacity: 1, textureCapacity: 1);
        InstrumentObservation baseObservation = new(
            1,
            InstrumentKind.TextureSample,
            InstrumentDiagnosticState.RegionSampled,
            "probe",
            new Vector2(0.25f, 0.75f),
            hasUv: true,
            sampleValue: false);
        TextureSampleObservation sample = SampleObservation();

        buffer.TryAppend(in baseObservation);
        buffer.TryAppendTextureSample(in sample);
        buffer.Clear();

        TestAssert.Equal(0, buffer.Count, "texture clear: primary count reset");
        TestAssert.Equal(0, buffer.TextureSampleCount, "texture clear: texture count reset");
        TestAssert.Equal(0, buffer.TextureSamples.Length, "texture clear: texture span empty");
    }

    private static void TextureSampleObservation_AllFields_RoundTrip()
    {
        var color = new SampledColor(0.1f, 0.2f, 0.3f, 0.4f);
        var asset = new TextureAssetId("albedo_probe");
        var slot = TextureSlotId.BaseColor;
        TextureSamplingPolicy policy = TextureSamplingPolicy.NearestRepeat;
        var observation = new TextureSampleObservation(
            rayIndex: 42,
            diagnosticState: InstrumentDiagnosticState.RegionSampled,
            color: color,
            assetId: asset,
            slotId: slot,
            texelX: 3,
            texelY: 5,
            mipLevel: 0,
            samplingPolicy: policy);

        TestAssert.Equal(42, observation.RayIndex, "round trip: ray index");
        TestAssert.Equal(InstrumentDiagnosticState.RegionSampled, observation.DiagnosticState, "round trip: state");
        TestAssert.Equal(color, observation.Color, "round trip: color");
        TestAssert.Equal(asset, observation.AssetId, "round trip: asset");
        TestAssert.Equal(slot, observation.SlotId, "round trip: slot");
        TestAssert.Equal(3, observation.TexelX, "round trip: texel x");
        TestAssert.Equal(5, observation.TexelY, "round trip: texel y");
        TestAssert.Equal(0, observation.MipLevel, "round trip: mip");
        TestAssert.Equal(policy, observation.SamplingPolicy, "round trip: policy");
    }

    private static void TextureSampleInstrument_MissingPayload_FailsClosed()
    {
        var instrument = new TextureSampleInstrument(bindings: null, resources: null);
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        InstrumentContext context = Stage1BTestData.Surface();

        instrument.ObserveWithExtensions(
            in context,
            catalogAvailable: true,
            metadataFound: true,
            in metadata,
            out InstrumentObservation observation,
            out TextureSampleObservation textureObservation,
            out bool hasTextureObservation);

        TestAssert.Equal(InstrumentKind.TextureSample, observation.Instrument, "texture fail closed: kind");
        TestAssert.Equal(
            InstrumentDiagnosticState.DiagnosticUnresolved,
            observation.DiagnosticState,
            "texture fail closed: base observation does not pretend sampling succeeded");
        TestAssert.False(hasTextureObservation, "texture fail closed: no typed payload emitted");
        TestAssert.Equal(default, textureObservation, "texture fail closed: typed payload default");
    }

    private static void TextureSampleInstrument_ValidSample_EmitsColorAndTexel()
    {
        TextureAssetId assetId = new("valid_tex");
        var instrument = CreateInstrument(assetId, out _, out _);
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        InstrumentContext context = Stage1BTestData.Surface(rayIndex: 11);

        instrument.ObserveWithExtensions(
            in context,
            catalogAvailable: true,
            metadataFound: true,
            in metadata,
            out InstrumentObservation observation,
            out TextureSampleObservation textureObservation,
            out bool hasTextureObservation);

        TestAssert.Equal(InstrumentDiagnosticState.RegionSampled, observation.DiagnosticState, "valid texture: base sampled");
        TestAssert.True(observation.HasUv, "valid texture: base includes uv");
        TestAssert.True(hasTextureObservation, "valid texture: typed payload emitted");
        TestAssert.Equal(11, textureObservation.RayIndex, "valid texture: ray index");
        TestAssert.Equal(2, textureObservation.TexelX, "valid texture: texel x from u=0.5 on 4-wide texture");
        TestAssert.Equal(2, textureObservation.TexelY, "valid texture: texel y from v=0.5 on 4-high texture");
        TestAssert.Equal(0, textureObservation.MipLevel, "valid texture: mip level zero");
        TestAssert.Equal(assetId, textureObservation.AssetId, "valid texture: asset id");
        TestAssert.Equal(TextureSlotId.BaseColor, textureObservation.SlotId, "valid texture: base color slot");
        TestAssert.Equal(TextureSamplingPolicy.NearestClamp, textureObservation.SamplingPolicy, "valid texture: policy");
        TestAssert.Equal(ColorAt(2, 2), textureObservation.Color, "valid texture: sampled color");
    }

    private static void TextureSampleInstrument_MissingBinding_FailsClosed()
    {
        TextureResourceSnapshot.TryBuild(
            new[] { MakeTexture(new TextureAssetId("valid_tex")) },
            out TextureResourceSnapshot? resources,
            out _);
        TextureBindingCatalog.TryCreate(
            Array.Empty<(string colliderName, TextureResourceBinding binding)>(),
            out TextureBindingCatalog? bindings,
            out _);
        var instrument = new TextureSampleInstrument(bindings, resources);

        ObserveTexture(
            instrument,
            Stage1BTestData.Surface(),
            out InstrumentObservation observation,
            out TextureSampleObservation textureObservation,
            out bool hasTextureObservation);

        AssertUnresolvedWithoutPayload(
            observation,
            textureObservation,
            hasTextureObservation,
            "missing binding");
    }

    private static void TextureSampleInstrument_MissingTexture_FailsClosed()
    {
        TextureAssetId assetId = new("missing_tex");
        TextureBindingCatalog bindings = CreateBindings(assetId, TextureSamplingPolicy.NearestClamp);
        var instrument = new TextureSampleInstrument(bindings, TextureResourceSnapshot.Empty);

        ObserveTexture(
            instrument,
            Stage1BTestData.Surface(),
            out InstrumentObservation observation,
            out TextureSampleObservation textureObservation,
            out bool hasTextureObservation);

        AssertUnresolvedWithoutPayload(
            observation,
            textureObservation,
            hasTextureObservation,
            "missing texture");
    }

    private static void TextureSampleInstrument_UnsupportedPolicy_FailsClosed()
    {
        TextureAssetId assetId = new("policy_tex");
        TextureBindingCatalog bindings = CreateBindings(
            assetId,
            new TextureSamplingPolicy(
                TextureWrapMode.Clamp,
                TextureFilterMode.Nearest,
                ColorInterpretation.Linear));
        TextureResourceSnapshot.TryBuild(
            new[] { MakeTexture(assetId) },
            out TextureResourceSnapshot? resources,
            out _);
        var instrument = new TextureSampleInstrument(bindings, resources);

        ObserveTexture(
            instrument,
            Stage1BTestData.Surface(),
            out InstrumentObservation observation,
            out TextureSampleObservation textureObservation,
            out bool hasTextureObservation);

        AssertUnresolvedWithoutPayload(
            observation,
            textureObservation,
            hasTextureObservation,
            "unsupported policy");
    }

    private static void TextureSampleInstrument_NonSurfaceHit_NoTypedPayload()
    {
        TextureAssetId assetId = new("non_surface_tex");
        var instrument = CreateInstrument(assetId, out _, out _);
        InstrumentContext context = new(
            12,
            InstrumentHitKind.NonSurfaceHit,
            Vector3.UnitX,
            Vector3.UnitX,
            Stage1BTestData.ProbeName);

        ObserveTexture(
            instrument,
            context,
            out InstrumentObservation observation,
            out TextureSampleObservation textureObservation,
            out bool hasTextureObservation);

        TestAssert.Equal(
            InstrumentDiagnosticState.TransportClassNotSurfaceHit,
            observation.DiagnosticState,
            "non-surface: classified as transport/non-surface");
        TestAssert.False(hasTextureObservation, "non-surface: no typed payload");
        TestAssert.Equal(default, textureObservation, "non-surface: typed payload default");
    }

    private static void TextureSampleInstrument_NonFiniteUv_FailsClosed()
    {
        TextureAssetId assetId = new("non_finite_tex");
        var instrument = CreateInstrument(assetId, out _, out _);
        InstrumentContext context = new(
            13,
            InstrumentHitKind.SurfaceHit,
            new Vector3(float.NaN, 0f, 0f),
            Vector3.UnitX,
            Stage1BTestData.ProbeName);

        ObserveTexture(
            instrument,
            context,
            out InstrumentObservation observation,
            out TextureSampleObservation textureObservation,
            out bool hasTextureObservation);

        AssertUnresolvedWithoutPayload(
            observation,
            textureObservation,
            hasTextureObservation,
            "non-finite uv");
    }

    private static void TextureSampleInstrument_SessionTextureSpanCounts()
    {
        TextureAssetId assetId = new("session_tex");
        TextureBindingCatalog bindings = CreateBindings(assetId, TextureSamplingPolicy.NearestClamp);
        TextureResourceSnapshot.TryBuild(
            new[] { MakeTexture(assetId) },
            out TextureResourceSnapshot? resources,
            out _);
        var config = new ObserverInstrumentationConfiguration
        {
            EnabledMask = ObserverInstrumentMask.TextureSample,
            Catalog = new InstrumentMetadataCatalog(Stage1BTestData.Metadata()),
            TextureBindings = bindings,
            TextureResources = resources
        };
        ObserverInstrumentationSession session = ObserverInstrumentationSession.Create(config, maxHitsCapacity: 2);

        session.RunFrame(new[] { Stage1BTestData.Surface(rayIndex: 20) });

        TestAssert.Equal(1, session.LastObservationCount, "session texture: one base observation");
        TestAssert.Equal(1, session.FrameBuffer.TextureSampleCount, "session texture: one typed sample");
        TestAssert.Equal(1, session.FrameBuffer.TextureSamples.Length, "session texture: texture span length");
        TestAssert.Equal(20, session.FrameBuffer.TextureSamples[0].RayIndex, "session texture: ray index preserved");
        TestAssert.Equal(ColorAt(2, 2), session.FrameBuffer.TextureSamples[0].Color, "session texture: color");
    }

    private static TextureSampleObservation SampleObservation(int rayIndex = 1) =>
        new(
            rayIndex,
            InstrumentDiagnosticState.RegionSampled,
            new SampledColor(0.25f, 0.5f, 0.75f, 1f),
            new TextureAssetId("texture_a"),
            TextureSlotId.BaseColor,
            texelX: 2,
            texelY: 3,
            mipLevel: 0,
            TextureSamplingPolicy.NearestClamp);

    private static TextureSampleInstrument CreateInstrument(
        TextureAssetId assetId,
        out TextureBindingCatalog bindings,
        out TextureResourceSnapshot resources)
    {
        bindings = CreateBindings(assetId, TextureSamplingPolicy.NearestClamp);
        TextureResourceSnapshot.TryBuild(
            new[] { MakeTexture(assetId) },
            out TextureResourceSnapshot? builtResources,
            out string? errorReason);
        TestAssert.True(builtResources != null, $"test resources build: {errorReason}");
        resources = builtResources!;
        return new TextureSampleInstrument(bindings, resources);
    }

    private static TextureBindingCatalog CreateBindings(
        TextureAssetId assetId,
        TextureSamplingPolicy policy)
    {
        var binding = new TextureResourceBinding
        {
            AssetId = assetId,
            SlotId = TextureSlotId.BaseColor,
            SamplingPolicy = policy,
            UvChannel = 0
        };
        TextureBindingCatalog.TryCreate(
            new[] { (Stage1BTestData.ProbeName, binding) },
            out TextureBindingCatalog? catalog,
            out string? errorReason);
        TestAssert.True(catalog != null, $"test binding catalog builds: {errorReason}");
        return catalog!;
    }

    private static TextureSnapshot MakeTexture(TextureAssetId assetId)
    {
        var provenance = new TextureProvenance
        {
            AssetId = assetId,
            SourceDescription = "oi-010-test",
            LoadedAt = DateTimeOffset.UnixEpoch,
            Width = 4,
            Height = 4,
            Format = TexturePixelFormat.Rgba8,
            ColorSpace = ColorSpace.Linear
        };
        var pixels = new byte[4 * 4 * 4];
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                int offset = (y * 4 + x) * 4;
                pixels[offset + 0] = (byte)(x * 40);
                pixels[offset + 1] = (byte)(y * 50);
                pixels[offset + 2] = (byte)(x * 10 + y * 20);
                pixels[offset + 3] = 255;
            }
        }

        TestAssert.True(
            TextureSnapshot.TryCreate(provenance, pixels, out TextureSnapshot? snapshot),
            "test texture snapshot creates");
        return snapshot!;
    }

    private static SampledColor ColorAt(int x, int y)
    {
        const float inv = 1f / 255f;
        return new SampledColor(
            x * 40 * inv,
            y * 50 * inv,
            (x * 10 + y * 20) * inv,
            1f);
    }

    private static void ObserveTexture(
        TextureSampleInstrument instrument,
        in InstrumentContext context,
        out InstrumentObservation observation,
        out TextureSampleObservation textureObservation,
        out bool hasTextureObservation)
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        instrument.ObserveWithExtensions(
            in context,
            catalogAvailable: true,
            metadataFound: true,
            in metadata,
            out observation,
            out textureObservation,
            out hasTextureObservation);
    }

    private static void AssertUnresolvedWithoutPayload(
        InstrumentObservation observation,
        TextureSampleObservation textureObservation,
        bool hasTextureObservation,
        string label)
    {
        TestAssert.Equal(
            InstrumentDiagnosticState.DiagnosticUnresolved,
            observation.DiagnosticState,
            $"{label}: base observation unresolved");
        TestAssert.False(hasTextureObservation, $"{label}: no typed payload");
        TestAssert.Equal(default, textureObservation, $"{label}: typed payload default");
    }
}
