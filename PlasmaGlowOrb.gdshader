shader_type spatial;
render_mode blend_add, depth_draw_never, cull_disabled, unshaded;

// 0 = blueshift (destination orbs), 1 = redshift (throat)
uniform float redshift_blend : hint_range(0.0, 1.0) = 0.0;
uniform vec4 blue_color : source_color = vec4(0.18, 0.55, 1.0, 1.0);
uniform vec4 red_color : source_color = vec4(1.0, 0.28, 0.06, 1.0);
uniform float rim_power : hint_range(1.0, 8.0) = 3.0;
uniform float energy : hint_range(0.1, 4.0) = 1.5;
uniform float pulse_phase = 0.0;

void fragment() {
	float ndotv = dot(NORMAL, VIEW);
	float rim = 1.0 - abs(ndotv);
	rim = pow(rim, rim_power);

	// Soft core glow
	float core = exp(-abs(ndotv) * 4.0) * 0.35;

	vec4 col = mix(blue_color, red_color, redshift_blend);
	float brightness = (rim + core) * energy * (1.0 + 0.12 * sin(pulse_phase));

	ALBEDO = col.rgb * brightness;
	ALPHA = clamp(rim * 0.9 + core * 0.4, 0.0, 1.0);
}
