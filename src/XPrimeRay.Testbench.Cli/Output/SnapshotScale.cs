using XPrimeRay.Core.Transport;

namespace XPrimeRay.Testbench.Cli.Output;

internal static class SnapshotScale
{
    public static double MaxBend(IReadOnlyList<RayMetric> metrics)
    {
        var max = 0.0;
        foreach (var metric in metrics)
        {
            if (double.IsFinite(metric.BendMagnitude))
            {
                max = Math.Max(max, Math.Max(0.0, metric.BendMagnitude));
            }
        }

        return max;
    }

    public static double Normalize(double bendMagnitude, double maxBend)
    {
        if (maxBend <= 0.0 || !double.IsFinite(maxBend) || !double.IsFinite(bendMagnitude))
        {
            return 0.0;
        }

        return Math.Clamp(bendMagnitude / maxBend, 0.0, 1.0);
    }
}
