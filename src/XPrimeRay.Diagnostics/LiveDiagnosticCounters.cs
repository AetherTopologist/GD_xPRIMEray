namespace XPrimeRay.Diagnostics;

public struct LiveDiagnosticCounters
{
    public long Frames;
    public long RenderSteps;
    public long RowsAdvanced;
    public long PixelsTraced;
    public long PhysicsQueries;
    public long Geometry;
    public long Background;
    public long MaxSteps;
    public long Absorbed;
    public long Fault;
    public long Invalid;
    public long BlvRemaps;
    public long BlvCrossings;
    public long Watchdog;
    public long Yield;
    public long Warnings;
    public long Errors;
    public double RenderStepMillisecondsTotal;
    public double RenderStepMillisecondsMax;
    public bool SemanticCountersAvailable;

    public void AddRenderStep(double milliseconds, long rowsAdvanced, long pixelsTraced, long physicsQueries)
    {
        RenderSteps++;
        RowsAdvanced += Math.Max(0, rowsAdvanced);
        PixelsTraced += Math.Max(0, pixelsTraced);
        PhysicsQueries += Math.Max(0, physicsQueries);
        double clampedMs = Math.Max(0.0, milliseconds);
        RenderStepMillisecondsTotal += clampedMs;
        if (clampedMs > RenderStepMillisecondsMax)
        {
            RenderStepMillisecondsMax = clampedMs;
        }
    }

    public void AddOutcomes(
        long geometry,
        long background,
        long maxSteps,
        long absorbed,
        long fault,
        long invalid)
    {
        SemanticCountersAvailable = true;
        Geometry += Math.Max(0, geometry);
        Background += Math.Max(0, background);
        MaxSteps += Math.Max(0, maxSteps);
        Absorbed += Math.Max(0, absorbed);
        Fault += Math.Max(0, fault);
        Invalid += Math.Max(0, invalid);
    }

    public void RecordWatchdog()
    {
        Watchdog++;
    }

    public void RecordYield()
    {
        Yield++;
    }

    public readonly bool HasAnyEvents()
    {
        return Frames != 0
            || RenderSteps != 0
            || RowsAdvanced != 0
            || PixelsTraced != 0
            || PhysicsQueries != 0
            || Geometry != 0
            || Background != 0
            || MaxSteps != 0
            || Absorbed != 0
            || Fault != 0
            || Invalid != 0
            || BlvRemaps != 0
            || BlvCrossings != 0
            || Watchdog != 0
            || Yield != 0
            || Warnings != 0
            || Errors != 0;
    }
}
