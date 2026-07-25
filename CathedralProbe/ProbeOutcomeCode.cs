/// <summary>
/// Stage 0 outcome vocabulary derived from transport booleans and the pass-1 numerical guard.
/// </summary>
public enum ProbeOutcomeCode : byte
{
	Unprocessed = 0,
	HitGeometry = 1,
	BackgroundExit = 2,
	MaxStepsExhausted = 3,
	StoppedEarlyAbsorbed = 4,
	NumericalFailure = 5,
	Invalid = 255,
}
