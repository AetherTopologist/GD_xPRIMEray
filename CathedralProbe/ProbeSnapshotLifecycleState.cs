/// <summary>
/// Deterministic lifecycle state for one Cathedral Probe snapshot request.
/// </summary>
public enum ProbeSnapshotLifecycleState : byte
{
	Inactive = 0,
	Requested = 1,
	WaitingForPhysics = 2,
	Capturing = 3,
	Complete = 4,
	Incomplete = 5,
	Invalidated = 6,
	Failed = 7,
}
