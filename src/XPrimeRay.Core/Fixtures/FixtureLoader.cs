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

        if (fixture.Validation.MaxMisses < 0)
        {
            throw new InvalidDataException("Fixture validation maxMisses cannot be negative.");
        }
    }
}
