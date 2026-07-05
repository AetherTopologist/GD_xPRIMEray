using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Instruments;
using XPrimeRay.ObserverInstrumentation.Metadata;

namespace XPrimeRay.ObserverInstrumentation.Runtime;

// Pure, testable evaluation session. No Godot types. All overflow tracking lives here.
// The Godot adapter wraps this and converts HitPayload → InstrumentContext before calling ProcessContext.
// Tests use RunFrame(ReadOnlySpan<InstrumentContext>) directly.
public sealed class ObserverInstrumentationSession
{
    private readonly InstrumentRegistry _registry;
    private readonly InstrumentFrameBuffer _frameBuffer;
    private InstrumentMetadataCatalog? _catalog;

    public ObserverInstrumentationSession(
        InstrumentRegistry registry,
        int maxHitsCapacity,
        InstrumentMetadataCatalog? catalog = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (!registry.IsSealed)
            throw new InvalidOperationException("Registry must be sealed before creating a session.");
        if (maxHitsCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(maxHitsCapacity));
        _registry = registry;
        _catalog = catalog;
        _frameBuffer = new InstrumentFrameBuffer(registry.Count * maxHitsCapacity);
    }

    public bool IsEnabled => _registry.EnabledMask != ObserverInstrumentMask.None;

    // Per-frame overflow state. Reset at the start of each BeginFrame / RunFrame call.
    public bool OverflowOccurred { get; private set; }
    public int DroppedObservationCount { get; private set; }
    public int ObservationCapacity => _frameBuffer.Capacity;
    public int LastObservationCount { get; private set; }
    public ulong FrameSequence { get; private set; }

    public InstrumentFrameBuffer FrameBuffer => _frameBuffer;

    public void UpdateCatalog(InstrumentMetadataCatalog? catalog) => _catalog = catalog;

    // Builds a freshly sealed registry from config and returns a ready-to-use session.
    // All three instruments are always registered; EnabledMask controls which run per frame.
    public static ObserverInstrumentationSession Create(
        ObserverInstrumentationConfiguration config,
        int maxHitsCapacity)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (maxHitsCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(maxHitsCapacity));

        var registry = new InstrumentRegistry(
            new FlagCaptureInstrument(),
            new SurfaceUvInstrument(),
            new CheckerProbeInstrument());
        registry.SetEnabledMask(config.EnabledMask);
        registry.Seal();
        return new ObserverInstrumentationSession(registry, maxHitsCapacity, config.Catalog);
    }

    // Full-span entry point for pure tests (no Godot type conversion needed).
    public void RunFrame(ReadOnlySpan<InstrumentContext> contexts)
    {
        BeginFrame();
        if (IsEnabled)
        {
            for (int i = 0; i < contexts.Length; i++)
                ProcessContext(in contexts[i]);
        }
        EndFrame();
    }

    // Incremental API for the Godot adapter — avoids allocating an InstrumentContext array.
    public void BeginFrame()
    {
        OverflowOccurred = false;
        DroppedObservationCount = 0;
        _frameBuffer.Clear();
    }

    // Returns false when the buffer overflowed on this context; caller should break out of its loop.
    public bool ProcessContext(in InstrumentContext context)
    {
        if (_registry.Evaluate(in context, _catalog, _frameBuffer))
            return true;

        OverflowOccurred = true;
        DroppedObservationCount++;
        return false;
    }

    public void EndFrame()
    {
        LastObservationCount = _frameBuffer.Count;
        FrameSequence++;
    }
}
