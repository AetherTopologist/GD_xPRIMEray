using XPrimeRay.Core.Comparison;
using XPrimeRay.Core.Fixtures;
using XPrimeRay.Core.Transport;
using XPrimeRay.Core.Validation;
using XPrimeRay.Testbench.Cli.Output;

return Dispatch(args);

static int Dispatch(string[] args)
{
    if (args.Length == 0 || args[0] is "--help" or "-h")
    {
        PrintHelp();
        return 0;
    }

    return args[0] == "compare-packets" ? RunComparePackets(args) : RunFixture(args);
}

static int RunFixture(string[] args)
{

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
            var artifactResult = WriteArtifacts(options, fixture, result, report);
            Console.WriteLine($"Artifacts: {NormalizePath(artifactResult.OutputPath)}");
            Console.WriteLine($"Snapshot: {NormalizePath(Path.Combine(artifactResult.OutputPath, "snapshot.ppm"))}");
            if (artifactResult.DifferencePacket is not null)
            {
                PrintDifferenceStatus(artifactResult.DifferencePacket, filesWritten: true);
            }
        }
        else if (options.EmitDifference)
        {
            var fixturePath = NormalizeFixturePath(options.FixturePath);
            var packet = DifferencePacketBuilder.BuildCoreSelfComparison(
                fixture,
                fixturePath,
                $"{result.FixtureName}:in_memory",
                DateTimeOffset.UtcNow,
                result);
            PrintDifferenceStatus(packet, filesWritten: false);
        }

        return report.Passed ? 0 : 2;
    }
    catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException or OverflowException or System.Text.Json.JsonException)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        return 1;
    }
}

