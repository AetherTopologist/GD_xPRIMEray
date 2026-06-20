using System.Text.Json;

namespace XPrimeRay.Core.Fixtures;

public static class FixtureLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static FixtureDefinition Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Fixture path is required.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Fixture file was not found.", path);
        }

        var json = File.ReadAllText(path);
        var fixture = JsonSerializer.Deserialize<FixtureDefinition>(json, JsonOptions)
            ?? throw new InvalidDataException("Fixture JSON was empty or invalid.");

        Validate(fixture);
        return fixture;
    }

    private static void Validate(FixtureDefinition fixture)
    {
        if (string.IsNullOrWhiteSpace(fixture.Name))
        {
            throw new InvalidDataException("Fixture name is required.");
        }

        if (fixture.RayGrid.Width <= 0 || fixture.RayGrid.Height <= 0)
        {
            throw new InvalidDataException("Fixture rayGrid width and height must be positive.");
        }

        if (string.IsNullOrWhiteSpace(fixture.Transport.Mode))
        {
            throw new InvalidDataException("Fixture transport mode is required.");
        }

        if (fixture.Transport.MaxStepsPerRay < 0)
        {
            throw new InvalidDataException("Fixture transport maxStepsPerRay cannot be negative.");
        }

        if (fixture.Transport.Mode == "radial_grin_smoke")
        {
            ValidateRadialGrinSmoke(fixture);
        }

        if (fixture.Validation.MaxMisses < 0)
        {
            throw new InvalidDataException("Fixture validation maxMisses cannot be negative.");
        }
    }

    private static void ValidateRadialGrinSmoke(FixtureDefinition fixture)
    {
        if (fixture.Transport.MaxStepsPerRay <= 0)
        {
            throw new InvalidDataException("radial_grin_smoke requires transport maxStepsPerRay to be positive.");
        }

        if (fixture.Transport.StepSize <= 0f || !float.IsFinite(fixture.Transport.StepSize))
        {
            throw new InvalidDataException("radial_grin_smoke requires a positive finite transport stepSize.");
        }

        if (fixture.Observer == null)
        {
            throw new InvalidDataException("radial_grin_smoke requires an observer block.");
        }

        ValidateVector3(fixture.Observer.Origin, "observer origin");
        ValidateVector3(fixture.Observer.Forward, "observer forward");
        ValidateVector3(fixture.Observer.Up, "observer up");

        if (fixture.Observer.FovDegrees <= 0f || fixture.Observer.FovDegrees >= 179f || !float.IsFinite(fixture.Observer.FovDegrees))
        {
            throw new InvalidDataException("radial_grin_smoke observer fovDegrees must be finite and between 0 and 179.");
        }

        if (fixture.Fields.Count <= 0)
        {
            throw new InvalidDataException("radial_grin_smoke requires at least one field.");
        }

        foreach (var field in fixture.Fields)
        {
            if (field.Type != "grin_radial")
            {
                throw new InvalidDataException($"Unsupported radial_grin_smoke field type: {field.Type}");
            }

            ValidateVector3(field.Center, "field center");
            if (field.RadiusOuter <= 0f || !float.IsFinite(field.RadiusOuter))
            {
                throw new InvalidDataException("grin_radial field radiusOuter must be positive and finite.");
            }

            if (!float.IsFinite(field.Amplitude) || !float.IsFinite(field.Gamma))
            {
                throw new InvalidDataException("grin_radial amplitude and gamma must be finite.");
            }
        }
    }

    private static void ValidateVector3(float[] value, string name)
    {
        if (value.Length != 3)
        {
            throw new InvalidDataException($"{name} must contain exactly 3 numbers.");
        }

        if (!float.IsFinite(value[0]) || !float.IsFinite(value[1]) || !float.IsFinite(value[2]))
        {
            throw new InvalidDataException($"{name} must contain finite numbers.");
        }
    }
}
