namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class ProbeRegionAnalyzerTests
{
	public static void Run()
	{
		EmptyPlane();
		OneIsolatedPixel();
		TwoDiagonalPixelsFormTwoRegions();
		HorizontalConnectedStrip();
		VerticalConnectedStrip();
		IrregularLShape();
		TwoSeparatedComponents();
		ResolvedPixelsSplitComponents();
		NumericalFailureExcluded();
		StoppedEarlyAbsorbedExcluded();
		InvalidExcluded();
		DeterministicIdsAndBoundingBoxes();
		InsufficientOutputCapacityIsDeterministic();
		RepeatAnalysisProducesIdenticalLabelsAndRecords();
		WarmAnalysisAllocatesZeroBytes();
	}

	private static void EmptyPlane()
	{
		ProbeOutcomeCode[] outcomes = Plane(4, 3);
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(4, 3, outcomes, labels, records);

		TestAssert.Equal(0, records.Count, "empty plane region count");
		AssertAllLabelsZero(labels, "empty plane labels");
	}

	private static void OneIsolatedPixel()
	{
		ProbeOutcomeCode[] outcomes = Plane(3, 3, 4);
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(3, 3, outcomes, labels, records);

		TestAssert.Equal(1, records.Count, "single pixel region count");
		AssertRecord(records[0], 1, 1, 1, 1, 1, 1);
		TestAssert.Equal((ushort)1, labels[4], "single pixel label");
	}

	private static void TwoDiagonalPixelsFormTwoRegions()
	{
		ProbeOutcomeCode[] outcomes = Plane(3, 3, 0, 4);
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(3, 3, outcomes, labels, records);

		TestAssert.Equal(2, records.Count, "diagonal region count");
		AssertRecord(records[0], 1, 1, 0, 0, 0, 0);
		AssertRecord(records[1], 2, 1, 1, 1, 1, 1);
	}

	private static void HorizontalConnectedStrip()
	{
		ProbeOutcomeCode[] outcomes = Plane(5, 3, 6, 7, 8);
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(5, 3, outcomes, labels, records);

		TestAssert.Equal(1, records.Count, "horizontal strip count");
		AssertRecord(records[0], 1, 3, 1, 1, 3, 1);
	}

	private static void VerticalConnectedStrip()
	{
		ProbeOutcomeCode[] outcomes = Plane(4, 4, 1, 5, 9);
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(4, 4, outcomes, labels, records);

		TestAssert.Equal(1, records.Count, "vertical strip count");
		AssertRecord(records[0], 1, 3, 1, 0, 1, 2);
	}

	private static void IrregularLShape()
	{
		ProbeOutcomeCode[] outcomes = Plane(4, 4, 0, 4, 8, 9, 10);
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(4, 4, outcomes, labels, records);

		TestAssert.Equal(1, records.Count, "L-shape count");
		AssertRecord(records[0], 1, 5, 0, 0, 2, 2);
	}

	private static void TwoSeparatedComponents()
	{
		ProbeOutcomeCode[] outcomes = Plane(6, 2, 0, 1, 4, 5, 11);
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(6, 2, outcomes, labels, records);

		TestAssert.Equal(2, records.Count, "separated component count");
		AssertRecord(records[0], 2, 3, 4, 0, 5, 1);
		AssertRecord(records[1], 1, 2, 0, 0, 1, 0);
		TestAssert.Equal((ushort)1, labels[0], "separated first seed label");
		TestAssert.Equal((ushort)2, labels[4], "separated second seed label");
	}

	private static void ResolvedPixelsSplitComponents()
	{
		ProbeOutcomeCode[] outcomes = Plane(5, 1, 0, 1, 3, 4);
		outcomes[2] = ProbeOutcomeCode.BackgroundResolved;
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(5, 1, outcomes, labels, records);

		TestAssert.Equal(2, records.Count, "resolved pixel split count");
		AssertRecord(records[0], 1, 2, 0, 0, 1, 0);
		AssertRecord(records[1], 2, 2, 3, 0, 4, 0);
		TestAssert.Equal((ushort)0, labels[2], "resolved splitter label");
	}

	private static void NumericalFailureExcluded()
	{
		ProbeOutcomeCode[] outcomes = Plane(3, 1, 0, 2);
		outcomes[1] = ProbeOutcomeCode.NumericalFailure;
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(3, 1, outcomes, labels, records);

		TestAssert.Equal(2, records.Count, "numerical failure exclusion count");
		TestAssert.Equal((ushort)0, labels[1], "numerical failure label");
		TestAssert.Equal(0, records[0].CountNumericalFailure, "record 0 numerical count");
		TestAssert.Equal(0, records[1].CountNumericalFailure, "record 1 numerical count");
	}

	private static void StoppedEarlyAbsorbedExcluded()
	{
		ExcludedMiddleValue(ProbeOutcomeCode.StoppedEarlyAbsorbed, "stopped early");
	}

	private static void InvalidExcluded()
	{
		ExcludedMiddleValue(ProbeOutcomeCode.Invalid, "invalid");
	}

	private static void ExcludedMiddleValue(ProbeOutcomeCode excludedValue, string label)
	{
		ProbeOutcomeCode[] outcomes =
		{
			ProbeOutcomeCode.MaxStepsExhausted,
			excludedValue,
			ProbeOutcomeCode.MaxStepsExhausted
		};
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(3, 1, outcomes, labels, records);

		TestAssert.Equal(2, records.Count, $"{label} exclusion region count");
		TestAssert.Equal((ushort)0, labels[1], $"{label} middle label");
		TestAssert.True(labels[0] != 0, $"{label} left label nonzero");
		TestAssert.True(labels[2] != 0, $"{label} right label nonzero");
		TestAssert.True(labels[0] != labels[2], $"{label} outer labels distinct");
		for (int i = 0; i < records.Count; i++)
		{
			TestAssert.Equal(1, records[i].PixelCount, $"{label} record {i} pixel count");
			TestAssert.Equal(1, records[i].CountMaxStepsExhausted, $"{label} record {i} max-step count");
			TestAssert.Equal(0, records[i].CountHitGeometry, $"{label} record {i} hit count");
			TestAssert.Equal(0, records[i].CountBackgroundResolved, $"{label} record {i} background count");
			TestAssert.Equal(0, records[i].CountStoppedEarlyAbsorbed, $"{label} record {i} stopped count");
			TestAssert.Equal(0, records[i].CountNumericalFailure, $"{label} record {i} numerical count");
		}
	}

	private static void DeterministicIdsAndBoundingBoxes()
	{
		ProbeOutcomeCode[] outcomes = Plane(5, 3, 0, 6, 7, 8, 14);
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(5, 3, outcomes, labels, records);

		TestAssert.Equal(3, records.Count, "deterministic region count");
		AssertRecord(records[0], 2, 3, 1, 1, 3, 1);
		AssertRecord(records[1], 1, 1, 0, 0, 0, 0);
		AssertRecord(records[2], 3, 1, 4, 2, 4, 2);
		TestAssert.Equal((ushort)1, labels[0], "first seed id");
		TestAssert.Equal((ushort)2, labels[6], "second seed id");
		TestAssert.Equal((ushort)3, labels[14], "third seed id");
	}

	private static void InsufficientOutputCapacityIsDeterministic()
	{
		ProbeOutcomeCode[] outcomes = Plane(3, 3, 0);
		var records = new List<ProbeRegionRecord> { new ProbeRegionRecord { Id = 99 } };
		ushort[] labels = { 9, 9, 9 };

		try
		{
			ProbeRegionAnalyzer.Analyze(3, 3, outcomes, labels, records);
			throw new InvalidOperationException("expected insufficient label capacity exception");
		}
		catch (ArgumentException)
		{
			TestAssert.Equal(0, records.Count, "records cleared before capacity failure");
			AssertAllLabelsZero(labels, "labels cleared before capacity failure");
		}
	}

	private static void RepeatAnalysisProducesIdenticalLabelsAndRecords()
	{
		ProbeOutcomeCode[] outcomes = Plane(6, 4, 0, 1, 2, 8, 14, 15, 16, 23);
		ushort[] labelsA = new ushort[outcomes.Length];
		ushort[] labelsB = new ushort[outcomes.Length];
		var recordsA = new List<ProbeRegionRecord>();
		var recordsB = new List<ProbeRegionRecord>();

		ProbeRegionAnalyzer.Analyze(6, 4, outcomes, labelsA, recordsA);
		ProbeRegionAnalyzer.Analyze(6, 4, outcomes, labelsB, recordsB);

		TestAssert.Equal(labelsA.Length, labelsB.Length, "repeat label length");
		for (int i = 0; i < labelsA.Length; i++)
		{
			TestAssert.Equal(labelsA[i], labelsB[i], $"repeat label {i}");
		}
		TestAssert.Equal(recordsA.Count, recordsB.Count, "repeat record count");
		for (int i = 0; i < recordsA.Count; i++)
		{
			AssertSameRecord(recordsA[i], recordsB[i], $"repeat record {i}");
		}
	}

	private static void WarmAnalysisAllocatesZeroBytes()
	{
		ProbeOutcomeCode[] outcomes = Plane(16, 12, 0, 1, 2, 16, 32, 80, 81, 82, 83, 191);
		ushort[] labels = new ushort[outcomes.Length];
		var records = new List<ProbeRegionRecord>(16);
		ProbeRegionAnalyzer.Analyze(16, 12, outcomes, labels, records);

		long before = GC.GetAllocatedBytesForCurrentThread();
		ProbeRegionAnalyzer.Analyze(16, 12, outcomes, labels, records);
		long delta = GC.GetAllocatedBytesForCurrentThread() - before;

		Console.WriteLine($"  region allocation delta: {delta} bytes / warm analysis");
		TestAssert.Equal(0L, delta, "warm region analysis allocation");
	}

	private static ProbeOutcomeCode[] Plane(int filmW, int filmH, params int[] maxStepIndices)
	{
		var outcomes = new ProbeOutcomeCode[filmW * filmH];
		Array.Fill(outcomes, ProbeOutcomeCode.BackgroundResolved);
		foreach (int index in maxStepIndices)
		{
			outcomes[index] = ProbeOutcomeCode.MaxStepsExhausted;
		}
		return outcomes;
	}

	private static void AssertRecord(
		ProbeRegionRecord record,
		ushort id,
		int pixelCount,
		ushort minX,
		ushort minY,
		ushort maxX,
		ushort maxY)
	{
		TestAssert.Equal(id, record.Id, "region id");
		TestAssert.Equal(pixelCount, record.PixelCount, $"region {id} pixel count");
		TestAssert.Equal(minX, record.MinX, $"region {id} min x");
		TestAssert.Equal(minY, record.MinY, $"region {id} min y");
		TestAssert.Equal(maxX, record.MaxX, $"region {id} max x");
		TestAssert.Equal(maxY, record.MaxY, $"region {id} max y");
		TestAssert.Equal((byte)0, record.MaxRefinementLevel, $"region {id} refinement level");
		TestAssert.True(record.IsPrimarilyMaxStepsExhausted, $"region {id} selectable");
		TestAssert.Equal(pixelCount, record.CountMaxStepsExhausted, $"region {id} max-step count");
		TestAssert.Equal(0, record.CountHitGeometry, $"region {id} hit count");
		TestAssert.Equal(0, record.CountBackgroundResolved, $"region {id} background count");
		TestAssert.Equal(0, record.CountStoppedEarlyAbsorbed, $"region {id} stopped count");
		TestAssert.Equal(0, record.CountNumericalFailure, $"region {id} numerical count");
	}

	private static void AssertSameRecord(ProbeRegionRecord expected, ProbeRegionRecord actual, string label)
	{
		TestAssert.Equal(expected.Id, actual.Id, $"{label} id");
		TestAssert.Equal(expected.PixelCount, actual.PixelCount, $"{label} pixel count");
		TestAssert.Equal(expected.MinX, actual.MinX, $"{label} min x");
		TestAssert.Equal(expected.MinY, actual.MinY, $"{label} min y");
		TestAssert.Equal(expected.MaxX, actual.MaxX, $"{label} max x");
		TestAssert.Equal(expected.MaxY, actual.MaxY, $"{label} max y");
		TestAssert.Equal(expected.IsPrimarilyMaxStepsExhausted, actual.IsPrimarilyMaxStepsExhausted, $"{label} selectable");
	}

	private static void AssertAllLabelsZero(ushort[] labels, string message)
	{
		for (int i = 0; i < labels.Length; i++)
		{
			TestAssert.Equal((ushort)0, labels[i], $"{message} {i}");
		}
	}
}
