/// <summary>
/// Stage 0 outcome vocabulary derived from pass-1 transport state, surface class, and the numerical guard.
/// </summary>
public enum ProbeOutcomeCode : byte
{
	Unprocessed = 0,
	HitGeometry = 1,
	BackgroundResolved = 2,
	MaxStepsExhausted = 3,
	StoppedEarlyAbsorbed = 4,
	NumericalFailure = 5,
	Invalid = 255,
}
