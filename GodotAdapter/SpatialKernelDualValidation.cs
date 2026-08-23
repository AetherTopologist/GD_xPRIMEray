using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using XPrimeRay.Spatial;

namespace GodotAdapter;

public sealed class SpatialKernelDualValidationResult
{
    public required string GeometrySnapshotSha256 { get; init; }
    public required int PrimitiveCount { get; init; }
    public required int QueryCount { get; init; }
    public required string GodotContactCountSha256 { get; init; }
    public required string LinearContactCountSha256 { get; init; }
    public required string SpatialAuthorityContextSha256 { get; init; }
    public required string GodotContactHistogram { get; init; }
    public required string LinearContactHistogram { get; init; }
    public required int MismatchPixelCount { get; init; }
    public required int ContactCountMismatchPixelCount { get; init; }
    public required int SurfaceSemanticMismatchPixelCount { get; init; }
    public required int TotalAuthorityMismatchPixelCount { get; init; }
    public required int MismatchQueryCount { get; init; }
    public required string FirstMismatch { get; init; }
    public required long PrimitiveTestCount { get; init; }
    public required double ElapsedMilliseconds { get; init; }
    public required int[] LinearContactCounts { get; init; }
    public required string BvhContactCountSha256 { get; init; }
    public required int BvhHitMismatchQueryCount { get; init; }
    public required int BvhPrimitiveIdentityMismatchQueryCount { get; init; }
    public required int BvhSurfaceSemanticMismatchQueryCount { get; init; }
    public required int BvhSegmentTMismatchQueryCount { get; init; }
    public required int BvhMismatchQueryCount { get; init; }
    public required int BvhContactCountMismatchPixelCount { get; init; }
    public required int BvhSurfaceSemanticMismatchPixelCount { get; init; }
    public required int BvhTotalMismatchPixelCount { get; init; }
    public required long BvhPrimitiveTestCount { get; init; }
    public required long BvhNodeTestCount { get; init; }
    public required double BvhElapsedMilliseconds { get; init; }
    public required double BvhBuildElapsedMilliseconds { get; init; }
    public required int BvhNodeCount { get; init; }
    public required int BvhLeafCount { get; init; }
    public required int BvhMaxDepth { get; init; }
    public required string BvhBuildSha256 { get; init; }
}

public static class SpatialKernelDualValidator
{
    public const string DiagnosticAuthorityToken = LinearScanSpatialQuery.AuthorityTokenValue;

    public static SpatialKernelDualValidationResult Evaluate(
        Godot.Node sceneRoot,
        RayBeamRenderer.FormalProbeQuery[] queries,
        int[] godotContactCounts,
        int totalPixels,
        bool[]? godotQueryHits = null,
        byte[]? godotHadAnyGeometryContact = null,
        byte[]? godotHadAnyBackgroundContact = null,
        string sourceGroup = "fixture_source",
        string backgroundGroup = "fixture_background")
    {
        if (sceneRoot == null) throw new ArgumentNullException(nameof(sceneRoot));
        if (queries == null) throw new ArgumentNullException(nameof(queries));
        if (godotContactCounts == null) throw new ArgumentNullException(nameof(godotContactCounts));
        if (totalPixels < 0 || godotContactCounts.Length < totalPixels)
            throw new InvalidDataException("Godot contact channel is shorter than the film.");
        FrozenGeometrySnapshot snapshot = SpatialSnapshotBuilder.BuildFromGodotScene(sceneRoot, sourceGroup, backgroundGroup);
        return Evaluate(snapshot, queries, godotContactCounts, totalPixels, godotQueryHits, godotHadAnyGeometryContact, godotHadAnyBackgroundContact);
    }

