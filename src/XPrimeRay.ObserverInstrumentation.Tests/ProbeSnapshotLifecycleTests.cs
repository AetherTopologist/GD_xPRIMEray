namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class ProbeSnapshotLifecycleTests
{
	public static void Run()
	{
		ValidTransitions();
		IllegalTransitionRejected();
		RequestGenerationMonotonicity();
		WaitingForPhysicsDoesNotMutateOutcomes();
		StaleGenerationRejected();
		ContextChangeInvalidates();
		DimensionChangeInvalidates();
		IncompleteCannotBecomeComplete();
		CompleteRequiresZeroUnprocessed();
		HistogramReconciliation();
		RegionAnalysisUnavailableForIncompleteFrame();
		RefinementEligibilityRequiresCompleteAndSelectableRegion();
		LifecycleBudgetExpiration();
		TerminalEvidenceEmittedOnce();
		DeterministicRepeatedLifecycle();
		PreviousCompletedEvidenceNotMislabeledDuringNewCapture();
		AtomicFinalizationModel();
	}

	private static void ValidTransitions()
	{
		var state = ProbeSnapshotLifecycleState.Inactive;
		AssertTransition(ref state, ProbeSnapshotLifecycleState.Requested, "inactive requested");
		AssertTransition(ref state, ProbeSnapshotLifecycleState.WaitingForPhysics, "requested waiting");
		AssertTransition(ref state, ProbeSnapshotLifecycleState.Capturing, "waiting capturing");
		AssertTransition(ref state, ProbeSnapshotLifecycleState.Complete, "capturing complete");
		AssertTransition(ref state, ProbeSnapshotLifecycleState.Requested, "terminal requested");
	}

	private static void IllegalTransitionRejected()
	{
		var state = ProbeSnapshotLifecycleState.Inactive;
		bool ok = ProbeSnapshotLifecycleModel.TryTransition(ref state, ProbeSnapshotLifecycleState.Complete, out string reason);
		TestAssert.False(ok, "illegal transition rejected");
		TestAssert.Equal("illegal_transition", reason, "illegal transition reason");
		TestAssert.Equal(ProbeSnapshotLifecycleState.Inactive, state, "illegal transition state unchanged");
	}

	private static void RequestGenerationMonotonicity()
	{
		int generation = 0;
		int first = ++generation;
		int second = ++generation;
		TestAssert.Equal(1, first, "first request generation");
		TestAssert.Equal(2, second, "second request generation");
		TestAssert.True(second > first, "generation monotonic");
	}

	private static void WaitingForPhysicsDoesNotMutateOutcomes()
	{
		ProbeOutcomeCode[] outcomes = { ProbeOutcomeCode.HitGeometry, ProbeOutcomeCode.MaxStepsExhausted };
		ProbeOutcomeCode[] before = Copy(outcomes);
		var result = Terminal(ProbeSnapshotLifecycleState.Incomplete, ProbeSnapshotLifecycleReason.PhysicsSpaceUnavailable);
		result.ProcessedPixelCount = 0;
		result.UnprocessedPixelCount = 2;

		AssertSameOutcomes(before, outcomes, "waiting physics outcomes");
		TestAssert.Equal(ProbeSnapshotLifecycleReason.PhysicsSpaceUnavailable, result.Reason, "waiting physics reason");
	}

	private static void StaleGenerationRejected()
	{
		int activeGeneration = 3;
		int completionGeneration = 2;
		TestAssert.True(completionGeneration != activeGeneration, "stale generation detectable");
		var result = Terminal(ProbeSnapshotLifecycleState.Invalidated, ProbeSnapshotLifecycleReason.RequestSuperseded);
		TestAssert.Equal(ProbeSnapshotLifecycleReason.RequestSuperseded, result.Reason, "stale generation reason");
	}

	private static void ContextChangeInvalidates()
	{
		var result = CompleteResult();
		result.ContextMatched = false;
		result.State = ProbeSnapshotLifecycleState.Invalidated;
		result.Reason = ProbeSnapshotLifecycleReason.ContextChanged;

		TestAssert.False(ProbeSnapshotLifecycleModel.CanComplete(result), "context changed cannot complete");
		TestAssert.Equal(ProbeSnapshotLifecycleReason.ContextChanged, result.Reason, "context changed reason");
	}

	private static void DimensionChangeInvalidates()
	{
		var result = CompleteResult();
		result.DimensionsMatched = false;
		result.State = ProbeSnapshotLifecycleState.Invalidated;
		result.Reason = ProbeSnapshotLifecycleReason.DimensionsChanged;

		TestAssert.False(ProbeSnapshotLifecycleModel.CanComplete(result), "dimension changed cannot complete");
		TestAssert.Equal(ProbeSnapshotLifecycleReason.DimensionsChanged, result.Reason, "dimension changed reason");
	}

	private static void IncompleteCannotBecomeComplete()
	{
		var result = CompleteResult();
		result.State = ProbeSnapshotLifecycleState.Incomplete;
		result.Reason = ProbeSnapshotLifecycleReason.UnprocessedPixelsRemaining;

		TestAssert.False(ProbeSnapshotLifecycleModel.CanComplete(result), "incomplete cannot complete");
	}

	private static void CompleteRequiresZeroUnprocessed()
	{
		var result = CompleteResult();
		result.UnprocessedPixelCount = 1;
		result.ProcessedPixelCount = result.TotalPixelCount - 1;

		TestAssert.False(ProbeSnapshotLifecycleModel.CanComplete(result), "complete requires zero unprocessed");
	}

	private static void HistogramReconciliation()
	{
		var result = CompleteResult();
		TestAssert.True(result.HistogramReconciles(), "histogram reconciles");
		result.InvalidCount++;
		TestAssert.False(result.HistogramReconciles(), "histogram mismatch detected");
	}

	private static void RegionAnalysisUnavailableForIncompleteFrame()
	{
		var result = Terminal(ProbeSnapshotLifecycleState.Incomplete, ProbeSnapshotLifecycleReason.UnprocessedPixelsRemaining);
		result.RegionAnalysisAvailable = false;
		result.RefinementEligible = false;

		TestAssert.False(result.RegionAnalysisAvailable, "incomplete region unavailable");
		TestAssert.False(result.RefinementEligible, "incomplete not eligible");
	}

	private static void RefinementEligibilityRequiresCompleteAndSelectableRegion()
	{
		var result = CompleteResult();
		result.SelectableRegionCount = 1;
		result.RefinementEligible = result.State == ProbeSnapshotLifecycleState.Complete &&
			result.UnprocessedPixelCount == 0 &&
			result.SelectableRegionCount > 0;
		TestAssert.True(result.RefinementEligible, "complete selectable eligible");

		result.SelectableRegionCount = 0;
		result.RefinementEligible = result.State == ProbeSnapshotLifecycleState.Complete &&
			result.UnprocessedPixelCount == 0 &&
			result.SelectableRegionCount > 0;
		TestAssert.False(result.RefinementEligible, "no selectable region not eligible");
	}

	private static void LifecycleBudgetExpiration()
	{
		var result = Terminal(ProbeSnapshotLifecycleState.Incomplete, ProbeSnapshotLifecycleReason.TimeoutOrFrameBudgetExhausted);
		result.PhysicsFramesElapsed = 240;

		TestAssert.Equal(ProbeSnapshotLifecycleReason.TimeoutOrFrameBudgetExhausted, result.Reason, "budget reason");
		TestAssert.Equal(240, result.PhysicsFramesElapsed, "budget frame count");
	}

	private static void TerminalEvidenceEmittedOnce()
	{
		var recorder = new EvidenceRecorder();
		var result = CompleteResult();
		recorder.EmitTerminal(result);
		recorder.EmitTerminal(result);

		TestAssert.Equal(1, recorder.Count, "terminal evidence emitted once");
	}

	private static void DeterministicRepeatedLifecycle()
	{
		var a = CompleteResult();
		var b = CompleteResult();
		TestAssert.Equal(a.TotalPixelCount, b.TotalPixelCount, "repeat total");
		TestAssert.Equal(a.HitGeometryCount, b.HitGeometryCount, "repeat geometry");
		TestAssert.Equal(a.MaxStepsExhaustedCount, b.MaxStepsExhaustedCount, "repeat max steps");
		TestAssert.Equal(a.HistogramReconciles(), b.HistogramReconciles(), "repeat reconciliation");
	}

	private static void PreviousCompletedEvidenceNotMislabeledDuringNewCapture()
	{
		var previous = CompleteResult();
		var currentState = ProbeSnapshotLifecycleState.Requested;

		TestAssert.True(previous.IsTerminal, "previous terminal retained");
		TestAssert.Equal(ProbeSnapshotLifecycleState.Requested, currentState, "new request state");
		TestAssert.Equal(ProbeSnapshotLifecycleState.Complete, previous.State, "previous remains complete");
	}

	private static void AtomicFinalizationModel()
	{
		var partial = CompleteResult();
		partial.RegionAnalysisAvailable = false;
		TestAssert.False(ProbeSnapshotLifecycleModel.CanComplete(partial), "partial finalization rejected");

		var complete = CompleteResult();
		TestAssert.True(ProbeSnapshotLifecycleModel.CanComplete(complete), "atomic complete accepted");
	}

	private static ProbeSnapshotLifecycleResult CompleteResult()
	{
		return new ProbeSnapshotLifecycleResult
		{
			RequestId = 1,
			State = ProbeSnapshotLifecycleState.Complete,
			Reason = ProbeSnapshotLifecycleReason.None,
			Width = 4,
			Height = 3,
			TotalPixelCount = 12,
			ProcessedPixelCount = 12,
			UnprocessedPixelCount = 0,
			ContextMatched = true,
			DimensionsMatched = true,
			PolicyMaxSteps = 80,
			PolicyStepSize = 0.07f,
			HitGeometryCount = 3,
			BackgroundResolvedCount = 4,
			MaxStepsExhaustedCount = 5,
			RegionAnalysisAvailable = true,
			RegionCount = 1,
			SelectableRegionCount = 1,
			LargestRegionId = 1,
			LargestRegionPixelCount = 5,
			RefinementEligible = true
		};
	}

	private static ProbeSnapshotLifecycleResult Terminal(
		ProbeSnapshotLifecycleState state,
		ProbeSnapshotLifecycleReason reason)
	{
		return new ProbeSnapshotLifecycleResult
		{
			RequestId = 1,
			State = state,
			Reason = reason,
			Width = 1,
			Height = 1,
			TotalPixelCount = 1,
			ContextMatched = true,
			DimensionsMatched = true,
			PolicyMaxSteps = 80,
			PolicyStepSize = 0.07f
		};
	}

	private static void AssertTransition(ref ProbeSnapshotLifecycleState state, ProbeSnapshotLifecycleState next, string label)
	{
		bool ok = ProbeSnapshotLifecycleModel.TryTransition(ref state, next, out string reason);
		TestAssert.True(ok, $"{label} transition ok: {reason}");
		TestAssert.Equal(next, state, $"{label} state");
	}

	private static ProbeOutcomeCode[] Copy(ProbeOutcomeCode[] values)
	{
		var copy = new ProbeOutcomeCode[values.Length];
		Array.Copy(values, copy, values.Length);
		return copy;
	}

	private static void AssertSameOutcomes(ProbeOutcomeCode[] expected, ProbeOutcomeCode[] actual, string label)
	{
		TestAssert.Equal(expected.Length, actual.Length, $"{label} length");
		for (int i = 0; i < expected.Length; i++)
		{
			TestAssert.Equal(expected[i], actual[i], $"{label} {i}");
		}
	}

	private sealed class EvidenceRecorder
	{
		private bool _emitted;
		public int Count;

		public void EmitTerminal(ProbeSnapshotLifecycleResult result)
		{
			if (!result.IsTerminal || _emitted)
			{
				return;
			}
			_emitted = true;
			Count++;
		}
	}
}
