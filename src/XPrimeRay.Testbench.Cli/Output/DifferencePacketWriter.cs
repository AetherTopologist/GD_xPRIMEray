using System.Text.Json;
using System.Text.Json.Serialization;
using XPrimeRay.Core.Comparison;

namespace XPrimeRay.Testbench.Cli.Output;

public static class DifferencePacketWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static void Write(string outputDirectory, DifferencePacket packet)
    {
        packet.Validate();

        var json = JsonSerializer.Serialize(packet, JsonOptions);
        File.WriteAllText(
            Path.Combine(outputDirectory, "difference_packet.json"),
            json + Environment.NewLine);

    }
}
