using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Metadata;

namespace XPrimeRay.ObserverInstrumentation.Runtime;

// Immutable configuration snapshot. Pass to ObserverInstrumentationSession.Create
// to build a freshly sealed registry and session in one step.
// The default state (EnabledMask=None, Catalog=null) produces zero work per frame.
public sealed class ObserverInstrumentationConfiguration
{
    public static readonly ObserverInstrumentationConfiguration Disabled = new();

    public ObserverInstrumentMask EnabledMask { get; init; } = ObserverInstrumentMask.None;
    public InstrumentMetadataCatalog? Catalog { get; init; }
}
