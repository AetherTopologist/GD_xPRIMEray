using System.Reflection;

namespace XPrimeRay.ObservationLayer.Tests;

internal static class ObservationLayerContractTests
{
    public static void Run()
    {
        NoForbiddenDependencies();
        SceneIdValidation();
        SceneClassIsCategoryOnly();
        SealedFrameCopiesSourceArrays();
        ChannelGenerationMustEqualFrameGeneration();
        ChannelLookupAndMissingChannel();
        TypedMismatchRejected();
        IncompleteDenseEvidenceRejected();
        VisualizationMappingDoesNotMutateSource();
        ChannelDependenciesRoundTrip();
        PolicyFingerprintIncludesBaseStepLength();
        TransportEventUsesAccumulatedPathLength();
        IdentityCodesAreNumericOnly();
        CathedralStyleDenseAdapterPreservesOutcomeBytes();
        SparseOiStyleRecordPreservesRayData();
        FrameCompletenessGatesDenseSemanticEvidence();
    }

    private static void NoForbiddenDependencies()
    {
        AssemblyName[] references = typeof(SealedObservationFrame).Assembly.GetReferencedAssemblies();
        foreach (AssemblyName reference in references)
        {
            string name = reference.Name ?? string.Empty;
            Assert.False(name.Contains("Godot", StringComparison.OrdinalIgnoreCase), "no Godot reference");
            Assert.False(name.Contains("ObserverInstrumentation", StringComparison.OrdinalIgnoreCase), "no OI implementation reference");
            Assert.False(name.Contains("CathedralProbe", StringComparison.OrdinalIgnoreCase), "no Cathedral implementation reference");
        }
    }

    private static void SceneIdValidation()
    {
        Assert.True(SceneId.TryCreate("cathedral_probe_snapshot.v1", out SceneId scene), "valid scene id");
        Assert.Equal("cathedral_probe_snapshot.v1", scene.Value, "scene value");
        Assert.False(SceneId.TryCreate("res://test.tscn", out _), "reject Godot path");
        Assert.False(SceneId.TryCreate("Transport Chamber", out _), "reject UI label");
        Assert.False(SceneId.TryCreate("Node/Path", out _), "reject slash");
        Assert.False(SceneId.TryCreate("Uppercase.v1", out _), "reject uppercase");
    }

    private static void SceneClassIsCategoryOnly()
    {
        Assert.True(Enum.IsDefined(SceneClass.DiagnosticChamber), "diagnostic chamber category exists");
        Assert.True(SceneId.TryCreate("transport_chamber.v1", out SceneId id), "scene id valid");
        ObservationFrameDescriptor frame = Frame(id, SceneClass.InteractiveScene, complete: true);
        Assert.Equal(SceneClass.InteractiveScene, frame.SceneClass, "scene class stored separately");
    }

    private static void SealedFrameCopiesSourceArrays()
    {
        byte[] source = { 1, 2, 3 };
        SealedObservationFrame frame = DenseByteFrame("cathedral.probe.outcome", ObservationDataType.ProbeOutcomeCode, source);
        source[0] = 99;
        ObservationChannelId.TryCreate("cathedral.probe.outcome", out ObservationChannelId id);
        Assert.True(frame.TryGetChannel(id, out SealedObservationChannel<byte>? channel, out ObservationValidity validity), "channel found");
        Assert.Equal(ObservationValidity.Valid, validity, "valid channel");
        Assert.Equal((byte)1, channel!.Span[0], "sealed copy preserved original");
    }

    private static void ChannelGenerationMustEqualFrameGeneration()
    {
        ObservationFrameDescriptor frame = Frame(generation: 7, complete: true);
        ObservationChannelDescriptor descriptor = Descriptor("transport.final_step_count", ObservationDataType.Int32, ObservationDomain.DensePixelPlane);
        var channel = new SealedObservationChannel<int>(descriptor, generation: 6, stackalloc int[] { 1 });
        Assert.Throws<ArgumentException>(() => new SealedObservationFrame(frame, new ISealedObservationChannel[] { channel }), "generation invariant");
    }

    private static void ChannelLookupAndMissingChannel()
    {
        SealedObservationFrame frame = DenseByteFrame("cathedral.probe.outcome", ObservationDataType.ProbeOutcomeCode, new byte[] { 1 });
        ObservationChannelId.TryCreate("cathedral.probe.outcome", out ObservationChannelId exists);
        ObservationChannelId.TryCreate("cathedral.probe.region_label", out ObservationChannelId missing);
        Assert.True(frame.TryGetChannel(exists, out SealedObservationChannel<byte>? _, out ObservationValidity foundValidity), "existing channel found");
        Assert.Equal(ObservationValidity.Valid, foundValidity, "existing validity");
        Assert.False(frame.TryGetChannel(missing, out SealedObservationChannel<ushort>? _, out ObservationValidity missingValidity), "missing channel rejected");
        Assert.Equal(ObservationValidity.MissingChannel, missingValidity, "missing validity");
    }

