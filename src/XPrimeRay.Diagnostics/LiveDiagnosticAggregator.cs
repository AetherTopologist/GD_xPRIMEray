namespace XPrimeRay.Diagnostics;

public sealed class LiveDiagnosticAggregator
{
    private readonly TimeSpan _interval;
    private long _lastFlushTicks;
    private bool _hasLastFlushTicks;
    private LiveDiagnosticCounters _counters;

    public LiveDiagnosticAggregator(TimeSpan interval)
    {
        _interval = interval <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : interval;
    }

    public ref LiveDiagnosticCounters Counters => ref _counters;

    public bool TryFlush(long nowTicks, out LiveDiagnosticCounters snapshot)
    {
        if (!_hasLastFlushTicks)
        {
            _lastFlushTicks = nowTicks;
            _hasLastFlushTicks = true;
            snapshot = default;
            return false;
        }

        if (nowTicks - _lastFlushTicks < _interval.Ticks)
        {
            snapshot = default;
            return false;
        }

        if (!_counters.HasAnyEvents())
        {
            _lastFlushTicks = nowTicks;
            snapshot = default;
            return false;
        }

        snapshot = _counters;
        _counters = default;
        _lastFlushTicks = nowTicks;
        return true;
    }
}
