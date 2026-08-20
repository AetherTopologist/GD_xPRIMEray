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
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        WriteString(writer, SchemaVersion);
        WriteString(writer, context.GeometrySnapshotSha256);
        WriteString(writer, context.SpatialKernelVersion);
        WriteString(writer, context.IntersectionPolicyVersion);
        writer.Flush();
        return stream.ToArray();
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }
}
