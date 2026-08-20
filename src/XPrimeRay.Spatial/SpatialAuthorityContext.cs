using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace XPrimeRay.Spatial;

/// <summary>
/// Versioned diagnostic sub-context for the future spatial authority handoff.
/// It is deliberately separate from the existing ProbeContextKey in v0.
/// </summary>
public sealed class SpatialAuthorityContext
{
    public const string SchemaVersion = "SpatialAuthorityContext-v1";

    public string GeometrySnapshotSha256 { get; }
    public string SpatialKernelVersion { get; }
    public string IntersectionPolicyVersion { get; }
    public string CanonicalSha256 { get; }

    public SpatialAuthorityContext(string geometrySnapshotSha256, string spatialKernelVersion, string intersectionPolicyVersion)
    {
        if (string.IsNullOrWhiteSpace(geometrySnapshotSha256)) throw new ArgumentException("Geometry hash is required.", nameof(geometrySnapshotSha256));
        if (string.IsNullOrWhiteSpace(spatialKernelVersion)) throw new ArgumentException("Kernel version is required.", nameof(spatialKernelVersion));
        if (string.IsNullOrWhiteSpace(intersectionPolicyVersion)) throw new ArgumentException("Intersection policy version is required.", nameof(intersectionPolicyVersion));
        GeometrySnapshotSha256 = geometrySnapshotSha256;
        SpatialKernelVersion = spatialKernelVersion;
        IntersectionPolicyVersion = intersectionPolicyVersion;
        CanonicalSha256 = Convert.ToHexString(SHA256.HashData(Serialize(this))).ToLowerInvariant();
    }

    public static byte[] Serialize(SpatialAuthorityContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        using MemoryStream stream = new();
        WriteString(stream, SchemaVersion);
        WriteString(stream, context.GeometrySnapshotSha256);
        WriteString(stream, context.SpatialKernelVersion);
        WriteString(stream, context.IntersectionPolicyVersion);
        return stream.ToArray();
    }

    private static void WriteString(Stream stream, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(length, bytes.Length);
        stream.Write(length);
        stream.Write(bytes);
    }
}
