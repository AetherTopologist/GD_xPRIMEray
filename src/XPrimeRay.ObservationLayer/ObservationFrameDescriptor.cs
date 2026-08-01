namespace XPrimeRay.ObservationLayer;

public readonly struct ObservationFrameDescriptor
{
    public ObservationFrameDescriptor(
        SceneId sceneId,
        SceneClass sceneClass,
        int generation,
        int width,
        int height,
        int totalSampleCount,
        bool isComplete,
        string contextIdentity,
        ObservationPolicyFingerprint policyFingerprint)
    {
        if (!sceneId.IsValid)
        {
            throw new ArgumentException("Frame descriptor requires a valid scene id.", nameof(sceneId));
        }

        if (generation < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(generation));
        }

        if (width < 0 || height < 0 || totalSampleCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalSampleCount));
        }

        SceneId = sceneId;
        SceneClass = sceneClass;
        Generation = generation;
        Width = width;
        Height = height;
        TotalSampleCount = totalSampleCount;
        IsComplete = isComplete;
        ContextIdentity = string.IsNullOrWhiteSpace(contextIdentity) ? "unknown" : contextIdentity.Trim();
        PolicyFingerprint = policyFingerprint;
    }

    public SceneId SceneId { get; }
    public SceneClass SceneClass { get; }
    public int Generation { get; }
    public int Width { get; }
    public int Height { get; }
    public int TotalSampleCount { get; }
    public bool IsComplete { get; }

    /// <summary>
    /// Stable host-supplied identity for camera pose, frame dimensions, scene epochs, and observer basis.
    /// It must not contain Godot node paths, group names, instance ids, or UI labels.
    /// </summary>
    public string ContextIdentity { get; }

    public ObservationPolicyFingerprint PolicyFingerprint { get; }
}
