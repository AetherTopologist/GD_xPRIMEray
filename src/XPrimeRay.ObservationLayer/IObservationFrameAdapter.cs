namespace XPrimeRay.ObservationLayer;

public interface IObservationFrameAdapter<in TSource>
{
    bool TryCreateSealedFrame(
        TSource source,
        out SealedObservationFrame? frame,
        out ObservationValidity validity);
}
