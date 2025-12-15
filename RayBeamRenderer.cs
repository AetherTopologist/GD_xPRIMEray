using Godot;
using System;
using System.Collections.Generic;

public partial class RayBeamRenderer : Node3D
{
	[Export] public NodePath CameraPath;
	[Export] public bool UpdateEveryFrame = true;     // set false later for “only when changed”
	[Export] public int StepsPerRay = 64;
	[Export] public float StepLength = 0.25f;         // distance per step
	[Export] public float QuadSize = 0.04f;           // billboard size per sample
	[Export] public float BendScale = 0.12f;          // visual bend strength
	[Export] public float Alpha = 0.20f;              // base alpha
	[Export] public bool StopOnHit = false;
	[Export] public uint CollisionMask = 0xFFFFFFFF;
	[Export] public bool UseIntegratedField = true;
	[Export] public Vector3 FieldCenter = Vector3.Zero;
	[Export] public bool FieldCenterIsCamera = true;
	[Export] public float FieldStrength = 1.0f; // extra multiplier

	private MultiMeshInstance3D _mmi;
	private MultiMesh _mm;
	private StandardMaterial3D _mat;

	private float _lastBeta = float.NaN;
	private float _lastGamma = float.NaN;

	//public override void _Ready()
	public override async void _Ready()
	{
		_mm = new MultiMesh();
		_mm.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;

		// Enable instance colors (for per-ray tint)
		_mm.UseColors = true;

		// We’re not using per-instance custom data (yet)
		_mm.UseCustomData = false;


		_mmi = new MultiMeshInstance3D
		{
			Multimesh = _mm
		};

		// A simple QuadMesh that we will billboard by orienting transforms toward camera
		//var quad = new QuadMesh { Size = new Vector2(QuadSize, QuadSize) };
		var quad = new QuadMesh { Size = new Vector2(1, 1) };
		//_mmi.Mesh = quad;
		_mm.Mesh = quad;

		_mat = new StandardMaterial3D
		{
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
			Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
			BlendMode = BaseMaterial3D.BlendModeEnum.Add,
			VertexColorUseAsAlbedo = true,
			AlbedoColor = new Color(1, 1, 1, 1),
			EmissionEnabled = true,
			Emission = new Color(1, 1, 1, 1),
			EmissionEnergyMultiplier = 2.0f
		};

		_mmi.MaterialOverride = _mat;

		AddChild(_mmi);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Rebuild();
	}

	public override void _Process(double delta)
	{
		if (!UpdateEveryFrame) return;

		var cam = GetCamera();
		if (cam == null) return;

		float beta = ReadFloat(cam, "Beta", 0f);
		float gamma = ReadFloat(cam, "Gamma", 2f);

		// (Optional) only rebuild when Beta/Gamma changes
		if (Mathf.IsEqualApprox(beta, _lastBeta) && Mathf.IsEqualApprox(gamma, _lastGamma))
		{
			// Comment this out if you want constant animation later.
			return;
		}

		Rebuild();
	}

	private Camera3D GetCamera()
	{
		if (CameraPath != null && !CameraPath.IsEmpty)
			return GetNodeOrNull<Camera3D>(CameraPath);

		return GetViewport()?.GetCamera3D();
	}

