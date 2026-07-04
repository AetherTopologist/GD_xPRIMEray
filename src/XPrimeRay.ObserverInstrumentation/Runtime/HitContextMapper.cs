using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Abstractions;

namespace XPrimeRay.ObserverInstrumentation.Runtime;

// Pure mapping from raw hit fields (no Godot dependency) to InstrumentContext.
// Allows the Godot-aware adapter to delegate classification here while keeping
// this logic reachable from the pure test project.
public static class HitContextMapper
{
    // valid=true wins regardless of absorbed; absorbed is only meaningful when valid=false.
    public static InstrumentHitKind ClassifyHit(bool valid, int absorbed) =>
        valid ? InstrumentHitKind.SurfaceHit
              : absorbed == 1 ? InstrumentHitKind.NonSurfaceHit
              : InstrumentHitKind.Miss;

    // The Godot pipeline writes "<none>" when no hit name was captured.
    // Instruments receive null for any non-probe collider.
    public static string? NormalizeColliderName(string? name) =>
        !string.IsNullOrEmpty(name) && name != "<none>" ? name : null;

    public static InstrumentContext BuildContext(
        int rayIndex,
        bool valid,
        int absorbed,
        Vector3 position,
        Vector3 normal,
        string? colliderName) =>
        new InstrumentContext(
            rayIndex,
            ClassifyHit(valid, absorbed),
            position,
            normal,
            NormalizeColliderName(colliderName));
}
