namespace XPrimeRay.Core.Transport;

public readonly record struct RayMetric(
    int X,
    int Y,
    double BendMagnitude,
    int IntegrationSteps = 0
);
