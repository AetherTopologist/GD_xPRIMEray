/// <summary>
/// Optional Stage 0 data contract for inspecting configured seam pixels without fabricating telemetry.
/// </summary>
public struct ProbeSeamDiagnosticPixel
{
	public string Label;
	public int PixelX;
	public int PixelY;
	public int PixelIndex;
	public bool InBounds;
	public ProbeOutcomeCode Outcome;
	public ushort RegionId;
	public byte RefinementLevel;
	public bool HasStepsUsed;
	public int StepsUsed;
	public int ConfiguredMaxSteps;
	public float StepSize;
	public bool HasIntegratedPathLength;
	public float IntegratedPathLength;
	public bool HasTerminalColliderIdentity;
	public ulong TerminalColliderId;
	public ProbeSurfaceClass SurfaceClass;
	public bool HasNormal;
	public bool NormalValid;
	public float HitNormalX;
	public float HitNormalY;
	public float HitNormalZ;
	public bool ContextMatched;
}
