public enum ObservationAcquisitionOwner
{
	Idle = 0,
	Live = 1,
	Snapshot = 2,
	RegionRefinement = 3
}

public sealed class ObservationAcquisitionOwnership
{
	public ObservationAcquisitionOwner Owner { get; private set; } = ObservationAcquisitionOwner.Idle;

	public bool TryAcquire(ObservationAcquisitionOwner requested)
	{
		if (requested == ObservationAcquisitionOwner.Idle || Owner != ObservationAcquisitionOwner.Idle)
			return false;
		Owner = requested;
		return true;
	}

	public bool Release(ObservationAcquisitionOwner releasing)
	{
		if (Owner != releasing)
			return false;
		Owner = ObservationAcquisitionOwner.Idle;
		return true;
	}
}
