using System.Text.RegularExpressions;

namespace XPrimeRay.ObservationLayer;

public readonly partial struct ObservationChannelId : IEquatable<ObservationChannelId>
{
    private ObservationChannelId(string value) => Value = value;

    public string Value { get; }
    public bool IsValid => !string.IsNullOrEmpty(Value) && ChannelIdPattern().IsMatch(Value);

    public static bool TryCreate(string value, out ObservationChannelId id)
    {
        id = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value.Trim();
        if (!ChannelIdPattern().IsMatch(trimmed))
        {
            return false;
        }

        id = new ObservationChannelId(trimmed);
        return true;
    }

    public bool Equals(ObservationChannelId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is ObservationChannelId other && Equals(other);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
    public override string ToString() => Value ?? string.Empty;
    public static bool operator ==(ObservationChannelId left, ObservationChannelId right) => left.Equals(right);
    public static bool operator !=(ObservationChannelId left, ObservationChannelId right) => !left.Equals(right);

    [GeneratedRegex("^[a-z][a-z0-9_]*(\\.[a-z0-9_]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex ChannelIdPattern();
}
