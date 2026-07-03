using XPrimeRay.ObserverInstrumentation.Math;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class CheckerProbeMathTests
{
    public static void Run()
    {
        VerifiesParity();
        VerifiesBoundariesAndWrapping();
        RejectsInvalidInputs();
    }

    private static void VerifiesParity()
    {
        Expect(0.01f, 0.01f, 6, 6, false, "6x6 origin cell");
        Expect(1f / 6f, 0.01f, 6, 6, true, "6x6 adjacent U cell");
        Expect(1f / 6f, 1f / 6f, 6, 6, false, "6x6 diagonal cell");
        Expect(1f / 8f, 0.01f, 8, 8, true, "8x8 adjacent U cell");
        Expect(1f / 8f, 1f / 8f, 8, 8, false, "8x8 diagonal cell");
    }

    private static void VerifiesBoundariesAndWrapping()
    {
        Expect(1f, 0.01f, 6, 6, false, "U=1 wraps to first column");
        Expect(-1f / 6f, 0.01f, 6, 6, true, "negative U wraps to last column");
        Expect(0.01f, 1f, 6, 6, true, "V=1 selects final row");
        Expect(0.01f, -5f, 6, 6, false, "finite V below zero clamps");
        Expect(0.01f, 5f, 6, 6, true, "finite V above one clamps");
    }

    private static void RejectsInvalidInputs()
    {
        TestAssert.False(CheckerProbeMath.TryIsDark(0f, 0f, 0, 6, out _), "zero U tiles fail");
        TestAssert.False(CheckerProbeMath.TryIsDark(0f, 0f, 6, -1, out _), "negative V tiles fail");
        TestAssert.False(CheckerProbeMath.TryIsDark(float.NaN, 0f, 6, 6, out _), "NaN U fails");
        TestAssert.False(CheckerProbeMath.TryIsDark(0f, float.PositiveInfinity, 6, 6, out _), "infinite V fails");
    }

    private static void Expect(float u, float v, int tilesU, int tilesV, bool expected, string label)
    {
        TestAssert.True(CheckerProbeMath.TryIsDark(u, v, tilesU, tilesV, out bool actual), $"{label} evaluates");
        TestAssert.Equal(expected, actual, label);
    }
}
