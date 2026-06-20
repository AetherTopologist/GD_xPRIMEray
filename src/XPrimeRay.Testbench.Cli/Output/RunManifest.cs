using System.Globalization;
using XPrimeRay.Core.Transport;
using XPrimeRay.Core.Validation;

namespace XPrimeRay.Testbench.Cli.Output;

public sealed record RunManifest
{
    public string Schema { get; init; } = "xprimeray.glowing_heart.run_manifest.v0.3";
    public string RunId { get; init; } = "";
    public string TimestampUtc { get; init; } = "";
    public string Source { get; init; } = "cli";
    public string Phase { get; init; } = "Project Glowing Heart v0.3";
    public FixtureManifest Fixture { get; init; } = new();
    public ResultManifest Result { get; init; } = new();
    public ArtifactManifest Artifacts { get; init; } = new();
    public string[] Limitations { get; init; } = Array.Empty<string>();

    public static RunManifest Create(
        string runId,
        DateTimeOffset timestampUtc,
        string fixturePath,
        TransportResult result,
        ValidationReport report)
    {
        return new RunManifest
        {
            RunId = runId,
            TimestampUtc = timestampUtc.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
            Fixture = new FixtureManifest
            {
                Name = result.FixtureName,
                Path = fixturePath,
                Mode = result.Mode,
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
            Artifacts = new ArtifactManifest(),
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
    public string Path { get; init; } = "";
    public string Mode { get; init; } = "";
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
}
