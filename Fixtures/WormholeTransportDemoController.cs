using Godot;
using System;
using System.Globalization;
using RendererCore.Fields;
using RendererCore.Config;

/// <summary>
/// Controller for the GRIN wormhole transport demo fixture.
/// Composes a Morris-Thorne-inspired effective index profile from existing
/// FieldSource3D primitives: one negative-amp throat + N positive-amp orb ring.
/// No new integrator code — the existing heuristic geodesic integrator handles it.
/// </summary>
public partial class WormholeTransportDemoController : Node3D
{
	// ===== CLI arg prefixes for scripted capture =====
	private const string ThroatRadiusPrefix    = "--wt-throat-radius=";
	private const string NegAmpPrefix          = "--wt-neg-amp=";
	private const string ThroatGammaPrefix     = "--wt-throat-gamma=";
	private const string OrbCountPrefix        = "--wt-orb-count=";
	private const string OrbRingRadiusPrefix   = "--wt-orb-ring-radius=";
	private const string OrbAmpPrefix          = "--wt-orb-amp=";
	private const string OrbROuterPrefix       = "--wt-orb-r-outer=";
	private const string SpinRatePrefix        = "--wt-spin-rate=";
	private const string PlasmaFreqPrefix      = "--wt-plasma-freq=";
	private const string BendScalePrefix       = "--wt-bend-scale=";
	private const string StepsPerRayPrefix     = "--wt-steps-per-ray=";

	// ===== Tunable physics exports =====
	[ExportGroup("Throat (Negative-Energy Core)")]
	[Export(PropertyHint.Range, "0.5,5.0,0.1")]  public float ThroatRadius     = 1.5f;
	[Export(PropertyHint.Range, "-2.0,0.0,0.05")] public float NegativeEnergyAmp = -0.8f;
	[Export(PropertyHint.Range, "0.5,4.0,0.1")]  public float ThroatGamma      = 1.5f;

	[ExportGroup("Orb Ring (Converging Channel)")]
	[Export(PropertyHint.Range, "3,12,1")]        public int   OrbCount         = 5;
	[Export(PropertyHint.Range, "2.0,10.0,0.5")] public float OrbRingRadius    = 4.0f;
	[Export(PropertyHint.Range, "0.0,2.0,0.05")] public float OrbAmp           = 0.4f;
	[Export(PropertyHint.Range, "1.0,6.0,0.5")]  public float OrbROuter        = 2.5f;

	[ExportGroup("Dynamics")]
	[Export] public float SpinPhaseRate   = 0.3f;  // rad/s ring rotation
	[Export] public float PlasmaFrequency = 1.0f;  // Hz temporal orb modulation

	[ExportGroup("Renderer")]
	[Export] public float BendScale    = 1.0f;
	[Export] public int   StepsPerRay  = 400;

	// ===== Scene node references (matched by name in tscn) =====
	[Export] public NodePath RendererPath    = new("RayBeamRenderer");
	[Export] public NodePath ThroatFieldPath = new("ThroatFieldSource3D");
	[Export] public NodePath OrbGroupPath    = new("OrbGroup");
	[Export] public NodePath FilmCameraPath  = new("../GrinFilmCamera");

	private RayBeamRenderer _renderer;
	private FieldSource3D _throatField;
	private Node3D _orbGroup;
	private GrinFilmCamera _film;
	private FieldSource3D[] _orbs = Array.Empty<FieldSource3D>();
	private double _time;

	public override void _Ready()
	{
		ReadCliOverrides();

		_renderer    = GetNodeOrNull<RayBeamRenderer>(RendererPath);
		_throatField = GetNodeOrNull<FieldSource3D>(ThroatFieldPath);
		_orbGroup    = GetNodeOrNull<Node3D>(OrbGroupPath);
		_film        = GetNodeOrNull<GrinFilmCamera>(FilmCameraPath);

		ConfigureThroat();
		BuildOrbRing();
		ConfigureRenderer();
		_film?.SetHudFixtureName("wormhole_transport_demo");

		GD.Print(
			$"[WormholeTransportDemo] throat_radius={ThroatRadius:0.##} " +
			$"neg_amp={NegativeEnergyAmp:0.##} orb_count={OrbCount} " +
			$"orb_ring_radius={OrbRingRadius:0.##} spin_rate={SpinPhaseRate:0.##}");

		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		_time += delta;

		// Rotate orb ring for dynamic interference pattern
		if (_orbGroup != null)
		{
			_orbGroup.RotateY((float)(SpinPhaseRate * delta));
		}

		// Temporal amplitude modulation of each orb (plasma pulsing)
		float mod = OrbAmp * (1f + 0.15f * Mathf.Sin((float)(_time * PlasmaFrequency * Mathf.Tau)));
		foreach (FieldSource3D orb in _orbs)
		{
			if (GodotObject.IsInstanceValid(orb))
			{
				orb.Amp = mod;
			}
		}
	}

