using System.Numerics;
using System.Security.Cryptography;
using XPrimeRay.Core.Geometry;
using XPrimeRay.Spatial;

namespace XPrimeRay.ObserverInstrumentation.Tests;

public static class SpatialKernelTests
{
    public static void Run()
    {
        SameGeometryHasSameSnapshotHash();
        PrimitivePermutationHasSameSnapshotHash();
        ProcessStableCanonicalBytes();
        SpatialAuthorityContextIsDeterministic();
        InsideSegment();
        FaceTouch();
        EdgeTouch();
        CornerTouch();
        ZeroLengthInsideAndOutside();
        TransformedBox();
        NearestPrimitive();
        EqualDistanceTieBreak();
        ExcludedPrimitive();
        RepeatedQueriesStable();
        Console.WriteLine("PASS SpatialKernelTests");
    }

    private static void SameGeometryHasSameSnapshotHash()
    {
        FrozenGeometrySnapshot a = Snapshot(Box("/World/Wall", Vector3.Zero));
        FrozenGeometrySnapshot b = Snapshot(Box("/World/Wall", Vector3.Zero));
        Assert(a.GeometrySnapshotSha256 == b.GeometrySnapshotSha256, "same geometry hash");
    }

    private static void PrimitivePermutationHasSameSnapshotHash()
    {
        FrozenOrientedBox a = Box("/World/A", new Vector3(-2, 0, 0));
        FrozenOrientedBox b = Box("/World/B", new Vector3(2, 0, 0));
        Assert(Snapshot(a, b).GeometrySnapshotSha256 == Snapshot(b, a).GeometrySnapshotSha256, "permuted primitive order hash");
    }

    private static void ProcessStableCanonicalBytes()
    {
        FrozenGeometrySnapshot snapshot = Snapshot(Box("/World/Wall", Vector3.Zero));
        string a = Convert.ToHexString(SHA256.HashData(FrozenGeometrySnapshotCanonicalSerializer.Serialize(snapshot)));
        string b = Convert.ToHexString(SHA256.HashData(FrozenGeometrySnapshotCanonicalSerializer.Serialize(snapshot)));
        Assert(a == b, "canonical serialization repeat");
    }

    private static void SpatialAuthorityContextIsDeterministic()
    {
        FrozenGeometrySnapshot snapshot = Snapshot(Box("/World/Wall", Vector3.Zero));
        SpatialAuthorityContext a = new(snapshot.GeometrySnapshotSha256, LinearScanSpatialQuery.AuthorityTokenValue, LinearScanSpatialQuery.IntersectionPolicyVersion);
        SpatialAuthorityContext b = new(snapshot.GeometrySnapshotSha256, LinearScanSpatialQuery.AuthorityTokenValue, LinearScanSpatialQuery.IntersectionPolicyVersion);
        Assert(a.CanonicalSha256 == b.CanonicalSha256, "spatial authority context hash");
        SpatialAuthorityContext changed = new(snapshot.GeometrySnapshotSha256, "other", LinearScanSpatialQuery.IntersectionPolicyVersion);
        Assert(a.CanonicalSha256 != changed.CanonicalSha256, "spatial authority context distinguishes kernel");
    }

    private static void InsideSegment()
    {
        LinearScanSpatialQuery query = new(Snapshot(Box("/World/Box", Vector3.Zero)));
        Assert(query.IntersectsSegment(Vector3.Zero, new Vector3(2, 0, 0), 1, out SurfaceHit hit), "inside hit");
        Assert(hit.SegmentT == 0f && !hit.NormalValid, "inside endpoint semantics");
    }

    private static void FaceTouch() => AssertHit(new Vector3(-2, 0, 0), new Vector3(-1, 0, 0), 1f, "face touch");
    private static void EdgeTouch() => AssertHit(new Vector3(-2, 1, 0), new Vector3(0, 1, 0), 0.5f, "edge touch");
    private static void CornerTouch() => AssertHit(new Vector3(-2, 1, 1), new Vector3(0, 1, 1), 0.5f, "corner touch");

    private static void ZeroLengthInsideAndOutside()
    {
        LinearScanSpatialQuery query = new(Snapshot(Box("/World/Box", Vector3.Zero)));
        Assert(query.IntersectsSegment(Vector3.Zero, Vector3.Zero, 1, out _), "zero length inside");
        Assert(!query.IntersectsSegment(new Vector3(2, 0, 0), new Vector3(2, 0, 0), 1, out _), "zero length outside");
    }

