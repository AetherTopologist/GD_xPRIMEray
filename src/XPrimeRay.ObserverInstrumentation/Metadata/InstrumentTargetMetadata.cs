using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Math;

namespace XPrimeRay.ObserverInstrumentation.Metadata;

public readonly struct InstrumentTargetMetadata
{
    private InstrumentTargetMetadata(
        string colliderName,
        Vector3 center,
        int checkerTilesU,
        int checkerTilesV,
        UvRevealRegion revealRegion)
    {
        IsValid = true;
        ColliderName = colliderName;
        Center = center;
        CheckerTilesU = checkerTilesU;
        CheckerTilesV = checkerTilesV;
        RevealRegion = revealRegion;
    }

    public bool IsValid { get; }
    public string? ColliderName { get; }
    public Vector3 Center { get; }
    public int CheckerTilesU { get; }
    public int CheckerTilesV { get; }
    public UvRevealRegion RevealRegion { get; }

    public static bool TryCreate(
        string? colliderName,
        Vector3 center,
        int checkerTilesU,
        int checkerTilesV,
        UvRevealRegion revealRegion,
        out InstrumentTargetMetadata metadata)
    {
        metadata = default;
        if (string.IsNullOrWhiteSpace(colliderName) ||
            !IsFinite(center) ||
            checkerTilesU <= 0 ||
            checkerTilesV <= 0 ||
            !revealRegion.IsValid)
        {
            return false;
        }

        metadata = new InstrumentTargetMetadata(
            colliderName,
            center,
            checkerTilesU,
            checkerTilesV,
            revealRegion);
        return true;
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
}
