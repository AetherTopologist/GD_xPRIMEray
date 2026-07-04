using XPrimeRay.ObserverInstrumentation.Abstractions;

namespace XPrimeRay.ObserverInstrumentation.Runtime;

public sealed class InstrumentFrameBuffer
{
    private readonly InstrumentObservation[] _observations;

    public InstrumentFrameBuffer(int capacity)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        _observations = new InstrumentObservation[capacity];
    }

    public int Capacity => _observations.Length;
    public int Count { get; private set; }

    public void Clear()
    {
        Array.Clear(_observations, 0, Count);
        Count = 0;
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
}
