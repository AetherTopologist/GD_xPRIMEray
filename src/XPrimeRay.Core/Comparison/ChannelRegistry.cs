namespace XPrimeRay.Core.Comparison;

public sealed class ChannelRegistry
{
    public const string CurrentVersion = "v0.preview";

    private readonly IReadOnlyDictionary<string, ChannelDescriptor> _channels;

    public static ChannelRegistry Default { get; } = new(CreateDefaultChannels());

    public ChannelRegistry(IEnumerable<ChannelDescriptor> channels)
    {
        ArgumentNullException.ThrowIfNull(channels);
        _channels = channels.ToDictionary(channel => channel.Id, StringComparer.Ordinal);
    }

    public IEnumerable<ChannelDescriptor> Channels => _channels.Values;

    public ChannelDescriptor? Find(string? id)
    {
        return id is not null && _channels.TryGetValue(id, out var channel) ? channel : null;
    }

    private static ChannelDescriptor[] CreateDefaultChannels()
    {
        return
        [
            new ChannelDescriptor
            {
                Id = "bend_magnitude_metric",
                Kind = "measurement",
                DataType = "float64",
                Units = "unitless_metric",
                Representation = "scalar_grid",
                Description = "Mean ray-direction bend magnitude recorded for each Core sample coordinate.",
            },
            new ChannelDescriptor
            {
                Id = "ray_hit_flag",
                Kind = "measurement",
                DataType = "boolean",
                Units = "categorical",
                Representation = "binary_grid",
                Description = "Per-ray hit or miss classification under a declared fixture and observer.",
            },
            new ChannelDescriptor
            {
                Id = "traversal_step_count",
                Kind = "measurement",
                DataType = "integer",
                Units = "steps",
                Representation = "scalar_grid",
                Description = "Number of transport steps recorded for each sample.",
            },
            new ChannelDescriptor
            {
                Id = "observer_basis",
                Kind = "identity",
                DataType = "object",
                Units = "not_applicable",
                Representation = "metadata",
                Description = "Declared observer pose, orientation, projection, and sampling basis.",
            },
            new ChannelDescriptor
            {
                Id = "fixture_identity",
                Kind = "identity",
                DataType = "string",
                Units = "not_applicable",
                Representation = "metadata",
                Description = "Stable identity and source reference for a fixture declaration.",
            },
            new ChannelDescriptor
            {
                Id = "runtime_metadata",
                Kind = "metadata",
                DataType = "object",
                Units = "not_applicable",
                Representation = "metadata",
                Description = "Execution and provenance metadata associated with an artifact packet.",
            },
        ];
    }
}
