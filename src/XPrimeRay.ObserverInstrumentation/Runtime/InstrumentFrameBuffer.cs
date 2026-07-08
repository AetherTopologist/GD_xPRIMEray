using XPrimeRay.ObserverInstrumentation.Abstractions;

namespace XPrimeRay.ObserverInstrumentation.Runtime;

public sealed class InstrumentFrameBuffer
{
    private readonly InstrumentObservation[] _observations;
    private readonly TextureSampleObservation[]? _textureSamples;
    private int _textureCount;

    public InstrumentFrameBuffer(int capacity, int textureCapacity = 0)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }
        if (textureCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(textureCapacity));
        }

        _observations = new InstrumentObservation[capacity];
        _textureSamples = textureCapacity > 0
            ? new TextureSampleObservation[textureCapacity]
            : null;
    }

    public int Capacity => _observations.Length;
    public int Count { get; private set; }
    public int TextureSampleCapacity => _textureSamples?.Length ?? 0;
    public int TextureSampleCount => _textureCount;

    public void Clear()
    {
        Array.Clear(_observations, 0, Count);
        if (_textureSamples is not null)
        {
            Array.Clear(_textureSamples, 0, _textureCount);
        }
        Count = 0;
        _textureCount = 0;
    }

    public bool TryAppend(in InstrumentObservation observation)
    {
        if ((uint)Count >= (uint)_observations.Length)
        {
            return false;
        }

        _observations[Count++] = observation;
        return true;
    }

    public bool TryGet(int index, out InstrumentObservation observation)
    {
        if ((uint)index >= (uint)Count)
        {
            observation = default;
            return false;
        }

        observation = _observations[index];
        return true;
    }

    public ReadOnlySpan<InstrumentObservation> AsSpan() => _observations.AsSpan(0, Count);

    public ReadOnlySpan<TextureSampleObservation> TextureSamples =>
        _textureSamples is null
            ? ReadOnlySpan<TextureSampleObservation>.Empty
            : _textureSamples.AsSpan(0, _textureCount);

    public bool TryAppendTextureSample(in TextureSampleObservation observation)
    {
        // Texture payload overflow is independent of the primary observation buffer.
        // Callers decide whether a missing typed payload is fatal for their fixture.
        if (_textureSamples is null || (uint)_textureCount >= (uint)_textureSamples.Length)
        {
            return false;
        }

        _textureSamples[_textureCount++] = observation;
        return true;
    }
}
