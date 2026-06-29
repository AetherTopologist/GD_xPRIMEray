using XPrimeRay.Core.Comparison;
using XPrimeRay.Core.Fixtures;
using XPrimeRay.Core.Transport;
using XPrimeRay.Core.Validation;
using XPrimeRay.Testbench.Cli.Output;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args[0] is "--help" or "-h")
    {
        PrintHelp();
        return 0;
    }

    if (!TryParseArgs(args, out var options, out var parseError))
    {
        Console.Error.WriteLine(parseError);
        PrintHelp();
        return 1;
    }

    try
    {
        var fixture = FixtureLoader.Load(options.FixturePath);
        var runner = new TransportRunner();
        var result = runner.Run(fixture);
        var report = ClosureValidator.Validate(fixture, result);

        PrintSummary(result, report);
        if (options.OutputEnabled)
        {
            var outputPath = WriteArtifacts(options, fixture, result, report);
            Console.WriteLine($"Artifacts: {NormalizePath(outputPath)}");
            Console.WriteLine($"Snapshot: {NormalizePath(Path.Combine(outputPath, "snapshot.ppm"))}");
        }

        return report.Passed ? 0 : 2;
    }
    catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException or OverflowException or System.Text.Json.JsonException)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        return 1;
    }
}

static void PrintHelp()
{
    Console.WriteLine("xPRIMEray-Core Testbench v0.2");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture <fixture.json> [--output <path>] [--no-output]");
}

static void PrintSummary(TransportResult result, ValidationReport report)
{
    Console.WriteLine("xPRIMEray-Core Testbench v0.2");
    Console.WriteLine($"Fixture: {result.FixtureName}");
    Console.WriteLine($"Mode: {result.Mode}");
    Console.WriteLine($"Resolution: {result.Width}x{result.Height}");
    Console.WriteLine($"Rays: {result.Rays}");
    if (result.Mode == "radial_grin_smoke")
    {
        Console.WriteLine($"Steps/Ray: {result.StepsPerRay}");
        Console.WriteLine($"Field Samples: {result.FieldSampleCount}");
        Console.WriteLine($"Mean Bend: {FormatFloat(result.MeanBendMagnitude)}");
        Console.WriteLine($"Max Bend: {FormatFloat(result.MaxBendMagnitude)}");
    }

    Console.WriteLine($"Hits: {result.Hits}");
    Console.WriteLine($"Misses: {result.Misses}");
    Console.WriteLine($"Validation: {report.Verdict}");
    if (!report.Passed && !string.IsNullOrWhiteSpace(report.Reason))
    {
        Console.WriteLine($"Reason: {report.Reason}");
    }

    Console.WriteLine($"Note: {result.Note}");
}

static bool TryParseArgs(string[] args, out RunOptions options, out string error)
{
    options = new RunOptions();
    error = "";

    if (args.Length < 2 || args[0] != "run-fixture")
    {
        error = "Invalid arguments.";
        return false;
    }

    options = options with { FixturePath = args[1] };
    var sawOutput = false;
    var sawNoOutput = false;

    for (var i = 2; i < args.Length; i++)
    {
        var arg = args[i];
        switch (arg)
        {
            case "--no-output":
                if (sawNoOutput)
                {
                    error = "Invalid arguments: --no-output was specified more than once.";
                    return false;
                }

                if (sawOutput)
                {
                    error = "Invalid arguments: --output cannot be combined with --no-output.";
                    return false;
                }

                sawNoOutput = true;
                options = options with { OutputEnabled = false };
                break;

            case "--output":
                if (sawOutput)
                {
                    error = "Invalid arguments: --output was specified more than once.";
                    return false;
                }

                if (sawNoOutput)
                {
                    error = "Invalid arguments: --output cannot be combined with --no-output.";
                    return false;
                }

                if (i + 1 >= args.Length || string.IsNullOrWhiteSpace(args[i + 1]))
                {
                    error = "Invalid arguments: --output requires a path.";
                    return false;
                }

                sawOutput = true;
                options = options with { OutputRoot = args[++i] };
                break;

            default:
                error = $"Invalid arguments: unknown option '{arg}'.";
                return false;
        }
    }

    return true;
}

static string WriteArtifacts(
    RunOptions options,
    FixtureDefinition fixture,
    TransportResult result,
    ValidationReport report)
{
    var runStamp = DateTimeOffset.UtcNow;
    var outputDirectory = ManifestWriter.CreateRunDirectory(
        options.OutputRoot,
        runStamp,
        result.FixtureName);
    var runId = Path.GetFileName(outputDirectory);
    var fixturePath = NormalizePath(Path.IsPathRooted(options.FixturePath)
        ? Path.GetRelativePath(Environment.CurrentDirectory, options.FixturePath)
        : options.FixturePath);

    var manifest = RunManifest.Create(
        runId,
        runStamp,
        fixturePath,
        result,
        report);

    ManifestWriter.Write(outputDirectory, manifest);
    CsvReportWriter.Write(outputDirectory, result, report);
    MarkdownSummaryWriter.Write(outputDirectory, result, report);
    ObservatoryEntryWriter.Write(outputDirectory, manifest);
    PpmSnapshotWriter.Write(outputDirectory, result);
    HeatmapCsvWriter.Write(outputDirectory, result);
    AsciiSnapshotWriter.Write(outputDirectory, result);
    var differencePacket = DifferencePacket.CreateCoreIdentityPacket(
        fixture,
        fixturePath,
        runId,
        runStamp);
    DifferencePacketWriter.Write(outputDirectory, differencePacket);
    return outputDirectory;
}

static string FormatFloat(float value)
{
    return value.ToString("G9", System.Globalization.CultureInfo.InvariantCulture);
}

static string NormalizePath(string path)
{
    return path.Replace('\\', '/');
}

internal sealed record RunOptions
{
    public string FixturePath { get; init; } = "";
    public bool OutputEnabled { get; init; } = true;
    public string OutputRoot { get; init; } = "output/glowing_heart";
}
