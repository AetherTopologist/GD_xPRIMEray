using System.Text.Json;

namespace XPrimeRay.Testbench.Cli.Output;

public static class ObservatoryEntryWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static void Write(string outputDirectory, RunManifest manifest)
    {
        var outputPath = Path.GetFullPath(outputDirectory);
        var manifestPath = Path.Combine(outputPath, "manifest.json");
        var manifestSourcePath = Path.GetRelativePath(Environment.CurrentDirectory, manifestPath).Replace('\\', '/');
        var entry = ObservatoryEntry.FromManifest(manifest, manifestSourcePath);
        var json = JsonSerializer.Serialize(entry, JsonOptions);
        File.WriteAllText(Path.Combine(outputPath, "observatory_entry.json"), json + Environment.NewLine);
    }
}
