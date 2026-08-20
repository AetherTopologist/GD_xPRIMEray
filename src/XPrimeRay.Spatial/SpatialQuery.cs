using System.Numerics;

namespace XPrimeRay.Spatial;

public readonly struct SurfaceHit
{
    public string CanonicalPrimitiveId { get; }
    public SpatialSurfaceClass SurfaceClass { get; }
    public float SegmentT { get; }
    public Vector3 Position { get; }
    public Vector3 Normal { get; }
    public bool NormalValid { get; }

    public SurfaceHit(string canonicalPrimitiveId, SpatialSurfaceClass surfaceClass, float segmentT, Vector3 position, Vector3 normal, bool normalValid)
    {
        CanonicalPrimitiveId = canonicalPrimitiveId;
        SurfaceClass = surfaceClass;
        SegmentT = segmentT;
        Position = position;
        Normal = normal;
        NormalValid = normalValid;
    }
}

public interface IPass1SpatialQuery
{
    string AuthorityToken { get; }
    bool IntersectsSegment(Vector3 from, Vector3 to, uint collisionMask, out SurfaceHit hit);
}

public sealed class LinearScanSpatialQuery : IPass1SpatialQuery
{
    public const string AuthorityTokenValue = "XPrimeRaySpatialKernel/LinearScan-v0";
    public const string IntersectionPolicyVersion = "finite-segment-oriented-box-slab-v0";
    private readonly FrozenGeometrySnapshot _snapshot;

    public string AuthorityToken => AuthorityTokenValue;
    public long QueryCount { get; private set; }
    public long PrimitiveTestCount { get; private set; }

    public LinearScanSpatialQuery(FrozenGeometrySnapshot snapshot)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    }

    public bool IntersectsSegment(Vector3 from, Vector3 to, uint collisionMask, out SurfaceHit hit)
    {
        QueryCount++;
        bool found = false;
        SurfaceHit nearest = default;
        foreach (FrozenOrientedBox primitive in _snapshot.Primitives)
        {
            if (primitive.IsExcluded || (primitive.CollisionLayer & collisionMask) == 0)
                continue;
            PrimitiveTestCount++;
            if (!OrientedBoxIntersection.TryIntersectSegment(primitive, from, to, out SurfaceHit candidate))
                continue;
            if (!found || candidate.SegmentT < nearest.SegmentT ||
                (candidate.SegmentT == nearest.SegmentT && string.CompareOrdinal(candidate.CanonicalPrimitiveId, nearest.CanonicalPrimitiveId) < 0))
            {
                found = true;
                nearest = candidate;
            }
        }
        hit = nearest;
        return found;
    }
}

public static class OrientedBoxIntersection
{
    public static bool TryIntersectSegment(FrozenOrientedBox box, Vector3 from, Vector3 to, out SurfaceHit hit)
    {
        Vector3 localFrom = Vector3.Transform(from, box.LocalFromWorld);
        Vector3 localTo = Vector3.Transform(to, box.LocalFromWorld);
        Vector3 direction = localTo - localFrom;
        Vector3 h = box.HalfExtents;

        if (IsInside(localFrom, h))
        {
            hit = new SurfaceHit(box.CanonicalPrimitiveId, box.SurfaceClass, 0f, from, Vector3.Zero, false);
            return true;
        }

        if (direction == Vector3.Zero)
        {
            hit = default;
            return false;
        }

        double enter = 0.0;
        double exit = 1.0;
        Vector3 enterNormal = Vector3.Zero;
        for (int axis = 0; axis < 3; axis++)
        {
            double origin = Get(localFrom, axis);
            double delta = Get(direction, axis);
            double extent = Get(h, axis);
            if (delta == 0.0)
            {
                if (origin < -extent || origin > extent)
                {
                    hit = default;
                    return false;
                }
                continue;
            }

            double a = (-extent - origin) / delta;
            double b = (extent - origin) / delta;
            Vector3 nearNormal;
            if (a <= b)
            {
                nearNormal = Axis(axis, -1f);
            }
            else
            {
                (a, b) = (b, a);
                nearNormal = Axis(axis, 1f);
            }

            if (a > enter)
            {
                enter = a;
                enterNormal = nearNormal;
            }
            if (b < exit) exit = b;
            if (enter > exit) break;
        }

        if (enter > exit || exit < 0.0 || enter > 1.0)
        {
            hit = default;
            return false;
        }

        float t = (float)Math.Clamp(enter, 0.0, 1.0);
        Vector3 localPosition = localFrom + direction * t;
        Vector3 worldPosition = Vector3.Transform(localPosition, box.WorldFromLocal);
        Vector3 worldNormal = Vector3.TransformNormal(enterNormal, box.WorldFromLocal);
        bool normalValid = worldNormal.LengthSquared() > 1e-20f;
        if (normalValid) worldNormal = Vector3.Normalize(worldNormal);
        hit = new SurfaceHit(box.CanonicalPrimitiveId, box.SurfaceClass, t, worldPosition, worldNormal, normalValid);
        return true;
    }

    private static bool IsInside(Vector3 p, Vector3 h) =>
        p.X >= -h.X && p.X <= h.X && p.Y >= -h.Y && p.Y <= h.Y && p.Z >= -h.Z && p.Z <= h.Z;

    private static double Get(Vector3 value, int axis) => axis switch { 1 => value.Y, 2 => value.Z, _ => value.X };

    private static Vector3 Axis(int axis, float sign) => axis switch
    {
        1 => new Vector3(0f, sign, 0f),
        2 => new Vector3(0f, 0f, sign),
        _ => new Vector3(sign, 0f, 0f)
    };
}
