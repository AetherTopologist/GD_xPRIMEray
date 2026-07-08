using XPrimeRay.ObserverInstrumentation.Resources;

namespace XPrimeRay.ObserverInstrumentation.Abstractions;

public readonly struct TextureSampleObservation
{
    public TextureSampleObservation(
        int rayIndex,
        InstrumentDiagnosticState diagnosticState,
        SampledColor color,
        TextureAssetId assetId,
        TextureSlotId slotId,
        int texelX,
        int texelY,
        int mipLevel,
        TextureSamplingPolicy samplingPolicy)
    {
        RayIndex = rayIndex;
        DiagnosticState = diagnosticState;
        Color = color;
        AssetId = assetId;
        SlotId = slotId;
        TexelX = texelX;
        TexelY = texelY;
        MipLevel = mipLevel;
        SamplingPolicy = samplingPolicy;
    }

    public int RayIndex { get; }
    public InstrumentDiagnosticState DiagnosticState { get; }
    public SampledColor Color { get; }
    public TextureAssetId AssetId { get; }
    public TextureSlotId SlotId { get; }
    public int TexelX { get; }
    public int TexelY { get; }
    public int MipLevel { get; }
    public TextureSamplingPolicy SamplingPolicy { get; }
}
