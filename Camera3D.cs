using Godot;

public partial class CurvedCamera : Camera3D
{
	[Export] public float Beta = 0.0f;
	[Export] public float Gamma = 2.0f;

	public override void _Process(double delta)
	{
		// Example per-frame debug:
		// GD.Print(GetCurvedRay(new Vector2(0, 0)));
	}

	public Vector3 GetCurvedRay(Vector2 ndc)
	{
		Vector3 ray = ProjectRayNormal(ndc);
		float r = ray.Length();
		float k = Mathf.Pow(r, Gamma) * Beta;
		return (ray + ray.Normalized() * k).Normalized();
	}
}
