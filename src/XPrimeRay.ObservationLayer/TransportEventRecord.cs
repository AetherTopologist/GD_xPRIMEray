namespace XPrimeRay.ObservationLayer;

/// <summary>
/// Minimal portable transport event. AccumulatedPathLength is the sum of transport step lengths
/// through the event, in scene units; it is not affine parameter, coordinate time, or proper time.
/// </summary>
public readonly struct TransportEventRecord
{
    public TransportEventRecord(
        int rayIndex,
        int stepIndex,
        TransportEventKind kind,
        BoundaryIdentityCode boundaryId,
        FieldSourceIdentityCode fieldSourceId,
        float accumulatedPathLength)
    {
        RayIndex = rayIndex;
        StepIndex = stepIndex;
        Kind = kind;
        BoundaryId = boundaryId;
        FieldSourceId = fieldSourceId;
        AccumulatedPathLength = accumulatedPathLength;
    }

    public int RayIndex { get; }
    public int StepIndex { get; }
    public TransportEventKind Kind { get; }
    public BoundaryIdentityCode BoundaryId { get; }
    public FieldSourceIdentityCode FieldSourceId { get; }
    public float AccumulatedPathLength { get; }
}
