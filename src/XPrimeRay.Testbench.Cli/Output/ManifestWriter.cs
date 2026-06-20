using System.Text;
using System.Text.Json;

namespace XPrimeRay.Testbench.Cli.Output;

public static class ManifestWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static string CreateRunDirectory(string outputRoot, DateTimeOffset timestampUtc, string fixtureName)
    {
        if (string.IsNullOrWhiteSpace(outputRoot))
        {
            throw new ArgumentException("Output root cannot be empty.", nameof(outputRoot));
        }

        var safeFixtureName = SanitizeSegment(fixtureName);
        var stamp = timestampUtc.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'");
        var baseRunId = $"{stamp}_{safeFixtureName}";

        Directory.CreateDirectory(outputRoot);
        for (var attempt = 0; attempt < 1000; attempt++)
        {
            var suffix = attempt == 0 ? "" : $"_{attempt:000}";
            var candidate = Path.Combine(outputRoot, baseRunId + suffix);
            if (Directory.Exists(candidate))
            {
                continue;
            }

            Directory.CreateDirectory(candidate);
            return candidate;
        }

        throw new IOException($"Could not create a unique run directory under '{outputRoot}'.");
    }

    public static void Write(string outputDirectory, RunManifest manifest)
    {
        var path = Path.Combine(outputDirectory, "manifest.json");
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        File.WriteAllText(path, json + Environment.NewLine);
    }

    private static string SanitizeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "fixture";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value.ToLowerInvariant())
        {
            if ((ch >= 'a' && ch <= 'z')
                || (ch >= '0' && ch <= '9')
                || ch == '_'
                || ch == '-')
            {
                builder.Append(ch);
            }
            else
            {
                builder.Append('_');
            }
        }

        var sanitized = builder.ToString().Trim('_');
        return sanitized.Length == 0 ? "fixture" : sanitized;
    }
}
