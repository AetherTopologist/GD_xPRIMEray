/// <summary>
/// Pure C# region analyzer entry point; Stage 0 wiring supplies caller-owned buffers.
/// </summary>
public static class ProbeRegionAnalyzer
{
	[System.ThreadStatic]
	private static int[] _queueScratch = System.Array.Empty<int>();

	public static void Analyze(
		int filmW,
		int filmH,
		System.ReadOnlySpan<ProbeOutcomeCode> outcomes,
		System.Span<ushort> regionLabels,
		System.Collections.Generic.List<ProbeRegionRecord> results)
	{
		if (results == null)
		{
			throw new System.ArgumentNullException(nameof(results));
		}
		results.Clear();
		regionLabels.Clear();
		if (filmW <= 0 || filmH <= 0)
		{
			return;
		}

		int pixelCount = checked(filmW * filmH);
		if (outcomes.Length < pixelCount)
		{
			throw new System.ArgumentException("Outcome span is smaller than film dimensions.", nameof(outcomes));
		}
		if (regionLabels.Length < pixelCount)
		{
			throw new System.ArgumentException("Region label span is smaller than film dimensions.", nameof(regionLabels));
		}

		if (_queueScratch == null || _queueScratch.Length < pixelCount)
		{
			_queueScratch = new int[pixelCount];
		}

		ushort nextRegionId = 1;
		for (int seed = 0; seed < pixelCount; seed++)
		{
			if (regionLabels[seed] != 0 || outcomes[seed] != ProbeOutcomeCode.MaxStepsExhausted)
			{
				continue;
			}
			if (nextRegionId == 0)
			{
				throw new System.InvalidOperationException("Region id capacity exceeded.");
			}

			ProbeRegionRecord record = FloodFillRegion(
				filmW,
				filmH,
				outcomes,
				regionLabels,
				_queueScratch,
				seed,
				nextRegionId);
			results.Add(record);
			nextRegionId++;
		}

		results.Sort(CompareRegionRecords);
	}

	private static ProbeRegionRecord FloodFillRegion(
		int filmW,
		int filmH,
		System.ReadOnlySpan<ProbeOutcomeCode> outcomes,
		System.Span<ushort> regionLabels,
		int[] queue,
		int seed,
		ushort regionId)
	{
		int head = 0;
		int tail = 0;
		queue[tail++] = seed;
		regionLabels[seed] = regionId;

		var record = new ProbeRegionRecord
		{
			Id = regionId,
			MinX = (ushort)(seed % filmW),
			MinY = (ushort)(seed / filmW),
			MaxX = (ushort)(seed % filmW),
			MaxY = (ushort)(seed / filmW),
			MaxRefinementLevel = 0
		};

		while (head < tail)
		{
			int index = queue[head++];
			int x = index % filmW;
			int y = index / filmW;
			AddPixelToRecord(ref record, outcomes[index], x, y);

			TryEnqueue(index - filmW, y > 0, outcomes, regionLabels, queue, ref tail, regionId);
			TryEnqueue(index - 1, x > 0, outcomes, regionLabels, queue, ref tail, regionId);
			TryEnqueue(index + 1, x + 1 < filmW, outcomes, regionLabels, queue, ref tail, regionId);
			TryEnqueue(index + filmW, y + 1 < filmH, outcomes, regionLabels, queue, ref tail, regionId);
		}

		record.IsPrimarilyMaxStepsExhausted =
			record.CountMaxStepsExhausted > record.PixelCount / 2;
		return record;
	}

	private static void TryEnqueue(
		int neighborIndex,
		bool inBounds,
		System.ReadOnlySpan<ProbeOutcomeCode> outcomes,
		System.Span<ushort> regionLabels,
		int[] queue,
		ref int tail,
		ushort regionId)
	{
		if (!inBounds ||
			regionLabels[neighborIndex] != 0 ||
			outcomes[neighborIndex] != ProbeOutcomeCode.MaxStepsExhausted)
		{
			return;
		}

		regionLabels[neighborIndex] = regionId;
		queue[tail++] = neighborIndex;
	}

	private static void AddPixelToRecord(
		ref ProbeRegionRecord record,
		ProbeOutcomeCode outcome,
		int x,
		int y)
	{
		record.PixelCount++;
		if (x < record.MinX) record.MinX = (ushort)x;
		if (y < record.MinY) record.MinY = (ushort)y;
		if (x > record.MaxX) record.MaxX = (ushort)x;
		if (y > record.MaxY) record.MaxY = (ushort)y;

		switch (outcome)
		{
			case ProbeOutcomeCode.HitGeometry:
				record.CountHitGeometry++;
				break;
			case ProbeOutcomeCode.BackgroundResolved:
				record.CountBackgroundResolved++;
				break;
			case ProbeOutcomeCode.MaxStepsExhausted:
				record.CountMaxStepsExhausted++;
				break;
			case ProbeOutcomeCode.StoppedEarlyAbsorbed:
				record.CountStoppedEarlyAbsorbed++;
				break;
			case ProbeOutcomeCode.NumericalFailure:
				record.CountNumericalFailure++;
				break;
		}
	}

	private static int CompareRegionRecords(ProbeRegionRecord left, ProbeRegionRecord right)
	{
		int byPixelCount = right.PixelCount.CompareTo(left.PixelCount);
		return byPixelCount != 0 ? byPixelCount : left.Id.CompareTo(right.Id);
	}
}
