using System.Text;
using XPrimeRay.Core.Transport;

namespace XPrimeRay.Testbench.Cli.Output;

public static class PpmSnapshotWriter
{
    public static void Write(string outputDirectory, TransportResult result)
    {
        var path = Path.Combine(outputDirectory, "snapshot.ppm");
        var pixels = BuildPixelGrid(result);
        var maxBend = SnapshotScale.MaxBend(result.RayMetrics);
        var ppm = new StringBuilder();

        ppm.AppendLine("P3");
        ppm.AppendLine($"{result.Width} {result.Height}");
        ppm.AppendLine("255");

        for (var y = 0; y < result.Height; y++)
        {
            for (var x = 0; x < result.Width; x++)
            {
                var normalized = SnapshotScale.Normalize(pixels[y, x], maxBend);
                var intensity = (int)Math.Round(normalized * 255.0);
                ppm.Append(intensity).Append(' ')
                    .Append(intensity).Append(' ')
                    .Append(intensity);

                if (x < result.Width - 1)
                {
                    ppm.Append(' ');
                }
            }

            ppm.AppendLine();
        }

        File.WriteAllText(path, ppm.ToString());
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
