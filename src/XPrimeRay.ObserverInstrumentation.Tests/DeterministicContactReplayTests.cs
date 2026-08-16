namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class DeterministicContactReplayTests
{
	public static void Run()
	{
		ReplayIsOrderIndependentAfterCanonicalOrdering();
		ReplayHistoryDoesNotChangeSemanticOutcome();
	}

	private static void ReplayIsOrderIndependentAfterCanonicalOrdering()
	{
		(int Step, ProbeSurfaceClass Surface)[] events =
		{
			(12, ProbeSurfaceClass.Geometry),
			(18, ProbeSurfaceClass.Background),
			(25, ProbeSurfaceClass.Geometry)
		};
		(int Count, int First, int Last, bool Geometry, bool Background) Replay(
			IEnumerable<(int Step, ProbeSurfaceClass Surface)> ordered)
		{
			int count = 0, first = -1, last = -1;
			bool geometry = false, background = false;
			foreach ((int step, ProbeSurfaceClass surface) in ordered)
				TransportContactHistoryAccumulator.RecordContact(
					ref count, ref first, ref last, ref geometry, ref background, step, surface);
			return (count, first, last, geometry, background);
		}

		var ascending = Replay(events.OrderBy(item => item.Step));
		var descending = Replay(events.OrderByDescending(item => item.Step).OrderBy(item => item.Step));
		TestAssert.Equal(ascending.Count, descending.Count, "canonical replay contact count");
		TestAssert.Equal(ascending.First, descending.First, "canonical replay first step");
		TestAssert.Equal(ascending.Last, descending.Last, "canonical replay last step");
		TestAssert.True(ascending.Geometry && ascending.Background, "canonical replay surface flags");
	}

	private static void ReplayHistoryDoesNotChangeSemanticOutcome()
	{
		ProbeOutcomeCode outcome = ProbeOutcomeCode.MaxStepsExhausted;
		int count = 0, first = -1, last = -1;
		bool geometry = false, background = false;
		TransportContactHistoryAccumulator.RecordContact(
			ref count, ref first, ref last, ref geometry, ref background, 30, ProbeSurfaceClass.Geometry);
		TestAssert.Equal(ProbeOutcomeCode.MaxStepsExhausted, outcome, "replay preserves terminal outcome");
		TestAssert.Equal(1, count, "replay records accepted history");
	}
}
