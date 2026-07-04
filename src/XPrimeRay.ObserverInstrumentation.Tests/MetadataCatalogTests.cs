using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Math;
using XPrimeRay.ObserverInstrumentation.Metadata;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class MetadataCatalogTests
{
    public static void Run()
    {
        InstrumentTargetMetadata metadata = Stage1BTestData.Metadata();
        var catalog = new InstrumentMetadataCatalog(metadata);
        TestAssert.Equal(1, catalog.Count, "catalog count");
        TestAssert.True(catalog.TryGet(Stage1BTestData.ProbeName, out var found), "ordinal key resolves");
        TestAssert.True(found.IsValid, "resolved metadata is valid");

        ExpectArgument(() => new InstrumentMetadataCatalog(metadata, metadata), "duplicate names reject");
        ExpectArgument(
            () => new InstrumentMetadataCatalog(default(InstrumentTargetMetadata)),
            "invalid metadata rejects");

        TestAssert.True(
            UvRevealRegion.TryCreate(0.2f, 0.4f, 0.2f, 0.4f, out UvRevealRegion region),
            "valid region creates");
        TestAssert.False(
            InstrumentTargetMetadata.TryCreate("", Vector3.Zero, 8, 8, region, out _),
            "empty name rejects");
        TestAssert.False(
            InstrumentTargetMetadata.TryCreate("probe", Vector3.Zero, 0, 8, region, out _),
            "invalid tile count rejects");
    }

    private static void ExpectArgument(Action action, string message)
    {
        try
        {
            action();
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }
}
