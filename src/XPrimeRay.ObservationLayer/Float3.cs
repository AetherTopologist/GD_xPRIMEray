namespace XPrimeRay.ObservationLayer;

public readonly struct Float3 : IEquatable<Float3>
{
    public Float3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public bool Equals(Float3 other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    public override bool Equals(object? obj) => obj is Float3 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public static bool operator ==(Float3 left, Float3 right) => left.Equals(right);
    public static bool operator !=(Float3 left, Float3 right) => !left.Equals(right);
}