    private static void TypedMismatchRejected()
    {
        SealedObservationFrame frame = DenseByteFrame("cathedral.probe.outcome", ObservationDataType.ProbeOutcomeCode, new byte[] { 1 });
        ObservationChannelId.TryCreate("cathedral.probe.outcome", out ObservationChannelId id);
        Assert.False(frame.TryGetChannel(id, out SealedObservationChannel<ushort>? _, out ObservationValidity validity), "wrong typed access rejected");
        Assert.Equal(ObservationValidity.TypeMismatch, validity, "type mismatch validity");
    }

    private static void IncompleteDenseEvidenceRejected()
    {
        ObservationFrameDescriptor frame = Frame(complete: false);
        ObservationChannelDescriptor descriptor = Descriptor("cathedral.probe.outcome", ObservationDataType.ProbeOutcomeCode, ObservationDomain.DensePixelPlane);
        var channel = new SealedObservationChannel<byte>(descriptor, frame.Generation, stackalloc byte[] { 1 });
        Assert.Throws<ArgumentException>(() => new SealedObservationFrame(frame, new ISealedObservationChannel[] { channel }), "incomplete dense gated");
    }

    private static void VisualizationMappingDoesNotMutateSource()
    {
        byte[] source = { 1, 2, 3 };
        SealedObservationFrame frame = DenseByteFrame("cathedral.probe.outcome", ObservationDataType.ProbeOutcomeCode, source);
        ObservationChannelId.TryCreate("cathedral.probe.outcome", out ObservationChannelId id);
        var mapping = new VisualizationMappingDescriptor(
            id,
            "outcome_palette",
            "v1",
            VisualizationRangePolicy.Fixed,
            0,
            255,
            "categorical.outcome",
            "Display mapping only; not an observation channel.");
        source[1] = 99;
        frame.TryGetChannel(id, out SealedObservationChannel<byte>? channel, out _);
        Assert.Equal("outcome_palette", mapping.MappingId, "mapping stored");
        Assert.Equal((byte)2, channel!.Span[1], "mapping did not mutate source");
    }

    private static void ChannelDependenciesRoundTrip()
    {
        ObservationChannelId.TryCreate("cathedral.probe.outcome", out ObservationChannelId dependencyId);
        ObservationChannelId.TryCreate("cathedral.probe.region_label", out ObservationChannelId id);
        var dependency = new ObservationChannelDependency(dependencyId, "Regions derive from max-step outcome components.");
        var descriptor = new ObservationChannelDescriptor(
            id,
            "Region Labels",
            ObservationDataType.UInt16,
            ObservationDomain.DensePixelPlane,
            InstrumentTier.SemanticClassification,
            "label",
            "Complete frame and region analysis available.",
            "Region labels classify semantic outcome-plane components only.",
            new[] { dependency });
        Assert.Equal(1, descriptor.SourceChannelDependencies.Count, "dependency count");
        Assert.Equal(dependencyId, descriptor.SourceChannelDependencies[0].ChannelId, "dependency id");
    }

    private static void PolicyFingerprintIncludesBaseStepLength()
    {
        ObservationPolicyFingerprint policy = Policy(baseStepLength: 0.07f);
        Assert.Equal(0.07f, policy.BaseStepLength, "base step length stored");
    }

    private static void TransportEventUsesAccumulatedPathLength()
    {
        var record = new TransportEventRecord(
            7,
            12,
            TransportEventKind.BoundaryRemap,
            new BoundaryIdentityCode(9),
            new FieldSourceIdentityCode(0),
            1.25f);
        Assert.Equal(1.25f, record.AccumulatedPathLength, "accumulated path length");
    }

    private static void IdentityCodesAreNumericOnly()
    {
        Assert.False(new BoundaryIdentityCode(0).IsKnown, "zero boundary unknown");
        Assert.True(new BoundaryIdentityCode(42).IsKnown, "boundary known");
        Assert.True(new FieldSourceIdentityCode(5).IsKnown, "field known");
    }

    private static void CathedralStyleDenseAdapterPreservesOutcomeBytes()
    {
        byte[] outcomes = { 0, 1, 2, 3, 4, 5, 255 };
        SealedObservationFrame frame = DenseByteFrame("cathedral.probe.outcome", ObservationDataType.ProbeOutcomeCode, outcomes);
        ObservationChannelId.TryCreate("cathedral.probe.outcome", out ObservationChannelId id);
        frame.TryGetChannel(id, out SealedObservationChannel<byte>? channel, out _);
        for (int i = 0; i < outcomes.Length; i++)
        {
            Assert.Equal(outcomes[i], channel!.Span[i], $"outcome byte {i}");
        }
    }

