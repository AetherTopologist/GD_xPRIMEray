#nullable enable
using System;
using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Runtime;

// Stage 1C adapter — the only bridge between the sovereign validated hit pipeline
// and the Observer Instrumentation Layer.
//
// Ownership:
//   _dbgHits is owned by GrinFilmCamera and passed in read-only via ReadOnlySpan.
//   The adapter does not store, modify, or re-derive intersection results.
//
// Feature mask:
//   When EnabledMask is None the adapter returns immediately from RunFrame,
//   performing zero work and zero allocations.
public sealed class ObserverInstrumentationAdapter
{
    private readonly InstrumentRegistry _registry;
    private readonly InstrumentFrameBuffer _frameBuffer;
    private InstrumentMetadataCatalog? _catalog;

    // registry must be sealed before passing to this constructor.
    // capacity is the maximum number of ray hits expected per RunFrame call.
    // The internal buffer is pre-allocated to registry.Count * capacity.
    public ObserverInstrumentationAdapter(
        InstrumentRegistry registry,
        int maxHitsCapacity,
        InstrumentMetadataCatalog? catalog = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (!registry.IsSealed)
            throw new InvalidOperationException("Registry must be sealed before creating an adapter.");
        if (maxHitsCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(maxHitsCapacity));

        _registry = registry;
        _catalog = catalog;
        _frameBuffer = new InstrumentFrameBuffer(registry.Count * maxHitsCapacity);
    }

    public bool IsEnabled => _registry.EnabledMask != ObserverInstrumentMask.None;
    public InstrumentFrameBuffer FrameBuffer => _frameBuffer;

    // Replace the active catalog without re-allocating the buffer or re-sealing the registry.
    public void UpdateCatalog(InstrumentMetadataCatalog? catalog) => _catalog = catalog;

    // Process a completed band's hit buffer. Clears and repopulates FrameBuffer.
    // Performs zero work when IsEnabled is false.
    public void RunFrame(ReadOnlySpan<RayBeamRenderer.HitPayload> hits)
    {
        if (!IsEnabled)
            return;

        _frameBuffer.Clear();
        for (int i = 0; i < hits.Length; i++)
        {
            ref readonly RayBeamRenderer.HitPayload hit = ref hits[i];

            // Godot.Vector3 → System.Numerics.Vector3 (no allocation)
            InstrumentContext context = HitContextMapper.BuildContext(
                i,
                hit.Valid,
                hit.Absorbed,
                new Vector3(hit.Position.X, hit.Position.Y, hit.Position.Z),
                new Vector3(hit.Normal.X, hit.Normal.Y, hit.Normal.Z),
                hit.ColliderName);

            _registry.Evaluate(in context, _catalog, _frameBuffer);
        }
    }
}
