shader_type spatial;
render_mode unshaded;

uniform float beta : hint_range(-2.0, 2.0);
uniform float gamma : hint_range(0.0, 8.0);

void vertex() {
    // pass through
}

void fragment() {
    vec3 ray = normalize(VIEW);
    float r = length(ray);
    float k = pow(r, gamma) * beta;
    ray = normalize(ray + ray * k);

    // sample the scene texture
    COLOR = texture(SCREEN_TEXTURE, SCREEN_UV + ray.xy * 0.01);
}
