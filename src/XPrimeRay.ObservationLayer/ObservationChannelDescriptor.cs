namespace XPrimeRay.ObservationLayer;

public sealed class ObservationChannelDescriptor
{
    public ObservationChannelDescriptor(
        ObservationChannelId id,
        string displayName,
        ObservationDataType dataType,
        ObservationDomain domain,
        InstrumentTier scientificTier,
        string units,
        string validityCondition,
        string claimBoundary,
        IEnumerable<ObservationChannelDependency>? sourceChannelDependencies = null)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException("Channel descriptor requires a valid id.", nameof(id));
        }

        Id = id;
        DisplayName = RequireText(displayName, nameof(displayName));
        DataType = dataType == ObservationDataType.Unknown
            ? throw new ArgumentException("Channel descriptor requires a known data type.", nameof(dataType))
            : dataType;
        Domain = Enum.IsDefined(domain)
            ? domain
            : throw new ArgumentException("Channel descriptor requires a known domain.", nameof(domain));
        ScientificTier = Enum.IsDefined(scientificTier)
            ? scientificTier
            : throw new ArgumentException("Channel descriptor requires a known scientific tier.", nameof(scientificTier));
        Units = RequireText(units, nameof(units));
        ValidityCondition = RequireText(validityCondition, nameof(validityCondition));
        ClaimBoundary = RequireText(claimBoundary, nameof(claimBoundary));
        SourceChannelDependencies = (sourceChannelDependencies ?? Array.Empty<ObservationChannelDependency>()).ToArray();
    }

    public ObservationChannelId Id { get; }
    public string DisplayName { get; }
    public ObservationDataType DataType { get; }
    public ObservationDomain Domain { get; }
    public InstrumentTier ScientificTier { get; }
    public string Units { get; }
    public string ValidityCondition { get; }
    public string ClaimBoundary { get; }
    public IReadOnlyList<ObservationChannelDependency> SourceChannelDependencies { get; }

    private static string RequireText(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value must not be empty.", name);
        }

        return value.Trim();
    }
}
