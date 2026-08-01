using System.Numerics;
using XPrimeRay.ObservationLayer;
using XPrimeRay.ObserverInstrumentation.Abstractions;
using XPrimeRay.ObserverInstrumentation.ObservationLayerAdapters;

namespace XPrimeRay.ObserverInstrumentation.Tests;

internal static class ObservationFrameAdapterTests
{
    public static void Run()
    {
        CathedralExactOutcomeBytePreservation();
        CathedralCopiedSourceIsolation();
        CathedralMismatchedLengthsRejected();
        CathedralIncompleteSnapshotRejected();
        CathedralUnprocessedOutcomeRejected();
        CathedralGenerationMismatchRejected();
        CathedralRegionUnavailableRejected();
        CathedralDescriptorsAndDependenciesCorrect();
        SparseOiRecordsPreserved();
        UnsupportedRichOiRecordsRejected();
    }

    private static void CathedralExactOutcomeBytePreservation()
    {
        ProbeOutcomeCode[] outcomes =
        {
            ProbeOutcomeCode.HitGeometry,
            ProbeOutcomeCode.BackgroundResolved,
            ProbeOutcomeCode.MaxStepsExhausted,
            ProbeOutcomeCode.StoppedEarlyAbsorbed,
            ProbeOutcomeCode.NumericalFailure,
            ProbeOutcomeCode.Invalid
        };
        SealedObservationFrame frame = CreateCathedralFrame(outcomes);
        ObservationChannelId.TryCreate(CathedralProbeObservationFrameAdapter.OutcomeChannelId, out ObservationChannelId id);
        TestAssert.True(frame.TryGetChannel(id, out SealedObservationChannel<byte>? channel, out ObservationValidity validity), "outcome channel found");
        TestAssert.Equal(ObservationValidity.Valid, validity, "outcome validity");
        for (int i = 0; i < outcomes.Length; i++)
        {
            TestAssert.Equal((byte)outcomes[i], channel!.Span[i], $"outcome byte {i}");
        }
    }

    private static void CathedralCopiedSourceIsolation()
    {
        ProbeOutcomeCode[] outcomes = { ProbeOutcomeCode.HitGeometry, ProbeOutcomeCode.BackgroundResolved };
        SealedObservationFrame frame = CreateCathedralFrame(outcomes, width: 2, height: 1);
        outcomes[0] = ProbeOutcomeCode.Invalid;
        ObservationChannelId.TryCreate(CathedralProbeObservationFrameAdapter.OutcomeChannelId, out ObservationChannelId id);
        frame.TryGetChannel(id, out SealedObservationChannel<byte>? channel, out _);
        TestAssert.Equal((byte)ProbeOutcomeCode.HitGeometry, channel!.Span[0], "sealed outcome copy isolated");
    }

    private static void CathedralMismatchedLengthsRejected()
    {
        ProbeOutcomeCode[] outcomes = { ProbeOutcomeCode.HitGeometry, ProbeOutcomeCode.BackgroundResolved };
        ushort[] regions = { 0 };
        byte[] levels = { 0, 0 };
        bool ok = CathedralProbeObservationFrameAdapter.TryCreateCompletedFrame(
            Snapshot(2, 1, outcomes),
            Scene(),
            SceneClass.DiagnosticChamber,
            1,
            1,
            Policy(),
            Context(),
            outcomes,
            regions,
            levels,
            out _,
            out string reason);
        TestAssert.False(ok, "mismatched lengths rejected");
        TestAssert.Equal("channel_length_mismatch", reason, "mismatched length reason");
    }

    private static void CathedralIncompleteSnapshotRejected()
    {
        ProbeOutcomeCode[] outcomes = { ProbeOutcomeCode.HitGeometry };
        ProbeSnapshotLifecycleResult snapshot = Snapshot(1, 1, outcomes);
        snapshot.State = ProbeSnapshotLifecycleState.Incomplete;
        bool ok = TryCreate(snapshot, outcomes, out string reason);
        TestAssert.False(ok, "incomplete snapshot rejected");
        TestAssert.Equal("snapshot_not_complete", reason, "incomplete reason");
    }

    private static void CathedralUnprocessedOutcomeRejected()
    {
        ProbeOutcomeCode[] outcomes = { ProbeOutcomeCode.Unprocessed };
        ProbeSnapshotLifecycleResult snapshot = Snapshot(1, 1, outcomes);
        snapshot.UnprocessedPixelCount = 0;
        bool ok = TryCreate(snapshot, outcomes, out string reason);
        TestAssert.False(ok, "unprocessed outcome rejected");
        TestAssert.Equal("outcome_contains_unprocessed", reason, "unprocessed outcome reason");
    }

