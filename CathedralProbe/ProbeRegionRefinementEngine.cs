#nullable enable

using System;
using System.Collections.Generic;

/// <summary>
/// Pure selected-region refinement bookkeeping. Transport execution stays owned by the caller.
/// </summary>
public static class ProbeRegionRefinementEngine
{
	public static bool CollectRegionPixelsRowMajor(
		int filmWidth,
		int filmHeight,
		ReadOnlySpan<ushort> regionLabels,
		ushort regionId,
		Span<int> destination,
		out int selectedPixelCount,
		out string failureReason)
	{
		selectedPixelCount = 0;
		failureReason = string.Empty;
		int totalPixels = SafeTotalPixels(filmWidth, filmHeight);
		if (totalPixels <= 0 || regionLabels.Length < totalPixels)
		{
			failureReason = "film_dimensions_invalid";
			return false;
		}
		if (regionId == 0)
		{
			failureReason = "region_id_zero";
			return false;
		}

		for (int i = 0; i < totalPixels; i++)
		{
			if (regionLabels[i] != regionId)
			{
				continue;
			}
			if (selectedPixelCount >= destination.Length)
			{
				failureReason = "destination_capacity_exceeded";
				selectedPixelCount = 0;
				return false;
			}
			destination[selectedPixelCount++] = i;
		}

		if (selectedPixelCount == 0)
		{
			failureReason = "region_not_found_or_empty";
			return false;
		}
		return true;
	}

	public static ProbeRefinementSummary ApplySelectedRegionResults(
		int requestId,
		ushort sourceRegionId,
		byte requestedRefinementLevel,
		int filmWidth,
		int filmHeight,
		int sourceFrameGeneration,
		int currentFrameGeneration,
		in ProbeContextKey sourceContext,
		in ProbeContextKey currentContext,
		int policyMaxSteps,
		float policyStepSize,
		long elapsedTicks,
		ReadOnlySpan<int> selectedPixelIndices,
		ReadOnlySpan<ProbeSelectedIndexResult> results,
		ProbeOutcomeCode[] outcomes,
		byte[] refinementLevels,
		ushort[] regionLabels,
		List<ProbeRegionRecord> regions,
		ref ProbeFrameSummary frameSummary)
	{
		if (!ValidateForAtomicApply(
			filmWidth,
			filmHeight,
			sourceFrameGeneration,
			currentFrameGeneration,
			in sourceContext,
			in currentContext,
			selectedPixelIndices,
			results,
			out int totalPixels,
			out string failureReason,
			out bool contextMatched))
		{
			return ProbeRefinementSummary.Failure(
				requestId,
				sourceRegionId,
				requestedRefinementLevel,
				selectedPixelIndices.Length,
				contextMatched,
				failureReason);
		}

		if (outcomes == null || outcomes.Length < totalPixels ||
			refinementLevels == null || refinementLevels.Length < totalPixels ||
			regionLabels == null || regionLabels.Length < totalPixels)
		{
			return ProbeRefinementSummary.Failure(
				requestId,
				sourceRegionId,
				requestedRefinementLevel,
				selectedPixelIndices.Length,
				true,
				"persistent_buffer_capacity_invalid");
		}
		if (regions == null)
		{
			return ProbeRefinementSummary.Failure(
				requestId,
				sourceRegionId,
				requestedRefinementLevel,
				selectedPixelIndices.Length,
				true,
				"region_record_storage_invalid");
		}

		var summary = new ProbeRefinementSummary
		{
			RequestId = requestId,
			SourceRegionId = sourceRegionId,
			RequestedRefinementLevel = requestedRefinementLevel,
			SelectedPixelCount = selectedPixelIndices.Length,
			PolicyMaxSteps = policyMaxSteps,
			PolicyStepSize = policyStepSize,
			ElapsedTicks = elapsedTicks,
			ContextMatched = true,
			AppliedAtomically = true,
			FailureReason = string.Empty
		};

		for (int i = 0; i < selectedPixelIndices.Length; i++)
		{
			int pixelIndex = selectedPixelIndices[i];
			if (outcomes[pixelIndex] == ProbeOutcomeCode.MaxStepsExhausted)
			{
				summary.PreviousMaxStepsCount++;
			}
		}

		for (int i = 0; i < selectedPixelIndices.Length; i++)
		{
			int pixelIndex = selectedPixelIndices[i];
			ProbeOutcomeCode previous = outcomes[pixelIndex];
			ProbeOutcomeCode refined = results[i].Outcome;
			outcomes[pixelIndex] = refined;
			if (refinementLevels[pixelIndex] < requestedRefinementLevel)
			{
				refinementLevels[pixelIndex] = requestedRefinementLevel;
			}
			summary.AppliedPixelCount++;

			if (previous != ProbeOutcomeCode.MaxStepsExhausted)
			{
				continue;
			}

			switch (refined)
			{
				case ProbeOutcomeCode.HitGeometry:
					summary.BecameHitGeometryCount++;
					break;
				case ProbeOutcomeCode.BackgroundResolved:
					summary.BecameBackgroundResolvedCount++;
					break;
				case ProbeOutcomeCode.StoppedEarlyAbsorbed:
					summary.BecameStoppedEarlyAbsorbedCount++;
					break;
				case ProbeOutcomeCode.NumericalFailure:
					summary.BecameNumericalFailureCount++;
					break;
				case ProbeOutcomeCode.Invalid:
					summary.BecameInvalidCount++;
					break;
				case ProbeOutcomeCode.MaxStepsExhausted:
					summary.UnchangedMaxStepsCount++;
					break;
			}
		}

		summary.RemainingMaxStepsCount = summary.UnchangedMaxStepsCount;
		summary.ResolvedPixelCount = summary.BecameHitGeometryCount + summary.BecameBackgroundResolvedCount;

		ProbeRegionAnalyzer.Analyze(
			filmWidth,
			filmHeight,
			new ReadOnlySpan<ProbeOutcomeCode>(outcomes, 0, totalPixels),
			new Span<ushort>(regionLabels, 0, totalPixels),
			regions);
		ComputeChildRegionStats(selectedPixelIndices, regionLabels, regions, ref summary);
		frameSummary = BuildFrameSummary(filmWidth, filmHeight, outcomes, regions, in sourceContext, summary);
		return summary;
	}

