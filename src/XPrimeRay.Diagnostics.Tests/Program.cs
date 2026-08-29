namespace XPrimeRay.Diagnostics.Tests;

internal static class Program
{
    private static int Main()
    {
        (string Name, Action Run)[] suites =
        {
            (nameof(RuntimeDiagnosticPolicyTests), RuntimeDiagnosticPolicyTests.Run),
            (nameof(RuntimeHealthWatchdogTests), RuntimeHealthWatchdogTests.Run),
            (nameof(ComputeResourcePolicyTests), ComputeResourcePolicyTests.Run)
        };

        int failures = 0;
        foreach ((string name, Action run) in suites)
        {
            try
            {
                run();
                Console.WriteLine($"PASS {name}");
            }
            catch (Exception exception)
            {
                failures++;
                Console.Error.WriteLine($"FAIL {name}: {exception.Message}");
            }
        }

        Console.WriteLine($"{(failures == 0 ? "PASS" : "FAIL")}: {suites.Length} suites, {failures} failures");
        return failures == 0 ? 0 : 1;
    }
}
