namespace XPrimeRay.ObserverInstrumentation.Abstractions;

public enum InstrumentDiagnosticState
{
    DiagnosticUnresolved = 0,
    RegionSampled = 1,
    ProbeHitOutsideRegion = 2,
    OtherGeometryHit = 3,
    TransportClassNotSurfaceHit = 4
}

public enum InstrumentKind
{
    SurfaceUv = 0,
    CheckerProbe = 1,
    FlagCapture = 2
}

public enum InstrumentHitKind
{
    Miss = 0,
    SurfaceHit = 1,
    NonSurfaceHit = 2
}

[Flags]
public enum ObserverInstrumentMask
{
    None = 0,
    SurfaceUv = 1 << 0,
    CheckerProbe = 1 << 1,
    FlagCapture = 1 << 2,
    All = SurfaceUv | CheckerProbe | FlagCapture
}
