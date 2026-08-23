using System.Text.Json;
using XPrimeRay.Diagnostics;

namespace XPrimeRay.Diagnostics.Tests;

internal static class RuntimeHealthWatchdogTests
{
    public static void Run()
    {
        WritesRunAndHeartbeatRecords();
        DisposeCompletesRunWhenNeeded();
        DetectsIncompletePriorRun();
        DevelopmentSafeLimitsAreOptIn();
    }

    private static void WritesRunAndHeartbeatRecords()
    {
        string directory = NewDirectory();
        using (var watchdog = new RuntimeHealthWatchdog(new RuntimeHealthWatchdogOptions(directory, TimeSpan.Zero)))
        {
            watchdog.BeginAcquisition("snapshot-7", 3, "ctx-sha");
            watchdog.Heartbeat("capturing", 12, 34, 5, 3, "ctx-sha", "band-1", force: true);
            watchdog.CompleteAcquisition(3, "ctx-sha", "finalize");
            watchdog.CompleteRun();
        }
        string[] lines = File.ReadAllLines(Path.Combine(directory, "runtime_health.ndjson"));
        Assert.True(lines.Any(line => JsonDocument.Parse(line).RootElement.GetProperty("event").GetString() == "RUN_START"), "run start");
        Assert.True(lines.Any(line => JsonDocument.Parse(line).RootElement.GetProperty("event").GetString() == "heartbeat"), "heartbeat");
        Assert.True(lines.Any(line => JsonDocument.Parse(line).RootElement.GetProperty("event").GetString() == "RUN_COMPLETE"), "run complete");
        Assert.True(File.ReadAllLines(Path.Combine(directory, "runtime_health.csv")).Length >= 2, "csv append");
    }

    private static void DetectsIncompletePriorRun()
    {
        string directory = NewDirectory();
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "runtime_health.ndjson");
        File.WriteAllText(path, "{\"event\":\"RUN_START\",\"run_id\":\"crashed\"}\n");
        using var watchdog = new RuntimeHealthWatchdog(new RuntimeHealthWatchdogOptions(directory, TimeSpan.FromSeconds(1)));
        Assert.True(watchdog.AbnormalPreviousRun, "abnormal previous run");
        Assert.True(File.ReadAllText(path).Contains("ABNORMAL_PREVIOUS_RUN", StringComparison.Ordinal), "abnormal marker");
    }

    private static void DisposeCompletesRunWhenNeeded()
    {
        string directory = NewDirectory();
        using (var watchdog = new RuntimeHealthWatchdog(new RuntimeHealthWatchdogOptions(directory, TimeSpan.FromSeconds(1))))
        {
        }
        Assert.True(File.ReadAllText(Path.Combine(directory, "runtime_health.ndjson")).Contains("RUN_COMPLETE", StringComparison.Ordinal), "dispose completes run");
    }

    private static void DevelopmentSafeLimitsAreOptIn()
    {
        string directory = NewDirectory();
        using var disabled = new RuntimeHealthWatchdog(new RuntimeHealthWatchdogOptions(directory, TimeSpan.FromSeconds(1), DevelopmentSafe: false, MaxWorkerCount: 1));
        Assert.False(disabled.CheckLimits(2).Abort, "limits disabled by default");
        string safeDirectory = NewDirectory();
        using var enabled = new RuntimeHealthWatchdog(new RuntimeHealthWatchdogOptions(safeDirectory, TimeSpan.FromSeconds(1), DevelopmentSafe: true, MaxWorkerCount: 1));
        Assert.True(enabled.CheckLimits(2).Abort, "worker limit abort");
    }

    private static string NewDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), "xprimeray-health-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
