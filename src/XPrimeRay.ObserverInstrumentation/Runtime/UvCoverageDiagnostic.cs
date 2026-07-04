using XPrimeRay.ObserverInstrumentation.Abstractions;

namespace XPrimeRay.ObserverInstrumentation.Runtime;

public sealed class UvCoverageDiagnostic
{
    public const float SeamBandWidth = 0.01f;

    public int ObservationCount { get; private set; }
    public int ValidUvCount { get; private set; }
    public int DiagnosticUnresolvedCount { get; private set; }
    public int OtherGeometryCount { get; private set; }
    public int NonSurfaceCount { get; private set; }
    public int LowerSeamBandCount { get; private set; }
    public int UpperSeamBandCount { get; private set; }
    public int NorthPoleCount { get; private set; }
    public int SouthPoleCount { get; private set; }
    public float MinimumU { get; private set; }
    public float MaximumU { get; private set; }
    public float MinimumV { get; private set; }
    public float MaximumV { get; private set; }
    public bool HasCoverage => ValidUvCount > 0;
    public bool CoversBothSeamBands => LowerSeamBandCount > 0 && UpperSeamBandCount > 0;

    public void Reset()
    {
        ObservationCount = 0;
        ValidUvCount = 0;
        DiagnosticUnresolvedCount = 0;
        OtherGeometryCount = 0;
        NonSurfaceCount = 0;
        LowerSeamBandCount = 0;
        UpperSeamBandCount = 0;
        NorthPoleCount = 0;
        SouthPoleCount = 0;
        MinimumU = 0f;
        MaximumU = 0f;
        MinimumV = 0f;
        MaximumV = 0f;
    }

    public void Evaluate(ReadOnlySpan<InstrumentObservation> observations)
    {
        Reset();
        for (int index = 0; index < observations.Length; index++)
        {
            ref readonly InstrumentObservation observation = ref observations[index];
            if (observation.Instrument != InstrumentKind.SurfaceUv)
            {
                continue;
            }

            ObservationCount++;
            if (!observation.HasUv || !IsValidUv(observation.Uv.X, observation.Uv.Y))
            {
                CountDiagnosticState(observation.DiagnosticState);
                continue;
            }

            float u = observation.Uv.X;
            float v = observation.Uv.Y;
            if (ValidUvCount == 0)
            {
                MinimumU = MaximumU = u;
                MinimumV = MaximumV = v;
            }
            else
            {
                MinimumU = MathF.Min(MinimumU, u);
                MaximumU = MathF.Max(MaximumU, u);
                MinimumV = MathF.Min(MinimumV, v);
                MaximumV = MathF.Max(MaximumV, v);
            }

            ValidUvCount++;
            if (u <= SeamBandWidth)
            {
                LowerSeamBandCount++;
            }
            if (u >= 1f - SeamBandWidth)
            {
                UpperSeamBandCount++;
            }
            if (v == 0f)
            {
                NorthPoleCount++;
            }
            if (v == 1f)
            {
                SouthPoleCount++;
            }
        }
    }

    private void CountDiagnosticState(InstrumentDiagnosticState state)
    {
        switch (state)
        {
            case InstrumentDiagnosticState.OtherGeometryHit:
                OtherGeometryCount++;
                break;
            case InstrumentDiagnosticState.TransportClassNotSurfaceHit:
                NonSurfaceCount++;
                break;
            default:
                DiagnosticUnresolvedCount++;
                break;
        }
    }

    private static bool IsValidUv(float u, float v) =>
        float.IsFinite(u) &&
        float.IsFinite(v) &&
        u >= 0f &&
        u < 1f &&
        v >= 0f &&
        v <= 1f;
}
