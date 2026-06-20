using System.Numerics;
using XPrimeRay.Core.Fixtures;

namespace XPrimeRay.Core.Fields;

public sealed record RadialGrinField
{
    public Vector3 Center { get; init; }
    public float RadiusOuter { get; init; }
    public float Amplitude { get; init; }
    public FieldCurveType CurveType { get; init; }
    public float Gamma { get; init; } = 1f;

    public static RadialGrinField FromDefinition(FieldDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!Enum.TryParse<FieldCurveType>(definition.CurveType, ignoreCase: true, out var curveType))
        {
            curveType = FieldCurveType.Power;
        }

        return new RadialGrinField
        {
            Center = new Vector3(definition.Center[0], definition.Center[1], definition.Center[2]),
            RadiusOuter = definition.RadiusOuter,
            Amplitude = definition.Amplitude,
            CurveType = curveType,
            Gamma = definition.Gamma,
        };
    }

    public Vector3 SampleBend(Vector3 position, Vector3 direction, float stepSize)
    {
        var toCenter = Center - position;
        var distanceSq = toCenter.LengthSquared();
        if (distanceSq <= 1e-12f || !float.IsFinite(distanceSq))
        {
            return Vector3.Zero;
        }

        var distance = MathF.Sqrt(distanceSq);
        if (distance > RadiusOuter)
        {
            return Vector3.Zero;
        }

        var radial = toCenter / distance;
        var u = Math.Clamp(distance / RadiusOuter, 0f, 1f);
        var profile = FieldCurves.Eval(CurveType, u, Gamma, 0f, 0f, clamp01: true);
        var signedStrength = Amplitude * profile * stepSize;
        var requestedBend = radial * signedStrength;

        var safeDirection = NormalizeOrFallback(direction, Vector3.UnitZ);
        var perpendicularBend = requestedBend - (safeDirection * Vector3.Dot(requestedBend, safeDirection));
        return IsFinite(perpendicularBend) ? perpendicularBend : Vector3.Zero;
    }

    private static Vector3 NormalizeOrFallback(Vector3 value, Vector3 fallback)
    {
        var lengthSq = value.LengthSquared();
        if (lengthSq <= 1e-20f || !float.IsFinite(lengthSq))
        {
            return fallback;
        }

        return value / MathF.Sqrt(lengthSq);
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
    }
}
