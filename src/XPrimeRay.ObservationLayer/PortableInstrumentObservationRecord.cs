namespace XPrimeRay.ObservationLayer;

/// <summary>
/// Host-neutral sparse observer-instrument record. This mirrors instrument evidence without
/// referencing ObserverInstrumentation implementation types.
/// </summary>
public readonly struct PortableInstrumentObservationRecord
{
    public PortableInstrumentObservationRecord(
        int rayIndex,
        string instrumentId,
        string diagnosticState,
        string? targetIdentity,
        Float2 uv,
        bool hasUv,
        bool sampleValue)
    {
        RayIndex = rayIndex;
        InstrumentId = string.IsNullOrWhiteSpace(instrumentId) ? "unknown" : instrumentId.Trim();
        DiagnosticState = string.IsNullOrWhiteSpace(diagnosticState) ? "unknown" : diagnosticState.Trim();
        TargetIdentity = string.IsNullOrWhiteSpace(targetIdentity) ? null : targetIdentity.Trim();
        Uv = uv;
        HasUv = hasUv;
        SampleValue = sampleValue;
    }

    public int RayIndex { get; }
    public string InstrumentId { get; }
    public string DiagnosticState { get; }
    public string? TargetIdentity { get; }
    public Float2 Uv { get; }
    public bool HasUv { get; }
    public bool SampleValue { get; }
}
