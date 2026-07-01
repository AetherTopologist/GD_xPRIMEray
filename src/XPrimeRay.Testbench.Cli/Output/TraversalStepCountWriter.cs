using System.Globalization;
using System.Text;
using XPrimeRay.Core.Transport;

namespace XPrimeRay.Testbench.Cli.Output;

public static class TraversalStepCountWriter
{
    public const string FileName = "traversal_step_count.csv";

    public static void Write(string outputDirectory, TransportResult result)
    {
        var csv = new StringBuilder("x,y,value\n");
        foreach (var metric in result.RayMetrics.OrderBy(metric => metric.Y).ThenBy(metric => metric.X))
        {
            csv.Append(metric.X.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(metric.Y.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(metric.IntegrationSteps.ToString(CultureInfo.InvariantCulture)).AppendLine();
        }

        File.WriteAllText(Path.Combine(outputDirectory, FileName), csv.ToString());
    }
}
