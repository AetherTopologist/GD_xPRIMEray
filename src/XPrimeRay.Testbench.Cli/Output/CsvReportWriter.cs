using System.Globalization;
using System.Text;
using XPrimeRay.Core.Transport;
using XPrimeRay.Core.Validation;

namespace XPrimeRay.Testbench.Cli.Output;

public static class CsvReportWriter
{
    public static void Write(string outputDirectory, TransportResult result, ValidationReport report)
    {
        var path = Path.Combine(outputDirectory, "ray_metrics.csv");
        var csv = new StringBuilder();
        csv.AppendLine("fixture,mode,width,height,rays,steps_per_ray,field_samples,hits,misses,mean_bend,max_bend,validation");
        csv.Append(Csv(result.FixtureName)).Append(',')
            .Append(Csv(result.Mode)).Append(',')
            .Append(result.Width).Append(',')
            .Append(result.Height).Append(',')
            .Append(result.Rays).Append(',')
            .Append(result.StepsPerRay).Append(',')
            .Append(result.FieldSampleCount).Append(',')
            .Append(result.Hits).Append(',')
            .Append(result.Misses).Append(',')
            .Append(FormatFloat(result.MeanBendMagnitude)).Append(',')
            .Append(FormatFloat(result.MaxBendMagnitude)).Append(',')
            .Append(Csv(report.Verdict)).AppendLine();

        File.WriteAllText(path, csv.ToString());
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }

    private static string Csv(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
        {
            return value;
        }

        return '"' + value.Replace("\"", "\"\"") + '"';
    }
}