static int RunComparePackets(string[] args)
{
    if (!TryParseCompareArgs(args, out var options, out var parseError))
    {
        Console.Error.WriteLine(parseError);
        PrintHelp();
        return 1;
    }

    try
    {
        var left = RetainedPacketLoader.Load(options.LeftPacketDirectory, options.LeftChannel);
        var right = RetainedPacketLoader.Load(options.RightPacketDirectory, options.RightChannel);
        var packet = RetainedSnapshotComparison.Build(
            left,
            right,
            options.LeftChannel,
            options.RightChannel,
            DateTimeOffset.UtcNow);
        var output = RetainedComparisonWriter.Write(
            options.OutputDirectory,
            options.LeftPacketDirectory,
            options.RightPacketDirectory,
            packet);

        Console.WriteLine($"Comparison Artifacts: {NormalizePath(output)}");
        PrintDifferenceStatus(packet, filesWritten: true);
        return 0;
    }
    catch (Exception ex) when (ex is ArgumentException
        or IOException
        or InvalidDataException
        or OverflowException
        or System.Text.Json.JsonException)
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
    Console.WriteLine("  dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture <fixture.json> [--emit-difference] [--output <path>] [--no-output]");
    Console.WriteLine("  dotnet run --project src/XPrimeRay.Testbench.Cli -- compare-packets <left_packet_dir> <right_packet_dir> --channel <channel_id> --output <path>");
    Console.WriteLine("  dotnet run --project src/XPrimeRay.Testbench.Cli -- compare-packets <left_packet_dir> <right_packet_dir> --left-channel <channel_id> --right-channel <channel_id> --output <path>");
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
    var sawEmitDifference = false;

    for (var i = 2; i < args.Length; i++)
    {
        var arg = args[i];
        switch (arg)
        {
            case "--emit-difference":
                if (sawEmitDifference)
                {
                    error = "Invalid arguments: --emit-difference was specified more than once.";
                    return false;
                }

                sawEmitDifference = true;
                options = options with { EmitDifference = true };
                break;

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

static bool TryParseCompareArgs(string[] args, out ComparePacketOptions options, out string error)
{
    options = new ComparePacketOptions();
    error = "";
    if (args.Length < 3 || args[0] != "compare-packets")
    {
        error = "Invalid compare-packets arguments.";
        return false;
    }

    options = options with
    {
        LeftPacketDirectory = args[1],
        RightPacketDirectory = args[2],
    };
    var sawChannel = false;
    var sawLeftChannel = false;
    var sawRightChannel = false;
    var sawOutput = false;
    for (var index = 3; index < args.Length; index++)
    {
        switch (args[index])
        {
            case "--channel":
                if (sawChannel || index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
                {
                    error = "Invalid arguments: --channel requires one value.";
                    return false;
                }

                sawChannel = true;
                var channel = args[++index];
                options = options with { LeftChannel = channel, RightChannel = channel };
                break;

            case "--left-channel":
                if (sawLeftChannel || index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
                {
                    error = "Invalid arguments: --left-channel requires one value.";
                    return false;
                }

                sawLeftChannel = true;
                options = options with { LeftChannel = args[++index] };
                break;

            case "--right-channel":
                if (sawRightChannel || index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
                {
                    error = "Invalid arguments: --right-channel requires one value.";
                    return false;
                }

                sawRightChannel = true;
                options = options with { RightChannel = args[++index] };
                break;

            case "--output":
                if (sawOutput || index + 1 >= args.Length || string.IsNullOrWhiteSpace(args[index + 1]))
                {
                    error = "Invalid arguments: --output requires one value.";
                    return false;
                }

                sawOutput = true;
                options = options with { OutputDirectory = args[++index] };
                break;

            default:
                error = $"Invalid arguments: unknown compare-packets option '{args[index]}'.";
                return false;
        }
    }

    if (sawChannel && (sawLeftChannel || sawRightChannel))
    {
        error = "Invalid arguments: --channel cannot be combined with --left-channel or --right-channel.";
        return false;
    }

    if ((!sawChannel && !(sawLeftChannel && sawRightChannel)) || !sawOutput)
    {
        error = "Invalid arguments: compare-packets requires --channel, or both --left-channel and --right-channel, plus --output.";
        return false;
    }

    return true;
}

static ArtifactWriteResult WriteArtifacts(
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
    var fixturePath = NormalizeFixturePath(options.FixturePath);

    var manifest = RunManifest.Create(
        runId,
        runStamp,
        fixturePath,
        fixture,
        result,
        report,
        options.EmitDifference);

    ManifestWriter.Write(outputDirectory, manifest);
    CsvReportWriter.Write(outputDirectory, result, report);
    MarkdownSummaryWriter.Write(outputDirectory, result, report);
    ObservatoryEntryWriter.Write(outputDirectory, manifest);
    PpmSnapshotWriter.Write(outputDirectory, result);
    HeatmapCsvWriter.Write(outputDirectory, result);
    TraversalStepCountWriter.Write(outputDirectory, result);
    AsciiSnapshotWriter.Write(outputDirectory, result);
    DifferencePacket? differencePacket = null;
    if (options.EmitDifference)
    {
        differencePacket = DifferencePacketBuilder.BuildCoreSelfComparison(
            fixture,
            fixturePath,
            $"{runId}:snapshot_heatmap.csv",
            runStamp,
            result);
        DifferencePacketWriter.Write(outputDirectory, differencePacket);
        DifferenceSummaryWriter.Write(outputDirectory, differencePacket);
    }

    return new ArtifactWriteResult(outputDirectory, differencePacket);
}

static void PrintDifferenceStatus(DifferencePacket packet, bool filesWritten)
{
    Console.WriteLine($"Difference: {packet.Status}");
    Console.WriteLine($"Difference Comparable: {packet.Comparable.ToString().ToLowerInvariant()}");
    Console.WriteLine($"Difference Reason: {packet.Reason}");
    Console.WriteLine($"Difference Files Written: {filesWritten.ToString().ToLowerInvariant()}");
}

static string FormatFloat(float value)
{
    return value.ToString("G9", System.Globalization.CultureInfo.InvariantCulture);
}

static string NormalizePath(string path)
{
    return path.Replace('\\', '/');
}

static string NormalizeFixturePath(string fixturePath)
{
    return NormalizePath(Path.IsPathRooted(fixturePath)
        ? Path.GetRelativePath(Environment.CurrentDirectory, fixturePath)
        : fixturePath);
}

internal sealed record RunOptions
{
    public string FixturePath { get; init; } = "";
    public bool OutputEnabled { get; init; } = true;
    public string OutputRoot { get; init; } = "output/glowing_heart";
    public bool EmitDifference { get; init; }
}

internal sealed record ArtifactWriteResult(string OutputPath, DifferencePacket? DifferencePacket);

internal sealed record ComparePacketOptions
{
    public string LeftPacketDirectory { get; init; } = "";
    public string RightPacketDirectory { get; init; } = "";
    public string LeftChannel { get; init; } = "";
    public string RightChannel { get; init; } = "";
    public string OutputDirectory { get; init; } = "";
}
