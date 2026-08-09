namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class ObservationAcquisitionOwnershipTests
{
	public static void Run()
	{
		var ownership = new ObservationAcquisitionOwnership();
		TestAssert.Equal(ObservationAcquisitionOwner.Idle, ownership.Owner, "initial owner");
		TestAssert.True(ownership.TryAcquire(ObservationAcquisitionOwner.Live), "live acquires");
		TestAssert.False(ownership.TryAcquire(ObservationAcquisitionOwner.Snapshot), "snapshot cannot overlap live");
		TestAssert.True(ownership.Release(ObservationAcquisitionOwner.Live), "live releases");
		TestAssert.True(ownership.TryAcquire(ObservationAcquisitionOwner.Snapshot), "snapshot acquires");
		TestAssert.False(ownership.TryAcquire(ObservationAcquisitionOwner.RegionRefinement), "refinement cannot overlap snapshot");
		TestAssert.False(ownership.Release(ObservationAcquisitionOwner.Live), "wrong owner cannot release snapshot");
		TestAssert.True(ownership.Release(ObservationAcquisitionOwner.Snapshot), "snapshot terminal releases");
		TestAssert.True(ownership.TryAcquire(ObservationAcquisitionOwner.RegionRefinement), "refinement resumes after snapshot");
		TestAssert.False(ownership.TryAcquire(ObservationAcquisitionOwner.Live), "live cannot overlap refinement");
		TestAssert.True(ownership.Release(ObservationAcquisitionOwner.RegionRefinement), "refinement releases");
		TestAssert.True(ownership.TryAcquire(ObservationAcquisitionOwner.Live), "live resumes after refinement");
		TestAssert.True(ownership.Release(ObservationAcquisitionOwner.Live), "live terminal releases");
	}
}
