using System.Globalization;
using System.Text;
using XPrimeRay.Core.Transport;

namespace XPrimeRay.Testbench.Cli.Output;

public static class AsciiSnapshotWriter
{
    private const string Ramp = " .:-=+*#%@";

    public static void Write(string outputDirectory, TransportResult result)
    {
        var path = Path.Combine(outputDirectory, "snapshot_ascii.txt");
        var pixels = BuildPixelGrid(result);
        var maxBend = SnapshotScale.MaxBend(result.RayMetrics);
        var text = new StringBuilder();

        text.AppendLine("xPRIMEray-Core v0.6 snapshot_ascii");
        text.AppendLine($"fixture={result.FixtureName}");
        text.AppendLine("metric=bend_magnitude");
        text.AppendLine($"width={result.Width}");
        text.AppendLine($"height={result.Height}");
        text.AppendLine($"max_bend={maxBend.ToString("G9", CultureInfo.InvariantCulture)}");
        text.AppendLine();

        for (var y = 0; y < result.Height; y++)
        {
            for (var x = 0; x < result.Width; x++)
            {
                var normalized = SnapshotScale.Normalize(pixels[y, x], maxBend);
                var index = (int)Math.Round(normalized * (Ramp.Length - 1));
                text.Append(Ramp[Math.Clamp(index, 0, Ramp.Length - 1)]);
            }

            text.AppendLine();
        }

        File.WriteAllText(path, text.ToString());
    }

    private static double[,] BuildPixelGrid(TransportResult result)
    {
        var pixels = new double[result.Height, result.Width];
        foreach (var metric in result.RayMetrics)
        {
            if (metric.X >= 0 && metric.Y >= 0 && metric.X < result.Width && metric.Y < result.Height)
            {
                pixels[metric.Y, metric.X] = metric.BendMagnitude;
            }
        }

        return pixels;
    }
}
