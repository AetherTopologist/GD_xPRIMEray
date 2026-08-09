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
    }
}
