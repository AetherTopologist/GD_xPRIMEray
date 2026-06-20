using XPrimeRay.Core.Fixtures;

namespace XPrimeRay.Core.Transport;

public sealed class TransportRunner
{
    public const string ArtifactRunnerNote =
        "v0.1 artifact runner only; full optical transport remains in GD_xPRIMEray harnesses.";

    public TransportResult Run(FixtureDefinition fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);

        // This is an intentionally modest v0.1 artifact runner. It proves the
        // Core project and CLI can execute outside Godot; it is not optical transport.
        return TransportResult.FromFixture(fixture);
    }
}
