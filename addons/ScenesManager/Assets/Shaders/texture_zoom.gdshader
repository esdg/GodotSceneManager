shader_type canvas_item;

// Mask texture (defines the shape of the transition)
uniform sampler2D mask_texture : hint_default_black, filter_linear_mipmap;

// Zoom parameter - animate this for the transition effect
// 0.0 = fully zoomed out (mask covers everything)
// 1.0 = fully zoomed in (no masking, shows everything)
uniform float zoom_progress : hint_range(0.0, 1.0) = 0.0;
uniform float zoom_ratio : hint_range(1.0, 200.0) = 100.0;

// Additional parameters for fine-tuning
uniform vec2 mask_offset = vec2(0.0, 0.0);
uniform float edge_softness : hint_range(0.0, 1.0) = 0.05;
uniform bool invert_mask = false;

// Function to maintain aspect ratio
vec2 maintain_aspect_ratio(vec2 uv, vec2 texture_size, vec2 target_size) {
    vec2 ratio = texture_size / target_size;
    float scale_factor = max(ratio.x, ratio.y);
    vec2 scaled_size = texture_size / scale_factor;
    vec2 offset = (target_size - scaled_size) * 0.5 / target_size;
    return (uv - offset) * (target_size / scaled_size);
}

// Easing function for smoother zoom
float ease_out_cubic(float t) {
    return 1.0 - pow(1.0 - t, 3.0);
}

void fragment() {
    // Get the texture color from the node this shader is applied to
    vec4 main_color = texture(TEXTURE, UV);
    
    // Calculate zoom effect based on invert_mask setting
    float zoom_factor;
    float effective_progress = invert_mask ? (1.0 - zoom_progress) : zoom_progress;
    
    // Apply easing to the progress
    float eased_progress = ease_out_cubic(effective_progress);
    
    // The zoom now goes from a large value to a small one as progress increases
    zoom_factor = mix(zoom_ratio, 0.001, eased_progress);
    
    // Get screen aspect ratio to maintain circular mask
    vec2 screen_size = 1.0 / fwidth(UV);
    float aspect_ratio = screen_size.x / screen_size.y;
    
    // Center the UV coordinates and apply aspect ratio correction
    vec2 centered_uv = (UV - vec2(0.5)) + mask_offset;
    centered_uv.x *= aspect_ratio; // Correct for aspect ratio
    centered_uv = centered_uv / zoom_factor + vec2(0.5);
    
    float mask_value;
    if (edge_softness > 0.0) {
        float total = 0.0;
        vec2 tex_offset = (1.0 / vec2(textureSize(mask_texture, 0))) * edge_softness * 5.0;
        
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                vec2 sample_uv = centered_uv + vec2(float(x), float(y)) * tex_offset;
                // Clamp UV to prevent reading outside the texture, which causes artifacts
                if (sample_uv.x >= 0.0 && sample_uv.x <= 1.0 && sample_uv.y >= 0.0 && sample_uv.y <= 1.0) {
                    total += texture(mask_texture, sample_uv).r;
                }
            }
        }
        mask_value = total / 9.0;
    } else {
        // Original behavior when no softness is needed
        if (centered_uv.x >= 0.0 && centered_uv.x <= 1.0 && centered_uv.y >= 0.0 && centered_uv.y <= 1.0) {
            mask_value = texture(mask_texture, centered_uv).r;
        } else {
            mask_value = 0.0;
        }
    }
    
    // Create the final alpha
    float final_alpha = mask_value * main_color.a;
    
    COLOR = vec4(main_color.rgb, final_alpha);
}