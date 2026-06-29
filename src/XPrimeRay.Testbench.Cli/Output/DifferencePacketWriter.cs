using System.Text.Json;
using XPrimeRay.Core.Comparison;

namespace XPrimeRay.Testbench.Cli.Output;

public static class DifferencePacketWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static void Write(string outputDirectory, DifferencePacket packet)
    {
        packet.Validate();

        var json = JsonSerializer.Serialize(packet, JsonOptions);
        File.WriteAllText(
            Path.Combine(outputDirectory, "difference_packet.json"),
            json + Environment.NewLine);

        var summary = DifferenceSummary.FromPacket(packet);
        File.WriteAllText(
            Path.Combine(outputDirectory, "difference_summary.md"),
            summary.ToMarkdown(packet));
    }
}
