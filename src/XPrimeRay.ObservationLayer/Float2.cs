namespace XPrimeRay.ObservationLayer;

public readonly struct Float2 : IEquatable<Float2>
{
    public Float2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float X { get; }
    public float Y { get; }

    public bool Equals(Float2 other) => X.Equals(other.X) && Y.Equals(other.Y);
    public override bool Equals(object? obj) => obj is Float2 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public static bool operator ==(Float2 left, Float2 right) => left.Equals(right);
    public static bool operator !=(Float2 left, Float2 right) => !left.Equals(right);
}
