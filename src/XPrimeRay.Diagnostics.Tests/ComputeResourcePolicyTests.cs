using XPrimeRay.Diagnostics;

namespace XPrimeRay.Diagnostics.Tests;

internal static class ComputeResourcePolicyTests
{
    public static void Run()
    {
        ProfilesResolveInOrderAndKeepProtection();
        SmallHostsReportHonestProfileCollapse();
        CustomValidationHappensBeforeUse();
        FailedHostDetectionFallsBackConservatively();
        ProfileDoesNotEnterScientificIdentity();
    }

    private static void ProfilesResolveInOrderAndKeepProtection()
    {
        var host = new HostCapabilitySnapshot(24, 32L * 1024 * 1024 * 1024, 16L * 1024 * 1024 * 1024, "X64", "test", false, "");
        var safe = ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Safe, host);
        var balanced = ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Balanced, host);
        var max = ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Max, host);
        Assert.Equal(6, safe.EffectiveBandWorkerCount, "safe workers");
        Assert.Equal(12, balanced.EffectiveBandWorkerCount, "balanced workers");
        Assert.Equal(24, max.EffectiveBandWorkerCount, "max workers");
        Assert.True(safe.WatchdogEnforced && balanced.WatchdogEnforced && max.WatchdogEnforced, "all profiles enforce watchdog");
        Assert.False(max.ExperimentalPass2ThreadingEnabled, "max enables experimental pass2 threading");
        Assert.True(max.WorkingSetAbortBytes > balanced.WorkingSetAbortBytes && balanced.WorkingSetAbortBytes > safe.WorkingSetAbortBytes, "memory ordering");
    }

    private static void CustomValidationHappensBeforeUse()
    {
        var host = new HostCapabilitySnapshot(8, null, null, "X64", "test", false, "");
        Throws<ArgumentOutOfRangeException>(() => ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Custom, host, customWorkerCount: 0), "zero workers rejected");
        Throws<ArgumentOutOfRangeException>(() => ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Custom, host, customWorkerCount: -1), "negative workers rejected");
        Throws<ArgumentOutOfRangeException>(() => ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Custom, host, customWorkerCount: 1, customWorkingSetAbortBytes: -1), "negative memory rejected");
        Throws<ArgumentOutOfRangeException>(() => ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Custom, host, customWorkerCount: 9), "host ceiling rejected");
        var disabled = ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Custom, host, customWorkerCount: 1);
        Assert.Equal(0L, disabled.WorkingSetAbortBytes, "zero explicitly disables abort limit");
    }

    private static void SmallHostsReportHonestProfileCollapse()
    {
        var oneCore = new HostCapabilitySnapshot(1, null, null, "X64", "test", false, "");
        var oneMax = ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Max, oneCore);
        Assert.Equal(1, oneMax.EffectiveBandWorkerCount, "one-core max workers");
        Assert.True(oneMax.ResolutionNote.Contains("SAFE=1, BALANCED=1, MAX=1", StringComparison.Ordinal), "one-core collapse note");

        var twoCore = new HostCapabilitySnapshot(2, null, null, "X64", "test", false, "");
        var twoBalanced = ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Balanced, twoCore);
        var twoMax = ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Max, twoCore);
        Assert.Equal(1, twoBalanced.EffectiveBandWorkerCount, "two-core balanced workers");
        Assert.Equal(2, twoMax.EffectiveBandWorkerCount, "two-core max workers");
        Assert.True(twoBalanced.ResolutionNote.Contains("SAFE=1, BALANCED=1, MAX=2", StringComparison.Ordinal), "two-core collapse note");
    }

    private static void Throws<T>(Action action, string message) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }
        throw new InvalidOperationException(message);
    }

    private static void FailedHostDetectionFallsBackConservatively()
    {
        var host = HostCapabilitySnapshot.Detect(() => throw new InvalidOperationException("probe"));
        var policy = ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Max, host);
        Assert.False(host.LogicalProcessorCount.HasValue, "failed host count is unknown");
        Assert.Equal(1, policy.EffectiveBandWorkerCount, "unknown host worker fallback");
        Assert.Equal("unknown", policy.HostLogicalProcessors, "unknown provenance");
        Assert.True(policy.ResolutionNote.Contains("unknown", StringComparison.OrdinalIgnoreCase) || policy.ResolutionNote.Contains("failed", StringComparison.OrdinalIgnoreCase), "fallback reason");
    }

    private static void ProfileDoesNotEnterScientificIdentity()
    {
        var host = new HostCapabilitySnapshot(8, null, null, "X64", "test", false, "");
        var safe = ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Safe, host);
        var max = ComputeResourcePolicyResolver.Resolve(ComputeResourceProfile.Max, host);
        Assert.True(safe.EffectiveBandWorkerCount != max.EffectiveBandWorkerCount, "profiles differ for execution");
        Assert.Equal("compute-resource-policy-v0", safe.SchemaVersion, "policy schema");
    }
}