    private static void CathedralGenerationMismatchRejected()
    {
        ProbeOutcomeCode[] outcomes = { ProbeOutcomeCode.HitGeometry };
        bool ok = CathedralProbeObservationFrameAdapter.TryCreateCompletedFrame(
            Snapshot(1, 1, outcomes),
            Scene(),
            SceneClass.DiagnosticChamber,
            4,
            3,
            Policy(),
            Context(),
            outcomes,
            new ushort[] { 0 },
            new byte[] { 0 },
            out _,
            out string reason);
        TestAssert.False(ok, "generation mismatch rejected");
        TestAssert.Equal("generation_mismatch", reason, "generation mismatch reason");
    }

    private static void CathedralRegionUnavailableRejected()
    {
        ProbeOutcomeCode[] outcomes = { ProbeOutcomeCode.HitGeometry };
        ProbeSnapshotLifecycleResult snapshot = Snapshot(1, 1, outcomes);
        snapshot.RegionAnalysisAvailable = false;
        bool ok = TryCreate(snapshot, outcomes, out string reason);
        TestAssert.False(ok, "region unavailable rejected");
        TestAssert.Equal("region_analysis_unavailable", reason, "region unavailable reason");
    }

    private static void CathedralDescriptorsAndDependenciesCorrect()
    {
        ProbeOutcomeCode[] outcomes = { ProbeOutcomeCode.MaxStepsExhausted, ProbeOutcomeCode.HitGeometry };
        SealedObservationFrame frame = CreateCathedralFrame(outcomes, width: 2, height: 1);
        IReadOnlyList<ObservationChannelDescriptor> descriptors = frame.EnumerateChannels();
        TestAssert.Equal(3, descriptors.Count, "cathedral descriptor count");
        ObservationChannelDescriptor region = descriptors.Single(d => d.Id.Value == CathedralProbeObservationFrameAdapter.RegionLabelChannelId);
        TestAssert.Equal(ObservationDataType.UInt16, region.DataType, "region type");
        TestAssert.Equal(ObservationDomain.DensePixelPlane, region.Domain, "region domain");
        TestAssert.Equal(InstrumentTier.SemanticClassification, region.ScientificTier, "region tier");
        TestAssert.Equal(1, region.SourceChannelDependencies.Count, "region dependency count");
        TestAssert.Equal(CathedralProbeObservationFrameAdapter.OutcomeChannelId, region.SourceChannelDependencies[0].ChannelId.Value, "region source dependency");
    }

    private static void SparseOiRecordsPreserved()
    {
        InstrumentObservation[] observations =
        {
            new(9, InstrumentKind.SurfaceUv, InstrumentDiagnosticState.RegionSampled, "uv_probe", new Vector2(0.25f, 0.5f), true, false),
            new(10, InstrumentKind.CheckerProbe, InstrumentDiagnosticState.RegionSampled, "uv_probe", new Vector2(0.75f, 0.5f), true, true)
        };
        bool ok = ObserverInstrumentationObservationFrameAdapter.TryCreateCompletedFrame(
            Descriptor(),
            observations,
            out SealedObservationFrame? frame,
            out string reason);
        TestAssert.True(ok, $"sparse OI adapter ok: {reason}");
        ObservationChannelId.TryCreate(ObserverInstrumentationObservationFrameAdapter.SparseObservationChannelId, out ObservationChannelId id);
        TestAssert.True(frame!.TryGetChannel(id, out SealedObservationChannel<PortableInstrumentObservationRecord>? channel, out _), "sparse channel found");
        TestAssert.Equal(2, channel!.Count, "sparse count");
        TestAssert.Equal(9, channel.Span[0].RayIndex, "ray index preserved");
        TestAssert.Equal("surface_uv", channel.Span[0].InstrumentId, "surface id");
        TestAssert.Equal(0.25f, channel.Span[0].Uv.X, "u preserved");
        TestAssert.Equal(true, channel.Span[1].SampleValue, "checker sample preserved");
    }

    private static void UnsupportedRichOiRecordsRejected()
    {
        InstrumentObservation[] observations =
        {
            new(3, InstrumentKind.TextureSample, InstrumentDiagnosticState.RegionSampled, "texture_probe", new Vector2(0.25f, 0.5f), true, false)
        };
        bool ok = ObserverInstrumentationObservationFrameAdapter.TryCreateCompletedFrame(
            Descriptor(),
            observations,
            out _,
            out string reason);
        TestAssert.False(ok, "texture sample rejected");
        TestAssert.Equal("unsupported_instrument:TextureSample", reason, "unsupported reason");
    }

