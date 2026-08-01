namespace XPrimeRay.ObservationLayer;

public readonly struct BoundaryIdentityCode : IEquatable<BoundaryIdentityCode>
{
    public BoundaryIdentityCode(uint value) => Value = value;
    public uint Value { get; }
    public bool IsKnown => Value != 0;
    public bool Equals(BoundaryIdentityCode other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is BoundaryIdentityCode other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
}

public readonly struct FieldSourceIdentityCode : IEquatable<FieldSourceIdentityCode>
{
    public FieldSourceIdentityCode(uint value) => Value = value;
    public uint Value { get; }
    public bool IsKnown => Value != 0;
    public bool Equals(FieldSourceIdentityCode other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is FieldSourceIdentityCode other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
}
