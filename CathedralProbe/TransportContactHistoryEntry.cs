public readonly struct TransportContactHistoryEntry
{
	public readonly int PixelIndex;
	public readonly int ContactCount;
	public readonly int FirstContactStep;
	public readonly int LastContactStep;
	public readonly int FinalStepCount;
	public readonly bool HadAnyGeometryContact;
	public readonly bool HadAnyBackgroundContact;
	public readonly ulong NearestAcceptedColliderId;
	public readonly float NearestAcceptedNormalX;
	public readonly float NearestAcceptedNormalY;
	public readonly float NearestAcceptedNormalZ;
	public readonly bool NormalValid;
	public readonly ProbeOutcomeCode Outcome;

	public TransportContactHistoryEntry(int pixelIndex, int contactCount, int firstContactStep, int lastContactStep,
		int finalStepCount, bool hadAnyGeometryContact, bool hadAnyBackgroundContact, ulong nearestAcceptedColliderId,
		float nearestAcceptedNormalX, float nearestAcceptedNormalY, float nearestAcceptedNormalZ, bool normalValid, ProbeOutcomeCode outcome)
	{
		PixelIndex = pixelIndex;
		ContactCount = contactCount;
		FirstContactStep = firstContactStep;
		LastContactStep = lastContactStep;
		FinalStepCount = finalStepCount;
		HadAnyGeometryContact = hadAnyGeometryContact;
		HadAnyBackgroundContact = hadAnyBackgroundContact;
		NearestAcceptedColliderId = nearestAcceptedColliderId;
		NearestAcceptedNormalX = nearestAcceptedNormalX;
		NearestAcceptedNormalY = nearestAcceptedNormalY;
		NearestAcceptedNormalZ = nearestAcceptedNormalZ;
		NormalValid = normalValid;
		Outcome = outcome;
	}
}
