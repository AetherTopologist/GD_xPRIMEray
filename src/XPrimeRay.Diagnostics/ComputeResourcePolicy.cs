using System.Globalization;

namespace XPrimeRay.Diagnostics;

public enum ComputeResourceProfile
{
    Safe,
    Balanced,
    Max,
    Custom
}

public sealed record HostCapabilitySnapshot(
    int? LogicalProcessorCount,
    long? TotalPhysicalMemoryBytes,
    long? AvailableMemoryBytes,
    string ProcessArchitecture,
    string OperatingSystem,
    bool ExternalWatchdogAvailable,
    string DetectionNote)
{
    public static HostCapabilitySnapshot Detect(
        Func<int>? logicalProcessorProvider = null,
        Func<long?>? totalMemoryProvider = null,
        Func<long?>? availableMemoryProvider = null)
    {
        int? logicalProcessors = null;
        string note = string.Empty;
        try
        {
            int value = (logicalProcessorProvider ?? (() => Environment.ProcessorCount))();
            if (value > 0 && value <= 4096)
                logicalProcessors = value;
            else
                note = $"logical processor count implausible: {value}";
        }
        catch (Exception exception)
        {
            note = $"logical processor detection failed: {exception.GetType().Name}";
        }

        long? total = TryRead(totalMemoryProvider, ref note);
        long? available = TryRead(availableMemoryProvider, ref note);
        return new HostCapabilitySnapshot(
            logicalProcessors,
            total,
            available,
            System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            false,
            note);
    }

    private static long? TryRead(Func<long?>? provider, ref string note)
    {
        if (provider == null)
            return null;
        try
        {
            long? value = provider();
            return value is > 0 ? value : null;
        }
        catch (Exception exception)
        {
            if (note.Length == 0)
                note = $"memory detection failed: {exception.GetType().Name}";
            return null;
        }
    }
}

public sealed record ResolvedComputeResourcePolicy(
    string SchemaVersion,
    ComputeResourceProfile RequestedProfile,
    ComputeResourceProfile EffectiveProfile,
    int? HostLogicalProcessorCount,
    int EffectiveBandWorkerCount,
    long WorkingSetWarningBytes,
    long WorkingSetAbortBytes,
    long PrivateMemoryAbortBytes,
    bool WatchdogEnforced,
    bool ExperimentalPass2ThreadingEnabled,
    string ResolutionNote)
{
    public string HostLogicalProcessors => HostLogicalProcessorCount?.ToString(CultureInfo.InvariantCulture) ?? "unknown";
}

public static class ComputeResourcePolicyResolver
{
    public const string SchemaVersion = "compute-resource-policy-v0";

    public static ResolvedComputeResourcePolicy Resolve(
        ComputeResourceProfile profile,
        HostCapabilitySnapshot host,
        int customWorkerCount = 0,
        long customWorkingSetWarningBytes = 0,
        long customWorkingSetAbortBytes = 0,
        long customPrivateMemoryAbortBytes = 0)
    {
        if (host == null) throw new ArgumentNullException(nameof(host));
        int hostWorkers = host.LogicalProcessorCount is > 0 ? host.LogicalProcessorCount.Value : 1;
        string note = host.LogicalProcessorCount is > 0 ? host.DetectionNote : "logical processor count unknown; worker fallback=1";
        int workers;
        long warning;
        long abort;
        long privateAbort;

        switch (profile)
        {
            case ComputeResourceProfile.Safe:
                workers = Math.Max(1, hostWorkers / 4);
                (warning, abort, privateAbort) = MemoryLimits(host, 0.50, 0.70);
                break;
            case ComputeResourceProfile.Balanced:
                workers = Math.Max(1, hostWorkers / 2);
                (warning, abort, privateAbort) = MemoryLimits(host, 0.65, 0.80);
                break;
            case ComputeResourceProfile.Max:
                workers = hostWorkers;
                (warning, abort, privateAbort) = MemoryLimits(host, 0.80, 0.90);
                break;
            case ComputeResourceProfile.Custom:
                ValidateCustom(customWorkerCount, customWorkingSetWarningBytes, customWorkingSetAbortBytes, customPrivateMemoryAbortBytes, host.LogicalProcessorCount);
                workers = customWorkerCount;
                warning = customWorkingSetWarningBytes;
                abort = customWorkingSetAbortBytes;
                privateAbort = customPrivateMemoryAbortBytes;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(profile));
        }

        return new ResolvedComputeResourcePolicy(
            SchemaVersion,
            profile,
            profile,
            host.LogicalProcessorCount,
            workers,
            warning,
            abort,
            privateAbort,
            WatchdogEnforced: true,
            ExperimentalPass2ThreadingEnabled: false,
            note);
    }

    private static (long Warning, long Abort, long PrivateAbort) MemoryLimits(HostCapabilitySnapshot host, double warningFraction, double abortFraction)
    {
        if (host.TotalPhysicalMemoryBytes is not > 0)
            return (0, 0, 0);
        long total = host.TotalPhysicalMemoryBytes.Value;
        return ((long)(total * warningFraction), (long)(total * abortFraction), (long)(total * abortFraction));
    }

    private static void ValidateCustom(int workers, long warning, long abort, long privateAbort, int? hostWorkers)
    {
        if (workers <= 0)
            throw new ArgumentOutOfRangeException(nameof(workers), "CUSTOM worker count must be positive.");
        if (hostWorkers is > 0 && workers > hostWorkers.Value)
            throw new ArgumentOutOfRangeException(nameof(workers), "CUSTOM worker count exceeds detected host ceiling.");
        if (warning < 0 || abort < 0 || privateAbort < 0)
            throw new ArgumentOutOfRangeException(nameof(warning), "CUSTOM memory limits cannot be negative; zero disables that limit.");
        if (warning > 0 && abort > 0 && warning > abort)
            throw new ArgumentException("CUSTOM working-set warning cannot exceed its abort limit.");
    }
}
