/// <summary>
/// Bounded base/refinement transport effort for Cathedral Probe.
/// </summary>
public readonly struct ProbePolicy
{
	public readonly int BaseStepsPerRay;
	public readonly float BaseStepLength;
	public readonly int RefinedStepsPerRay;
	public readonly float RefinedStepLength;
	public readonly int MaxPixelsPerRequest;
	public readonly byte MaxRefinementLevel;
	public readonly byte Version;
}
