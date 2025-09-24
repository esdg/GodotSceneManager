shader_type canvas_item;

// Preferred driver for animations (animate `shader_parameter/progress` from 0->1)
uniform float progress : hint_range(0.0, 1.0) = 0.0;

// Kept for backwards compatibility; if set, it will be combined with `progress`.
uniform float dissolve_amount : hint_range(0.0, 1.0) = 0.0;
uniform float pixel_size : hint_range(1.0, 32.0) = 8.0;
uniform sampler2D noise_texture : source_color;

void fragment() {
    // Quantize screen UVs to a grid in screen space based on `pixel_size`
    // SCREEN_PIXEL_SIZE is 1.0 / VIEWPORT_SIZE in UV units
    vec2 uv_step = SCREEN_PIXEL_SIZE * pixel_size;
    vec2 uv = floor(SCREEN_UV / uv_step) * uv_step;
    // Sample noise texture (should be grayscale)
    float noise = texture(noise_texture, uv).r;

    // Effective cutoff uses the higher of progress and dissolve_amount
    float cutoff = max(progress, dissolve_amount);

    // Dissolve: compare noise to cutoff
    if (noise < cutoff) {
        COLOR.a = 0.0;
    } else {
        COLOR = texture(TEXTURE, UV);
    }
}