    private static void SparseOiStyleRecordPreservesRayData()
    {
        ObservationFrameDescriptor frameDescriptor = Frame(complete: true);
        ObservationChannelDescriptor descriptor = Descriptor(
            "observer.instrument.sparse_observation",
            ObservationDataType.InstrumentObservationRecord,
            ObservationDomain.SparseRayRecord,
            InstrumentTier.DirectObservation);
        var record = new PortableInstrumentObservationRecord(
            11,
            "surface_uv",
            "RegionSampled",
            "uv_probe",
            new Float2(0.25f, 0.5f),
            hasUv: true,
            sampleValue: false);
        var channel = new SealedObservationChannel<PortableInstrumentObservationRecord>(
            descriptor,
            frameDescriptor.Generation,
            new PortableInstrumentObservationRecord[] { record });
        var frame = new SealedObservationFrame(frameDescriptor, new ISealedObservationChannel[] { channel });
        ObservationChannelId.TryCreate("observer.instrument.sparse_observation", out ObservationChannelId id);
        Assert.True(frame.TryGetChannel(id, out SealedObservationChannel<PortableInstrumentObservationRecord>? stored, out _), "sparse found");
        Assert.Equal(11, stored!.Span[0].RayIndex, "ray index");
        Assert.Equal("surface_uv", stored.Span[0].InstrumentId, "instrument id");
    }

    private static void FrameCompletenessGatesDenseSemanticEvidence()
    {
        SealedObservationFrame frame = DenseByteFrame("cathedral.probe.region_label", ObservationDataType.UInt16, new ushort[] { 1, 2 });
        Assert.True(frame.IsComplete, "dense frame complete");
    }

    private static SealedObservationFrame DenseByteFrame(string idText, ObservationDataType dataType, ReadOnlySpan<byte> values)
    {
        ObservationFrameDescriptor frame = Frame(complete: true);
        ObservationChannelDescriptor descriptor = Descriptor(idText, dataType, ObservationDomain.DensePixelPlane, InstrumentTier.SemanticClassification);
        var channel = new SealedObservationChannel<byte>(descriptor, frame.Generation, values);
        return new SealedObservationFrame(frame, new ISealedObservationChannel[] { channel });
    }

    private static SealedObservationFrame DenseByteFrame(string idText, ObservationDataType dataType, ReadOnlySpan<ushort> values)
    {
        ObservationFrameDescriptor frame = Frame(complete: true);
        ObservationChannelDescriptor descriptor = Descriptor(idText, dataType, ObservationDomain.DensePixelPlane, InstrumentTier.SemanticClassification);
        var channel = new SealedObservationChannel<ushort>(descriptor, frame.Generation, values);
        return new SealedObservationFrame(frame, new ISealedObservationChannel[] { channel });
    }

    private static ObservationFrameDescriptor Frame(
        SceneId sceneId = default,
        SceneClass sceneClass = SceneClass.DiagnosticChamber,
        int generation = 3,
        bool complete = true)
    {
        if (!sceneId.IsValid)
        {
            SceneId.TryCreate("cathedral_probe_snapshot.v1", out sceneId);
        }

        return new ObservationFrameDescriptor(
            sceneId,
            sceneClass,
            generation,
            width: 4,
            height: 2,
            totalSampleCount: 8,
            isComplete: complete,
            contextIdentity: "camera=hash:1;basis=hash:2;observer=base",
            policyFingerprint: Policy());
    }

    private static ObservationPolicyFingerprint Policy(float baseStepLength = 0.05f) =>
        new(
            baseStepsPerRay: 80,
            baseStepLength: baseStepLength,
            minStepLength: 0.001f,
            maxStepLength: 0.35f,
            bendScale: 1f,
            fieldStrength: 1f,
            fieldSourceEpoch: 1,
            geometryEpoch: 2,
            boundaryLayerEpoch: 3,
            policyVersion: 1,
            transportMode: "GRIN_Optical");

    private static ObservationChannelDescriptor Descriptor(
        string idText,
        ObservationDataType dataType,
        ObservationDomain domain,
        InstrumentTier tier = InstrumentTier.SemanticClassification)
    {
        ObservationChannelId.TryCreate(idText, out ObservationChannelId id);
        return new ObservationChannelDescriptor(
            id,
            idText,
            dataType,
            domain,
            tier,
            "unitless",
            "Complete sealed frame required.",
            "Portable observation channel; display mappings are not evidence.");
    }

    private static class Assert
    {
        public static void True(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException($"Expected true: {label}");
        }

        public static void False(bool condition, string label)
        {
            if (condition) throw new InvalidOperationException($"Expected false: {label}");
        }

        public static void Equal<T>(T expected, T actual, string label)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException($"{label}: expected '{expected}', got '{actual}'");
            }
        }

        public static void Throws<TException>(Action action, string label)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }

            throw new InvalidOperationException($"Expected {typeof(TException).Name}: {label}");
        }
    }
}