	private void Rebuild()
	{
		var cam = GetCamera();
		if (cam == null) return;

		Vector3 center = FieldCenterIsCamera ? cam.GlobalPosition : FieldCenter;

		float beta = ReadFloat(cam, "Beta", 0f);
		float gamma = ReadFloat(cam, "Gamma", 2f);

		_lastBeta = beta;
		_lastGamma = gamma;

		var fieldSources = GetTree().GetNodesInGroup("field_sources");

		// Optional fallback: if none exist, fall back to old single-center behavior
		bool hasSources = fieldSources.Count > 0;

		// Gather emitters
		var emitters = GetTree().GetNodesInGroup("ray_emitters");
		int emitterCount = emitters.Count;
		if (emitterCount == 0)
		{
			_mm.InstanceCount = 0;
			return;
		}
		//GD.Print($"emitters={emitters.Count}");
		GD.Print($"RayBeamRenderer: emitters={emitters.Count}");

		// Total instances = sum over emitters of (rays * steps)
		int total = 0;
		var emitterList = new List<RayEmitter3D>(emitterCount);

		foreach (var node in emitters)
		{
			if (node is RayEmitter3D e)
			{
				emitterList.Add(e);
				total += Math.Max(1, e.Rays) * (StepsPerRay + 1);
			}
		}

		_mm.InstanceCount = total;

		// Camera basis for billboarding
		Vector3 camRight = cam.GlobalTransform.Basis.X.Normalized();
		Vector3 camUp = cam.GlobalTransform.Basis.Y.Normalized();
		Vector3 camForward = (-cam.GlobalTransform.Basis.Z).Normalized();

		int idx = 0;
		var rng = new Random(12345); // deterministic for now

		// Optional physics for stop-on-hit
		PhysicsDirectSpaceState3D space = GetWorld3D().DirectSpaceState;

		foreach (var e in emitterList)
		{
			Color baseC = e.RayColor;
			float maxDist = e.MaxDistance;

			int rays = Math.Max(1, e.Rays);
			float spreadRad = Mathf.DegToRad(e.SpreadDegrees);

			Vector3 origin = e.GlobalTransform.Origin;

			for (int r = 0; r < rays; r++)
			{
				// Random direction in a cone around emitter -Z (forward in local space)
				Vector3 localDir;
				if (e.UseFan)
				{
					float yawTotal = Mathf.DegToRad(e.FanYawDegrees);
					float pitch = Mathf.DegToRad(e.FanPitchDegrees);

					float u = (rays == 1) ? 0.0f : (float)r / (rays - 1);
					float yaw = Mathf.Lerp(-yawTotal * 0.5f, yawTotal * 0.5f, u);

					localDir = new Vector3(0, 0, -1);
					localDir = localDir.Rotated(Vector3.Up, yaw);
					localDir = localDir.Rotated(Vector3.Right, pitch);
				}
				else
				{
					localDir = RandomInCone(rng, spreadRad);
				}

				Vector3 dir = (e.GlobalTransform.Basis * localDir).Normalized();

				// Bend direction: perpendicular within camera plane, based on direction relative to screen center.
				// For “radiate outward” v0, we bend in camera plane by projecting dir onto camRight/camUp.
				float dx = dir.Dot(camRight);
				float dy = dir.Dot(camUp);
				Vector2 d2 = new Vector2(dx, -dy);
				Vector2 d2n = d2.Length() > 1e-6f ? d2 / d2.Length() : Vector2.Right;
				Vector3 bendDir = (camRight * d2n.X + camUp * -d2n.Y).Normalized();

				Vector3 p = origin;
				Vector3 v = dir;

				for (int s = 0; s <= StepsPerRay; s++)
				{
					SetBillboardInstance(idx++, p, camRight, camUp, camForward, baseC, e.Intensity);

					Vector3 next;

					if (UseIntegratedField)
					{
						Vector3 a;

						if (hasSources)
						{
							a = ComputeAccelerationAtPoint(p, fieldSources, beta, gamma);
						}
						else
						{
							// fallback to your single-center mode if no sources exist
							Vector3 rvec = p - center;
							float rr = Mathf.Max(0.001f, rvec.Length());
							a = (-rvec / rr) * (beta * Mathf.Pow(rr, gamma) * BendScale * FieldStrength);
						}

						// Safety clamp so sliders don't yeet rays into hyperspace
						float aLen = a.Length();
						if (aLen > 50.0f)
							a = a / aLen * 50.0f;

						v = (v + a * StepLength).Normalized();
						next = p + v * StepLength;
					}
					else
					{
						float t = s * StepLength;
						float bend = beta * Mathf.Pow(t, gamma) * BendScale;
						next = origin + dir * t + bendDir * bend;
					}

					if ((next - origin).Length() > maxDist)
						break;

					if (StopOnHit && s > 0)
					{
						var q = PhysicsRayQueryParameters3D.Create(p, next, CollisionMask);
						var hit = space.IntersectRay(q);
						if (hit.Count > 0)
						{
							Vector3 hp = (Vector3)hit["position"];
							SetBillboardInstance(idx++, hp, camRight, camUp, camForward, baseC, e.Intensity);
							break;
						}
					}
					p = next;
				}
			}
		}

		// If we didn’t fill all instances due to MaxDistance breaks, trim
		if (idx < total)
			_mm.InstanceCount = idx;
	}

