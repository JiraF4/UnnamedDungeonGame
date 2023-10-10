shader_type spatial;
uniform sampler2D albedo_texture : filter_nearest_mipmap;

void fragment() {
	//vec2 uv = vec2(step(UV2.x, UV.x), step(UV2.y, UV.y));
	float offset = 0.0; // 0.0015625 * 4.0;
	float quad_size = 0.03125;
	vec2 uv = vec2(
		UV.x - mod(UV.x, quad_size) + clamp(quad_size * mod(UV2.x, 1), offset, quad_size - offset),
		UV.y - mod(UV.y, quad_size) + clamp(quad_size * mod(UV2.y, 1), offset, quad_size - offset)
	);
	ALBEDO = texture(albedo_texture, uv).rgb;
	//ALBEDO = vec3(mod(UV2.x, 1), mod(UV2.y, 1), 0);
}
