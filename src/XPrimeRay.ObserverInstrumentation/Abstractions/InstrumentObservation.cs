using System.Numerics;

namespace XPrimeRay.ObserverInstrumentation.Abstractions;

public readonly struct InstrumentObservation
{
    public InstrumentObservation(
        int rayIndex,
        InstrumentKind instrument,
        InstrumentDiagnosticState diagnosticState,
        string? colliderName,
        Vector2 uv,
        bool hasUv,
        bool sampleValue)
    {
        RayIndex = rayIndex;
        Instrument = instrument;
        DiagnosticState = diagnosticState;
        ColliderName = colliderName;
        Uv = uv;
        HasUv = hasUv;
        SampleValue = sampleValue;
    }

    public int RayIndex { get; }
    public InstrumentKind Instrument { get; }
    public InstrumentDiagnosticState DiagnosticState { get; }
    public string? ColliderName { get; }
    public Vector2 Uv { get; }
    public bool HasUv { get; }
    public bool SampleValue { get; }
}
