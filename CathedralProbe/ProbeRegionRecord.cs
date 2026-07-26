/// <summary>
/// Region metadata with inline outcome distribution and no pixel-member storage.
/// </summary>
public struct ProbeRegionRecord
{
	public ushort Id;
	public int PixelCount;
	public ushort MinX;
	public ushort MinY;
	public ushort MaxX;
	public ushort MaxY;
	public byte MaxRefinementLevel;
	public bool IsPrimarilyMaxStepsExhausted;
	public int CountHitGeometry;
	public int CountBackgroundResolved;
	public int CountMaxStepsExhausted;
	public int CountStoppedEarlyAbsorbed;
	public int CountNumericalFailure;
}
