namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class TransportContactHistoryTests
{
    public static void Run()
    {
        int count = 0;
        int first = -1;
        int last = -1;
        bool geometry = false;
        bool background = false;

        TransportContactHistoryAccumulator.Reset(ref count, ref first, ref last, ref geometry, ref background);
        TestAssert.Equal(0, count, "reset count");
        TestAssert.Equal(-1, first, "reset first step");
        TestAssert.Equal(-1, last, "reset last step");

        TransportContactHistoryAccumulator.RecordContact(ref count, ref first, ref last, ref geometry, ref background, 30, ProbeSurfaceClass.Geometry);
        TransportContactHistoryAccumulator.RecordContact(ref count, ref first, ref last, ref geometry, ref background, 40, ProbeSurfaceClass.Background);
        TransportContactHistoryAccumulator.RecordContact(ref count, ref first, ref last, ref geometry, ref background, 35, ProbeSurfaceClass.Unknown);
        TestAssert.Equal(3, count, "accepted contacts only");
        TestAssert.Equal(30, first, "first contact is write-once");
        TestAssert.Equal(40, last, "last contact is monotonic");
        TestAssert.True(geometry, "geometry history");
        TestAssert.True(background, "background history");

        TransportContactHistoryAccumulator.Reset(ref count, ref first, ref last, ref geometry, ref background);
        TestAssert.Equal(0, count, "frame reset count");
        TestAssert.False(geometry, "frame reset geometry");
        TestAssert.False(background, "frame reset background");

        // A replicated film block copies one source history; it cannot create an additional contact.
        int replicatedCount = count;
        int replicatedFirst = first;
        int replicatedLast = last;
        TestAssert.Equal(count, replicatedCount, "stride count is copied");
        TestAssert.Equal(first, replicatedFirst, "stride first step is copied");
        TestAssert.Equal(last, replicatedLast, "stride last step is copied");

        foreach ((int width, int height) in new[] { (80, 45), (160, 90), (37, 19) })
        {
            TransportContactHistoryPlaneLayout layout = new(width, height);
            TestAssert.Equal(width * height, layout.Capacity, "full-frame capacity");
            TestAssert.True(layout.IsFullFrameCapacity(layout.Capacity), "capacity accepted");
            TestAssert.False(layout.IsFullFrameCapacity(layout.Capacity - 1), "band capacity rejected");
            TestAssert.True(layout.TryGetDestinationIndex(width - 2, height - 2, 4, width - 1, height - 1, out int destination), "partial stride destination");
            TestAssert.Equal((height - 1) * width + width - 1, destination, "partial stride index");
        }

        TransportContactHistoryPlaneLayout resized = new(80, 45);
        TestAssert.True(resized.IsFullFrameCapacity(160 * 90), "larger capacity can cover the new layout");
        resized = new TransportContactHistoryPlaneLayout(160, 90);
        TestAssert.True(resized.IsFullFrameCapacity(160 * 90), "reallocated capacity");

        TestAssert.True(TransportEffortValidity.TryNormalize(40, 80, false, out float effort), "effort valid");
        TestAssert.NearlyEqual(0.5f, effort, 0.0001f, "effort ratio");
        TestAssert.False(TransportEffortValidity.TryNormalize(40, 80, true, out _), "numerical effort unavailable");
        TestAssert.False(TransportEffortValidity.TryNormalize(40, 0, false, out _), "zero budget effort unavailable");
    }
}
