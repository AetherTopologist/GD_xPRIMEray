using XPrimeRay.ObservationLayer;
using XPrimeRay.ObserverInstrumentation.Abstractions;

namespace XPrimeRay.ObserverInstrumentation.ObservationLayerAdapters;

public static class ObserverInstrumentationObservationFrameAdapter
{
    public const string SparseObservationChannelId = "observer.instrument.sparse_observation";

    public static bool TryCreateCompletedFrame(
        ObservationFrameDescriptor descriptor,
        ReadOnlySpan<InstrumentObservation> observations,
        out SealedObservationFrame? frame,
        out string reason)
    {
        frame = null;
        reason = string.Empty;
        if (!descriptor.SceneId.IsValid)
        {
            reason = "invalid_scene_id";
            return false;
        }
        if (!descriptor.IsComplete)
        {
            reason = "frame_not_complete";
            return false;
        }

        var records = new PortableInstrumentObservationRecord[observations.Length];
        for (int i = 0; i < observations.Length; i++)
        {
            InstrumentObservation observation = observations[i];
            if (!CanRepresentTruthfully(in observation))
            {
                reason = $"unsupported_instrument:{observation.Instrument}";
                return false;
            }
            records[i] = new PortableInstrumentObservationRecord(
                observation.RayIndex,
                InstrumentId(observation.Instrument),
                observation.DiagnosticState.ToString(),
                observation.ColliderName,
                new Float2(observation.Uv.X, observation.Uv.Y),
                observation.HasUv,
                observation.SampleValue);
        }

        var channel = new SealedObservationChannel<PortableInstrumentObservationRecord>(
            CreateSparseObservationDescriptor(),
            descriptor.Generation,
            records);
        frame = new SealedObservationFrame(descriptor, new ISealedObservationChannel[] { channel });
        return true;
    }

    public static bool CanRepresentTruthfully(in InstrumentObservation observation)
    {
        return observation.Instrument == InstrumentKind.SurfaceUv ||
            observation.Instrument == InstrumentKind.CheckerProbe ||
            observation.Instrument == InstrumentKind.FlagCapture;
    }

    private static string InstrumentId(InstrumentKind kind) => kind switch
    {
        InstrumentKind.SurfaceUv => "surface_uv",
        InstrumentKind.CheckerProbe => "checker_probe",
        InstrumentKind.FlagCapture => "flag_capture",
        _ => "unsupported"
    };

    private static ObservationChannelDescriptor CreateSparseObservationDescriptor()
    {
        ObservationChannelId.TryCreate(SparseObservationChannelId, out ObservationChannelId id);
        return new ObservationChannelDescriptor(
            id,
            "Observer Instrument Sparse Observation",
            ObservationDataType.InstrumentObservationRecord,
            ObservationDomain.SparseRayRecord,
            InstrumentTier.DirectObservation,
            "record",
            "Completed Observer Instrumentation frame.",
            "Sparse records preserve currently representable instrument observations; richer typed payloads are not flattened into booleans.");
    }
}
