using System.Globalization;
using System.Text.Json;
using XPrimeRay.Core.Comparison;
using XPrimeRay.Core.Transport;

namespace XPrimeRay.Testbench.Cli.Output;

public static class RetainedPacketLoader
{
    public static RetainedSnapshotDescriptor Load(string packetDirectory, string channelId = "bend_magnitude_metric")
    {
        var root = Path.GetFullPath(packetDirectory);
        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException($"Retained packet directory was not found: {packetDirectory}");
        }

        var manifestPath = Path.Combine(root, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("Retained packet manifest was not found.", manifestPath);
        }

        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var rootElement = manifest.RootElement;
        var fixture = RequiredObject(rootElement, "fixture", manifestPath);
        var artifacts = RequiredObject(rootElement, "artifacts", manifestPath);
        var channel = ResolveChannel(artifacts, channelId, manifestPath);
        var snapshotName = channel.ArtifactPath;
        var snapshotPath = Path.Combine(root, snapshotName);
        if (!File.Exists(snapshotPath))
        {
            throw new FileNotFoundException("Retained scalar-grid snapshot was not found.", snapshotPath);
        }

        return new RetainedSnapshotDescriptor
        {
            RunId = RequiredString(rootElement, "runId", manifestPath),
            ManifestPath = manifestPath.Replace('\\', '/'),
            SnapshotPath = snapshotPath.Replace('\\', '/'),
            FixtureId = RequiredString(fixture, "name", manifestPath),
            FixtureComparisonIdentity = OptionalString(fixture, "comparisonIdentity")
                ?? RequiredString(fixture, "name", manifestPath),
            FixturePath = RequiredString(fixture, "path", manifestPath),
            ObserverBasis = RequiredString(fixture, "observerBasis", manifestPath),
            ChannelId = channel.ChannelId,
            Representation = channel.Representation,
            Samples = LoadSamples(snapshotPath, channel.ChannelId),
        };
    }

    private static RetainedChannel ResolveChannel(JsonElement artifacts, string channelId, string manifestPath)
    {
        if (artifacts.TryGetProperty("retainedChannels", out var channels)
            && channels.ValueKind == JsonValueKind.Array)
        {
            foreach (var channel in channels.EnumerateArray())
            {
                if (OptionalString(channel, "channelId") == channelId)
                {
                    return new RetainedChannel(
                        channelId,
                        RequiredString(channel, "representation", manifestPath),
                        RequiredString(channel, "artifactPath", manifestPath));
                }
            }

            throw new InvalidDataException($"{manifestPath}: retained channel '{channelId}' is not declared");
        }

        var legacyChannel = RequiredString(artifacts, "snapshotChannel", manifestPath);
        if (legacyChannel != channelId)
        {
            throw new InvalidDataException($"{manifestPath}: retained channel '{channelId}' is not declared");
        }

        return new RetainedChannel(
            legacyChannel,
            RequiredString(artifacts, "snapshotRepresentation", manifestPath),
            RequiredString(artifacts, "snapshotHeatmapCsv", manifestPath));
    }

    private static IReadOnlyList<RayMetric> LoadSamples(string path, string channelId)
    {
        var lines = File.ReadAllLines(path);
        var isBend = channelId == "bend_magnitude_metric";
        var expectedHeader = isBend ? "x,y,bend_magnitude,normalized_intensity" : "x,y,value";
        if (lines.Length == 0 || lines[0] != expectedHeader)
        {
            throw new InvalidDataException($"{path}: unsupported scalar-grid header");
        }

        var samples = new List<RayMetric>(Math.Max(0, lines.Length - 1));
        var coordinates = new HashSet<(int X, int Y)>();
        for (var index = 1; index < lines.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(lines[index]))
            {
                continue;
            }

            var columns = lines[index].Split(',');
            var expectedColumns = isBend ? 4 : 3;
            if (columns.Length != expectedColumns
                || !int.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x)
                || !int.TryParse(columns[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y)
                || !double.TryParse(columns[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var bend)
                || !double.IsFinite(bend))
            {
                throw new InvalidDataException($"{path}:{index + 1}: invalid scalar-grid row");
            }

            if (!coordinates.Add((x, y)))
            {
                throw new InvalidDataException($"{path}:{index + 1}: duplicate coordinate ({x},{y})");
            }

            samples.Add(new RayMetric(x, y, bend));
        }

        return samples;
    }

    private static JsonElement RequiredObject(JsonElement parent, string name, string path)
    {
        if (!parent.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException($"{path}: required object '{name}' is missing");
        }

        return value;
    }

    private static string RequiredString(JsonElement parent, string name, string path)
    {
        if (!parent.TryGetProperty(name, out var value)
            || value.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw new InvalidDataException($"{path}: required string '{name}' is missing");
        }

        return value.GetString()!;
    }

    private static string? OptionalString(JsonElement parent, string name)
    {
        return parent.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(value.GetString())
                ? value.GetString()
                : null;
    }

    private sealed record RetainedChannel(string ChannelId, string Representation, string ArtifactPath);
}
