using System.Text.Json.Serialization;

namespace XPrimeRay.Testbench.Cli.Output;

public sealed record ObservatoryEntry
{
    [JsonPropertyName("category")]
    public string Category { get; init; } = "Experimental";

    [JsonPropertyName("fixture")]
    public string Fixture { get; init; } = "";

    [JsonPropertyName("run_id")]
    public string RunId { get; init; } = "";

    [JsonPropertyName("artifact_type")]
    public string ArtifactType { get; init; } = "glowing_heart_core_smoke";

    [JsonPropertyName("coverage")]
    public string Coverage { get; init; } = "";

    [JsonPropertyName("closure")]
    public string Closure { get; init; } = "MISSING";

    [JsonPropertyName("verdict")]
    public string Verdict { get; init; } = "";

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; init; } = "";

    [JsonPropertyName("source_path")]
    public string SourcePath { get; init; } = "";

    [JsonPropertyName("source")]
    public string Source { get; init; } = "cli";

    [JsonPropertyName("phase")]
    public string Phase { get; init; } = "Project Glowing Heart v0.4";

    public static ObservatoryEntry FromManifest(RunManifest manifest, string manifestSourcePath)
    {
        var validation = manifest.Result.Validation;
        var observed = validation == "PASS" ? "PASS" : "FAIL";
        return new ObservatoryEntry
        {
            Fixture = manifest.Fixture.Name,
            RunId = manifest.RunId,
            Coverage = observed,
            Verdict = validation == "PASS" ? "OBSERVED" : "FAIL",
            Timestamp = manifest.TimestampUtc,
            SourcePath = manifestSourcePath,
        };
    }
}
