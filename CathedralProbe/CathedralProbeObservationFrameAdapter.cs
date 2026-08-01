#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using XPrimeRay.ObservationLayer;

public static class CathedralProbeObservationFrameAdapter
{
	public const string OutcomeChannelId = "cathedral.probe.outcome";
	public const string RegionLabelChannelId = "cathedral.probe.region_label";
	public const string RefinementLevelChannelId = "cathedral.probe.refinement_level";

	public static bool TryCreateCompletedFrame(
		ProbeSnapshotLifecycleResult snapshot,
		SceneId sceneId,
		SceneClass sceneClass,
		int frameGeneration,
		int sourceGeneration,
		ObservationPolicyFingerprint policyFingerprint,
		string contextIdentity,
		ReadOnlySpan<ProbeOutcomeCode> outcomes,
		ReadOnlySpan<ushort> regionLabels,
		ReadOnlySpan<byte> refinementLevels,
		out SealedObservationFrame? frame,
		out string reason)
	{
		frame = null;
		reason = string.Empty;
		if (!sceneId.IsValid)
		{
			reason = "invalid_scene_id";
			return false;
		}
		if (frameGeneration < 0 || sourceGeneration < 0 || frameGeneration != sourceGeneration)
		{
			reason = "generation_mismatch";
			return false;
		}
		if (snapshot.State != ProbeSnapshotLifecycleState.Complete || !snapshot.IsTerminal)
		{
			reason = "snapshot_not_complete";
			return false;
		}
		if (snapshot.UnprocessedPixelCount != 0)
		{
			reason = "snapshot_has_unprocessed_pixels";
			return false;
		}
		if (!snapshot.RegionAnalysisAvailable)
		{
			reason = "region_analysis_unavailable";
			return false;
		}

		int expectedCount = checked(snapshot.Width * snapshot.Height);
		if (snapshot.TotalPixelCount != expectedCount ||
			outcomes.Length != expectedCount ||
			regionLabels.Length != expectedCount ||
			refinementLevels.Length != expectedCount)
		{
			reason = "channel_length_mismatch";
			return false;
		}

		byte[] outcomeBytes = new byte[expectedCount];
		for (int i = 0; i < outcomes.Length; i++)
		{
			ProbeOutcomeCode outcome = outcomes[i];
			if (outcome == ProbeOutcomeCode.Unprocessed)
			{
				reason = "outcome_contains_unprocessed";
				return false;
			}
			outcomeBytes[i] = (byte)outcome;
		}
		if (!snapshot.HistogramReconciles())
		{
			reason = "snapshot_histogram_mismatch";
			return false;
		}

		var descriptor = new ObservationFrameDescriptor(
			sceneId,
			sceneClass,
			frameGeneration,
			snapshot.Width,
			snapshot.Height,
			snapshot.TotalPixelCount,
			isComplete: true,
			contextIdentity,
			policyFingerprint);

		var channels = new ISealedObservationChannel[]
		{
			new SealedObservationChannel<byte>(
				CreateOutcomeDescriptor(),
				frameGeneration,
				outcomeBytes),
			new SealedObservationChannel<ushort>(
				CreateRegionLabelDescriptor(),
				frameGeneration,
				regionLabels),
			new SealedObservationChannel<byte>(
				CreateRefinementLevelDescriptor(),
				frameGeneration,
				refinementLevels)
		};
		frame = new SealedObservationFrame(descriptor, channels);
		return true;
	}

	public static string[] BuildDiagnosticLines(SealedObservationFrame frame)
	{
		ArgumentNullException.ThrowIfNull(frame);
		var lines = new List<string>
		{
			"SEALED OBSERVATION FRAME",
			$"scene={frame.Descriptor.SceneId.Value}",
			$"class={frame.Descriptor.SceneClass}",
			$"generation={frame.Descriptor.Generation.ToString(CultureInfo.InvariantCulture)}",
			$"dimensions={frame.Descriptor.Width.ToString(CultureInfo.InvariantCulture)}x{frame.Descriptor.Height.ToString(CultureInfo.InvariantCulture)}",
			$"complete={(frame.Descriptor.IsComplete ? "true" : "false")}",
			$"context={frame.Descriptor.ContextIdentity}",
			"CHANNELS"
		};
		foreach (ObservationChannelDescriptor descriptor in frame.EnumerateChannels())
		{
			int count = 0;
			ObservationValidity validity = ObservationValidity.Unknown;
			if (descriptor.DataType == ObservationDataType.ProbeOutcomeCode || descriptor.DataType == ObservationDataType.UInt8)
			{
				frame.TryGetChannel(descriptor.Id, out SealedObservationChannel<byte>? channel, out validity);
				count = channel?.Count ?? 0;
			}
			else if (descriptor.DataType == ObservationDataType.UInt16)
			{
				frame.TryGetChannel(descriptor.Id, out SealedObservationChannel<ushort>? channel, out validity);
				count = channel?.Count ?? 0;
			}
			lines.Add($"id={descriptor.Id.Value}");
			lines.Add($"type={descriptor.DataType}");
			lines.Add($"domain={descriptor.Domain}");
			lines.Add($"tier={descriptor.ScientificTier}");
			lines.Add($"count={count.ToString(CultureInfo.InvariantCulture)}");
			lines.Add($"validity={validity}");
			lines.Add($"claim={descriptor.ClaimBoundary}");
		}
		return lines.ToArray();
	}

	private static ObservationChannelDescriptor CreateOutcomeDescriptor()
	{
		ObservationChannelId.TryCreate(OutcomeChannelId, out ObservationChannelId id);
		return new ObservationChannelDescriptor(
			id,
			"Cathedral Probe Outcome",
			ObservationDataType.ProbeOutcomeCode,
			ObservationDomain.DensePixelPlane,
			InstrumentTier.SemanticClassification,
			"code",
			"Complete Cathedral snapshot with zero Unprocessed pixels.",
			"Outcome codes are semantic transport classifications, not RGB or display pixels.");
	}

	private static ObservationChannelDescriptor CreateRegionLabelDescriptor()
	{
		ObservationChannelId.TryCreate(RegionLabelChannelId, out ObservationChannelId id);
		ObservationChannelId.TryCreate(OutcomeChannelId, out ObservationChannelId outcomeId);
		return new ObservationChannelDescriptor(
			id,
			"Cathedral Probe Region Label",
			ObservationDataType.UInt16,
			ObservationDomain.DensePixelPlane,
			InstrumentTier.SemanticClassification,
			"label",
			"Complete Cathedral snapshot with region analysis available.",
			"Region labels identify connected semantic outcome components only.",
			new[]
			{
				new ObservationChannelDependency(outcomeId, "Regions are derived from the Cathedral outcome plane.")
			});
	}

	private static ObservationChannelDescriptor CreateRefinementLevelDescriptor()
	{
		ObservationChannelId.TryCreate(RefinementLevelChannelId, out ObservationChannelId id);
		ObservationChannelId.TryCreate(OutcomeChannelId, out ObservationChannelId outcomeId);
		return new ObservationChannelDescriptor(
			id,
			"Cathedral Probe Refinement Level",
			ObservationDataType.UInt8,
			ObservationDomain.DensePixelPlane,
			InstrumentTier.TransportRecord,
			"level",
			"Complete Cathedral snapshot with retained refinement-level plane.",
			"Refinement level records transport-effort history; deeper refinement is more evidence, not automatically more truth.",
			new[]
			{
				new ObservationChannelDependency(outcomeId, "Refinement levels describe pixels in the same Cathedral outcome plane.")
			});
	}
}

#nullable restore
