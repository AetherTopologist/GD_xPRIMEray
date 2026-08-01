namespace XPrimeRay.ObservationLayer;

/// <summary>
/// Host-neutral transport policy identity for reproducible observation frames.
/// ContextIdentity owns camera/context composition; this type owns known policy inputs.
/// </summary>
public readonly struct ObservationPolicyFingerprint : IEquatable<ObservationPolicyFingerprint>
{
    public ObservationPolicyFingerprint(
        int baseStepsPerRay,
        float baseStepLength,
        float minStepLength,
        float maxStepLength,
        float bendScale,
        float fieldStrength,
        uint fieldSourceEpoch,
        uint geometryEpoch,
        uint boundaryLayerEpoch,
        byte policyVersion,
        string transportMode)
    {
        BaseStepsPerRay = baseStepsPerRay;
        BaseStepLength = baseStepLength;
        MinStepLength = minStepLength;
        MaxStepLength = maxStepLength;
        BendScale = bendScale;
        FieldStrength = fieldStrength;
        FieldSourceEpoch = fieldSourceEpoch;
        GeometryEpoch = geometryEpoch;
        BoundaryLayerEpoch = boundaryLayerEpoch;
        PolicyVersion = policyVersion;
        TransportMode = string.IsNullOrWhiteSpace(transportMode) ? "unknown" : transportMode.Trim();
    }

    public int BaseStepsPerRay { get; }
    public float BaseStepLength { get; }
    public float MinStepLength { get; }
    public float MaxStepLength { get; }
    public float BendScale { get; }
    public float FieldStrength { get; }
    public uint FieldSourceEpoch { get; }
    public uint GeometryEpoch { get; }
    public uint BoundaryLayerEpoch { get; }
    public byte PolicyVersion { get; }
    public string TransportMode { get; }

    public bool Equals(ObservationPolicyFingerprint other) =>
        BaseStepsPerRay == other.BaseStepsPerRay &&
        BaseStepLength.Equals(other.BaseStepLength) &&
        MinStepLength.Equals(other.MinStepLength) &&
        MaxStepLength.Equals(other.MaxStepLength) &&
        BendScale.Equals(other.BendScale) &&
        FieldStrength.Equals(other.FieldStrength) &&
        FieldSourceEpoch == other.FieldSourceEpoch &&
        GeometryEpoch == other.GeometryEpoch &&
        BoundaryLayerEpoch == other.BoundaryLayerEpoch &&
        PolicyVersion == other.PolicyVersion &&
        string.Equals(TransportMode, other.TransportMode, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ObservationPolicyFingerprint other && Equals(other);
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(BaseStepsPerRay);
        hash.Add(BaseStepLength);
        hash.Add(MinStepLength);
        hash.Add(MaxStepLength);
        hash.Add(BendScale);
        hash.Add(FieldStrength);
        hash.Add(FieldSourceEpoch);
        hash.Add(GeometryEpoch);
        hash.Add(BoundaryLayerEpoch);
        hash.Add(PolicyVersion);
        hash.Add(TransportMode, StringComparer.Ordinal);
        return hash.ToHashCode();
    }
}
