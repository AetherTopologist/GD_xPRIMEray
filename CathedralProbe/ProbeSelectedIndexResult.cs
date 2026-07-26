/// <summary>
/// Result for one caller-selected film pixel rerun. Owns no buffers.
/// </summary>
public struct ProbeSelectedIndexResult
{
	public int PixelIndex;
	public ProbeOutcomeCode Outcome;
	public ProbeSurfaceClass SurfaceClass;
	public int FinalStepCount;
	public bool HadNumericalFailure;
	public bool Found;
	public bool MaxStepsReached;
	public bool StoppedEarly;
}
