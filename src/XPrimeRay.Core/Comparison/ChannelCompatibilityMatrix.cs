namespace XPrimeRay.Core.Comparison;

public sealed class ChannelCompatibilityMatrix
{
    public const string CurrentVersion = "v0.preview";

    public static ChannelCompatibilityMatrix Default { get; } = new();

    public ChannelCompatibility Evaluate(
        ChannelDescriptor? left,
        ChannelDescriptor? right,
        ChannelComparisonContext context)
    {
        if (left is null || right is null)
        {
            return Result(
                "missing_channel_declaration",
                DifferenceStatus.Unknown,
                false,
                "One or both channel declarations are missing.");
        }

        if (IsCoreToFutureImagePair(left, right))
        {
            return Result(
                "core_to_future_image_requires_transform",
                DifferenceStatus.RequiresTransform,
                true,
                "Core measurement channels and future Godot/image channels require a declared transform before comparison.",
                "declared_transform");
        }

        if (IsPair(left, right, "bend_magnitude_metric", "bend_magnitude_metric"))
        {
            return context.SameFixtureIdentity && context.SameObserverBasis && context.SameCoordinateGrid
                ? Result(
                    "bend_magnitude_same_observer",
                    DifferenceStatus.Comparable,
                    false,
                    "Bend magnitude comparison requires matching fixture comparison identity, observer basis, and coordinate grid because each scalar is tied to the retained Core fixture/sample basis.",
                    "same_channel_type",
                    "same_fixture_identity",
                    "same_observer_basis",
                    "same_coordinate_grid")
                : Result(
                    "bend_magnitude_context_mismatch",
                    DifferenceStatus.Unknown,
                    false,
                    ContextMismatchReason("bend_magnitude_metric", context),
                    "same_channel_type",
                    "same_fixture_identity",
                    "same_observer_basis",
                    "same_coordinate_grid");
        }

        if (IsPair(left, right, "ray_hit_flag", "ray_hit_flag"))
        {
            return context.SameFixtureIdentity && context.SameObserverBasis && context.SameCoordinateGrid
                ? Result(
                    "ray_hit_same_fixture_observer",
                    DifferenceStatus.Comparable,
                    false,
                    "Hit/miss classification depends on both fixture geometry/bounds and observer sampling; two ray_hit_flag channels are comparable only when fixture comparison identity, observer basis, and coordinate grid match.",
                    "same_channel_type",
                    "same_fixture_identity",
                    "same_observer_basis",
                    "same_coordinate_grid")
                : Result(
                    "ray_hit_context_mismatch",
                    DifferenceStatus.Unknown,
                    false,
                    ContextMismatchReason("ray_hit_flag", context),
                    "same_channel_type",
                    "same_fixture_identity",
                    "same_observer_basis",
                    "same_coordinate_grid");
        }

        if (IsPair(left, right, "traversal_step_count", "bend_magnitude_metric")
            || IsPair(left, right, "bend_magnitude_metric", "traversal_step_count"))
        {
            return Result(
                "traversal_steps_vs_bend_not_comparable",
                DifferenceStatus.NotComparable,
                false,
                "Bend magnitude and traversal step count represent different retained Core quantities and are not directly comparable without a declared transform.");
        }

        return Result(
            "undeclared_channel_pair",
            DifferenceStatus.Unknown,
            false,
            "No compatibility rule declares this channel pair comparable.");
    }

    private static bool IsCoreToFutureImagePair(ChannelDescriptor left, ChannelDescriptor right)
    {
        return IsCore(left) && IsFutureImage(right) || IsCore(right) && IsFutureImage(left);
    }

    private static string ContextMismatchReason(string channelId, ChannelComparisonContext context)
    {
        if (!context.SameCoordinateGrid)
        {
            return $"The retained coordinate-grid mismatch makes {channelId} comparison eligibility unknown.";
        }

        if (!context.SameFixtureIdentity)
        {
            return $"The retained fixture identity mismatch makes {channelId} comparison eligibility unknown.";
        }

        return $"The retained observer basis mismatch makes {channelId} comparison eligibility unknown.";
    }

    private static bool IsCore(ChannelDescriptor channel)
    {
        return channel.Producer == "core";
    }

    private static bool IsFutureImage(ChannelDescriptor channel)
    {
        return channel.Producer == "future_godot" || channel.Kind == "image";
    }

    private static bool IsPair(
        ChannelDescriptor left,
        ChannelDescriptor right,
        string leftId,
        string rightId)
    {
        return left.Id == leftId && right.Id == rightId;
    }

    private static ChannelCompatibility Result(
        string ruleId,
        DifferenceStatus status,
        bool transformRequired,
        string reason,
        params string[] requiredConditions)
    {
        return new ChannelCompatibility
        {
            RuleId = ruleId,
            Status = status,
            TransformRequired = transformRequired,
            Reason = reason,
            RequiredConditions = requiredConditions,
        };
    }
}
