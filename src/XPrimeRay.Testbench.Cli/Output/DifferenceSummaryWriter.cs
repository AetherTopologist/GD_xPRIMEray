using System.Globalization;
using System.Text;
using XPrimeRay.Core.Comparison;

namespace XPrimeRay.Testbench.Cli.Output;

public static class DifferenceSummaryWriter
{
    public static void Write(string outputDirectory, DifferencePacket packet)
    {
        packet.Validate();
        var summary = packet.Summary;
        var markdown = new StringBuilder();
        markdown.AppendLine("# Project Glowing Heart Difference Summary");
        markdown.AppendLine();
        markdown.AppendLine("Runtime executed: false");
        markdown.AppendLine();
        markdown.AppendLine("Parity claim: NONE");
        markdown.AppendLine();
        markdown.AppendLine("## Comparison");
        markdown.AppendLine();
        markdown.AppendLine($"- Mode: `{packet.ComparisonMode}`");
        markdown.AppendLine($"- Status: `{packet.Status}`");
        markdown.AppendLine($"- Left snapshot: `{packet.LeftSnapshot.Id}`");
        markdown.AppendLine($"- Right snapshot: `{packet.RightSnapshot.Id}`");
        markdown.AppendLine($"- Left channel: `{packet.LeftChannel}`");
        markdown.AppendLine($"- Right channel: `{packet.RightChannel}`");
        markdown.AppendLine($"- Observer basis: `{packet.ObserverBasis}`");
        markdown.AppendLine($"- Comparison basis: `{packet.ComparisonBasis}`");
        markdown.AppendLine($"- Compatibility rule: `{packet.CompatibilityRuleId ?? "unknown"}`");
        markdown.AppendLine($"- Channel registry version: `{packet.ChannelRegistryVersion ?? "unknown"}`");
        markdown.AppendLine($"- Compatibility matrix version: `{packet.CompatibilityMatrixVersion ?? "unknown"}`");
        markdown.AppendLine($"- Transform required: `{FormatBool(packet.TransformRequired)}`");
        markdown.AppendLine($"- Reason: {packet.Reason}");
        markdown.AppendLine();
        markdown.AppendLine("## Difference Metrics");
        markdown.AppendLine();
        markdown.AppendLine($"- Count compared: {summary.CountCompared}");
        markdown.AppendLine($"- Maximum absolute difference: {FormatDouble(summary.MaxAbsDifference)}");
        markdown.AppendLine($"- Mean absolute difference: {FormatDouble(summary.MeanAbsDifference)}");
        markdown.AppendLine($"- Non-zero count: {summary.NonZeroCount}");
        markdown.AppendLine();
        markdown.AppendLine("## Claim Boundary");
        markdown.AppendLine();
        foreach (var boundary in packet.ClaimBoundary)
        {
            markdown.AppendLine(boundary);
        }

        File.WriteAllText(
            Path.Combine(outputDirectory, "difference_summary.md"),
            markdown.ToString());
    }

    private static string FormatBool(bool value)
    {
        return value ? "true" : "false";
    }

    private static string FormatDouble(double value)
    {
        return value.ToString("G17", CultureInfo.InvariantCulture);
    }
}
