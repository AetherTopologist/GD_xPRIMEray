public enum ProbeViewMode : byte
{
	Outcome = 0,
	ContactEvents = 1,
	TransportEffort = 2
}

public static class ProbeViewCycle
{
	public static ProbeViewMode Next(ProbeViewMode mode)
	{
		return (ProbeViewMode)(((int)mode + 1) % 3);
	}

	public static ProbeViewMode Previous(ProbeViewMode mode)
	{
		return (ProbeViewMode)(((int)mode + 2) % 3);
	}
}
