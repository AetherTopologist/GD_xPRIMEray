namespace XPrimeRay.Core.Validation;

public sealed record ValidationReport
{
    public bool Passed { get; init; }
    public string Verdict => Passed ? "PASS" : "FAIL";
    public string Reason { get; init; } = "";
}
