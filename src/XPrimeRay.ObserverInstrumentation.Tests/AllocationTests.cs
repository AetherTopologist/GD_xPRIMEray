using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Instruments;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Runtime;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class AllocationTests
{
    public static long LastHotPathBytes { get; private set; }

    public static void Run()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        var catalog = new InstrumentMetadataCatalog(metadata);
        var registry = new InstrumentRegistry(
            new SurfaceUvInstrument(),
            new CheckerProbeInstrument(),
            new FlagCaptureInstrument());
        registry.SetEnabledMask(ObserverInstrumentMask.All);
        registry.Seal();
        var buffer = new InstrumentFrameBuffer(3);
        InstrumentContext context = Stage1BTestData.Surface();

        for (int index = 0; index < 2_000; index++)
        {
            buffer.Clear();
            registry.Evaluate(in context, catalog, buffer);
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int index = 0; index < 100_000; index++)
        {
            buffer.Clear();
            if (!registry.Evaluate(in context, catalog, buffer))
            {
                throw new InvalidOperationException("preallocated buffer unexpectedly filled");
            }
        }
        LastHotPathBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        TestAssert.Equal(0L, LastHotPathBytes, "100,000 hot-path evaluations allocate zero bytes");
        Console.WriteLine($"  allocation delta: {LastHotPathBytes} bytes / 100000 evaluations");
    }
}
