using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using XPrimeRay.Core.Geometry;

namespace XPrimeRay.Spatial;

[Flags]
public enum FrozenPrimitiveFlags : uint
{
    None = 0,
    Excluded = 1,
    CollideWithBodies = 2,
    CollideWithAreas = 4
}

public sealed class FrozenOrientedBox
{
    public string CanonicalPrimitiveId { get; }
    public SpatialSurfaceClass SurfaceClass { get; }
    public Matrix4x4 WorldFromLocal { get; }
    public Matrix4x4 LocalFromWorld { get; }
    public Vector3 HalfExtents { get; }
    public Aabb3 WorldBounds { get; }
    public uint CollisionLayer { get; }
    public FrozenPrimitiveFlags Flags { get; }

    public FrozenOrientedBox(
        string canonicalPrimitiveId,
        SpatialSurfaceClass surfaceClass,
        Matrix4x4 worldFromLocal,
        Matrix4x4 localFromWorld,
        Vector3 halfExtents,
        Aabb3 worldBounds,
        uint collisionLayer,
        FrozenPrimitiveFlags flags)
    {
        if (string.IsNullOrWhiteSpace(canonicalPrimitiveId))
            throw new ArgumentException("Canonical primitive identity is required.", nameof(canonicalPrimitiveId));
        if (!Matrix4x4.Invert(worldFromLocal, out Matrix4x4 verifiedInverse))
            throw new ArgumentException("World transform must be invertible.", nameof(worldFromLocal));
        if (halfExtents.X < 0 || halfExtents.Y < 0 || halfExtents.Z < 0)
            throw new ArgumentOutOfRangeException(nameof(halfExtents));

        CanonicalPrimitiveId = canonicalPrimitiveId;
        SurfaceClass = surfaceClass;
        WorldFromLocal = worldFromLocal;
        LocalFromWorld = localFromWorld;
        HalfExtents = halfExtents;
        WorldBounds = worldBounds;
        CollisionLayer = collisionLayer;
        Flags = flags;

        // The adapter supplies the inverse, but reject a divergent inverse so
        // frozen measurement data cannot silently contain two transforms.
        if (!ApproximatelyEqual(verifiedInverse, localFromWorld))
            throw new ArgumentException("LocalFromWorld is not the inverse of WorldFromLocal.", nameof(localFromWorld));
    }

    public bool IsExcluded => (Flags & FrozenPrimitiveFlags.Excluded) != 0;

    private static bool ApproximatelyEqual(Matrix4x4 a, Matrix4x4 b)
    {
        ReadOnlySpan<float> av = stackalloc float[]
        {
            a.M11, a.M12, a.M13, a.M14, a.M21, a.M22, a.M23, a.M24,
            a.M31, a.M32, a.M33, a.M34, a.M41, a.M42, a.M43, a.M44
        };
        ReadOnlySpan<float> bv = stackalloc float[]
        {
            b.M11, b.M12, b.M13, b.M14, b.M21, b.M22, b.M23, b.M24,
            b.M31, b.M32, b.M33, b.M34, b.M41, b.M42, b.M43, b.M44
        };
        for (int i = 0; i < av.Length; i++)
            if (MathF.Abs(av[i] - bv[i]) > 1e-5f) return false;
        return true;
    }
}

public sealed class FrozenGeometrySnapshot
{
    public const string SchemaVersion = "FrozenGeometrySnapshot-v1";
    public IReadOnlyList<FrozenOrientedBox> Primitives { get; }
    public string GeometrySnapshotSha256 { get; }
    public int PrimitiveCount => Primitives.Count;

    public FrozenGeometrySnapshot(IEnumerable<FrozenOrientedBox> primitives)
    {
        if (primitives == null) throw new ArgumentNullException(nameof(primitives));
        FrozenOrientedBox[] ordered = primitives
            .OrderBy(p => p.CanonicalPrimitiveId, StringComparer.Ordinal)
            .ToArray();
        if (ordered.Select(p => p.CanonicalPrimitiveId).Distinct(StringComparer.Ordinal).Count() != ordered.Length)
            throw new ArgumentException("Canonical primitive identities must be unique.", nameof(primitives));
        Primitives = Array.AsReadOnly(ordered);
        GeometrySnapshotSha256 = FrozenGeometrySnapshotCanonicalSerializer.ComputeSha256(this);
    }
}

public static class FrozenGeometrySnapshotCanonicalSerializer
{
    public static byte[] Serialize(FrozenGeometrySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        WriteString(writer, FrozenGeometrySnapshot.SchemaVersion);
        writer.Write(snapshot.PrimitiveCount);
        foreach (FrozenOrientedBox primitive in snapshot.Primitives)
        {
            WriteString(writer, primitive.CanonicalPrimitiveId);
            writer.Write((byte)primitive.SurfaceClass);
            writer.Write(primitive.CollisionLayer);
            writer.Write((uint)primitive.Flags);
            WriteVector(writer, primitive.HalfExtents);
            WriteAabb(writer, primitive.WorldBounds);
            WriteMatrix(writer, primitive.WorldFromLocal);
            WriteMatrix(writer, primitive.LocalFromWorld);
        }
        writer.Flush();
        return stream.ToArray();
    }

    public static string ComputeSha256(FrozenGeometrySnapshot snapshot)
    {
        return Convert.ToHexString(SHA256.HashData(Serialize(snapshot))).ToLowerInvariant();
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    private static void WriteVector(BinaryWriter writer, Vector3 value)
    {
        writer.Write(value.X); writer.Write(value.Y); writer.Write(value.Z);
    }

    private static void WriteAabb(BinaryWriter writer, Aabb3 value)
    {
        WriteVector(writer, value.Min);
        WriteVector(writer, value.Max);
    }

    private static void WriteMatrix(BinaryWriter writer, Matrix4x4 value)
    {
        writer.Write(value.M11); writer.Write(value.M12); writer.Write(value.M13); writer.Write(value.M14);
        writer.Write(value.M21); writer.Write(value.M22); writer.Write(value.M23); writer.Write(value.M24);
        writer.Write(value.M31); writer.Write(value.M32); writer.Write(value.M33); writer.Write(value.M34);
        writer.Write(value.M41); writer.Write(value.M42); writer.Write(value.M43); writer.Write(value.M44);
    }
}
