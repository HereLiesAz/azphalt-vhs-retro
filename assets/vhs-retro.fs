/*{
  "DESCRIPTION": "Authentic VHS decay \u2014 scanlines, chroma bleed, edge wobble and tape noise that drift over time like a worn cassette.",
  "CATEGORIES": ["Guillotine", "Distortion"],
  "INPUTS": [
  {
    "NAME": "inputImage",
    "TYPE": "image"
  },
  {
    "NAME": "amount",
    "TYPE": "float",
    "DEFAULT": 0.6,
    "MIN": 0.0,
    "MAX": 1.0
  },
  {
    "NAME": "wobble",
    "TYPE": "float",
    "DEFAULT": 0.4,
    "MIN": 0.0,
    "MAX": 1.0
  }
]
}*/
float hash(vec2 p) { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }

void main() {
  vec2 uv = isf_FragNormCoord;

  // Horizontal tape wobble: a slow sine plus a per-line jitter that resettles each frame.
  float line = floor(uv.y * RENDERSIZE.y);
  float drift = sin(uv.y * 8.0 + TIME * 1.7) * 0.0035 * wobble;
  float jitter = (hash(vec2(line, floor(TIME * 12.0))) - 0.5) * 0.004 * wobble;
  vec2 warped = vec2(uv.x + drift + jitter, uv.y);

  // Chroma bleed: luma stays sharp, colour smears sideways like composite video.
  float bleed = 0.004 * amount;
  float r = IMG_NORM_PIXEL(inputImage, warped + vec2(bleed, 0.0)).r;
  vec4 g = IMG_NORM_PIXEL(inputImage, warped);
  float b = IMG_NORM_PIXEL(inputImage, warped - vec2(bleed, 0.0)).b;
  vec3 col = vec3(r, g.g, b);

  // Scanlines and a faint noise floor.
  float scan = 1.0 - 0.18 * amount * step(0.5, fract(uv.y * RENDERSIZE.y * 0.5));
  float grain = (hash(uv * RENDERSIZE + fract(TIME) * 71.0) - 0.5) * 0.06 * amount;

  gl_FragColor = vec4(col * scan + grain, g.a);
}
