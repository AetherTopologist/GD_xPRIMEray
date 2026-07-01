using System.Globalization;
using XPrimeRay.Core.Fixtures;
using XPrimeRay.Core.Transport;

namespace XPrimeRay.Core.Comparison;

public static class DifferencePacketBuilder
{
    private const string Channel = "bend_magnitude_metric";
    private const string Representation = "scalar_grid";

    public static DifferencePacket BuildCoreSelfComparison(
        FixtureDefinition fixture,
        string fixturePath,
        string snapshotId,
        DateTimeOffset generatedUtc,
        TransportResult result)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        ArgumentNullException.ThrowIfNull(result);

        var snapshot = new DifferenceSnapshot
        {
            Id = snapshotId,
            FixtureId = fixture.Name,
            FixtureComparisonIdentity = fixture.ComparisonIdentity ?? fixture.Name,
            FixturePath = fixturePath,
            ArtifactPath = "snapshot_heatmap.csv",
        };
        var channel = ChannelRegistry.Default.Find(Channel);

        if (fixture.Observer is null)
        {
            var compatibility = ChannelCompatibilityMatrix.Default.Evaluate(
                channel,
                channel,
                new ChannelComparisonContext(false, true, true));
            return CreateUnavailablePacket(
                generatedUtc,
                snapshot,
                compatibility,
                "The Core fixture does not declare an observer basis.");
        }

        var observerBasis = FormatObserverBasis(fixture.Observer);
        var compatibilityResult = ChannelCompatibilityMatrix.Default.Evaluate(
            channel,
            channel,
            new ChannelComparisonContext(true, true, true));

        if (compatibilityResult.Status != DifferenceStatus.Comparable)
        {
            return CreateUnavailablePacket(
                generatedUtc,
                snapshot,
                compatibilityResult,
                compatibilityResult.Reason,
                observerBasis);
        }

        if (result.RayMetrics.Count == 0)
        {
            return CreateUnavailablePacket(
                generatedUtc,
                snapshot,
                new ChannelCompatibility
                {
                    RuleId = "missing_snapshot_samples",
                    Status = DifferenceStatus.Unknown,
                    Reason = "The Core result does not contain bend_magnitude_metric samples.",
                },
                "The Core result does not contain bend_magnitude_metric samples.",
                observerBasis);
        }

