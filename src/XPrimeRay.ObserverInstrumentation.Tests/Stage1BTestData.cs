using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Math;
using XPrimeRay.ObserverInstrumentation.Metadata;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class Stage1BTestData
{
    public const string ProbeName = "synthetic_probe";

    public static InstrumentTargetMetadata Metadata(float uMin = 0.4f, float uMax = 0.6f)
    {
        TestAssert.True(
            UvRevealRegion.TryCreate(uMin, uMax, 0.4f, 0.6f, out UvRevealRegion region),
            "test reveal region creates");
        TestAssert.True(
            InstrumentTargetMetadata.TryCreate(
                ProbeName, Vector3.Zero, 8, 8, region, out InstrumentTargetMetadata metadata),
            "test metadata creates");
        return metadata;
    }

    public static InstrumentContext Surface(int rayIndex = 7, string? colliderName = ProbeName) =>
        new(rayIndex, InstrumentHitKind.SurfaceHit, Vector3.UnitX, Vector3.UnitX, colliderName);
}
