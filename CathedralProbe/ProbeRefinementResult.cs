/// <summary>
/// Result counters for one bounded refinement request.
/// </summary>
public struct ProbeRefinementResult
{
	public ushort RegionId;
	public int PixelsAttempted;
	public int NewlyResolved;
	public int StillUnresolved;
	public byte RefinementLevelReached;
	public bool BudgetCapHit;
	public bool CeilingReached;
}
