/// <summary>
/// Bounded pass-1 surface classification derived from collider identity, not shaded pixels.
/// </summary>
public enum ProbeSurfaceClass : byte
{
	None = 0,
	Geometry = 1,
	Background = 2,
	Unknown = 255,
}
