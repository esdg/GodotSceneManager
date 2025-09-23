// CC0 licence
// Credit Bramwell Williams

shader_type canvas_item;

uniform float circle_size : hint_range(0,1) = 0.5f;

void fragment(){
	if (distance(vec2(0.5,0.5), UV) < circle_size){
		COLOR.a = 0f;
	}
}