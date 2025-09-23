/*
	ギザギザトランジションシェーダー by あるる（きのもと 結衣）
	Saw Transition Shader by KINOMOTO Yui

	MIT License
*/
shader_type canvas_item;

uniform float saw_b_shift = -0.267;
uniform float saw_a_interval = 1.0;
uniform float saw_b_interval = 2.0;
uniform float saw_a_scale = 1.0;
uniform float saw_b_scale = 0.821;

uniform vec2 uv_scale = vec2( 3.0, 8.0 );
uniform sampler2D tex : hint_albedo;
uniform vec4 color : hint_color = vec4( 0.0, 0.0, 0.0, 1.0 );

uniform float right = 0.0;
uniform float left = -1.0;

float calc_saw( float y, float interval, float scale )
{
	return max( ( abs( interval / 2.0 - mod( y, interval ) ) - ( interval / 2.0 - 0.5 ) ) * scale, 0.0 );
}

void fragment( )
{
	vec2 scaled_uv = UV * uv_scale;
	float saw_a = calc_saw( scaled_uv.y, saw_a_interval, saw_a_scale );
	float saw_b = calc_saw( scaled_uv.y + saw_b_shift, saw_b_interval, saw_b_scale );

	vec4 texture_pixel = texture( tex, UV );

	COLOR = vec4(
		color.rgb * texture_pixel.rgb
	,	color.a * texture_pixel.a
		* float( scaled_uv.x < max( saw_a, saw_b ) + right )
		* float( max( saw_a, saw_b ) + left < scaled_uv.x )
	);
}
