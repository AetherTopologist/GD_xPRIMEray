using XPrimeRay.Core.Comparison;

namespace XPrimeRay.Testbench.Cli.Output;

public static class RetainedComparisonWriter
{
    public static string Write(
        string outputDirectory,
        string leftPacketDirectory,
        string rightPacketDirectory,
        DifferencePacket packet)
    {
        var output = Path.GetFullPath(outputDirectory);
        var left = Path.GetFullPath(leftPacketDirectory);
        var right = Path.GetFullPath(rightPacketDirectory);
        if (output == left || output == right)
        {
            throw new InvalidDataException("Comparison output must not be either retained input packet directory.");
        }

        Directory.CreateDirectory(output);
        DifferencePacketWriter.Write(output, packet);
        DifferenceSummaryWriter.Write(output, packet);
        return output;
    }
}
