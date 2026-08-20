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
    public required int MismatchQueryCount { get; init; }
    public required string FirstMismatch { get; init; }
    public required long PrimitiveTestCount { get; init; }
    public required double ElapsedMilliseconds { get; init; }
    public required int[] LinearContactCounts { get; init; }
}

public static class SpatialKernelDualValidator
{
    public const string DiagnosticAuthorityToken = LinearScanSpatialQuery.AuthorityTokenValue;

    public static SpatialKernelDualValidationResult Evaluate(
        Godot.Node sceneRoot,
        RayBeamRenderer.FormalProbeQuery[] queries,
        int[] godotContactCounts,
        int totalPixels,
        bool[]? godotQueryHits = null)
    {
        if (sceneRoot == null) throw new ArgumentNullException(nameof(sceneRoot));
        if (queries == null) throw new ArgumentNullException(nameof(queries));
        if (godotContactCounts == null) throw new ArgumentNullException(nameof(godotContactCounts));
        if (totalPixels < 0 || godotContactCounts.Length < totalPixels)
            throw new InvalidDataException("Godot contact channel is shorter than the film.");
        FrozenGeometrySnapshot snapshot = SpatialSnapshotBuilder.BuildFromGodotScene(sceneRoot);
        return Evaluate(snapshot, queries, godotContactCounts, totalPixels, godotQueryHits);
    }

    public static SpatialKernelDualValidationResult Evaluate(
        FrozenGeometrySnapshot snapshot,
        RayBeamRenderer.FormalProbeQuery[] queries,
        int[] godotContactCounts,
        int totalPixels,
        bool[]? godotQueryHits = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(queries);
        ArgumentNullException.ThrowIfNull(godotContactCounts);
        if (totalPixels < 0 || godotContactCounts.Length < totalPixels)
            throw new InvalidDataException("Godot contact channel is shorter than the film.");
        if (godotQueryHits != null && godotQueryHits.Length != queries.Length)
            throw new InvalidDataException("Godot query-hit channel does not match the formal query dataset.");
        LinearScanSpatialQuery query = new(snapshot);
        int[] linearCounts = new int[totalPixels];
        int mismatchPixels = 0;
        int mismatchQueries = 0;
        string firstMismatch = "none";
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int queryIndex = 0; queryIndex < queries.Length; queryIndex++)
        {
            RayBeamRenderer.FormalProbeQuery recorded = queries[queryIndex];
            if (recorded.PixelIndex < 0 || recorded.PixelIndex >= totalPixels)
                throw new InvalidDataException($"formal query pixel out of range: {recorded.PixelIndex}");
            bool linearHit = query.IntersectsSegment(ToNumerics(recorded.From), ToNumerics(recorded.To), recorded.CollisionMask, out _);
            if (linearHit)
                linearCounts[recorded.PixelIndex]++;
            if (godotQueryHits != null && linearHit != godotQueryHits[queryIndex])
                mismatchQueries++;
        }
        stopwatch.Stop();

        for (int pixel = 0; pixel < totalPixels; pixel++)
        {
            int godot = pixel < godotContactCounts.Length ? godotContactCounts[pixel] : -1;
            if (godot == linearCounts[pixel]) continue;
            mismatchPixels++;
            if (firstMismatch == "none")
                firstMismatch = $"pixel={pixel} godot={godot} linear={linearCounts[pixel]}";
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
            MismatchPixelCount = mismatchPixels,
            MismatchQueryCount = godotQueryHits == null ? -1 : mismatchQueries,
            FirstMismatch = firstMismatch,
            PrimitiveTestCount = query.PrimitiveTestCount,
            ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
            LinearContactCounts = linearCounts
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

    private static string HashInt32(int[] values, int count)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        for (int i = 0; i < count; i++) writer.Write(values[i]);
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }
}
