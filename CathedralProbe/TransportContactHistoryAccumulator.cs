public static class TransportContactHistoryAccumulator
{
	public static void Reset(
		ref int contactCount,
		ref int firstContactStep,
		ref int lastContactStep,
		ref bool hadAnyGeometryContact,
		ref bool hadAnyBackgroundContact)
	{
		contactCount = 0;
		firstContactStep = -1;
		lastContactStep = -1;
		hadAnyGeometryContact = false;
		hadAnyBackgroundContact = false;
	}

	public static void RecordContact(
		ref int contactCount,
		ref int firstContactStep,
		ref int lastContactStep,
		ref bool hadAnyGeometryContact,
		ref bool hadAnyBackgroundContact,
		int step,
		ProbeSurfaceClass surfaceClass)
	{
		if (contactCount == 0)
			firstContactStep = step;
		contactCount++;
		if (step > lastContactStep)
			lastContactStep = step;
		hadAnyGeometryContact |= surfaceClass == ProbeSurfaceClass.Geometry;
		hadAnyBackgroundContact |= surfaceClass == ProbeSurfaceClass.Background;
	}
}
