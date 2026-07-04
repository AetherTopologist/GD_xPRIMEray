namespace XPrimeRay.ObserverInstrumentation.Metadata;

public sealed class InstrumentMetadataCatalog
{
    private readonly Dictionary<string, InstrumentTargetMetadata> _targets;

    public InstrumentMetadataCatalog(params InstrumentTargetMetadata[] targets)
    {
        ArgumentNullException.ThrowIfNull(targets);
        _targets = new Dictionary<string, InstrumentTargetMetadata>(targets.Length, StringComparer.Ordinal);
        foreach (InstrumentTargetMetadata target in targets)
        {
            if (!target.IsValid || string.IsNullOrWhiteSpace(target.ColliderName))
            {
                throw new ArgumentException("Metadata entries require a valid, non-empty ColliderName.", nameof(targets));
            }

            if (!_targets.TryAdd(target.ColliderName, target))
            {
                throw new ArgumentException($"Duplicate ColliderName: {target.ColliderName}", nameof(targets));
            }
        }
    }

    public int Count => _targets.Count;

    public bool TryGet(string colliderName, out InstrumentTargetMetadata metadata) =>
        _targets.TryGetValue(colliderName, out metadata);
}
