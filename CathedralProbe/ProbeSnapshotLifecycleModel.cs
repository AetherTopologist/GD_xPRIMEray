/// <summary>
/// Pure transition rules for Cathedral Probe snapshot lifecycle state.
/// </summary>
public static class ProbeSnapshotLifecycleModel
{
	public static bool CanTransition(
		ProbeSnapshotLifecycleState from,
		ProbeSnapshotLifecycleState to)
	{
		if (from == to)
		{
			return true;
		}

		switch (from)
		{
			case ProbeSnapshotLifecycleState.Inactive:
				return to == ProbeSnapshotLifecycleState.Requested;
			case ProbeSnapshotLifecycleState.Requested:
				return to == ProbeSnapshotLifecycleState.WaitingForPhysics ||
					to == ProbeSnapshotLifecycleState.Capturing ||
					to == ProbeSnapshotLifecycleState.Invalidated ||
					to == ProbeSnapshotLifecycleState.Failed;
			case ProbeSnapshotLifecycleState.WaitingForPhysics:
				return to == ProbeSnapshotLifecycleState.Capturing ||
					to == ProbeSnapshotLifecycleState.Incomplete ||
					to == ProbeSnapshotLifecycleState.Invalidated ||
					to == ProbeSnapshotLifecycleState.Failed;
			case ProbeSnapshotLifecycleState.Capturing:
				return to == ProbeSnapshotLifecycleState.Complete ||
					to == ProbeSnapshotLifecycleState.Incomplete ||
					to == ProbeSnapshotLifecycleState.Invalidated ||
					to == ProbeSnapshotLifecycleState.Failed;
			case ProbeSnapshotLifecycleState.Complete:
			case ProbeSnapshotLifecycleState.Incomplete:
			case ProbeSnapshotLifecycleState.Invalidated:
			case ProbeSnapshotLifecycleState.Failed:
				return to == ProbeSnapshotLifecycleState.Requested;
			default:
				return false;
		}
	}

	public static bool TryTransition(
		ref ProbeSnapshotLifecycleState state,
		ProbeSnapshotLifecycleState next,
		out string reason)
	{
		if (!CanTransition(state, next))
		{
			reason = "illegal_transition";
			return false;
		}

		state = next;
		reason = string.Empty;
		return true;
	}

	public static bool CanComplete(in ProbeSnapshotLifecycleResult result)
	{
		return result.State == ProbeSnapshotLifecycleState.Complete &&
			result.UnprocessedPixelCount == 0 &&
			result.ProcessedPixelCount == result.TotalPixelCount &&
			result.ContextMatched &&
			result.DimensionsMatched &&
			result.RegionAnalysisAvailable &&
			result.HistogramReconciles();
	}

	public static bool CanSealSnapshot(
		int totalPixels,
		int unprocessedPixels,
		bool pendingBandWork,
		int rowCursor,
		int filmHeight,
		bool contextMatched,
		ProbeSnapshotLifecycleState state,
		ProbeSnapshotLifecycleReason reason)
	{
		return state == ProbeSnapshotLifecycleState.Capturing &&
			reason == ProbeSnapshotLifecycleReason.None &&
			totalPixels > 0 &&
			unprocessedPixels == 0 &&
			!pendingBandWork &&
			rowCursor >= filmHeight &&
			contextMatched;
	}
}
