/// <summary>
/// Pure C# region analyzer entry point; Stage 0 wiring supplies caller-owned buffers.
/// </summary>
public static class ProbeRegionAnalyzer
{
	public static void Analyze(
		int filmW,
		int filmH,
		System.ReadOnlySpan<ProbeOutcomeCode> outcomes,
		System.Span<ushort> regionLabels,
		System.Collections.Generic.List<ProbeRegionRecord> results)
	{
		results.Clear();
		regionLabels.Clear();
	}
}
