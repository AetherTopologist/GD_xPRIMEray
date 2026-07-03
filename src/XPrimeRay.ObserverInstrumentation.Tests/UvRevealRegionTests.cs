using XPrimeRay.ObserverInstrumentation.Math;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class UvRevealRegionTests
{
    public static void Run()
    {
        OrdinaryRegion();
        SeamAwareRegion();
        RejectsInvalidRegions();
    }

    private static void OrdinaryRegion()
    {
        TestAssert.True(UvRevealRegion.TryCreate(0.2f, 0.6f, 0.3f, 0.7f, out UvRevealRegion region), "ordinary region creates");
        TestAssert.False(region.CrossesUSeam, "ordinary region does not cross seam");
        TestAssert.True(region.Contains(0.4f, 0.5f), "ordinary interior is contained");
        TestAssert.True(region.Contains(0.2f, 0.3f), "minimum boundary is inclusive");
        TestAssert.True(region.Contains(0.6f, 0.7f), "maximum boundary is inclusive");
        TestAssert.False(region.Contains(0.1f, 0.5f), "outside U is excluded");
        TestAssert.False(region.Contains(0.4f, 1.1f), "outside V is excluded");
    }

    private static void SeamAwareRegion()
    {
        TestAssert.True(UvRevealRegion.TryCreate(0.8f, 0.2f, 0.25f, 0.75f, out UvRevealRegion region), "seam region creates");
        TestAssert.True(region.CrossesUSeam, "seam region is identified");
        TestAssert.True(region.Contains(0.9f, 0.5f), "high seam side is contained");
        TestAssert.True(region.Contains(0.1f, 0.5f), "low seam side is contained");
        TestAssert.True(region.Contains(1f, 0.5f), "exact upper seam is contained");
        TestAssert.True(region.Contains(-0.1f, 0.5f), "wrapped negative U is contained");
        TestAssert.False(region.Contains(0.5f, 0.5f), "middle U is excluded");
    }

    private static void RejectsInvalidRegions()
    {
        TestAssert.False(default(UvRevealRegion).Contains(0f, 0f), "default region contains nothing");
        TestAssert.False(UvRevealRegion.TryCreate(0.2f, 0.2f, 0.3f, 0.7f, out _), "zero U width fails");
        TestAssert.False(UvRevealRegion.TryCreate(0.2f, 0.6f, 0.7f, 0.3f, out _), "reversed V fails");
        TestAssert.False(UvRevealRegion.TryCreate(-0.1f, 0.6f, 0.3f, 0.7f, out _), "U below range fails");
        TestAssert.False(UvRevealRegion.TryCreate(0.2f, 0.6f, 0.3f, 1.1f, out _), "V above range fails");
        TestAssert.False(UvRevealRegion.TryCreate(float.NaN, 0.6f, 0.3f, 0.7f, out _), "non-finite bound fails");
    }
}