    private static SealedObservationFrame CreateCathedralFrame(ProbeOutcomeCode[] outcomes, int width = 3, int height = 2)
    {
        ProbeSnapshotLifecycleResult snapshot = Snapshot(width, height, outcomes);
        ushort[] regions = Enumerable.Range(0, outcomes.Length).Select(i => (ushort)(i % 2)).ToArray();
        byte[] levels = Enumerable.Repeat((byte)0, outcomes.Length).ToArray();
        bool ok = CathedralProbeObservationFrameAdapter.TryCreateCompletedFrame(
            snapshot,
            Scene(),
            SceneClass.DiagnosticChamber,
            7,
            7,
            Policy(),
            Context(),
            outcomes,
            regions,
            levels,
            out SealedObservationFrame? frame,
            out string reason);
        TestAssert.True(ok, $"cathedral frame created: {reason}");
        return frame!;
    }

    private static bool TryCreate(ProbeSnapshotLifecycleResult snapshot, ProbeOutcomeCode[] outcomes, out string reason)
    {
        int total = outcomes.Length;
        return CathedralProbeObservationFrameAdapter.TryCreateCompletedFrame(
            snapshot,
            Scene(),
            SceneClass.DiagnosticChamber,
            1,
            1,
            Policy(),
            Context(),
            outcomes,
            Enumerable.Repeat((ushort)0, total).ToArray(),
            Enumerable.Repeat((byte)0, total).ToArray(),
            out _,
            out reason);
    }

    private static ProbeSnapshotLifecycleResult Snapshot(int width, int height, IReadOnlyList<ProbeOutcomeCode> outcomes)
    {
        int hit = 0;
        int background = 0;
        int maxSteps = 0;
        int absorbed = 0;
        int fault = 0;
        int invalid = 0;
        int unprocessed = 0;
        foreach (ProbeOutcomeCode outcome in outcomes)
        {
            switch (outcome)
            {
                case ProbeOutcomeCode.HitGeometry: hit++; break;
                case ProbeOutcomeCode.BackgroundResolved: background++; break;
                case ProbeOutcomeCode.MaxStepsExhausted: maxSteps++; break;
                case ProbeOutcomeCode.StoppedEarlyAbsorbed: absorbed++; break;
                case ProbeOutcomeCode.NumericalFailure: fault++; break;
                case ProbeOutcomeCode.Invalid: invalid++; break;
                case ProbeOutcomeCode.Unprocessed: unprocessed++; break;
            }
        }
        return new ProbeSnapshotLifecycleResult
        {
            RequestId = 1,
            State = ProbeSnapshotLifecycleState.Complete,
            Reason = ProbeSnapshotLifecycleReason.None,
            Width = width,
            Height = height,
            TotalPixelCount = width * height,
            ProcessedPixelCount = (width * height) - unprocessed,
            UnprocessedPixelCount = unprocessed,
            ContextMatched = true,
            DimensionsMatched = true,
            PolicyMaxSteps = 80,
            PolicyStepSize = 0.07f,
            HitGeometryCount = hit,
            BackgroundResolvedCount = background,
            MaxStepsExhaustedCount = maxSteps,
            StoppedEarlyAbsorbedCount = absorbed,
            NumericalFailureCount = fault,
            InvalidCount = invalid,
            RegionAnalysisAvailable = true,
            RegionCount = maxSteps > 0 ? 1 : 0,
            SelectableRegionCount = maxSteps > 0 ? 1 : 0,
            LargestRegionId = maxSteps > 0 ? (ushort)1 : (ushort)0,
            LargestRegionPixelCount = maxSteps
        };
    }

    private static SceneId Scene()
    {
        SceneId.TryCreate("cathedral_probe_snapshot.v1", out SceneId sceneId);
        return sceneId;
    }

    private static ObservationFrameDescriptor Descriptor() =>
        new(
            Scene(),
            SceneClass.DiagnosticChamber,
            generation: 1,
            width: 0,
            height: 0,
            totalSampleCount: 2,
            isComplete: true,
            contextIdentity: Context(),
            policyFingerprint: Policy());

    private static ObservationPolicyFingerprint Policy() =>
        new(
            baseStepsPerRay: 80,
            baseStepLength: 0.07f,
            minStepLength: 0.001f,
            maxStepLength: 0.35f,
            bendScale: 1f,
            fieldStrength: 1f,
            fieldSourceEpoch: 1,
            geometryEpoch: 2,
            boundaryLayerEpoch: 3,
            policyVersion: 1,
            transportMode: "GRIN_Optical");

    private static string Context() =>
        "cameraOrigin=00000001;cameraBasis=00000002;film=2x1;fieldEpoch=1;geometryEpoch=2;boundaryEpoch=3";
}
