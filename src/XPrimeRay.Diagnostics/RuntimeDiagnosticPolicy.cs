namespace XPrimeRay.Diagnostics;

public readonly struct RuntimeDiagnosticPolicy
{
    public const string EnvironmentVariable = "XPRIMERAY_DIAGNOSTIC_VERBOSITY";
    public const string TransportTraceEnvironmentVariable = "XPRIMERAY_DIAGNOSTIC_TRANSPORT_TRACE";

    public RuntimeDiagnosticPolicy(
        DiagnosticVerbosity verbosity,
        bool transportTraceOptIn = false,
        int selectedPixelTraceLimit = 64)
    {
        Verbosity = verbosity;
        TransportTraceOptIn = transportTraceOptIn;
        SelectedPixelTraceLimit = Math.Max(0, selectedPixelTraceLimit);
    }

    public DiagnosticVerbosity Verbosity { get; }

    public bool TransportTraceOptIn { get; }

    public int SelectedPixelTraceLimit { get; }

    public static DiagnosticVerbosity DefaultFor(DiagnosticRuntimeMode runtimeMode)
    {
        return runtimeMode switch
        {
            DiagnosticRuntimeMode.Live => DiagnosticVerbosity.Summary,
            DiagnosticRuntimeMode.Snapshot => DiagnosticVerbosity.Frame,
            DiagnosticRuntimeMode.HeadlessQualification => DiagnosticVerbosity.Frame,
            DiagnosticRuntimeMode.ExplicitRegionAudit => DiagnosticVerbosity.Region,
            DiagnosticRuntimeMode.ExplicitSelectedPixelAudit => DiagnosticVerbosity.PixelTrace,
            _ => DiagnosticVerbosity.Summary,
        };
    }

    public static RuntimeDiagnosticPolicy Resolve(
        DiagnosticVerbosityPreference preference,
        DiagnosticVerbosity explicitVerbosity,
        DiagnosticRuntimeMode runtimeMode,
        bool transportTraceOptIn = false,
        int selectedPixelTraceLimit = 64)
    {
        DiagnosticVerbosity verbosity = preference == DiagnosticVerbosityPreference.Explicit
            ? explicitVerbosity
            : DefaultFor(runtimeMode);
        return new RuntimeDiagnosticPolicy(verbosity, transportTraceOptIn, selectedPixelTraceLimit);
    }

    public bool Allows(DiagnosticVerbosity required, DiagnosticCategory category = DiagnosticCategory.Lifecycle, bool terminal = false)
    {
        if (terminal || category == DiagnosticCategory.Error)
        {
            return true;
        }

        if (required == DiagnosticVerbosity.TransportTrace && !TransportTraceOptIn)
        {
            return false;
        }

        return Verbosity >= required;
    }

    public bool AllowsSelectedPixelTrace(int selectedPixelCount)
    {
        return Verbosity >= DiagnosticVerbosity.PixelTrace
            && selectedPixelCount > 0
            && selectedPixelCount <= SelectedPixelTraceLimit;
    }

    public static RuntimeDiagnosticPolicy FromEnvironment(
        DiagnosticVerbosity defaultVerbosity,
        bool defaultTransportTraceOptIn = false,
        int selectedPixelTraceLimit = 64)
    {
        DiagnosticVerbosity verbosity = defaultVerbosity;
        string? value = Environment.GetEnvironmentVariable(EnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(value)
            && Enum.TryParse(value.Trim(), ignoreCase: true, out DiagnosticVerbosity parsed))
        {
            verbosity = parsed;
        }

        bool transportTrace = defaultTransportTraceOptIn;
        string? transportValue = Environment.GetEnvironmentVariable(TransportTraceEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(transportValue))
        {
            transportTrace = transportValue.Trim() is "1"
                || transportValue.Equals("true", StringComparison.OrdinalIgnoreCase)
                || transportValue.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        return new RuntimeDiagnosticPolicy(verbosity, transportTrace, selectedPixelTraceLimit);
    }

    public static RuntimeDiagnosticPolicy LiveDefault => new(DiagnosticVerbosity.Summary);

    public static RuntimeDiagnosticPolicy SnapshotDefault => new(DiagnosticVerbosity.Frame);
}