	private static bool ValidateForAtomicApply(
		int filmWidth,
		int filmHeight,
		int sourceFrameGeneration,
		int currentFrameGeneration,
		in ProbeContextKey sourceContext,
		in ProbeContextKey currentContext,
		ReadOnlySpan<int> selectedPixelIndices,
		ReadOnlySpan<ProbeSelectedIndexResult> results,
		out int totalPixels,
		out string failureReason,
		out bool contextMatched)
	{
		totalPixels = SafeTotalPixels(filmWidth, filmHeight);
		failureReason = string.Empty;
		contextMatched = sourceContext == currentContext;
		if (!contextMatched)
		{
			failureReason = "context_key_mismatch";
			return false;
		}
		if (sourceFrameGeneration != currentFrameGeneration)
		{
			failureReason = "frame_generation_mismatch";
			return false;
		}
		if (totalPixels <= 0)
		{
			failureReason = "film_dimensions_invalid";
			return false;
		}
		if (selectedPixelIndices.Length == 0)
		{
			failureReason = "no_selected_pixels";
			return false;
		}
		if (results.Length != selectedPixelIndices.Length)
		{
			failureReason = "result_count_mismatch";
			return false;
		}

		for (int i = 0; i < selectedPixelIndices.Length; i++)
		{
			int pixelIndex = selectedPixelIndices[i];
			if (pixelIndex < 0 || pixelIndex >= totalPixels)
			{
				failureReason = $"pixel_index_out_of_range:{pixelIndex}";
				return false;
			}
			if (results[i].PixelIndex != pixelIndex)
			{
				failureReason = $"result_pixel_mismatch:{i}";
				return false;
			}
			for (int j = 0; j < i; j++)
			{
				if (selectedPixelIndices[j] == pixelIndex)
				{
					failureReason = $"duplicate_pixel_index:{pixelIndex}";
					return false;
				}
			}
		}
		return true;
	}

