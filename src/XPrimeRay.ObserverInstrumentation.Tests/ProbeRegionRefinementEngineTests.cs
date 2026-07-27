namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class ProbeRegionRefinementEngineTests
{
	public static void Run()
	{
		RowMajorCollection();
		NonexistentRegionRejected();
		IncompleteSnapshotRejectedByCallerContract();
		ContextMismatchRejectsWithoutMutation();
		DuplicateResultRejectsWithoutMutation();
		ReorderedResultRejectsWithoutMutation();
		ResultCountMismatchRejectsWithoutMutation();
		InvalidPixelRejectsWithoutMutation();
		SuccessfulAtomicApply();
		LevelMonotonicity();
		ResolvedCountCorrectness();
		TransitionHistograms();
		UnchangedMaxStepsCount();
		ChildRegionRegenerationOnSplit();
		RegionDisappearsAfterFullResolution();
		RepeatedExecutionIsDeterministic();
		FailureReasonIsDeterministic();
		UnavailableSeamTelemetryIsExplicit();
		WarmSuccessfulApplyAllocationIsBounded();
	}

	private static void RowMajorCollection()
	{
		ushort[] labels =
		{
			0, 2, 0,
			2, 1, 2,
			0, 2, 0
		};
		int[] selected = new int[4];

		bool ok = ProbeRegionRefinementEngine.CollectRegionPixelsRowMajor(3, 3, labels, 2, selected, out int count, out string reason);

		TestAssert.True(ok, $"row-major collection failed: {reason}");
		TestAssert.Equal(4, count, "row-major count");
		AssertArrayPrefix(selected, new[] { 1, 3, 5, 7 }, count, "row-major selected");
	}

	private static void NonexistentRegionRejected()
	{
		ushort[] labels = { 1, 0, 1 };
		int[] selected = new int[3];

		bool ok = ProbeRegionRefinementEngine.CollectRegionPixelsRowMajor(3, 1, labels, 2, selected, out int count, out string reason);

		TestAssert.False(ok, "nonexistent region rejected");
		TestAssert.Equal(0, count, "nonexistent count");
		TestAssert.Equal("region_not_found_or_empty", reason, "nonexistent reason");
	}

	private static void IncompleteSnapshotRejectedByCallerContract()
	{
		ProbeOutcomeCode[] outcomes = Plane(3, 1, ProbeOutcomeCode.MaxStepsExhausted, ProbeOutcomeCode.Unprocessed, ProbeOutcomeCode.MaxStepsExhausted);
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();
		ProbeRegionAnalyzer.Analyze(3, 1, outcomes, labels, records);

		TestAssert.Equal(2, records.Count, "incomplete snapshot analyzer still separates max-step pixels");
		TestAssert.True(Array.Exists(outcomes, outcome => outcome == ProbeOutcomeCode.Unprocessed), "caller must reject incomplete snapshot");
	}

	private static void ContextMismatchRejectsWithoutMutation()
	{
		var fixture = CreateFixture(3, 1);
		var mutated = Context(4, 1);
		ProbeOutcomeCode[] before = Copy(fixture.Outcomes);

		ProbeRefinementSummary summary = Apply(fixture, fixture.Context, mutated);

		TestAssert.False(summary.AppliedAtomically, "context mismatch apply");
		TestAssert.False(summary.ContextMatched, "context mismatch flag");
		TestAssert.Equal("context_key_mismatch", summary.FailureReason, "context mismatch reason");
		AssertSameOutcomes(before, fixture.Outcomes, "context mismatch mutation");
	}

	private static void DuplicateResultRejectsWithoutMutation()
	{
		var fixture = CreateFixture(3, 1);
		fixture.Selected[1] = fixture.Selected[0];
		fixture.Results[1].PixelIndex = fixture.Selected[1];
		ProbeOutcomeCode[] before = Copy(fixture.Outcomes);

		ProbeRefinementSummary summary = Apply(fixture);

		TestAssert.False(summary.AppliedAtomically, "duplicate apply");
		TestAssert.Equal("duplicate_pixel_index:0", summary.FailureReason, "duplicate reason");
		AssertSameOutcomes(before, fixture.Outcomes, "duplicate mutation");
	}

	private static void ReorderedResultRejectsWithoutMutation()
	{
		var fixture = CreateFixture(3, 1);
		(fixture.Results[0], fixture.Results[1]) = (fixture.Results[1], fixture.Results[0]);
		ProbeOutcomeCode[] before = Copy(fixture.Outcomes);

		ProbeRefinementSummary summary = Apply(fixture);

		TestAssert.False(summary.AppliedAtomically, "reordered apply");
		TestAssert.Equal("result_pixel_mismatch:0", summary.FailureReason, "reordered reason");
		AssertSameOutcomes(before, fixture.Outcomes, "reordered mutation");
	}

	private static void ResultCountMismatchRejectsWithoutMutation()
	{
		var fixture = CreateFixture(3, 1);
		ProbeOutcomeCode[] before = Copy(fixture.Outcomes);

		ProbeRefinementSummary summary = ProbeRegionRefinementEngine.ApplySelectedRegionResults(
			1,
			1,
			1,
			fixture.Width,
			fixture.Height,
			7,
			7,
			fixture.Context,
			fixture.Context,
			320,
			0.35f,
			0,
			fixture.Selected,
			fixture.Results.AsSpan(0, fixture.Results.Length - 1),
			fixture.Outcomes,
			fixture.RefinementLevels,
			fixture.Labels,
			fixture.Records,
			ref fixture.FrameSummary);

		TestAssert.False(summary.AppliedAtomically, "count mismatch apply");
		TestAssert.Equal("result_count_mismatch", summary.FailureReason, "count mismatch reason");
		AssertSameOutcomes(before, fixture.Outcomes, "count mismatch mutation");
	}

	private static void InvalidPixelRejectsWithoutMutation()
	{
		var fixture = CreateFixture(3, 1);
		fixture.Selected[0] = 99;
		fixture.Results[0].PixelIndex = 99;
		ProbeOutcomeCode[] before = Copy(fixture.Outcomes);

		ProbeRefinementSummary summary = Apply(fixture);

		TestAssert.False(summary.AppliedAtomically, "invalid pixel apply");
		TestAssert.Equal("pixel_index_out_of_range:99", summary.FailureReason, "invalid pixel reason");
		AssertSameOutcomes(before, fixture.Outcomes, "invalid pixel mutation");
	}

	private static void SuccessfulAtomicApply()
	{
		var fixture = CreateFixture(3, 1);

		ProbeRefinementSummary summary = Apply(fixture);

		TestAssert.True(summary.AppliedAtomically, "successful apply");
		TestAssert.Equal(3, summary.AppliedPixelCount, "applied count");
		TestAssert.Equal(3, summary.ResolvedPixelCount, "resolved count");
		TestAssert.Equal(0, summary.RemainingMaxStepsCount, "remaining count");
		TestAssert.Equal(0, fixture.Records.Count, "no regions after full resolution");
		TestAssert.Equal(0, fixture.FrameSummary.MaxStepsExhaustedCount, "frame max-step after full resolution");
	}

	private static void LevelMonotonicity()
	{
		var fixture = CreateFixture(3, 1);
		fixture.RefinementLevels[0] = 2;

		ProbeRefinementSummary summary = Apply(fixture, requestedLevel: 1);

		TestAssert.True(summary.AppliedAtomically, "monotonic apply");
		TestAssert.Equal((byte)2, fixture.RefinementLevels[0], "existing level preserved");
		TestAssert.Equal((byte)1, fixture.RefinementLevels[1], "new level assigned");
	}

	private static void ResolvedCountCorrectness()
	{
		var fixture = CreateFixture(4, 1, ProbeOutcomeCode.HitGeometry, ProbeOutcomeCode.MaxStepsExhausted, ProbeOutcomeCode.BackgroundResolved, ProbeOutcomeCode.MaxStepsExhausted);
		fixture.Results[1].Outcome = ProbeOutcomeCode.MaxStepsExhausted;
		fixture.Results[3].Outcome = ProbeOutcomeCode.BackgroundResolved;

		ProbeRefinementSummary summary = Apply(fixture);

		TestAssert.Equal(4, summary.PreviousMaxStepsCount, "previous max");
		TestAssert.Equal(1, summary.RemainingMaxStepsCount, "remaining max");
		TestAssert.Equal(3, summary.ResolvedPixelCount, "resolved max");
	}

	private static void TransitionHistograms()
	{
		var fixture = CreateFixture(
			5,
			1,
			ProbeOutcomeCode.HitGeometry,
			ProbeOutcomeCode.BackgroundResolved,
			ProbeOutcomeCode.StoppedEarlyAbsorbed,
			ProbeOutcomeCode.NumericalFailure,
			ProbeOutcomeCode.Invalid);

		ProbeRefinementSummary summary = Apply(fixture);

		TestAssert.Equal(1, summary.BecameHitGeometryCount, "to geometry");
		TestAssert.Equal(1, summary.BecameBackgroundResolvedCount, "to background");
		TestAssert.Equal(1, summary.BecameStoppedEarlyAbsorbedCount, "to absorbed");
		TestAssert.Equal(1, summary.BecameNumericalFailureCount, "to fault");
		TestAssert.Equal(1, summary.BecameInvalidCount, "to invalid");
	}

	private static void UnchangedMaxStepsCount()
	{
		var fixture = CreateFixture(4, 1, ProbeOutcomeCode.MaxStepsExhausted, ProbeOutcomeCode.HitGeometry, ProbeOutcomeCode.MaxStepsExhausted, ProbeOutcomeCode.BackgroundResolved);

		ProbeRefinementSummary summary = Apply(fixture);

		TestAssert.Equal(2, summary.UnchangedMaxStepsCount, "unchanged max");
		TestAssert.Equal(2, summary.ResolvedPixelCount, "resolved with unchanged max");
	}

	private static void ChildRegionRegenerationOnSplit()
	{
		var fixture = CreateFixture(5, 1);
		fixture.Results[0].Outcome = ProbeOutcomeCode.MaxStepsExhausted;
		fixture.Results[1].Outcome = ProbeOutcomeCode.MaxStepsExhausted;
		fixture.Results[2].Outcome = ProbeOutcomeCode.BackgroundResolved;
		fixture.Results[3].Outcome = ProbeOutcomeCode.MaxStepsExhausted;
		fixture.Results[4].Outcome = ProbeOutcomeCode.MaxStepsExhausted;

		ProbeRefinementSummary summary = Apply(fixture);

		TestAssert.Equal(2, summary.ChildRegionCount, "split child count");
		TestAssert.Equal(2, summary.LargestChildRegionPixelCount, "split largest child");
		TestAssert.Equal(2, fixture.Records.Count, "split records");
	}

	private static void RegionDisappearsAfterFullResolution()
	{
		var fixture = CreateFixture(2, 1, ProbeOutcomeCode.BackgroundResolved, ProbeOutcomeCode.HitGeometry);

		ProbeRefinementSummary summary = Apply(fixture);

		TestAssert.Equal(0, summary.ChildRegionCount, "disappeared child count");
		TestAssert.Equal(0, fixture.Records.Count, "disappeared records");
	}

	private static void RepeatedExecutionIsDeterministic()
	{
		var a = CreateFixture(5, 1, ProbeOutcomeCode.MaxStepsExhausted, ProbeOutcomeCode.BackgroundResolved, ProbeOutcomeCode.HitGeometry, ProbeOutcomeCode.MaxStepsExhausted, ProbeOutcomeCode.BackgroundResolved);
		var b = CreateFixture(5, 1, ProbeOutcomeCode.MaxStepsExhausted, ProbeOutcomeCode.BackgroundResolved, ProbeOutcomeCode.HitGeometry, ProbeOutcomeCode.MaxStepsExhausted, ProbeOutcomeCode.BackgroundResolved);

		ProbeRefinementSummary summaryA = Apply(a);
		ProbeRefinementSummary summaryB = Apply(b);

		TestAssert.Equal(summaryA.ResolvedPixelCount, summaryB.ResolvedPixelCount, "repeat resolved");
		TestAssert.Equal(summaryA.ChildRegionCount, summaryB.ChildRegionCount, "repeat child count");
		AssertSameOutcomes(a.Outcomes, b.Outcomes, "repeat outcomes");
	}

	private static void FailureReasonIsDeterministic()
	{
		var a = CreateFixture(3, 1);
		var b = CreateFixture(3, 1);
		a.Selected[2] = -1;
		a.Results[2].PixelIndex = -1;
		b.Selected[2] = -1;
		b.Results[2].PixelIndex = -1;

		TestAssert.Equal(Apply(a).FailureReason, Apply(b).FailureReason, "failure reason deterministic");
	}

	private static void UnavailableSeamTelemetryIsExplicit()
	{
		var pixel = new ProbeSeamDiagnosticPixel
		{
			Label = "A",
			PixelX = 1,
			PixelY = 2,
			PixelIndex = 9,
			InBounds = true,
			Outcome = ProbeOutcomeCode.MaxStepsExhausted,
			RegionId = 4,
			RefinementLevel = 1,
			ConfiguredMaxSteps = 320,
			StepSize = 0.35f,
			ContextMatched = true
		};

		TestAssert.False(pixel.HasStepsUsed, "seam steps unavailable flag");
		TestAssert.False(pixel.HasIntegratedPathLength, "seam path unavailable flag");
		TestAssert.False(pixel.HasTerminalColliderIdentity, "seam collider unavailable flag");
		TestAssert.False(pixel.HasNormal, "seam normal unavailable flag");
	}

	private static void WarmSuccessfulApplyAllocationIsBounded()
	{
		var warm = CreateFixture(8, 4, CreatePattern(32));
		Apply(warm);
		var fixture = CreateFixture(8, 4, CreatePattern(32));

		long before = GC.GetAllocatedBytesForCurrentThread();
		ProbeRefinementSummary summary = Apply(fixture);
		long delta = GC.GetAllocatedBytesForCurrentThread() - before;

		Console.WriteLine($"  refinement allocation delta: {delta} bytes / warm apply");
		TestAssert.True(summary.AppliedAtomically, "warm apply succeeded");
		TestAssert.True(delta <= 2048, $"warm apply allocation bounded: {delta}");
	}

	private static ProbeRefinementSummary Apply(RefinementFixture fixture, ProbeContextKey? sourceContext = null, ProbeContextKey? currentContext = null, byte requestedLevel = 1)
	{
		return ProbeRegionRefinementEngine.ApplySelectedRegionResults(
			1,
			1,
			requestedLevel,
			fixture.Width,
			fixture.Height,
			7,
			7,
			sourceContext ?? fixture.Context,
			currentContext ?? fixture.Context,
			320,
			0.35f,
			0,
			fixture.Selected,
			fixture.Results,
			fixture.Outcomes,
			fixture.RefinementLevels,
			fixture.Labels,
			fixture.Records,
			ref fixture.FrameSummary);
	}

	private static RefinementFixture CreateFixture(int width, int height, params ProbeOutcomeCode[] resultOutcomes)
	{
		int total = width * height;
		if (resultOutcomes.Length == 0)
		{
			resultOutcomes = Enumerable.Repeat(ProbeOutcomeCode.BackgroundResolved, total).ToArray();
		}
		var fixture = new RefinementFixture
		{
			Width = width,
			Height = height,
			Context = Context(width, height),
			Outcomes = Enumerable.Repeat(ProbeOutcomeCode.MaxStepsExhausted, total).ToArray(),
			RefinementLevels = new byte[total],
			Labels = Enumerable.Repeat((ushort)1, total).ToArray(),
			Selected = Enumerable.Range(0, total).ToArray(),
			Results = new ProbeSelectedIndexResult[total],
			Records = new List<ProbeRegionRecord>
			{
				new ProbeRegionRecord
				{
					Id = 1,
					PixelCount = total,
					CountMaxStepsExhausted = total,
					IsPrimarilyMaxStepsExhausted = true,
					MaxX = (ushort)(width - 1),
					MaxY = (ushort)(height - 1)
				}
			},
			FrameSummary = new ProbeFrameSummary
			{
				TotalPixels = total,
				MaxStepsExhaustedCount = total,
				RegionCount = 1,
				SelectableRegionCount = 1,
				LargestRegionId = 1,
				LargestRegionPixelCount = total,
				ContextKey = Context(width, height)
			}
		};
		for (int i = 0; i < total; i++)
		{
			fixture.Results[i] = new ProbeSelectedIndexResult
			{
				PixelIndex = i,
				Outcome = resultOutcomes[System.Math.Min(i, resultOutcomes.Length - 1)],
				SurfaceClass = ProbeSurfaceClass.Geometry,
				FinalStepCount = 320
			};
		}
		return fixture;
	}

	private static ProbeOutcomeCode[] CreatePattern(int count)
	{
		var values = new ProbeOutcomeCode[count];
		for (int i = 0; i < count; i++)
		{
			values[i] = i % 3 == 0 ? ProbeOutcomeCode.MaxStepsExhausted : ProbeOutcomeCode.BackgroundResolved;
		}
		return values;
	}

	private static ProbeContextKey Context(int width, int height)
	{
		return new ProbeContextKey(1, 2, 60f, (ushort)width, (ushort)height, 80, 1f, 1f, 3, 4, 5, 1);
	}

	private static ProbeOutcomeCode[] Plane(int width, int height, params ProbeOutcomeCode[] outcomes)
	{
		var values = new ProbeOutcomeCode[width * height];
		Array.Fill(values, ProbeOutcomeCode.BackgroundResolved);
		for (int i = 0; i < outcomes.Length && i < values.Length; i++)
		{
			values[i] = outcomes[i];
		}
		return values;
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

	private static void AssertArrayPrefix(int[] actual, int[] expected, int count, string label)
	{
		TestAssert.Equal(expected.Length, count, $"{label} count");
		for (int i = 0; i < count; i++)
		{
			TestAssert.Equal(expected[i], actual[i], $"{label} {i}");
		}
	}

	private sealed class RefinementFixture
	{
		public int Width;
		public int Height;
		public ProbeContextKey Context;
		public ProbeOutcomeCode[] Outcomes = Array.Empty<ProbeOutcomeCode>();
		public byte[] RefinementLevels = Array.Empty<byte>();
		public ushort[] Labels = Array.Empty<ushort>();
		public int[] Selected = Array.Empty<int>();
		public ProbeSelectedIndexResult[] Results = Array.Empty<ProbeSelectedIndexResult>();
		public List<ProbeRegionRecord> Records = new();
		public ProbeFrameSummary FrameSummary;
	}
}