	private void ReadCliOverrides()
	{
		ThroatRadius      = Mathf.Max(0.1f, ReadFloat(ThroatRadiusPrefix, ThroatRadius));
		NegativeEnergyAmp = Mathf.Clamp(ReadFloat(NegAmpPrefix, NegativeEnergyAmp), -4f, 0f);
		ThroatGamma       = Mathf.Clamp(ReadFloat(ThroatGammaPrefix, ThroatGamma), 0.1f, 8f);
		OrbCount          = Math.Clamp(ReadInt(OrbCountPrefix, OrbCount), 1, 24);
		OrbRingRadius     = Mathf.Max(0.5f, ReadFloat(OrbRingRadiusPrefix, OrbRingRadius));
		OrbAmp            = Mathf.Max(0f, ReadFloat(OrbAmpPrefix, OrbAmp));
		OrbROuter         = Mathf.Max(0.5f, ReadFloat(OrbROuterPrefix, OrbROuter));
		SpinPhaseRate     = ReadFloat(SpinRatePrefix, SpinPhaseRate);
		PlasmaFrequency   = Mathf.Max(0f, ReadFloat(PlasmaFreqPrefix, PlasmaFrequency));
		BendScale         = Mathf.Max(0f, ReadFloat(BendScalePrefix, BendScale));
		StepsPerRay       = Math.Max(32, ReadInt(StepsPerRayPrefix, StepsPerRay));
	}

	private void ConfigureThroat()
	{
		if (_throatField == null)
		{
			return;
		}

		_throatField.Enabled         = true;
		_throatField.MetricModel     = MetricModel.GRIN;
		_throatField.TransportModel  = TransportModel.GRIN_Optical;
		_throatField.ShapeType       = FieldShapeType.SphereRadial;
		_throatField.CurveType       = FieldCurveType.Power;
		_throatField.CanonicalGamma  = ThroatGamma;
		_throatField.ROuter          = ThroatRadius * 2.0f;
		_throatField.Amp             = NegativeEnergyAmp;
		_throatField.CanonicalOverrideBetaScale = true;
		_throatField.CanonicalBetaScale         = 1.0f;
		_throatField.CanonicalEnableInnerRadius = true;
		_throatField.RInner                     = ThroatRadius * 0.15f;
		_throatField.DebugVizEnabled            = false;
	}

	private void BuildOrbRing()
	{
		if (_orbGroup == null)
		{
			GD.PrintErr("[WormholeTransportDemo] OrbGroup node not found — skipping orb ring");
			return;
		}

		// Remove any existing runtime orbs
		foreach (Node child in _orbGroup.GetChildren())
		{
			child.QueueFree();
		}

		_orbs = new FieldSource3D[OrbCount];
		for (int i = 0; i < OrbCount; i++)
		{
			float angle = (Mathf.Tau / OrbCount) * i;
			var orb = new FieldSource3D();
			orb.Name             = $"OrbField_{i}";
			orb.Enabled          = true;
			orb.MetricModel      = MetricModel.GRIN;
			orb.TransportModel   = TransportModel.GRIN_Optical;
			orb.ShapeType        = FieldShapeType.SphereRadial;
			orb.CurveType        = FieldCurveType.Power;
			orb.CanonicalGamma   = 1.0f;
			orb.ROuter           = OrbROuter;
			orb.Amp              = OrbAmp;
			orb.CanonicalOverrideBetaScale = true;
			orb.CanonicalBetaScale         = 1.0f;
			orb.DebugVizEnabled  = false;

			_orbGroup.AddChild(orb);
			orb.Position = new Vector3(
				Mathf.Cos(angle) * OrbRingRadius,
				0f,
				Mathf.Sin(angle) * OrbRingRadius);

			_orbs[i] = orb;
		}
	}

	private void ConfigureRenderer()
	{
		if (_renderer == null)
		{
			return;
		}

		_renderer.UseIntegratedField    = true;
		_renderer.BendScale             = BendScale;
		_renderer.FieldStrength         = 1.0f;
		_renderer.StepsPerRay           = StepsPerRay;
		_renderer.StepLength            = 0.05f;
		_renderer.MinStepLength         = 0.005f;
		_renderer.MaxStepLength         = 0.15f;
		_renderer.StopOnHit             = true;
		_renderer.TerminateTrailOnHit   = true;
		_renderer.RequireHitToRender    = false;
	}

	// ===== CLI helpers (same pattern as other controllers) =====
	private static float ReadFloat(string prefix, float fallback)
	{
		string raw = ReadString(prefix);
		return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : fallback;
	}

	private static int ReadInt(string prefix, int fallback)
	{
		string raw = ReadString(prefix);
		return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : fallback;
	}

	private static string ReadString(string prefix)
	{
		foreach (string arg in OS.GetCmdlineUserArgs())
		{
			if (!string.IsNullOrWhiteSpace(arg) && arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return arg.Substring(prefix.Length).Trim();
			}
		}

		foreach (string arg in OS.GetCmdlineArgs())
		{
			if (!string.IsNullOrWhiteSpace(arg) && arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return arg.Substring(prefix.Length).Trim();
			}
		}

		return string.Empty;
	}
}
