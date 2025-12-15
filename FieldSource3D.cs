using Godot;

public partial class FieldSource3D : Node3D
{
    // Multiplies the global (camera) Beta.
    [Export] public float Strength = 1.0f;

    // Optional: ignore contributions beyond this distance (0 = unlimited).
    [Export] public float MaxRadius = 0.0f;

    // Optional: soften singularity / avoid insane accel near center.
    [Export] public float Softening = 0.25f;

    // If false, this source repels instead of attracts (fun for saddle tests).
    [Export] public bool Attract = true;

    public override void _Ready()
    {
        AddToGroup("field_sources");
    }
}
