#nullable enable

using System;
using System.IO;
using System.Text;

/// <summary>Stable identity of the observer represented by a measurement context.</summary>
public readonly struct ObserverId : System.IEquatable<ObserverId>
{
	public static readonly ObserverId A = new("A");
	public static readonly ObserverId B = new("B");
	public static readonly ObserverId C = new("C");

	public string Label { get; }

	public ObserverId(string label)
	{
		if (string.IsNullOrEmpty(label))
			throw new System.ArgumentException("Observer label must not be empty.", nameof(label));
		Label = label;
	}

	public bool Equals(ObserverId other) => string.Equals(Label ?? string.Empty, other.Label ?? string.Empty, System.StringComparison.Ordinal);
	public override bool Equals(object? obj) => obj is ObserverId other && Equals(other);
	public override int GetHashCode() => System.StringComparer.Ordinal.GetHashCode(Label ?? string.Empty);
	public override string ToString() => Label ?? string.Empty;
	public static bool operator ==(ObserverId left, ObserverId right) => left.Equals(right);
	public static bool operator !=(ObserverId left, ObserverId right) => !left.Equals(right);

	/// <summary>Canonical v2 encoding: uint32 little-endian UTF-8 byte length, then UTF-8 bytes.</summary>
	public byte[] SerializeCanonical()
	{
		byte[] label = Encoding.UTF8.GetBytes(Label ?? string.Empty);
		using MemoryStream stream = new();
		Span<byte> length = stackalloc byte[4];
		System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(length, (uint)label.Length);
		stream.Write(length);
		stream.Write(label);
		return stream.ToArray();
	}
}

#nullable restore
