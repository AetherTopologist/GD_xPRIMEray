using Godot;

[GlobalClass]
public partial class CurvedCamera : Camera3D
{
	[Export] public float Beta = 0.0f;
	[Export] public float Gamma = 2.0f;

	public override void _Process(double delta)
	{
		//GD.Print($"Beta={Beta}, Gamma={Gamma}");
	}

	public Vector3 GetCurvedRay(Vector2 ndc)
	{
		Vector3 ray = ProjectRayNormal(ndc);
		float r = ray.Length();
		float k = Mathf.Pow(r, Gamma) * Beta;
		return (ray + ray.Normalized() * k).Normalized();
	}

}