	private static ProbeFrameSummary BuildFrameSummary(
		int filmWidth,
		int filmHeight,
		ProbeOutcomeCode[] outcomes,
		List<ProbeRegionRecord> regions,
		in ProbeContextKey context,
		ProbeRefinementSummary refinementSummary)
	{
		int totalPixels = SafeTotalPixels(filmWidth, filmHeight);
		var summary = new ProbeFrameSummary
		{
			TotalPixels = totalPixels,
			ContextKey = context,
			LastRefinementPixelsAttempted = refinementSummary.AppliedPixelCount,
			LastRefinementNewlyResolved = refinementSummary.ResolvedPixelCount,
			LastRefinementStillUnresolved = refinementSummary.RemainingMaxStepsCount
		};

		for (int i = 0; i < totalPixels; i++)
		{
			switch (outcomes[i])
			{
				case ProbeOutcomeCode.HitGeometry:
					summary.HitGeometryCount++;
					break;
				case ProbeOutcomeCode.BackgroundResolved:
					summary.BackgroundResolvedCount++;
					break;
				case ProbeOutcomeCode.MaxStepsExhausted:
					summary.MaxStepsExhaustedCount++;
					break;
				case ProbeOutcomeCode.StoppedEarlyAbsorbed:
					summary.StoppedEarlyAbsorbedCount++;
					break;
				case ProbeOutcomeCode.NumericalFailure:
					summary.NumericalFailureCount++;
					break;
			}
		}

		summary.RegionCount = regions.Count;
		for (int i = 0; i < regions.Count; i++)
		{
			ProbeRegionRecord region = regions[i];
			if (region.IsPrimarilyMaxStepsExhausted)
			{
				summary.SelectableRegionCount++;
			}
			if (region.PixelCount > summary.LargestRegionPixelCount)
			{
				summary.LargestRegionPixelCount = region.PixelCount;
				summary.LargestRegionId = region.Id;
			}
		}
		return summary;
	}

	private static void ComputeChildRegionStats(
		ReadOnlySpan<int> selectedPixelIndices,
		ushort[] regionLabels,
		List<ProbeRegionRecord> regions,
		ref ProbeRefinementSummary summary)
	{
		for (int i = 0; i < selectedPixelIndices.Length; i++)
		{
			ushort label = regionLabels[selectedPixelIndices[i]];
			if (label == 0 || HasSeenLabel(selectedPixelIndices, regionLabels, i, label))
			{
				continue;
			}
			summary.ChildRegionCount++;
			ProbeRegionRecord region = FindRegion(regions, label);
			if (region.PixelCount > summary.LargestChildRegionPixelCount)
			{
				summary.LargestChildRegionId = label;
				summary.LargestChildRegionPixelCount = region.PixelCount;
			}
		}
	}

	private static bool HasSeenLabel(ReadOnlySpan<int> selectedPixelIndices, ushort[] regionLabels, int beforeIndex, ushort label)
	{
		for (int i = 0; i < beforeIndex; i++)
		{
			if (regionLabels[selectedPixelIndices[i]] == label)
			{
				return true;
			}
		}
		return false;
	}

	private static ProbeRegionRecord FindRegion(List<ProbeRegionRecord> regions, ushort id)
	{
		for (int i = 0; i < regions.Count; i++)
		{
			if (regions[i].Id == id)
			{
				return regions[i];
			}
		}
		return default;
	}

	private static int SafeTotalPixels(int filmWidth, int filmHeight)
	{
		if (filmWidth <= 0 || filmHeight <= 0)
		{
			return 0;
		}
		long total = (long)filmWidth * filmHeight;
		return total > int.MaxValue ? 0 : (int)total;
	}
}

#nullable restore
