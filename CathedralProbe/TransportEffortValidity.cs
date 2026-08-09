public static class TransportEffortValidity
{
	public static bool TryNormalize(int finalStepCount, int policyMaxSteps, bool numericalFailure, out float normalized)
	{
		normalized = 0f;
		if (numericalFailure || policyMaxSteps <= 0 || finalStepCount < 0)
			return false;
		normalized = (float)finalStepCount / policyMaxSteps;
		return float.IsFinite(normalized);
	}
}