        var summary = Compare(result.RayMetrics, result.RayMetrics);
        var packet = new DifferencePacket
        {
            GeneratedUtc = FormatTimestamp(generatedUtc),
            LeftSnapshot = snapshot,
            RightSnapshot = snapshot,
            LeftChannel = Channel,
            RightChannel = Channel,
            LeftRepresentation = Representation,
            RightRepresentation = Representation,
            ObserverBasis = observerBasis,
            ComparisonBasis = "same_core_run_same_fixture_same_observer_same_channel",
            CompatibilityRuleId = compatibilityResult.RuleId,
            ChannelRegistryVersion = ChannelRegistry.CurrentVersion,
            CompatibilityMatrixVersion = ChannelCompatibilityMatrix.CurrentVersion,
            Status = compatibilityResult.Status,
            Comparable = compatibilityResult.Status == DifferenceStatus.Comparable,
            TransformRequired = compatibilityResult.TransformRequired,
            Reason = compatibilityResult.Reason,
            Summary = summary,
            ClaimBoundary =
            [
                "This is a Core-vs-Core self-comparison.",
                "This is not a Godot comparison.",
                "This is not parity.",
                "This is not validation.",
                "No image or cross-channel comparison was performed.",
            ],
        };
        packet.Validate();
        return packet;
    }

    public static DifferencePacket BuildRetainedComparison(
        RetainedSnapshotDescriptor left,
        RetainedSnapshotDescriptor right,
        string requestedChannel,
        DateTimeOffset generatedUtc)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var requestedChannelDeclaration = ChannelRegistry.Default.Find(requestedChannel);
        var sameFixture = left.FixtureComparisonIdentity == right.FixtureComparisonIdentity;
        var sameObserver = left.ObserverBasis == right.ObserverBasis;
        ChannelCompatibility compatibility;
        var sameGrid = false;

        if (requestedChannelDeclaration is null)
        {
            compatibility = new ChannelCompatibility
            {
                RuleId = "missing_channel_declaration",
                Status = DifferenceStatus.Unknown,
                Reason = $"The requested channel '{requestedChannel}' is not declared in the channel registry.",
            };
        }
        else if (left.ChannelId != requestedChannel || right.ChannelId != requestedChannel)
        {
            compatibility = new ChannelCompatibility
            {
                RuleId = "requested_channel_mismatch",
                Status = DifferenceStatus.Unknown,
                Reason = $"The requested channel '{requestedChannel}' does not match both retained packet channel declarations (left='{left.ChannelId}', right='{right.ChannelId}').",
                RequiredConditions = ["same_channel_type"],
            };
        }
        else
        {
            var leftChannel = ChannelRegistry.Default.Find(left.ChannelId);
            var rightChannel = ChannelRegistry.Default.Find(right.ChannelId);
            sameGrid = HasSameCoordinateGrid(left.Samples, right.Samples);
            compatibility = ChannelCompatibilityMatrix.Default.Evaluate(
                leftChannel,
                rightChannel,
                new ChannelComparisonContext(sameObserver, sameFixture, sameGrid));

            if (compatibility.Status == DifferenceStatus.Comparable
                && left.FixtureId != right.FixtureId)
            {
                compatibility = compatibility with
                {
                    Reason = $"{compatibility.Reason} The retained inputs declare distinct Core fixtures '{left.FixtureId}' and '{right.FixtureId}' under comparison identity '{left.FixtureComparisonIdentity}', so numeric differences may reflect their declared fixture parameters.",
                };
            }
        }

        var comparable = compatibility.Status == DifferenceStatus.Comparable;
        var summary = comparable
            ? Compare(left.Samples, right.Samples)
            : new DifferenceSummary();
        var packet = new DifferencePacket
        {
            GeneratedUtc = FormatTimestamp(generatedUtc),
            ComparisonScope = "retained_snapshot_pair",
            LeftRunId = left.RunId,
            RightRunId = right.RunId,
            LeftManifestPath = left.ManifestPath,
            RightManifestPath = right.ManifestPath,
            LeftSnapshot = ToSnapshot(left),
            RightSnapshot = ToSnapshot(right),
            LeftChannel = left.ChannelId,
            RightChannel = right.ChannelId,
            LeftRepresentation = left.Representation,
            RightRepresentation = right.Representation,
            ObserverBasis = sameObserver ? left.ObserverBasis : "retained_observer_basis_mismatch",
            ComparisonBasis = "retained_core_snapshots_same_channel_same_fixture_same_observer_same_coordinate_grid",
            CompatibilityRuleId = compatibility.RuleId,
            ChannelRegistryVersion = ChannelRegistry.CurrentVersion,
            CompatibilityMatrixVersion = ChannelCompatibilityMatrix.CurrentVersion,
            Status = compatibility.Status,
            Comparable = comparable,
            TransformRequired = compatibility.TransformRequired,
            Reason = compatibility.Reason,
            Summary = summary,
            ClaimBoundary =
            [
                "This is a retained Core-vs-Core snapshot comparison.",
                "This is not a Godot comparison.",
                "This is not image or pixel comparison.",
                "This is not parity.",
                "This is not physical validation.",
                "This is not renderer equivalence.",
                "A zero difference does not establish equivalence with another runtime or measurement system.",
            ],
        };
        packet.Validate();
        return packet;
    }

    private static DifferenceSummary Compare(
        IReadOnlyList<RayMetric> left,
        IReadOnlyList<RayMetric> right)
    {
        if (left.Count != right.Count)
        {
            throw new InvalidDataException("Core difference inputs must contain the same sample count.");
        }

        var leftOrdered = left.OrderBy(metric => metric.Y).ThenBy(metric => metric.X).ToArray();
        var rightOrdered = right.OrderBy(metric => metric.Y).ThenBy(metric => metric.X).ToArray();
        var max = 0.0;
        var sum = 0.0;
        var nonZero = 0;

        for (var index = 0; index < leftOrdered.Length; index++)
        {
            if (leftOrdered[index].X != rightOrdered[index].X
                || leftOrdered[index].Y != rightOrdered[index].Y)
            {
                throw new InvalidDataException("Core difference inputs must use the same sample coordinates.");
            }

            var difference = Math.Abs(leftOrdered[index].BendMagnitude - rightOrdered[index].BendMagnitude);
            if (!double.IsFinite(difference))
            {
                throw new InvalidDataException("Core difference inputs must contain finite values.");
            }

            max = Math.Max(max, difference);
            sum += difference;
            if (difference != 0.0)
            {
                nonZero++;
            }
        }

        return new DifferenceSummary
        {
            CountCompared = leftOrdered.Length,
            MaxAbsDifference = max,
            MeanAbsDifference = leftOrdered.Length == 0 ? 0.0 : sum / leftOrdered.Length,
            NonZeroCount = nonZero,
        };
    }

    private static bool HasSameCoordinateGrid(
        IReadOnlyList<RayMetric> left,
        IReadOnlyList<RayMetric> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        var leftCoordinates = left.Select(metric => (metric.X, metric.Y)).OrderBy(value => value.Y).ThenBy(value => value.X);
        var rightCoordinates = right.Select(metric => (metric.X, metric.Y)).OrderBy(value => value.Y).ThenBy(value => value.X);
        return leftCoordinates.SequenceEqual(rightCoordinates);
    }

    private static DifferenceSnapshot ToSnapshot(RetainedSnapshotDescriptor descriptor)
    {
        return new DifferenceSnapshot
        {
            Id = $"{descriptor.RunId}:{Path.GetFileName(descriptor.SnapshotPath)}",
            FixtureId = descriptor.FixtureId,
            FixtureComparisonIdentity = descriptor.FixtureComparisonIdentity,
            FixturePath = descriptor.FixturePath,
            ArtifactPath = descriptor.SnapshotPath,
        };
    }

    private static DifferencePacket CreateUnavailablePacket(
        DateTimeOffset generatedUtc,
        DifferenceSnapshot snapshot,
        ChannelCompatibility compatibility,
        string reason,
        string observerBasis = "unknown")
    {
        var packet = new DifferencePacket
        {
            GeneratedUtc = FormatTimestamp(generatedUtc),
            LeftSnapshot = snapshot,
            RightSnapshot = snapshot,
            LeftChannel = "unknown",
            RightChannel = "unknown",
            LeftRepresentation = "unknown",
            RightRepresentation = "unknown",
            ObserverBasis = observerBasis,
            ComparisonBasis = "insufficient_declared_core_snapshot_metadata",
            CompatibilityRuleId = compatibility.RuleId,
            ChannelRegistryVersion = ChannelRegistry.CurrentVersion,
            CompatibilityMatrixVersion = ChannelCompatibilityMatrix.CurrentVersion,
            Status = compatibility.Status,
            Comparable = compatibility.Status == DifferenceStatus.Comparable,
            TransformRequired = compatibility.TransformRequired,
            Reason = reason,
            Summary = new DifferenceSummary(),
            ClaimBoundary =
            [
                "This is a Core-vs-Core self-comparison.",
                "Comparison eligibility is unknown.",
                "This is not a Godot comparison.",
                "This is not parity.",
                "This is not validation.",
                "No image or cross-channel comparison was performed.",
            ],
        };
        packet.Validate();
        return packet;
    }

    public static string FormatObserverBasis(ObserverDefinition observer)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"origin={FormatVector(observer.Origin)};forward={FormatVector(observer.Forward)};up={FormatVector(observer.Up)};fovDegrees={observer.FovDegrees:G9}");
    }

    private static string FormatVector(IReadOnlyList<float> values)
    {
        return "[" + string.Join(",", values.Select(value => value.ToString("G9", CultureInfo.InvariantCulture))) + "]";
    }

    private static string FormatTimestamp(DateTimeOffset value)
    {
        return value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }
}
