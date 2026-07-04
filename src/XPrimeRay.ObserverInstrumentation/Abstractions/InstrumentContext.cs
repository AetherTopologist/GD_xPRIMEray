using System.Numerics;

namespace XPrimeRay.ObserverInstrumentation.Abstractions;

public readonly struct InstrumentContext
{
    public InstrumentContext(
        int rayIndex,
        InstrumentHitKind hitKind,
        Vector3 hitPosition,
        Vector3 hitNormal,
        string? colliderName)
    {
        RayIndex = rayIndex;
        HitKind = hitKind;
        HitPosition = hitPosition;
        HitNormal = hitNormal;
        ColliderName = colliderName;
    }

    public int RayIndex { get; }
    public InstrumentHitKind HitKind { get; }
    public Vector3 HitPosition { get; }
    public Vector3 HitNormal { get; }
    public string? ColliderName { get; }
}