    private static void TransformedBox()
    {
        Matrix4x4 world = Matrix4x4.CreateRotationZ(MathF.PI / 4f) * Matrix4x4.CreateTranslation(3, 0, 0);
        LinearScanSpatialQuery query = new(Snapshot(Box("/World/Rotated", Vector3.Zero, world)));
        Assert(query.IntersectsSegment(Vector3.Zero, new Vector3(6, 0, 0), 1, out _), "transformed oriented box");
    }

    private static void NearestPrimitive()
    {
        LinearScanSpatialQuery query = new(Snapshot(Box("/World/Far", new Vector3(4, 0, 0)), Box("/World/Near", new Vector3(2, 0, 0))));
        Assert(query.IntersectsSegment(Vector3.Zero, new Vector3(8, 0, 0), 1, out SurfaceHit hit), "nearest query hit");
        Assert(hit.CanonicalPrimitiveId.StartsWith("/World/Near", StringComparison.Ordinal), "nearest primitive");
    }

    private static void EqualDistanceTieBreak()
    {
        LinearScanSpatialQuery query = new(Snapshot(Box("/World/Z", new Vector3(2, 0, 0)), Box("/World/A", new Vector3(2, 0, 0))));
        Assert(query.IntersectsSegment(Vector3.Zero, new Vector3(8, 0, 0), 1, out SurfaceHit hit), "tie query hit");
        Assert(hit.CanonicalPrimitiveId.StartsWith("/World/A", StringComparison.Ordinal), "ordinal tie break");
    }

    private static void ExcludedPrimitive()
    {
        FrozenOrientedBox source = Box("/World/Excluded", Vector3.Zero);
        FrozenOrientedBox excluded = new(source.CanonicalPrimitiveId, source.SurfaceClass, source.WorldFromLocal,
            source.LocalFromWorld, source.HalfExtents, source.WorldBounds, source.CollisionLayer, FrozenPrimitiveFlags.Excluded);
        LinearScanSpatialQuery query = new(Snapshot(excluded));
        Assert(!query.IntersectsSegment(new Vector3(-2, 0, 0), new Vector3(2, 0, 0), 1, out _), "excluded primitive");
    }

    private static void RepeatedQueriesStable()
    {
        LinearScanSpatialQuery query = new(Snapshot(Box("/World/Box", Vector3.Zero)));
        for (int i = 0; i < 20; i++)
        {
            Assert(query.IntersectsSegment(new Vector3(-2, 0, 0), new Vector3(2, 0, 0), 1, out SurfaceHit hit), "repeat query hit");
            Assert(hit.SegmentT == 0.25f, "repeat query result");
        }
    }

    private static void AssertHit(Vector3 from, Vector3 to, float expectedT, string name)
    {
        LinearScanSpatialQuery query = new(Snapshot(Box("/World/Box", Vector3.Zero)));
        Assert(query.IntersectsSegment(from, to, 1, out SurfaceHit hit), name);
        Assert(hit.SegmentT == expectedT && hit.NormalValid, name + " t/normal");
    }

    private static FrozenGeometrySnapshot Snapshot(params FrozenOrientedBox[] boxes) => new(boxes);

    private static FrozenOrientedBox Box(string id, Vector3 center, Matrix4x4? transform = null)
    {
        Matrix4x4 world = transform ?? Matrix4x4.CreateTranslation(center);
        Matrix4x4.Invert(world, out Matrix4x4 inverse);
        Vector3 half = Vector3.One;
        Vector3 min = new(float.PositiveInfinity);
        Vector3 max = new(float.NegativeInfinity);
        for (int i = 0; i < 8; i++)
        {
            Vector3 local = new((i & 1) != 0 ? 1 : -1, (i & 2) != 0 ? 1 : -1, (i & 4) != 0 ? 1 : -1);
            Vector3 p = Vector3.Transform(local, world);
            min = Vector3.Min(min, p); max = Vector3.Max(max, p);
        }
        return new FrozenOrientedBox(id, SpatialSurfaceClass.Geometry, world, inverse, half, new Aabb3(min, max), 1, FrozenPrimitiveFlags.CollideWithBodies);
    }

    private static void Assert(bool value, string message)
    {
        if (!value) throw new InvalidOperationException("SpatialKernelTests: " + message);
    }
}
