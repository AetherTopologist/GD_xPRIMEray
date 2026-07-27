/// <summary>
/// Concise, deterministic evidence for one selected-region refinement request.
/// </summary>
public struct ProbeRefinementSummary
{
	public int RequestId;
	public ushort SourceRegionId;
	public byte RequestedRefinementLevel;
	public int SelectedPixelCount;
	public int AppliedPixelCount;
	public int PreviousMaxStepsCount;
	public int RemainingMaxStepsCount;
	public int ResolvedPixelCount;
	public int BecameHitGeometryCount;
	public int BecameBackgroundResolvedCount;
	public int BecameStoppedEarlyAbsorbedCount;
	public int BecameNumericalFailureCount;
	public int BecameInvalidCount;
	public int UnchangedMaxStepsCount;
	public int ChildRegionCount;
	public ushort LargestChildRegionId;
	public int LargestChildRegionPixelCount;
	public bool ContextMatched;
	public bool AppliedAtomically;
	public string FailureReason;
	public int PolicyMaxSteps;
	public float PolicyStepSize;
	public long ElapsedTicks;

	public static ProbeRefinementSummary Failure(
		int requestId,
		ushort sourceRegionId,
		byte requestedRefinementLevel,
		int selectedPixelCount,
		bool contextMatched,
		string failureReason)
	{
		return new ProbeRefinementSummary
		{
			RequestId = requestId,
			SourceRegionId = sourceRegionId,
			RequestedRefinementLevel = requestedRefinementLevel,
			SelectedPixelCount = selectedPixelCount,
			ContextMatched = contextMatched,
			AppliedAtomically = false,
			FailureReason = failureReason ?? string.Empty
		};
	}
}
