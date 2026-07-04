using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Metadata;

namespace XPrimeRay.ObserverInstrumentation.Runtime;

public sealed class InstrumentRegistry
{
    private readonly IObserverInstrument[] _instruments;
    private bool _sealed;

    public InstrumentRegistry(params IObserverInstrument[] instruments)
    {
        ArgumentNullException.ThrowIfNull(instruments);
        _instruments = (IObserverInstrument[])instruments.Clone();
        for (int index = 0; index < _instruments.Length; index++)
        {
            ArgumentNullException.ThrowIfNull(_instruments[index]);
        }
    }

    public int Count => _instruments.Length;
    public ObserverInstrumentMask EnabledMask { get; private set; } = ObserverInstrumentMask.None;

    public void SetEnabledMask(ObserverInstrumentMask enabledMask)
    {
        if (_sealed)
        {
            throw new InvalidOperationException("The registry is sealed.");
        }

        EnabledMask = enabledMask & ObserverInstrumentMask.All;
    }

    public void Seal() => _sealed = true;

    public bool Evaluate(
        in InstrumentContext context,
        InstrumentMetadataCatalog? catalog,
        InstrumentFrameBuffer frameBuffer)
    {
        ArgumentNullException.ThrowIfNull(frameBuffer);
        if (!_sealed)
        {
            throw new InvalidOperationException("The registry must be sealed before evaluation.");
        }

        bool catalogAvailable = catalog is not null;
        bool metadataAvailable = false;
        InstrumentTargetMetadata metadata = default;
        string? colliderName = context.ColliderName;
        if (catalogAvailable && !string.IsNullOrEmpty(colliderName))
        {
            metadataAvailable = catalog!.TryGet(colliderName, out metadata);
        }

        for (int index = 0; index < _instruments.Length; index++)
        {
            IObserverInstrument instrument = _instruments[index];
            if ((EnabledMask & instrument.Feature) == 0)
            {
                continue;
            }

            instrument.Observe(
                in context,
                catalogAvailable,
                metadataAvailable,
                in metadata,
                out InstrumentObservation observation);
            if (!frameBuffer.TryAppend(in observation))
            {
                return false;
            }
        }

        return true;
    }
}