    public static SpatialKernelDualValidationResult Evaluate(
        FrozenGeometrySnapshot snapshot,
        RayBeamRenderer.FormalProbeQuery[] queries,
        int[] godotContactCounts,
        int totalPixels,
        bool[]? godotQueryHits = null,
        byte[]? godotHadAnyGeometryContact = null,
        byte[]? godotHadAnyBackgroundContact = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(queries);
        ArgumentNullException.ThrowIfNull(godotContactCounts);
        if (totalPixels < 0 || godotContactCounts.Length < totalPixels)
            throw new InvalidDataException("Godot contact channel is shorter than the film.");
        if (godotQueryHits != null && godotQueryHits.Length != queries.Length)
            throw new InvalidDataException("Godot query-hit channel does not match the formal query dataset.");
        if (godotHadAnyGeometryContact != null && godotHadAnyGeometryContact.Length < totalPixels)
            throw new InvalidDataException("Godot geometry-contact channel is shorter than the film.");
        if (godotHadAnyBackgroundContact != null && godotHadAnyBackgroundContact.Length < totalPixels)
            throw new InvalidDataException("Godot background-contact channel is shorter than the film.");
        LinearScanSpatialQuery query = new(snapshot);
        SpatialBvhQuery bvh = new(snapshot);
        int[] linearCounts = new int[totalPixels];
        byte[] linearGeometry = new byte[totalPixels];
        byte[] linearBackground = new byte[totalPixels];
        int[] bvhCounts = new int[totalPixels];
        byte[] bvhGeometry = new byte[totalPixels];
        byte[] bvhBackground = new byte[totalPixels];
        int countMismatchPixels = 0;
        int semanticMismatchPixels = 0;
        int totalMismatchPixels = 0;
        int mismatchQueries = 0;
        string firstMismatch = "none";
        int bvhHitMismatches = 0;
        int bvhPrimitiveMismatches = 0;
        int bvhSurfaceMismatches = 0;
        int bvhSegmentTMismatches = 0;
        int bvhMismatchQueries = 0;
        long linearTicks = 0;
        long bvhTicks = 0;

        for (int queryIndex = 0; queryIndex < queries.Length; queryIndex++)
        {
            RayBeamRenderer.FormalProbeQuery recorded = queries[queryIndex];
            if (recorded.PixelIndex < 0 || recorded.PixelIndex >= totalPixels)
                throw new InvalidDataException($"formal query pixel out of range: {recorded.PixelIndex}");
            long linearStart = System.Diagnostics.Stopwatch.GetTimestamp();
            bool linearHit = query.IntersectsSegment(ToNumerics(recorded.From), ToNumerics(recorded.To), recorded.CollisionMask, out SurfaceHit surfaceHit);
            linearTicks += System.Diagnostics.Stopwatch.GetTimestamp() - linearStart;
            if (linearHit)
            {
                linearCounts[recorded.PixelIndex]++;
                if (surfaceHit.SurfaceClass == SpatialSurfaceClass.Geometry) linearGeometry[recorded.PixelIndex] = 1;
                if (surfaceHit.SurfaceClass == SpatialSurfaceClass.Background) linearBackground[recorded.PixelIndex] = 1;
            }
            if (godotQueryHits != null && linearHit != godotQueryHits[queryIndex])
                mismatchQueries++;

            long bvhStart = System.Diagnostics.Stopwatch.GetTimestamp();
            bool bvhHit = bvh.IntersectsSegment(ToNumerics(recorded.From), ToNumerics(recorded.To), recorded.CollisionMask, out SurfaceHit bvhResult);
            bvhTicks += System.Diagnostics.Stopwatch.GetTimestamp() - bvhStart;
            if (bvhHit)
            {
                bvhCounts[recorded.PixelIndex]++;
                if (bvhResult.SurfaceClass == SpatialSurfaceClass.Geometry) bvhGeometry[recorded.PixelIndex] = 1;
                if (bvhResult.SurfaceClass == SpatialSurfaceClass.Background) bvhBackground[recorded.PixelIndex] = 1;
            }
            bool bvhQueryMismatch = linearHit != bvhHit;
            if (bvhQueryMismatch) bvhHitMismatches++;
            else if (linearHit)
            {
                if (!string.Equals(surfaceHit.CanonicalPrimitiveId, bvhResult.CanonicalPrimitiveId, StringComparison.Ordinal)) { bvhPrimitiveMismatches++; bvhQueryMismatch = true; }
                if (surfaceHit.SurfaceClass != bvhResult.SurfaceClass) { bvhSurfaceMismatches++; bvhQueryMismatch = true; }
                if (surfaceHit.SegmentT != bvhResult.SegmentT) { bvhSegmentTMismatches++; bvhQueryMismatch = true; }
            }
            if (bvhQueryMismatch) bvhMismatchQueries++;
        }
        int bvhCountMismatchPixels = 0;
        int bvhSemanticMismatchPixels = 0;
        int bvhTotalMismatchPixels = 0;
        for (int pixel = 0; pixel < totalPixels; pixel++)
        {
            int godot = pixel < godotContactCounts.Length ? godotContactCounts[pixel] : -1;
            bool countMismatch = godot != linearCounts[pixel];
            bool semanticMismatch = godotHadAnyGeometryContact != null && godotHadAnyBackgroundContact != null &&
                (godotHadAnyGeometryContact[pixel] != linearGeometry[pixel] || godotHadAnyBackgroundContact[pixel] != linearBackground[pixel]);
            if (countMismatch) countMismatchPixels++;
            if (semanticMismatch) semanticMismatchPixels++;
            if (countMismatch || semanticMismatch) totalMismatchPixels++;
            if ((countMismatch || semanticMismatch) && firstMismatch == "none")
                firstMismatch = $"pixel={pixel} count={godot}->{linearCounts[pixel]} geometry={godotHadAnyGeometryContact?[pixel] ?? (byte)255}->{linearGeometry[pixel]} background={godotHadAnyBackgroundContact?[pixel] ?? (byte)255}->{linearBackground[pixel]}";
            bool bvhCountMismatch = linearCounts[pixel] != bvhCounts[pixel];
            bool bvhSemanticMismatch = linearGeometry[pixel] != bvhGeometry[pixel] || linearBackground[pixel] != bvhBackground[pixel];
            if (bvhCountMismatch) bvhCountMismatchPixels++;
            if (bvhSemanticMismatch) bvhSemanticMismatchPixels++;
            if (bvhCountMismatch || bvhSemanticMismatch) bvhTotalMismatchPixels++;
        }
        return new SpatialKernelDualValidationResult
        {
            GeometrySnapshotSha256 = snapshot.GeometrySnapshotSha256,
            PrimitiveCount = snapshot.PrimitiveCount,
            QueryCount = queries.Length,
            GodotContactCountSha256 = HashInt32(godotContactCounts, totalPixels),
            LinearContactCountSha256 = HashInt32(linearCounts, totalPixels),
            SpatialAuthorityContextSha256 = new SpatialAuthorityContext(
                snapshot.GeometrySnapshotSha256,
                LinearScanSpatialQuery.AuthorityTokenValue,
                LinearScanSpatialQuery.IntersectionPolicyVersion).CanonicalSha256,
            GodotContactHistogram = FormatHistogram(godotContactCounts, totalPixels),
            LinearContactHistogram = FormatHistogram(linearCounts, totalPixels),
            MismatchPixelCount = totalMismatchPixels,
            ContactCountMismatchPixelCount = countMismatchPixels,
            SurfaceSemanticMismatchPixelCount = semanticMismatchPixels,
            TotalAuthorityMismatchPixelCount = totalMismatchPixels,
            MismatchQueryCount = godotQueryHits == null ? -1 : mismatchQueries,
            FirstMismatch = firstMismatch,
            PrimitiveTestCount = query.PrimitiveTestCount,
            ElapsedMilliseconds = TicksToMilliseconds(linearTicks),
            LinearContactCounts = linearCounts,
            BvhContactCountSha256 = HashInt32(bvhCounts, totalPixels),
            BvhHitMismatchQueryCount = bvhHitMismatches,
            BvhPrimitiveIdentityMismatchQueryCount = bvhPrimitiveMismatches,
            BvhSurfaceSemanticMismatchQueryCount = bvhSurfaceMismatches,
            BvhSegmentTMismatchQueryCount = bvhSegmentTMismatches,
            BvhMismatchQueryCount = bvhMismatchQueries,
            BvhContactCountMismatchPixelCount = bvhCountMismatchPixels,
            BvhSurfaceSemanticMismatchPixelCount = bvhSemanticMismatchPixels,
            BvhTotalMismatchPixelCount = bvhTotalMismatchPixels,
            BvhPrimitiveTestCount = bvh.PrimitiveTestCount,
            BvhNodeTestCount = bvh.NodeTestCount,
            BvhElapsedMilliseconds = TicksToMilliseconds(bvhTicks),
            BvhBuildElapsedMilliseconds = bvh.BuildElapsedMilliseconds,
            BvhNodeCount = bvh.NodeCount,
            BvhLeafCount = bvh.LeafCount,
            BvhMaxDepth = bvh.MaxDepth,
            BvhBuildSha256 = bvh.BuildSha256
        };
    }

    private static string FormatHistogram(int[] values, int count)
    {
        int zero = 0, one = 0, two = 0, threePlus = 0;
        for (int i = 0; i < count; i++)
        {
            switch (values[i])
            {
                case 0: zero++; break;
                case 1: one++; break;
                case 2: two++; break;
                default: threePlus++; break;
            }
        }
        return $"0={zero},1={one},2={two},3+={threePlus}";
    }

    private static Vector3 ToNumerics(Godot.Vector3 value) => new(value.X, value.Y, value.Z);

    private static double TicksToMilliseconds(long ticks) => ticks * 1000.0 / System.Diagnostics.Stopwatch.Frequency;

    private static string HashInt32(int[] values, int count)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        for (int i = 0; i < count; i++) writer.Write(values[i]);
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }
}
