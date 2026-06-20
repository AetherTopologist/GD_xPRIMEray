using System.Globalization;
using System.Text;
using XPrimeRay.Core.Transport;
using XPrimeRay.Core.Validation;

namespace XPrimeRay.Testbench.Cli.Output;

public static class MarkdownSummaryWriter
{
    public static void Write(string outputDirectory, TransportResult result, ValidationReport report)
    {
        var path = Path.Combine(outputDirectory, "run_summary.md");
        var markdown = new StringBuilder();
        markdown.AppendLine("# xPRIMEray-Core Testbench Run");
        markdown.AppendLine();
        markdown.AppendLine("## Fixture");
        markdown.AppendLine();
        markdown.AppendLine($"- Name: {result.FixtureName}");
        markdown.AppendLine($"- Mode: {result.Mode}");
        markdown.AppendLine($"- Resolution: {result.Width}x{result.Height}");
        markdown.AppendLine();
        markdown.AppendLine("## Result");
        markdown.AppendLine();
        markdown.AppendLine($"- Validation: {report.Verdict}");
        markdown.AppendLine($"- Rays: {result.Rays}");
        markdown.AppendLine($"- Hits: {result.Hits}");
        markdown.AppendLine($"- Misses: {result.Misses}");
        markdown.AppendLine($"- Steps per ray: {result.StepsPerRay}");
        markdown.AppendLine($"- Field samples: {result.FieldSampleCount}");
        markdown.AppendLine($"- Mean bend: {FormatFloat(result.MeanBendMagnitude)}");
        markdown.AppendLine($"- Max bend: {FormatFloat(result.MaxBendMagnitude)}");
        markdown.AppendLine();
        markdown.AppendLine("## Interpretation");
        markdown.AppendLine();
        markdown.AppendLine("This run is a Project Glowing Heart v0.3 observable output artifact. It proves the Core CLI can emit portable evidence from a deterministic field-driven smoke fixture without launching Godot.");
        markdown.AppendLine();
        markdown.AppendLine("## Limitations");
        markdown.AppendLine();
        markdown.AppendLine("- Godot parity is not claimed.");
        markdown.AppendLine("- Hermetic closure is not claimed.");
        markdown.AppendLine("- Collision behavior is not modeled.");
        markdown.AppendLine("- Portal behavior is not modeled.");

        File.WriteAllText(path, markdown.ToString());
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }
}
