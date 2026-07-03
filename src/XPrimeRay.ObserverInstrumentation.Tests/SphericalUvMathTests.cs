using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Math;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class SphericalUvMathTests
{
    private const float Tolerance = 1e-6f;

    public static void Run()
    {
        CanonicalNormals();
        NormalizesInput();
        WrapsSeams();
        RejectsInvalidVectors();
    }

    private static void CanonicalNormals()
    {
        ExpectUv(Vector3.UnitX, 0.5f, 0.5f, "+X");
        ExpectUv(-Vector3.UnitX, 0f, 0.5f, "-X");
        ExpectUv(Vector3.UnitZ, 0.75f, 0.5f, "+Z");
        ExpectUv(-Vector3.UnitZ, 0.25f, 0.5f, "-Z");
        ExpectUv(Vector3.UnitY, 0.5f, 0f, "+Y pole");
        ExpectUv(-Vector3.UnitY, 0.5f, 1f, "-Y pole");
    }

    private static void NormalizesInput()
    {
        ExpectUv(new Vector3(8f, 0f, 0f), 0.5f, 0.5f, "non-unit +X");
    }

    private static void WrapsSeams()
    {
        TestAssert.NearlyEqual(0.75f, SphericalUvMath.Wrap01(-0.25f), Tolerance, "negative U wraps");
        TestAssert.NearlyEqual(0.25f, SphericalUvMath.Wrap01(1.25f), Tolerance, "U above one wraps");
        TestAssert.NearlyEqual(0f, SphericalUvMath.Wrap01(1f), Tolerance, "exact seam wraps");
        TestAssert.True(float.IsNaN(SphericalUvMath.Wrap01(float.PositiveInfinity)), "non-finite wrap returns NaN");
    }

    private static void RejectsInvalidVectors()
    {
        TestAssert.False(SphericalUvMath.TryFromNormal(Vector3.Zero, out _), "zero vector must fail");
        TestAssert.False(
            SphericalUvMath.TryFromNormal(new Vector3(float.NaN, 0f, 0f), out _),
            "NaN vector must fail");
        TestAssert.False(
            SphericalUvMath.TryFromNormal(new Vector3(float.PositiveInfinity, 0f, 0f), out _),
            "infinite vector must fail");
    }

    private static void ExpectUv(Vector3 normal, float expectedU, float expectedV, string label)
    {
        TestAssert.True(SphericalUvMath.TryFromNormal(normal, out Vector2 uv), $"{label} maps");
        TestAssert.NearlyEqual(expectedU, uv.X, Tolerance, $"{label} U");
        TestAssert.NearlyEqual(expectedV, uv.Y, Tolerance, $"{label} V");
    }
}
