using System.Globalization;
using XPrimeRay.Core.Fixtures;

namespace XPrimeRay.Core.Comparison;

public static class ComparisonStatuses
{
    public const string Comparable = "COMPARABLE";
    public const string NotComparable = "NOT_COMPARABLE";
    public const string Unknown = "UNKNOWN";

    public static bool IsSupported(string value)
    {
        return value is Comparable or NotComparable or Unknown;
    }
}

public sealed record DifferencePacket
{
    public string Schema { get; init; } = "xprimeray.glowing_heart.difference_packet.v2.0";
    public string Version { get; init; } = "v2.0";
    public string GeneratedUtc { get; init; } = "";
    public bool RuntimeExecuted { get; init; }
    public string ParityClaim { get; init; } = "NONE";
    public string Status { get; init; } = ComparisonStatuses.Unknown;
    public DifferenceObservation Left { get; init; } = new();
    public DifferenceObservation Right { get; init; } = new();
    public DifferenceComparison Comparison { get; init; } = new();
    public string[] Limitations { get; init; } = Array.Empty<string>();

    public static DifferencePacket CreateCoreIdentityPacket(
        FixtureDefinition fixture,
        string fixturePath,
        string runId,
        DateTimeOffset generatedUtc)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        if (fixture.Observer is null)
        {
            throw new ArgumentException("A difference packet requires fixture observer metadata.", nameof(fixture));
        }

        var observation = new DifferenceObservation
        {
            ObserverIdentity = new ObserverIdentity
            {
                Id = $"{fixture.Name}.observer",
                Producer = "core",
                Description = DescribeObserver(fixture.Observer),
            },
            FixtureIdentity = new FixtureIdentity
            {
                Id = fixture.Name,
                SourcePath = fixturePath,
            },
            SnapshotIdentity = new SnapshotIdentity
            {
                Id = $"{runId}:snapshot_heatmap.csv",
                ArtifactPath = "snapshot_heatmap.csv",
            },
            MeasurementChannel = "bend_magnitude_metric",
            RepresentationType = "scalar_grid",
        };

        return new DifferencePacket
        {
            GeneratedUtc = generatedUtc.UtcDateTime.ToString(
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.InvariantCulture),
            Status = ComparisonStatuses.Comparable,
            Left = observation with { Role = "core_reference" },
            Right = observation with { Role = "core_candidate" },
            Comparison = new DifferenceComparison
            {
                Basis = "identity_metadata_only",
                TransformRequired = false,
                Comparable = true,
                ImageComparisonPerformed = false,
                Reason = "Both observations identify the same Core fixture, observer, bend-magnitude measurement channel, and scalar-grid representation. This packet declares semantic comparability only; it does not compare sample or image values.",
            },
            Limitations =
            [
                "Core versus Core identity packet only",
                "No sample comparison was performed",
                "No image comparison was performed",
                "Godot runtime was not executed",
                "No parity claim",
            ],
        };
    }

    public void Validate()
    {
        if (!ComparisonStatuses.IsSupported(Status))
        {
            throw new InvalidDataException($"Unsupported comparison status '{Status}'.");
        }

        if (Status == ComparisonStatuses.Comparable && !Comparison.Comparable)
        {
            throw new InvalidDataException("COMPARABLE status requires comparable=true.");
        }

        if (Status == ComparisonStatuses.NotComparable && Comparison.Comparable)
        {
            throw new InvalidDataException("NOT_COMPARABLE status requires comparable=false.");
        }

        if (Comparison.TransformRequired && string.IsNullOrWhiteSpace(Comparison.TransformReference))
        {
            throw new InvalidDataException("A required transform must include a transform reference.");
        }

        if (ParityClaim != "NONE")
        {
            throw new InvalidDataException("Difference packets must preserve parityClaim=NONE.");
        }

        if (RuntimeExecuted)
        {
            throw new InvalidDataException("This Difference Packet version requires runtimeExecuted=false.");
        }
    }

    private static string DescribeObserver(ObserverDefinition observer)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"origin={FormatVector(observer.Origin)};forward={FormatVector(observer.Forward)};up={FormatVector(observer.Up)};fovDegrees={observer.FovDegrees:G9}");
    }

    private static string FormatVector(IReadOnlyList<float> values)
    {
        return "[" + string.Join(",", values.Select(value => value.ToString("G9", CultureInfo.InvariantCulture))) + "]";
    }
}

public sealed record DifferenceObservation
{
    public string Role { get; init; } = "";
    public ObserverIdentity ObserverIdentity { get; init; } = new();
    public FixtureIdentity FixtureIdentity { get; init; } = new();
    public SnapshotIdentity SnapshotIdentity { get; init; } = new();
    public string MeasurementChannel { get; init; } = "unknown";
    public string RepresentationType { get; init; } = "unknown";
}

public sealed record ObserverIdentity
{
    public string Id { get; init; } = "";
    public string Producer { get; init; } = "unknown";
    public string Description { get; init; } = "";
}

public sealed record FixtureIdentity
{
    public string Id { get; init; } = "";
    public string SourcePath { get; init; } = "";
}

public sealed record SnapshotIdentity
{
    public string Id { get; init; } = "";
    public string ArtifactPath { get; init; } = "";
}

public sealed record DifferenceComparison
{
    public string Basis { get; init; } = "unknown";
    public bool TransformRequired { get; init; }
    public string? TransformReference { get; init; }
    public bool Comparable { get; init; }
    public bool ImageComparisonPerformed { get; init; }
    public string Reason { get; init; } = "";
}
