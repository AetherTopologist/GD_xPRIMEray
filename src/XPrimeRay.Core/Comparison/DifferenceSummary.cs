using System.Text;

namespace XPrimeRay.Core.Comparison;

public sealed record DifferenceSummary
{
    public string Status { get; init; } = ComparisonStatuses.Unknown;
    public bool Comparable { get; init; }
    public bool TransformRequired { get; init; }
    public bool ImageComparisonPerformed { get; init; }
    public string Reason { get; init; } = "";

    public static DifferenceSummary FromPacket(DifferencePacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        packet.Validate();

        return new DifferenceSummary
        {
            Status = packet.Status,
            Comparable = packet.Comparison.Comparable,
            TransformRequired = packet.Comparison.TransformRequired,
            ImageComparisonPerformed = packet.Comparison.ImageComparisonPerformed,
            Reason = packet.Comparison.Reason,
        };
    }

    public string ToMarkdown(DifferencePacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var markdown = new StringBuilder();
        markdown.AppendLine("# Project Glowing Heart Difference Summary");
        markdown.AppendLine();
        markdown.AppendLine("Runtime executed: false");
        markdown.AppendLine();
        markdown.AppendLine("Parity claim: NONE");
        markdown.AppendLine();
        markdown.AppendLine("## Observations");
        markdown.AppendLine();
        markdown.AppendLine("| Side | Observer | Fixture | Snapshot | Channel | Representation |");
        markdown.AppendLine("|---|---|---|---|---|---|");
        AppendObservation(markdown, "Left", packet.Left);
        AppendObservation(markdown, "Right", packet.Right);
        markdown.AppendLine();
        markdown.AppendLine("## Comparison Basis");
        markdown.AppendLine();
        markdown.AppendLine($"- Status: `{Status}`");
        markdown.AppendLine($"- Basis: `{packet.Comparison.Basis}`");
        markdown.AppendLine($"- Comparable: `{FormatBool(Comparable)}`");
        markdown.AppendLine($"- Transform required: `{FormatBool(TransformRequired)}`");
        markdown.AppendLine($"- Image comparison performed: `{FormatBool(ImageComparisonPerformed)}`");
        markdown.AppendLine($"- Reason: {Reason}");
        markdown.AppendLine();
        markdown.AppendLine("## Claim Boundary");
        markdown.AppendLine();
        markdown.AppendLine("This packet describes comparison eligibility and identity metadata only. It contains no sample deltas or image comparison results.");
        markdown.AppendLine();
        markdown.AppendLine("No parity claim.");
        markdown.AppendLine("No Godot runtime execution.");
        markdown.AppendLine("No image comparison.");
        return markdown.ToString();
    }

    private static void AppendObservation(StringBuilder markdown, string side, DifferenceObservation observation)
    {
        markdown.AppendLine(
            $"| {side} | `{observation.ObserverIdentity.Id}` | `{observation.FixtureIdentity.Id}` | `{observation.SnapshotIdentity.Id}` | `{observation.MeasurementChannel}` | `{observation.RepresentationType}` |");
    }

    private static string FormatBool(bool value)
    {
        return value ? "true" : "false";
    }
}
