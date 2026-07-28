/// <summary>
/// Stable evidence model for one terminal Cathedral Probe snapshot lifecycle result.
/// </summary>
public struct ProbeSnapshotLifecycleResult
{
	public int RequestId;
	public ProbeSnapshotLifecycleState State;
	public ProbeSnapshotLifecycleReason Reason;
	public int Width;
	public int Height;
	public int TotalPixelCount;
	public int ProcessedPixelCount;
	public int UnprocessedPixelCount;
	public bool ContextMatched;
	public bool DimensionsMatched;
	public int PolicyMaxSteps;
	public float PolicyStepSize;
	public int HitGeometryCount;
	public int BackgroundResolvedCount;
	public int MaxStepsExhaustedCount;
	public int StoppedEarlyAbsorbedCount;
	public int NumericalFailureCount;
	public int InvalidCount;
	public bool RegionAnalysisAvailable;
	public int RegionCount;
	public int SelectableRegionCount;
	public ushort LargestRegionId;
	public int LargestRegionPixelCount;
	public bool RefinementEligible;
	public int ProcessFramesElapsed;
	public int PhysicsFramesElapsed;

	public bool IsTerminal =>
		State == ProbeSnapshotLifecycleState.Complete ||
		State == ProbeSnapshotLifecycleState.Incomplete ||
		State == ProbeSnapshotLifecycleState.Invalidated ||
		State == ProbeSnapshotLifecycleState.Failed;

	public bool HistogramReconciles()
	{
		return TotalPixelCount ==
			UnprocessedPixelCount +
			HitGeometryCount +
			BackgroundResolvedCount +
			MaxStepsExhaustedCount +
			StoppedEarlyAbsorbedCount +
			NumericalFailureCount +
			InvalidCount;
	}
}
