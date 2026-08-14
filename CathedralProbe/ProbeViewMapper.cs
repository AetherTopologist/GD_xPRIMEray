using System;

public readonly struct ProbeViewColor
{
	public readonly byte R;
	public readonly byte G;
	public readonly byte B;
	public readonly byte A;

	public ProbeViewColor(byte r, byte g, byte b, byte a = 255)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}
}

public readonly struct ProbeViewLegendEntry
{
	public readonly string Label;
	public readonly ProbeViewColor Color;

	public ProbeViewLegendEntry(string label, ProbeViewColor color)
	{
		Label = label;
		Color = color;
	}
}

public static class ProbeViewMapper
{
	private static readonly ProbeViewColor Unavailable = new(64, 68, 76);

	public static bool IsAvailable(bool complete, int unprocessed, int totalPixels, int sourceLength)
	{
		return complete && totalPixels > 0 && unprocessed == 0 && sourceLength >= totalPixels;
	}

	public static bool IsSealedStorageValid(
		bool available,
		int width,
		int height,
		int totalPixels,
		int outcomesLength,
		int contactCountsLength,
		int finalStepsLength,
		int policyMaxStepsLength,
		int effortValidLength)
	{
		return available && width > 0 && height > 0 && totalPixels == width * height &&
			outcomesLength >= totalPixels && contactCountsLength >= totalPixels &&
			finalStepsLength >= totalPixels && policyMaxStepsLength >= totalPixels &&
			effortValidLength >= totalPixels;
	}

	public static ProbeViewColor Map(
		ProbeViewMode mode,
		ProbeOutcomeCode outcome,
		int contactCount,
		int finalStepCount,
		int policyMaxSteps,
		bool effortValid)
	{
		return mode switch
		{
			ProbeViewMode.Outcome => MapOutcome(outcome),
			ProbeViewMode.ContactEvents => MapContactEvents(contactCount),
			ProbeViewMode.TransportEffort => MapTransportEffort(finalStepCount, policyMaxSteps, effortValid),
			_ => Unavailable
		};
	}

	public static ProbeViewColor MapOutcome(ProbeOutcomeCode outcome)
	{
		return outcome switch
		{
			ProbeOutcomeCode.Unprocessed => new ProbeViewColor(96, 96, 104),
			ProbeOutcomeCode.HitGeometry => new ProbeViewColor(56, 190, 112),
			ProbeOutcomeCode.BackgroundResolved => new ProbeViewColor(72, 142, 224),
			ProbeOutcomeCode.MaxStepsExhausted => new ProbeViewColor(232, 154, 48),
			ProbeOutcomeCode.StoppedEarlyAbsorbed => new ProbeViewColor(196, 72, 176),
			ProbeOutcomeCode.NumericalFailure => new ProbeViewColor(224, 64, 64),
			ProbeOutcomeCode.Invalid => new ProbeViewColor(248, 224, 88),
			_ => Unavailable
		};
	}

	public static ProbeViewColor MapContactEvents(int contactCount)
	{
		return contactCount switch
		{
			<= 0 => new ProbeViewColor(24, 48, 92),
			1 => new ProbeViewColor(56, 128, 190),
			2 => new ProbeViewColor(80, 190, 168),
			_ => new ProbeViewColor(236, 154, 48)
		};
	}

	public static ProbeViewColor MapTransportEffort(int finalStepCount, int policyMaxSteps, bool effortValid)
	{
		if (!effortValid || policyMaxSteps <= 0 || finalStepCount < 0)
			return Unavailable;
		float effort = Math.Clamp((float)finalStepCount / policyMaxSteps, 0f, 1f);
		byte red = (byte)Math.Round(32f + (effort * 220f));
		byte green = (byte)Math.Round(192f - (effort * 132f));
		return new ProbeViewColor(red, green, 72);
	}

	public static string DisplayName(ProbeViewMode mode)
	{
		return mode switch
		{
			ProbeViewMode.Outcome => "Outcome",
			ProbeViewMode.ContactEvents => "Contact Events",
			ProbeViewMode.TransportEffort => "Transport Effort",
			_ => "Unavailable"
		};
	}

	public static string Description(ProbeViewMode mode)
	{
		return mode switch
		{
			ProbeViewMode.Outcome => "Terminal semantic outcome",
			ProbeViewMode.ContactEvents => "Accepted per-step contacts · repeats count",
			ProbeViewMode.TransportEffort => "Numerical step budget used · not time · not field strength",
			_ => "Complete SNAPSHOT required"
		};
	}

	public static ProbeViewLegendEntry[] Legend(ProbeViewMode mode)
	{
		return mode switch
		{
			ProbeViewMode.Outcome => new[]
			{
				new ProbeViewLegendEntry("Unprocessed", MapOutcome(ProbeOutcomeCode.Unprocessed)),
				new ProbeViewLegendEntry("HitGeometry", MapOutcome(ProbeOutcomeCode.HitGeometry)),
				new ProbeViewLegendEntry("BackgroundResolved", MapOutcome(ProbeOutcomeCode.BackgroundResolved)),
				new ProbeViewLegendEntry("MaxStepsExhausted", MapOutcome(ProbeOutcomeCode.MaxStepsExhausted)),
				new ProbeViewLegendEntry("StoppedEarlyAbsorbed", MapOutcome(ProbeOutcomeCode.StoppedEarlyAbsorbed)),
				new ProbeViewLegendEntry("NumericalFailure", MapOutcome(ProbeOutcomeCode.NumericalFailure)),
				new ProbeViewLegendEntry("Invalid", MapOutcome(ProbeOutcomeCode.Invalid))
			},
			ProbeViewMode.ContactEvents => new[]
			{
				new ProbeViewLegendEntry("0", MapContactEvents(0)),
				new ProbeViewLegendEntry("1", MapContactEvents(1)),
				new ProbeViewLegendEntry("2", MapContactEvents(2)),
				new ProbeViewLegendEntry("3+", MapContactEvents(3))
			},
			ProbeViewMode.TransportEffort => new[]
			{
				new ProbeViewLegendEntry("0.0", MapTransportEffort(0, 1, true)),
				new ProbeViewLegendEntry("0.5", MapTransportEffort(1, 2, true)),
				new ProbeViewLegendEntry("1.0", MapTransportEffort(1, 1, true)),
				new ProbeViewLegendEntry("N/A", Unavailable)
			},
			_ => Array.Empty<ProbeViewLegendEntry>()
		};
	}
}
