using XPrimeRay.Core.Fixtures;
using XPrimeRay.Core.Transport;
using XPrimeRay.Core.Validation;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args[0] is "--help" or "-h")
    {
        PrintHelp();
        return 0;
    }

    if (args.Length != 2 || args[0] != "run-fixture")
    {
        Console.Error.WriteLine("Invalid arguments.");
        PrintHelp();
        return 1;
    }

    try
    {
        var fixture = FixtureLoader.Load(args[1]);
        var runner = new TransportRunner();
        var result = runner.Run(fixture);
        var report = ClosureValidator.Validate(fixture, result);

        PrintSummary(result, report);
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
    Console.WriteLine("xPRIMEray-Core Testbench v0.1");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture <fixture.json>");
}

static void PrintSummary(TransportResult result, ValidationReport report)
{
    Console.WriteLine("xPRIMEray-Core Testbench v0.1");
    Console.WriteLine($"Fixture: {result.FixtureName}");
    Console.WriteLine($"Mode: {result.Mode}");
    Console.WriteLine($"Resolution: {result.Width}x{result.Height}");
    Console.WriteLine($"Rays: {result.Rays}");
    Console.WriteLine($"Hits: {result.Hits}");
    Console.WriteLine($"Misses: {result.Misses}");
    Console.WriteLine($"Validation: {report.Verdict}");
    if (!report.Passed && !string.IsNullOrWhiteSpace(report.Reason))
    {
        Console.WriteLine($"Reason: {report.Reason}");
    }

    Console.WriteLine($"Note: {TransportRunner.ArtifactRunnerNote}");
}
