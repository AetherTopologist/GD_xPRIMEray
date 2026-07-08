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
    public bool IsSealed => _sealed;
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

            instrument.ObserveWithExtensions(
                in context,
                catalogAvailable,
                metadataAvailable,
                in metadata,
                out InstrumentObservation observation,
                out TextureSampleObservation textureObservation,
                out bool hasTextureObservation);
            if (!frameBuffer.TryAppend(in observation))
            {
                return false;
            }

            if (hasTextureObservation)
            {
                // Secondary texture payload overflow is explicitly non-authoritative here:
                // it must not turn a successfully appended base observation into a transport failure.
                frameBuffer.TryAppendTextureSample(in textureObservation);
            }
        }

        return true;
    }
}
