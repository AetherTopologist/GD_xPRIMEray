using XPrimeRay.Core.Fixtures;
using XPrimeRay.Core.Transport;

namespace XPrimeRay.Core.Validation;

public static class ClosureValidator
{
    public static ValidationReport Validate(FixtureDefinition fixture, TransportResult result)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        ArgumentNullException.ThrowIfNull(result);

        if (result.Rays != result.Hits + result.Misses)
        {
            return Fail("Ray accounting mismatch.");
        }

        if (result.Width != fixture.RayGrid.Width || result.Height != fixture.RayGrid.Height)
        {
            return Fail("Result resolution does not match fixture rayGrid.");
        }

        if (result.Misses > fixture.Validation.MaxMisses)
        {
            return Fail($"Miss count {result.Misses} exceeds maxMisses {fixture.Validation.MaxMisses}.");
        }

        if (fixture.Validation.RequireHermeticClosure && result.Misses != 0)
        {
            return Fail("Hermetic closure was required but misses were observed.");
        }

        if (result.FieldSampleCount < fixture.Validation.MinFieldSamples)
        {
            return Fail($"Field sample count {result.FieldSampleCount} is below minFieldSamples {fixture.Validation.MinFieldSamples}.");
        }

        if (result.MeanBendMagnitude < fixture.Validation.MinMeanBend)
        {
            return Fail($"Mean bend {result.MeanBendMagnitude:G9} is below minMeanBend {fixture.Validation.MinMeanBend:G9}.");
        }

        if (result.HasInvalidValues
            || !float.IsFinite(result.MeanBendMagnitude)
            || !float.IsFinite(result.MaxBendMagnitude))
        {
            return Fail("Transport result contained NaN or infinity values.");
        }

        return new ValidationReport
        {
            Passed = true,
            Reason = "Artifact validation passed.",
        };
    }

    private static ValidationReport Fail(string reason)
    {
        return new ValidationReport
        {
            Passed = false,
            Reason = reason,
        };
    }
}
