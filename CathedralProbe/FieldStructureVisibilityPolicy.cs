using System;

public static class FieldStructureVisibilityPolicy
{
	/// <summary>
	/// Resolves one deterministic target for a group toggle. Mixed or all-hidden
	/// sources become visible; an already all-visible group becomes hidden.
	/// </summary>
	public static bool ResolveToggleTarget(ReadOnlySpan<bool> eligibleVisibleStates)
	{
		if (eligibleVisibleStates.Length == 0)
			return false;

		for (int i = 0; i < eligibleVisibleStates.Length; i++)
		{
			if (!eligibleVisibleStates[i])
				return true;
		}

		return false;
	}
}
