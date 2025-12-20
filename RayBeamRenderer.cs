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
	[Export] public bool ColorByField = true;
	[Export] public float FieldColorGain = 0.15f;   // higher = gets “hot” faster
	[Export] public Color HotColor = new Color(0.2f, 1.0f, 1.0f, 1.0f); // cyan-ish glow
	[Export] public int RenderEveryNSteps = 1;     // 1 = every step, 2 = every other, etc.
	[Export] public float MinStepLength = 0.05f;
	[Export] public float MaxStepLength = 0.5f;
	[Export] public float StepAdaptGain = 0.05f;  // how strongly acceleration shrinks step

	// --- Collision robustness (INSIGHT-ish thick segment) ---
	[Export] public int CollisionEveryNSteps = 1;     // 1 = every step, 2 = every other, etc.
	[Export] public float CollisionRadius = 0.03f;    // thickness of beam for hit testing
	[Export] public bool UseSphereSweepCollision = true; // switch between IntersectRay vs IntersectShape

	[Export] public bool UseInsightPlaneFilter = true;
	[Export] public NodePath InsightPlaneNode; // drag your table body here

	[Export] public float CollisionRaySubdivideThreshold = 0.25f; // meters per sub-ray
	[Export] public int MaxCollisionSubsteps = 16;


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

		// If you have field sources, rebuild every frame (they might be moving)
		var hasSources = GetTree().GetNodesInGroup("field_sources").Count > 0;

		if (!hasSources && Mathf.IsEqualApprox(beta, _lastBeta) && Mathf.IsEqualApprox(gamma, _lastGamma))
			return;

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

		RefreshInsightPlane();

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
		//GD.Print($"RayBeamRenderer: emitters={emitters.Count}");

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

				float traveled = 0.0f;

				//////////////////
				for (int s = 0; s <= StepsPerRay; s++)
				{
					Vector3 a = Vector3.Zero;
					Vector3 next = p;


					if (UseIntegratedField)
					{
						if (hasSources)
						{
							a = ComputeAccelerationAtPoint(p, fieldSources, beta, gamma);
						}
						else
						{
							// ✅ real fallback (needs 'center' in scope)
							Vector3 rvec = p - center;
							float rr = Mathf.Max(0.001f, rvec.Length());
							a = (-rvec / rr) * (beta * Mathf.Pow(rr, gamma) * BendScale * FieldStrength);
						}

						float aLen = a.Length();
						if (aLen > 50.0f) a = a / aLen * 50.0f;

						// --- Adaptive step size (based on field strength) ---
						float step = StepLength;
						if (UseIntegratedField)
						{
							float aMag = a.Length();
							step = Mathf.Clamp(StepLength / (1.0f + aMag * StepAdaptGain), MinStepLength, MaxStepLength);
						}

						// Integrate using adaptive step
						v = (v + a * step).Normalized();
						next = p + v * step;

						traveled += (next - p).Length();
						if (traveled > maxDist) break;
					}
					else
					{
						float t = s * StepLength;
						float bend = beta * Mathf.Pow(t, gamma) * BendScale;
						next = origin + dir * t + bendDir * bend;
					}

					float step01 = (StepsPerRay <= 0) ? 0f : (float)s / StepsPerRay;
					float fade = 1.0f - step01;
					fade *= fade;
					float alpha = Alpha * e.Intensity * fade;

					Color c = baseC;
					if (ColorByField)
					{
						float heat = Mathf.Clamp(a.Length() * FieldColorGain, 0f, 1f);
						c = c.Lerp(HotColor, heat);
					}

					//if ((next - origin).Length() > maxDist) break;

					if (StopOnHit && s > 0)
					{
						int ce = Mathf.Max(1, CollisionEveryNSteps);
						if ((s % ce) == 0)
						{
							Vector3 segA = p;
							Vector3 segB = next;
							float segLen = (segB - segA).Length();

							// INSIGHT-style plane prefilter (optional)
							if (UseInsightPlaneFilter && _hasInsightPlane)
							{
								if (!SegmentCrossesPlane(segA, segB, _insightPlane, CollisionRadius))
									goto NoHit;
							}

							bool didHit = false;
							Vector3 hp = Vector3.Zero;

							if (segLen > 1e-6f)
							{
								if (UseSphereSweepCollision)
								{
									didHit = SweepSegmentHit(space, segA, segB, CollisionMask, CollisionRadius, out hp);
								}
								else
								{
									int sub = 1;
									if (segLen > CollisionRaySubdivideThreshold)
										sub = Mathf.CeilToInt(segLen / CollisionRaySubdivideThreshold);
									sub = Mathf.Clamp(sub, 1, MaxCollisionSubsteps);

									didHit = SubdividedRayHit(space, segA, segB, CollisionMask, sub, out hp);
								}
							}

							if (didHit)
							{
								if (idx < _mm.InstanceCount)
									SetBillboardInstance(idx++, hp, camRight, camUp, camForward, c, alpha);
								break;
							}

						NoHit:;
						}
					}

					// 3) then stamp (if desired)
					int every = Mathf.Max(1, RenderEveryNSteps);
					if ((s % every) == 0)
					{
						if (idx >= _mm.InstanceCount) break;
						SetBillboardInstance(idx++, next, camRight, camUp, camForward, c, alpha);
					}

					p = next;
				}
			}
		}

		// If we didn’t fill all instances due to MaxDistance breaks, trim
		if (idx < total)
			_mm.InstanceCount = idx;
	}

	private void SetBillboardInstance(int index, Vector3 pos,
		Vector3 camRight, Vector3 camUp, Vector3 camForward,
		Color c, float alpha)
	{
		if (index < 0 || index >= _mm.InstanceCount) return;

		float s = QuadSize;
		//var basis = new Basis(camRight * s, camUp * s, camForward * s);
		var basis = new Basis(camRight * s, camUp * s, camForward);
		var xform = new Transform3D(basis, pos);

		_mm.SetInstanceTransform(index, xform);

		c.A = Mathf.Clamp(alpha, 0.0f, 1.0f);
		_mm.SetInstanceColor(index, c);
	}

	private Vector3 ComputeAccelerationAtPoint(
		Vector3 p,
		Godot.Collections.Array<Node> sources,
		float globalBeta,
		float globalGamma)
	{
		Vector3 aSum = Vector3.Zero;

		foreach (var n in sources)
		{
			if (n is not FieldSource3D fs) continue;
			if (!fs.Enabled) continue;

			Vector3 center = fs.GlobalPosition;
			Vector3 rvec = p - center;

			float rRaw = rvec.Length();
			float soft = Mathf.Max(0.00001f, fs.Softening);

			// Softened radius (prevents blowups)
			float r = Mathf.Sqrt(rRaw * rRaw + soft * soft);

			// Zone limits
			if (fs.MinRadius > 0.0f && r < fs.MinRadius) continue;
			if (fs.MaxRadius > 0.0f && r > fs.MaxRadius) continue;

			// Direction (attract vs repel)
			Vector3 dir = (-rvec / r);
			if (!fs.Attract) dir = -dir;

			// Choose per-source gamma / beta scaling
			float gamma = fs.OverrideGamma ? fs.Gamma : globalGamma;
			float betaScale = fs.OverrideBetaScale ? fs.BetaScale : 1.0f;

			// Base amplitude (keeps your global controls still relevant)
			float amp = globalBeta * betaScale * BendScale * FieldStrength * fs.Strength;

			float mag = 0.0f;

			switch (fs.Profile)
			{
				case FieldSource3D.ProfileType.Power:
					// r^gamma
					mag = amp * Mathf.Pow(r, gamma);
					break;

				case FieldSource3D.ProfileType.InversePower:
					// 1 / r^gamma
					mag = amp / Mathf.Pow(r, Mathf.Max(0.0001f, gamma));
					break;

				case FieldSource3D.ProfileType.Gaussian:
					{
						float sigma = Mathf.Max(0.0001f, fs.Sigma);
						float x = r / sigma;
						mag = amp * Mathf.Exp(-x * x);
					}
					break;

				case FieldSource3D.ProfileType.Shell:
					{
						// Shell band: strong between InnerRadius..OuterRadius, smooth edges.
						float inner = Mathf.Max(0.0f, fs.InnerRadius);
						float outer = Mathf.Max(inner + 0.0001f, fs.OuterRadius);
						float edge = Mathf.Max(0.0001f, fs.EdgeSoftness);

						// Smooth step weights at inner/outer edges
						float wIn = SmoothStep(inner - edge, inner + edge, r);
						float wOut = 1.0f - SmoothStep(outer - edge, outer + edge, r);
						float w = Mathf.Clamp(wIn * wOut, 0.0f, 1.0f);

						// Within band, apply power law (or feel free to make this constant instead)
						mag = amp * w * Mathf.Pow(r, gamma);
					}
					break;
			}

			aSum += dir * mag;
		}

		return aSum;
	}

	private static float SmoothStep(float a, float b, float x)
	{
		float t = Mathf.Clamp((x - a) / (b - a), 0.0f, 1.0f);
		return t * t * (3.0f - 2.0f * t);
	}

	// Reads a float/int property from a Node (used for camera Beta/Gamma)
	private static float ReadFloat(Node obj, StringName prop, float fallback)
	{
		if (obj == null) return fallback;

		Variant v = obj.Get(prop);
		return v.VariantType switch
		{
			Variant.Type.Float => (float)v,
			Variant.Type.Int => (int)v,
			_ => fallback
		};
	}

	// Random direction around local -Z axis (forward) within cone angle
	// Godot's forward is -Z; we’ll treat cone around -Z.
	private static Vector3 RandomInCone(Random rng, float coneAngleRad)
	{
		double u = rng.NextDouble();
		double v = rng.NextDouble();

		float cosTheta = Mathf.Lerp(1.0f, Mathf.Cos(coneAngleRad), (float)u);
		float sinTheta = Mathf.Sqrt(Mathf.Max(0f, 1f - cosTheta * cosTheta));
		float phi = Mathf.Tau * (float)v;

		float x = sinTheta * Mathf.Cos(phi);
		float y = sinTheta * Mathf.Sin(phi);
		float z = -cosTheta;

		return new Vector3(x, y, z).Normalized();
	}
	private static bool SegmentCrossesPlane(Vector3 p, Vector3 q, Plane plane, float eps = 0.001f)
	{
		float dp = plane.DistanceTo(p);
		float dq = plane.DistanceTo(q);

		// if either end is close, treat as crossing-ish
		if (Mathf.Abs(dp) <= eps || Mathf.Abs(dq) <= eps) return true;

		// sign change => segment crosses infinite plane
		return (dp > 0f) != (dq > 0f);
	}

	private Plane _insightPlane;
	private bool _hasInsightPlane = false;

	private void RefreshInsightPlane()
	{
		_hasInsightPlane = false;
		if (InsightPlaneNode == null || InsightPlaneNode.IsEmpty) return;

		var n = GetNodeOrNull<Node3D>(InsightPlaneNode);
		if (n == null) return;

		// Use node's +Y as plane normal (good for flat “table top” if table is upright)
		Vector3 normal = n.GlobalTransform.Basis.Y.Normalized();
		Vector3 point  = n.GlobalPosition;

		_insightPlane = new Plane(normal, point);
		_hasInsightPlane = true;
	}

	private static bool SweepSegmentHit(
		PhysicsDirectSpaceState3D space,
		Vector3 a,
		Vector3 b,
		uint mask,
		float radius,
		out Vector3 hitPos)
	{
		hitPos = Vector3.Zero;

		Vector3 motion = b - a;
		float len = motion.Length();
		if (len <= 1e-6f) return false;

		var sphere = new SphereShape3D
		{
			Radius = Mathf.Max(0.0005f, radius)
		};

		var q = new PhysicsShapeQueryParameters3D
		{
			Shape = sphere,
			Transform = new Transform3D(Basis.Identity, a),
			Motion = motion,                  // ✅ motion goes HERE
			Margin = 0.0f,
			CollisionMask = mask,
			CollideWithBodies = true,
			CollideWithAreas = true
		};

		float[] res = space.CastMotion(q);

		// res[1] < 1.0 → collision occurred before full motion
		if (res.Length >= 2 && res[1] < 1.0f)
		{
			float t = res[1];
			hitPos = a + motion * t;
			return true;
		}

		return false;
	}


	private static bool SubdividedRayHit(
		PhysicsDirectSpaceState3D space,
		Vector3 a,
		Vector3 b,
		uint mask,
		int maxSubsteps,
		out Vector3 hitPos)
	{
		hitPos = Vector3.Zero;

		Vector3 d = b - a;
		float len = d.Length();
		if (len <= 1e-6f) return false;

		int steps = Mathf.Clamp(maxSubsteps, 1, 64);
		Vector3 prev = a;

		for (int i = 1; i <= steps; i++)
		{
			float t = (float)i / steps;
			Vector3 cur = a + d * t;

			var rq = PhysicsRayQueryParameters3D.Create(prev, cur, mask);
			rq.CollideWithBodies = true;
			rq.CollideWithAreas = true;
			rq.HitFromInside = true;

			var hit = space.IntersectRay(rq);
			if (hit.Count > 0)
			{
				hitPos = (Vector3)hit["position"];
				return true;
			}

			prev = cur;
		}

		return false;
	}


}