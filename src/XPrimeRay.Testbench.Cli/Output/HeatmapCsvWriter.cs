using System.Globalization;
using System.Text;
using XPrimeRay.Core.Transport;

namespace XPrimeRay.Testbench.Cli.Output;

public static class HeatmapCsvWriter
{
    public static void Write(string outputDirectory, TransportResult result)
    {
        var path = Path.Combine(outputDirectory, "snapshot_heatmap.csv");
        var metrics = result.RayMetrics
            .OrderBy(metric => metric.Y)
            .ThenBy(metric => metric.X)
            .ToArray();
        var maxBend = SnapshotScale.MaxBend(metrics);
        var csv = new StringBuilder();
        csv.AppendLine("x,y,bend_magnitude,normalized_intensity");

        foreach (var metric in metrics)
        {
            csv.Append(metric.X).Append(',')
                .Append(metric.Y).Append(',')
                .Append(FormatDouble(metric.BendMagnitude)).Append(',')
                .Append(FormatDouble(SnapshotScale.Normalize(metric.BendMagnitude, maxBend)))
                .AppendLine();
        }

        File.WriteAllText(path, csv.ToString());
    }

    private static string FormatDouble(double value)
    {
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }
}
