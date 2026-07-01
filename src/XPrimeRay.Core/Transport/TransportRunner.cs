using XPrimeRay.Core.Fixtures;
using XPrimeRay.Core.Fields;
using System.Numerics;

namespace XPrimeRay.Core.Transport;

public sealed class TransportRunner
{
    public const string ArtifactRunnerNote =
        "v0.1 artifact runner only; full optical transport remains in GD_xPRIMEray harnesses.";

    public const string FirstPulseNote =
        "v0.2 first real Core pulse; Godot parity is not claimed yet.";

    public TransportResult Run(FixtureDefinition fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        return fixture.Transport.Mode switch
        {
            "artifact_placeholder" => TransportResult.FromArtifactFixture(fixture, ArtifactRunnerNote),
            "radial_grin_smoke" => RunRadialGrinSmoke(fixture),
            _ => throw new InvalidDataException($"Unsupported transport mode: {fixture.Transport.Mode}"),
        };
    }

    private static TransportResult RunRadialGrinSmoke(FixtureDefinition fixture)
    {
        var observer = fixture.Observer ?? throw new InvalidDataException("radial_grin_smoke requires an observer.");
        var fields = fixture.Fields.Select(RadialGrinField.FromDefinition).ToArray();
        var width = fixture.RayGrid.Width;
        var height = fixture.RayGrid.Height;
        var rays = checked(width * height);
        var stepsPerRay = fixture.Transport.MaxStepsPerRay;
        var stepSize = fixture.Transport.StepSize;

        var origin = ToVector3(observer.Origin);
        var forward = NormalizeOrFallback(ToVector3(observer.Forward), Vector3.UnitZ);
        var upSeed = NormalizeOrFallback(ToVector3(observer.Up), Vector3.UnitY);
        var right = NormalizeOrFallback(Vector3.Cross(upSeed, forward), Vector3.UnitX);
        var up = NormalizeOrFallback(Vector3.Cross(forward, right), Vector3.UnitY);
        var tanHalfFov = MathF.Tan(DegreesToRadians(observer.FovDegrees) * 0.5f);
        var aspect = width / (float)height;

        var fieldSampleCount = 0;
        var bendSum = 0.0;
        var maxBend = 0f;
        var hasInvalidValues = false;
        var rayMetrics = new List<RayMetric>(rays);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var rayDirection = BuildRayDirection(x, y, width, height, aspect, tanHalfFov, forward, right, up);
                var state = MetricRayState.Initialize(origin, rayDirection, stepSize);
                var rayBendSum = 0.0;

                for (var step = 0; step < stepsPerRay; step++)
                {
                    var bend = Vector3.Zero;
                    foreach (var field in fields)
                    {
                        bend += field.SampleBend(state.Position, state.Direction, stepSize);
                    }

                    fieldSampleCount++;
                    var bendMagnitude = bend.Length();
                    if (!float.IsFinite(bendMagnitude) || !IsFinite(bend))
                    {
                        hasInvalidValues = true;
                        bend = Vector3.Zero;
                        bendMagnitude = 0f;
                    }

                    bendSum += bendMagnitude;
                    rayBendSum += bendMagnitude;
                    maxBend = MathF.Max(maxBend, bendMagnitude);

                    var newDirection = NormalizeOrFallback(state.Direction + bend, state.Direction);
                    var newPosition = state.Position + (newDirection * stepSize);
                    if (!IsFinite(newDirection) || !IsFinite(newPosition))
                    {
                        hasInvalidValues = true;
                        newDirection = state.Direction;
                        newPosition = state.Position;
                    }

                    state.Direction = newDirection;
                    state.Position = newPosition;
                    state.PathLength += stepSize;
                    state.AffineParameter += stepSize;
                    state.StepLast = stepSize;
                    state.IntegrationSteps++;
                    state.ResetFrameFromDirection();
                }

                var rayMeanBend = stepsPerRay > 0 ? rayBendSum / stepsPerRay : 0.0;
                if (!double.IsFinite(rayMeanBend))
                {
                    hasInvalidValues = true;
                    rayMeanBend = 0.0;
                }

                rayMetrics.Add(new RayMetric(x, y, rayMeanBend, state.IntegrationSteps));
            }
        }

        var meanBend = fieldSampleCount > 0 ? (float)(bendSum / fieldSampleCount) : 0f;
        if (!float.IsFinite(meanBend) || !float.IsFinite(maxBend))
        {
            hasInvalidValues = true;
            meanBend = 0f;
            maxBend = 0f;
        }

        return new TransportResult
        {
            FixtureName = fixture.Name,
            Mode = fixture.Transport.Mode,
            Width = width,
            Height = height,
            Rays = rays,
            Hits = 0,
            Misses = rays,
            MaxStepsPerRay = stepsPerRay,
            StepsPerRay = stepsPerRay,
            FieldSampleCount = fieldSampleCount,
            MeanBendMagnitude = meanBend,
            MaxBendMagnitude = maxBend,
            HasInvalidValues = hasInvalidValues,
            Note = FirstPulseNote,
            RayMetrics = rayMetrics,
        };
    }

    private static Vector3 BuildRayDirection(
        int x,
        int y,
        int width,
        int height,
        float aspect,
        float tanHalfFov,
        Vector3 forward,
        Vector3 right,
        Vector3 up)
    {
        var ndcX = ((((x + 0.5f) / width) * 2f) - 1f) * aspect * tanHalfFov;
        var ndcY = (1f - (((y + 0.5f) / height) * 2f)) * tanHalfFov;
        return NormalizeOrFallback(forward + (right * ndcX) + (up * ndcY), forward);
    }

    private static Vector3 ToVector3(float[] values)
    {
        return new Vector3(values[0], values[1], values[2]);
    }

    private static float DegreesToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180f);
    }

    private static Vector3 NormalizeOrFallback(Vector3 value, Vector3 fallback)
    {
        var lengthSq = value.LengthSquared();
        if (lengthSq <= 1e-20f || !float.IsFinite(lengthSq))
        {
            return fallback;
        }

        return value / MathF.Sqrt(lengthSq);
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
    }
}
