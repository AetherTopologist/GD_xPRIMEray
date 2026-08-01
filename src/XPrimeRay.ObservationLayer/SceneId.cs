using System.Text.RegularExpressions;

namespace XPrimeRay.ObservationLayer;

public readonly partial struct SceneId : IEquatable<SceneId>
{
    private SceneId(string value) => Value = value;

    public string Value { get; }
    public bool IsValid => !string.IsNullOrEmpty(Value) && SceneIdPattern().IsMatch(Value);

    public static bool TryCreate(string value, out SceneId sceneId)
    {
        sceneId = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value.Trim();
        if (!SceneIdPattern().IsMatch(trimmed))
        {
            return false;
        }

        sceneId = new SceneId(trimmed);
        return true;
    }

    public bool Equals(SceneId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is SceneId other && Equals(other);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
    public override string ToString() => Value ?? string.Empty;
    public static bool operator ==(SceneId left, SceneId right) => left.Equals(right);
    public static bool operator !=(SceneId left, SceneId right) => !left.Equals(right);

    [GeneratedRegex("^[a-z][a-z0-9_]*(\\.[a-z0-9_]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SceneIdPattern();
}
