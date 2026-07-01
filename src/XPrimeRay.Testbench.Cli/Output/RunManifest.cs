using System.Globalization;
using System.Text.Json.Serialization;
using XPrimeRay.Core.Comparison;
using XPrimeRay.Core.Fixtures;
using XPrimeRay.Core.Transport;
using XPrimeRay.Core.Validation;

namespace XPrimeRay.Testbench.Cli.Output;

public sealed record RunManifest
{
    public string Schema { get; init; } = "xprimeray.glowing_heart.run_manifest.v0.6";
    public string RunId { get; init; } = "";
    public string TimestampUtc { get; init; } = "";
    public string Source { get; init; } = "cli";
    public string Phase { get; init; } = "Project Glowing Heart v0.6";
    public FixtureManifest Fixture { get; init; } = new();
    public ResultManifest Result { get; init; } = new();
    public ArtifactManifest Artifacts { get; init; } = new();
    public string[] Limitations { get; init; } = Array.Empty<string>();

    public static RunManifest Create(
        string runId,
        DateTimeOffset timestampUtc,
        string fixturePath,
        FixtureDefinition fixture,
        TransportResult result,
        ValidationReport report,
        bool emitDifference)
    {
        return new RunManifest
        {
            RunId = runId,
            TimestampUtc = timestampUtc.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
            Fixture = new FixtureManifest
            {
                Name = result.FixtureName,
                ComparisonIdentity = fixture.ComparisonIdentity ?? result.FixtureName,
                Path = fixturePath,
                Mode = result.Mode,
                ObserverBasis = fixture.Observer is null
                    ? "unknown"
                    : DifferencePacketBuilder.FormatObserverBasis(fixture.Observer),
            },
            Result = new ResultManifest
            {
                Validation = report.Verdict,
                Rays = result.Rays,
                Hits = result.Hits,
                Misses = result.Misses,
                StepsPerRay = result.StepsPerRay,
                FieldSamples = result.FieldSampleCount,
                MeanBend = ToStableDouble(result.MeanBendMagnitude),
                MaxBend = ToStableDouble(result.MaxBendMagnitude),
            },
            Artifacts = ArtifactManifest.Create(emitDifference, fixture, result),
            Limitations =
            [
                "Core smoke transport only",
                "Godot parity is not claimed",
                "Hermetic closure is not claimed",
                "Collision behavior is not modeled",
                "Portal behavior is not modeled",
            ],
        };
    }

    private static double ToStableDouble(float value)
    {
        var stable = value.ToString("G9", CultureInfo.InvariantCulture);
        return double.Parse(stable, CultureInfo.InvariantCulture);
    }
}

public sealed record FixtureManifest
{
    public string Name { get; init; } = "";
    public string ComparisonIdentity { get; init; } = "";
    public string Path { get; init; } = "";
    public string Mode { get; init; } = "";
    public string ObserverBasis { get; init; } = "unknown";
}

public sealed record ResultManifest
{
    public string Validation { get; init; } = "";
    public int Rays { get; init; }
    public int Hits { get; init; }
    public int Misses { get; init; }
    public int StepsPerRay { get; init; }
    public int FieldSamples { get; init; }
    public double MeanBend { get; init; }
    public double MaxBend { get; init; }
}

public sealed record ArtifactManifest
{
    public string RayMetricsCsv { get; init; } = "ray_metrics.csv";
    public string RunSummaryMd { get; init; } = "run_summary.md";
    public string ObservatoryEntryJson { get; init; } = "observatory_entry.json";
    public string SnapshotPpm { get; init; } = "snapshot.ppm";
    public string SnapshotHeatmapCsv { get; init; } = "snapshot_heatmap.csv";
    public string SnapshotAsciiTxt { get; init; } = "snapshot_ascii.txt";
    public string SnapshotChannel { get; init; } = "bend_magnitude_metric";
    public string SnapshotRepresentation { get; init; } = "scalar_grid";
    public string TraversalStepCountCsv { get; init; } = "traversal_step_count.csv";
    public RetainedChannelManifest[] RetainedChannels { get; init; } = Array.Empty<RetainedChannelManifest>();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DifferencePacketJson { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DifferenceSummaryMarkdown { get; init; }

    public static ArtifactManifest Create(
        bool emitDifference,
        FixtureDefinition fixture,
        TransportResult result)
    {
        var observerBasis = fixture.Observer is null
            ? "unknown"
            : DifferencePacketBuilder.FormatObserverBasis(fixture.Observer);
        return new ArtifactManifest
        {
            DifferencePacketJson = emitDifference ? "difference_packet.json" : null,
            DifferenceSummaryMarkdown = emitDifference ? "difference_summary.md" : null,
            RetainedChannels =
            [
                new RetainedChannelManifest
                {
                    ChannelId = "bend_magnitude_metric",
                    Kind = "measurement",
                    Producer = "core",
                    Source = "RayMetric.BendMagnitude",
                    FixtureIdentity = result.FixtureName,
                    FixtureComparisonIdentity = fixture.ComparisonIdentity ?? result.FixtureName,
                    ObserverBasis = observerBasis,
                    Representation = "scalar_grid",
                    ArtifactPath = "snapshot_heatmap.csv",
                    GridWidth = result.Width,
                    GridHeight = result.Height,
                },
                new RetainedChannelManifest
                {
                    ChannelId = "traversal_step_count",
                    Kind = "measurement",
                    Producer = "core",
                    Source = "MetricRayState.IntegrationSteps",
                    FixtureIdentity = result.FixtureName,
                    FixtureComparisonIdentity = fixture.ComparisonIdentity ?? result.FixtureName,
                    ObserverBasis = observerBasis,
                    Representation = "scalar_grid",
                    ArtifactPath = "traversal_step_count.csv",
                    GridWidth = result.Width,
                    GridHeight = result.Height,
                },
            ],
        };
    }
}

public sealed record RetainedChannelManifest
{
    public string ChannelId { get; init; } = "unknown";
    public string Kind { get; init; } = "measurement";
    public string Producer { get; init; } = "core";
    public string Source { get; init; } = "";
    public string FixtureIdentity { get; init; } = "";
    public string FixtureComparisonIdentity { get; init; } = "";
    public string ObserverBasis { get; init; } = "unknown";
    public string Representation { get; init; } = "unknown";
    public string ArtifactPath { get; init; } = "";
    public int GridWidth { get; init; }
    public int GridHeight { get; init; }
}
