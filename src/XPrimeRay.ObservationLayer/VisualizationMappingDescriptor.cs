namespace XPrimeRay.ObservationLayer;

public sealed class VisualizationMappingDescriptor
{
    public VisualizationMappingDescriptor(
        ObservationChannelId sourceChannel,
        string mappingId,
        string mappingVersion,
        VisualizationRangePolicy rangePolicy,
        float rangeMin,
        float rangeMax,
        string paletteId,
        string claimBoundary)
    {
        if (!sourceChannel.IsValid)
        {
            throw new ArgumentException("Visualization mapping requires a valid source channel.", nameof(sourceChannel));
        }

        SourceChannel = sourceChannel;
        MappingId = RequireText(mappingId, nameof(mappingId));
        MappingVersion = RequireText(mappingVersion, nameof(mappingVersion));
        RangePolicy = rangePolicy;
        RangeMin = rangeMin;
        RangeMax = rangeMax;
        PaletteId = RequireText(paletteId, nameof(paletteId));
        ClaimBoundary = RequireText(claimBoundary, nameof(claimBoundary));
    }

    public ObservationChannelId SourceChannel { get; }
    public string MappingId { get; }
    public string MappingVersion { get; }
    public VisualizationRangePolicy RangePolicy { get; }
    public float RangeMin { get; }
    public float RangeMax { get; }
    public string PaletteId { get; }
    public string ClaimBoundary { get; }

    private static string RequireText(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value must not be empty.", name);
        }

        return value.Trim();
    }
}
