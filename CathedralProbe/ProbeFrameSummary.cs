/// <summary>
/// Frame-level Cathedral Probe counts for one context key.
/// </summary>
public struct ProbeFrameSummary
{
	public int TotalPixels;
	public int HitGeometryCount;
	public int BackgroundExitCount;
	public int MaxStepsExhaustedCount;
	public int StoppedEarlyAbsorbedCount;
	public int NumericalFailureCount;
	public int RegionCount;
	public int SelectableRegionCount;
	public int LargestRegionPixelCount;
	public ushort LargestRegionId;
	public int LastRefinementPixelsAttempted;
	public int LastRefinementNewlyResolved;
	public int LastRefinementStillUnresolved;
	public ProbeContextKey ContextKey;
}