	private void SetBillboardInstance(int index, Vector3 pos, Vector3 camRight, Vector3 camUp, Vector3 camForward, Color baseColor, float intensity)
	{
		if (index < 0 || index >= _mm.InstanceCount) return;
		// Billboard basis facing camera:
		// X=camRight, Y=camUp, Z=camForward
		//var basis = new Basis(camRight, camUp, camForward);
		float s = QuadSize; // or QuadSize * (0.75f + 0.25f * intensity)
		//var basis = new Basis(camRight * s, camUp * s, camForward);
		var basis = new Basis(camRight * s, camUp * s, camForward * s);


		var xform = new Transform3D(basis, pos);

		_mm.SetInstanceTransform(index, xform);

		// Color with intensity & alpha
		float a = Mathf.Clamp(Alpha * intensity, 0.0f, 1.0f);
		Color c = new Color(baseColor.R, baseColor.G, baseColor.B, a);
		_mm.SetInstanceColor(index, c);
	}
	private Vector3 ComputeAccelerationAtPoint(
		Vector3 p,
		Godot.Collections.Array<Node> sources,
		float beta,
		float gamma)
	{
		Vector3 aSum = Vector3.Zero;

		foreach (var n in sources)
		{
			if (n is not FieldSource3D fs)
				continue;

			Vector3 center = fs.GlobalPosition;
			Vector3 rvec = p - center;

			float r = rvec.Length();
			float soft = Mathf.Max(0.0001f, fs.Softening);
			r = Mathf.Sqrt(r * r + soft * soft); // softened radius

			if (fs.MaxRadius > 0.0f && r > fs.MaxRadius)
				continue;

			// Unit direction toward center (attract) or away (repel)
			Vector3 dir = (-rvec / r);
			if (!fs.Attract) dir = -dir;

			// Your global field law: beta * r^gamma (kept intentionally consistent with your current system)
			float mag = beta * Mathf.Pow(r, gamma) * BendScale * FieldStrength * fs.Strength;

			aSum += dir * mag;
		}

		return aSum;
	}

	// Random direction around local -Z axis (forward) within cone angle
	// Godot's forward is -Z; we’ll treat cone around -Z.
	private static Vector3 RandomInCone(Random rng, float coneAngleRad)
	{
		// Sample on cone cap with cosine-weight-ish distribution
		double u = rng.NextDouble();
		double v = rng.NextDouble();

		float cosTheta = Mathf.Lerp(1.0f, Mathf.Cos(coneAngleRad), (float)u);
		float sinTheta = Mathf.Sqrt(Mathf.Max(0f, 1f - cosTheta * cosTheta));
		//float phi = (float)(TAU * v);
		float phi = Mathf.Tau * (float)v;


		float x = sinTheta * Mathf.Cos(phi);
		float y = sinTheta * Mathf.Sin(phi);
		float z = -cosTheta; // around -Z

		return new Vector3(x, y, z).Normalized();
	}

	private static float ReadFloat(Node obj, StringName prop, float fallback)
	{
		Variant v = obj.Get(prop);
		return v.VariantType switch
		{
			Variant.Type.Float => (float)v,
			Variant.Type.Int => (int)v,
			_ => fallback
		};
	}
}
