#nullable enable
using System;
using System.Numerics;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.Metadata;
using XPrimeRay.ObserverInstrumentation.Runtime;

// Stage 1C adapter. The only bridge between the sovereign validated hit pipeline
// and the Observer Instrumentation Layer.
//
// Ownership:
//   _dbgHits is owned by GrinFilmCamera and passed in read-only via ReadOnlySpan.
//   The adapter does not store, modify, or re-derive intersection results.
//
// Reconfiguration:
//   A sealed InstrumentRegistry is never mutated. To change the mask or catalog,
//   call GrinFilmCamera.ApplyInstrumentationConfiguration, which creates a new adapter
//   via ObserverInstrumentationAdapter.FromConfiguration and discards the old session.
public sealed class ObserverInstrumentationAdapter
{
    private readonly ObserverInstrumentationSession _session;

    private ObserverInstrumentationAdapter(ObserverInstrumentationSession session) =>
        _session = session;

    public bool IsEnabled => _session.IsEnabled;

    // Per-frame overflow counters. Reset at the start of each RunFrame call.
    // Read after RunFrame to detect buffer saturation without hot-path allocation or throws.
    public bool OverflowOccurred => _session.OverflowOccurred;
    public int DroppedObservationCount => _session.DroppedObservationCount;
    public int ObservationCapacity => _session.ObservationCapacity;
    public int LastObservationCount => _session.LastObservationCount;
    internal ulong FrameSequence => _session.FrameSequence;
    internal ReadOnlySpan<InstrumentObservation> Observations => _session.FrameBuffer.AsSpan();

    public InstrumentFrameBuffer FrameBuffer => _session.FrameBuffer;

    public void UpdateCatalog(InstrumentMetadataCatalog? catalog) => _session.UpdateCatalog(catalog);

    // Safe activation path: builds a fresh sealed registry and session from config.
    // No existing registry is mutated; the new session replaces the old one entirely.
    public static ObserverInstrumentationAdapter FromConfiguration(
        ObserverInstrumentationConfiguration config,
        int maxHitsCapacity)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (maxHitsCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(maxHitsCapacity));

        return new ObserverInstrumentationAdapter(
            ObserverInstrumentationSession.Create(config, maxHitsCapacity));
    }

    // Process a completed band's hit buffer.
    // Converts HitPayload to InstrumentContext (no allocation) and runs all enabled instruments.
    // When the buffer fills, overflow is recorded and the loop breaks; no throws, no per-ray allocation.
    public void RunFrame(ReadOnlySpan<RayBeamRenderer.HitPayload> hits)
    {
        _session.BeginFrame();
        if (_session.IsEnabled)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                ref readonly RayBeamRenderer.HitPayload hit = ref hits[i];
                InstrumentContext context = HitContextMapper.BuildContext(
                    i,
                    hit.Valid,
                    hit.Absorbed,
                    new Vector3(hit.Position.X, hit.Position.Y, hit.Position.Z),
                    new Vector3(hit.Normal.X, hit.Normal.Y, hit.Normal.Z),
                    hit.ColliderName);
                if (!_session.ProcessContext(in context))
                    break;
            }
        }
        _session.EndFrame();
    }
}
