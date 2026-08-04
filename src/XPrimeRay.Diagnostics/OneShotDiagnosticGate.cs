namespace XPrimeRay.Diagnostics;

public struct OneShotDiagnosticGate
{
    private bool _emitted;

    public readonly bool HasEmitted => _emitted;

    public bool TryClaim(RuntimeDiagnosticPolicy policy, DiagnosticVerbosity required, DiagnosticCategory category)
    {
        if (_emitted || !policy.Allows(required, category))
        {
            return false;
        }

        _emitted = true;
        return true;
    }

    public void Reset()
    {
        _emitted = false;
    }
}
