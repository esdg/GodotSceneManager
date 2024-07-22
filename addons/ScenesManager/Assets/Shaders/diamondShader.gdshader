shader_type canvas_item;

// Ranges from 0 to 1 over the course of the transition.
// We use this to actually animate the shader.
uniform float progress : hint_range(0, 1);

// Size of each diamond, in pixels.
uniform float diamondPixelSize = 10.0f;

void fragment() {
	float xFraction = fract(FRAGCOORD.x / diamondPixelSize);
	float yFraction = fract(FRAGCOORD.y / diamondPixelSize);
	float xDistance = abs(xFraction - 0.5);
	float yDistance = abs(yFraction - 0.5);
	if (xDistance + yDistance + UV.x + UV.y > progress * 4.0f) {
		discard;
	}
}
