// Soffit effect adapted from the user-provided shader.
(() => {
"use strict";
document.querySelectorAll("[data-promotion-light]").forEach(canvas => {
const host = canvas.parentElement;
const reduced = matchMedia("(prefers-reduced-motion: reduce)");
if (reduced.matches) return;
const CONFIG = {
  "bgColor": "#062334",
  "colorA": "#2b7be8",
  "colorB": "#2bc8e8",
  "colorC": "#4fe0d8",
  "colorD": "#7fe8a0",
  "ambient": 0.24,
  "amount": 0.2,
  "bounce": 0.38,
  "bounceCurve": 4.25,
  "breathe": 0.29,
  "contrast": 2.45,
  "cursor": 1,
  "curve": 3.32,
  "direct": 0.97,
  "dither": 0.58,
  "flow": 0.475,
  "glow": 0.38,
  "grain": 0,
  "grainAnim": 0,
  "horizon": 0.36,
  "lacunarity": 1.99,
  "lift": 0.11,
  "maxDpr": 1,
  "midpoint": 0.57,
  "moteScale": 7,
  "motes": 0.074,
  "parallax": 0.0137,
  "rock": 0.12,
  "roughness": 0.29,
  "scale": 1,
  "sink": 0.24,
  "speed": 0.33,
  "spillCentre": 0.3,
  "spillFloor": 0.26,
  "spillWidth": 2.18,
  "spread": 0.41,
  "steer": -0.13,
  "sweep": 0.5,
  "tilt": 1.87,
  "vignette": 0.21,
  "warp": 2.58,
  "warpScale": 0.78
};
if (canvas.dataset.promotionLight === "offer") Object.assign(CONFIG, {"bgColor":"#062334","colorA":"#2b7be8","colorB":"#2bc8e8","colorC":"#4fe0d8","colorD":"#7fe8a0","ambient":0.04,"amount":0.22,"bounce":0.06,"bounceCurve":1.7,"breathe":0.09,"contrast":1.6,"cursor":1,"curve":2.6,"direct":0.72,"dither":1.2,"flow":0.15,"glow":0.34,"grain":0,"grainAnim":0,"horizon":-0.52,"lacunarity":2.05,"lift":0.2,"maxDpr":1,"midpoint":0.57,"moteScale":7,"motes":0.03,"parallax":0.002,"rock":0.11,"roughness":0.52,"scale":0.2,"sink":0.26,"speed":0.33,"spillCentre":0.04,"spillFloor":0.16,"spillWidth":0.2,"spread":1.3,"steer":0.22,"sweep":0.3,"tilt":-1.3,"vignette":0.44,"warp":1.1,"warpScale":0.72});
if (canvas.dataset.promotionLight === "story") Object.assign(CONFIG, {"bgColor":"#062334","colorA":"#2b7be8","colorB":"#2bc8e8","colorC":"#4fe0d8","colorD":"#7fe8a0","ambient":0.42,"amount":0.23,"bounce":0.23,"bounceCurve":1.65,"breathe":0.135,"contrast":2.18,"cursor":1,"curve":1.38,"direct":0.79,"dither":0.46,"flow":0.355,"glow":0.48,"grain":0,"grainAnim":0,"horizon":-0.55,"lacunarity":1.95,"lift":0.15,"maxDpr":1,"midpoint":0.81,"moteScale":6,"motes":0.068,"parallax":0.0103,"rock":0.1,"roughness":0.22,"scale":1,"sink":0.165,"speed":0.33,"spillCentre":0.33,"spillFloor":0.75,"spillWidth":1.42,"spread":-2.7,"steer":0.07,"sweep":-0.45,"tilt":2.31,"vignette":0.24,"warp":2.76,"warpScale":1.15});
if (canvas.dataset.promotionLight === "catalog") Object.assign(CONFIG, {"bgColor":"#08153a","colorA":"#2b4be8","colorB":"#5f8fff","colorC":"#c8d8ff","colorD":"#ffffff","ambient":0.33,"amount":0.34,"bounce":0.24,"bounceCurve":1.3,"breathe":0.19,"contrast":1.05,"cursor":1,"curve":1.61,"direct":0.34,"dither":0.42,"flow":0.52,"glow":0.26,"grain":0,"grainAnim":0,"horizon":-0.55,"lacunarity":2.13,"lift":0.16,"maxDpr":1,"midpoint":0.59,"moteScale":6.5,"motes":0.076,"parallax":0.0147,"rock":0.12,"roughness":0.53,"scale":1,"sink":0.28,"speed":0.33,"spillCentre":-0.06,"spillFloor":0.55,"spillWidth":0.58,"spread":-2.5,"steer":0.25,"sweep":0.31,"tilt":1.76,"vignette":0.06,"warp":1.31,"warpScale":2.25});
const VERT = `#version 300 es
void main() {
vec2 p = vec2((gl_VertexID << 1) & 2, gl_VertexID & 2);
gl_Position = vec4(p * 2.0 - 1.0, 0.0, 1.0);
}`;
const FRAG = "#version 300 es\r\nprecision highp float;\r\nout vec4 fragColor;\r\n\r\nuniform vec2  iResolution;\r\nuniform float iTime;\r\nuniform vec2  iMouse;          // aspect-corrected units, same space as uv\r\nuniform float uScale;          // zoom of the whole picture — uv and the pointer together\r\n\r\nuniform vec3  uBg, uColorA, uColorB, uColorC, uColorD;\r\nuniform float uSpeed, uTilt, uRock, uHorizon, uBreathe, uSpread, uCurve, uDirect;\r\nuniform float uBounce, uBounceCurve;\r\nuniform float uSpillCentre, uSpillWidth, uSpillFloor;\r\nuniform float uAmount, uWarp, uWarpScale, uFlow, uRoughness, uLacunarity, uMotes, uMoteScale;\r\nuniform float uAmbient, uContrast, uMidpoint, uSink, uGlow;\r\nuniform float uGrain, uDither, uVignette;\r\nuniform float uSteer, uLift, uSweep, uParallax;\r\n\r\n#define OCTAVES 4\r\n\r\nvec2 hash2(vec2 p) {\r\n  p = vec2(dot(p, vec2(127.1, 311.7)), dot(p, vec2(269.5, 183.3)));\r\n  return -1.0 + 2.0 * fract(sin(p) * 43758.5453123);\r\n}\r\n\r\nfloat snoise(vec2 p) {\r\n  const float K1 = 0.366025404, K2 = 0.211324865;\r\n  vec2 i = floor(p + (p.x + p.y) * K1);\r\n  vec2 a = p - i + (i.x + i.y) * K2;\r\n  float m = step(a.y, a.x);\r\n  vec2 o = vec2(m, 1.0 - m);\r\n  vec2 b = a - o + K2;\r\n  vec2 c = a - 1.0 + 2.0 * K2;\r\n  vec3 h = max(0.5 - vec3(dot(a, a), dot(b, b), dot(c, c)), 0.0);\r\n  vec3 n = h * h * h * h * vec3(dot(a, hash2(i)), dot(b, hash2(i + o)), dot(c, hash2(i + 1.0)));\r\n  return dot(n, vec3(70.0));\r\n}\r\n\r\nfloat fbm(vec2 p) {\r\n  float v = 0.0, amp = 0.5;\r\n  for (int i = 0; i < OCTAVES; i++) {\r\n    v += amp * snoise(p);\r\n    p *= uLacunarity;\r\n    amp *= uRoughness;\r\n  }\r\n  return v;\r\n}\r\n\r\nvec3 ramp4(float t) {\r\n  vec3 c = mix(uColorA, uColorB, smoothstep(0.00, 0.36, t));\r\n  c = mix(c, uColorC, smoothstep(0.32, 0.70, t));\r\n  c = mix(c, uColorD, smoothstep(0.66, 1.00, t));\r\n  return c;\r\n}\r\n\r\nfloat triDither(vec2 fc) {\r\n  float a = fract(sin(dot(fc, vec2(12.9898, 78.233))) * 43758.5453);\r\n  float b = fract(sin(dot(fc + 17.0, vec2(12.9898, 78.233))) * 43758.5453);\r\n  return (a + b - 1.0) / 255.0;\r\n}\r\n\r\n\r\n// ---- house grain. ONE look across the collection: an integer hash (no sin() streaks),\r\n// triangular so it reads as film rather than static, weighted into the midtones so it\r\n// never crusts a black or a white. Static by default; uGrainAnim re-seeds it 24×/s.\r\nuniform float uGrainAnim;\r\nfloat houseGrain(vec2 fc) {\r\n  uvec2 q = uvec2(fc) * uvec2(1597334677u, 3812015801u)\r\n          + uint(floor(iTime * 24.0 * uGrainAnim)) * 2654435769u;\r\n  uint n = q.x ^ q.y; n = n * 1664525u + 1013904223u; n ^= n >> 16u; n *= 2246822519u; n ^= n >> 13u;\r\n  float a = float(n & 0xffffu) / 65535.0;\r\n  n *= 3266489917u; n ^= n >> 16u;\r\n  float b = float(n & 0xffffu) / 65535.0;\r\n  return a + b - 1.0;\r\n}\r\nvoid main() {\r\n  vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution) / iResolution.y;\r\n  uv *= uScale;                             // scale: zoom of the whole picture\r\n  vec2 iM = iMouse * uScale;                // the pointer, in the same zoomed space\r\n  float t = iTime * uSpeed;\r\n\r\n  vec2 p = uv - iM * uParallax;\r\n\r\n  // THE LIGHT HAS A DIRECTION and the picture is that direction, not a pattern. Everything\r\n  // below is a function of ONE number: how far along the light axis this pixel is. The noise\r\n  // is only allowed to perturb that number, which is the difference between a lit surface\r\n  // and a noise field with a nice palette on it.\r\n  float tilt = uTilt + sin(t * 0.13) * uRock + iM.x * uSteer;\r\n  vec2 dir = vec2(cos(tilt), sin(tilt));\r\n  float axis = dot(p, dir);\r\n  float across = dot(p, vec2(-dir.y, dir.x));\r\n\r\n  // the air the light crosses, as a domain-warped field. Held to uAmount: past about a third\r\n  // it stops perturbing the light and starts being the subject, and the direction dissolves.\r\n  vec2 q = vec2(fbm(p * uWarpScale + vec2(0.0, t * uFlow)),\r\n                fbm(p * uWarpScale + vec2(5.2, 1.3) - t * uFlow * 0.7));\r\n  float air = fbm(p + uWarp * q + vec2(t * 0.12, -t * 0.09)) * 0.5 + 0.5;\r\n\r\n  float horizon = uHorizon + sin(t * 0.09 + 2.1) * uBreathe - iM.y * uLift;\r\n  float alt = clamp(0.5 + (axis - horizon) * uSpread + (air - 0.5) * uAmount, 0.0, 1.0);\r\n\r\n  // THE OPENING IS NOT INFINITELY WIDE. A band across the light axis — never a disc — so the\r\n  // brightest part of the frame sits off to one side and the composition has a long empty end.\r\n  float ac = (across - uSpillCentre - iM.x * uSweep) / max(0.05, uSpillWidth);\r\n  float spill = mix(uSpillFloor, 1.0, exp(-ac * ac));\r\n\r\n  float direct = pow(alt, max(0.05, uCurve)) * uDirect * spill;\r\n\r\n  // THE BOUNCE. Light that has already been in the shade and come back out of it: it falls off\r\n  // the OTHER way, so the deep end lifts off the floor instead of dying to a flat black. This\r\n  // is the whole reason a shadow can be a subject rather than an absence.\r\n  float bounce = uBounce * pow(1.0 - alt, max(0.05, uBounceCurve));\r\n\r\n  float f = uAmbient + direct + bounce;\r\n  f += uMotes * snoise(p * uMoteScale + vec2(-t * 0.5, t * 0.35)) * 0.5 * alt;\r\n\r\n  f = clamp((f - uMidpoint) * uContrast + 0.5, 0.0, 1.0);\r\n\r\n  vec3 col = ramp4(f);\r\n  col += uColorD * uGlow * pow(f, 4.0);\r\n  col = mix(uBg, col, smoothstep(0.0, max(0.01, uSink), f) * 0.90 + 0.10);\r\n\r\n  col *= 1.0 - uVignette * dot(uv, uv);\r\n  { float hgL = clamp(dot(col, vec3(0.299, 0.587, 0.114)), 0.0, 1.0);\r\n    col += houseGrain(gl_FragCoord.xy) * uGrain * mix(1.0, 4.0 * hgL * (1.0 - hgL), 0.6); }\r\n  col += triDither(gl_FragCoord.xy) * uDither;\r\n\r\n  fragColor = vec4(clamp(col, 0.0, 1.0), 1.0);\r\n}";
let gl, program, raf = 0, visible = false, clock = 0, previous = 0;
try {
gl = canvas.getContext("webgl2", {alpha:false, antialias:false, depth:false, stencil:false});
if (!gl) return;
function compile(type, source) {
 const s=gl.createShader(type); gl.shaderSource(s,source); gl.compileShader(s);
 if (!gl.getShaderParameter(s,gl.COMPILE_STATUS)) throw new Error(gl.getShaderInfoLog(s));
 return s;
}
program=gl.createProgram();
for (const [type,source] of [[gl.VERTEX_SHADER,VERT],[gl.FRAGMENT_SHADER,FRAG]]) {
 const s=compile(type,source); gl.attachShader(program,s); gl.deleteShader(s);
}
gl.linkProgram(program);
if (!gl.getProgramParameter(program,gl.LINK_STATUS)) throw new Error("Shader link failed");
gl.useProgram(program); gl.bindVertexArray(gl.createVertexArray());
const uniform = name => gl.getUniformLocation(program,name);
for (const [key,value] of Object.entries(CONFIG)) {
 const name=key==="bgColor"?"uBg":"u"+key[0].toUpperCase()+key.slice(1);
 const loc=uniform(name);
 if (typeof value==="string") {
  gl.uniform3f(loc,parseInt(value.slice(1,3),16)/255,parseInt(value.slice(3,5),16)/255,parseInt(value.slice(5,7),16)/255);
 } else gl.uniform1f(loc,value);
}
const resolution=uniform("iResolution"), time=uniform("iTime"), pointer=uniform("iMouse");
const mouse={x:0,y:0,ax:0,ay:0,tx:0,ty:0};
function resize() {
 const r=host.getBoundingClientRect(), dpr=Math.min(devicePixelRatio||1,CONFIG.maxDpr);
 const w=Math.max(1,Math.round(r.width*dpr)), h=Math.max(1,Math.round(r.height*dpr));
 if(canvas.width!==w || canvas.height!==h) {canvas.width=w;canvas.height=h;}
 gl.viewport(0,0,w,h); gl.uniform2f(resolution,w,h);
}
let dirty=true;
new ResizeObserver(()=>{dirty=true;}).observe(host);
function aim(e) {
 const r=host.getBoundingClientRect();
 mouse.tx=(e.clientX-r.left-r.width/2)/r.height;
 mouse.ty=(r.height/2-(e.clientY-r.top))/r.height;
}
host.addEventListener("pointermove",aim,{passive:true});
host.addEventListener("pointerdown",aim,{passive:true});
function frame(now) {
 raf=0;
 if (!visible || document.hidden || reduced.matches || gl.isContextLost()) return;
 if(dirty){resize();dirty=false;}
 const ms=Math.min(50,Math.max(4.167,previous?now-previous:16.667));
 previous=now;clock+=ms/1000;
 const s=Math.min(2.2,ms*.06);
 mouse.ax+=(mouse.tx-mouse.ax)*.105*s;mouse.ay+=(mouse.ty-mouse.ay)*.105*s;
 mouse.x+=(mouse.ax-mouse.x)*.043*s;mouse.y+=(mouse.ay-mouse.y)*.043*s;
 gl.uniform1f(time,clock);gl.uniform2f(pointer,mouse.x,mouse.y);
 gl.drawArrays(gl.TRIANGLES,0,3);
 canvas.hidden=false;
 raf=requestAnimationFrame(frame);
}
function sync() {
 cancelAnimationFrame(raf);raf=0;previous=0;
 if(reduced.matches) canvas.hidden=true;
 if(visible&&!document.hidden&&!reduced.matches) raf=requestAnimationFrame(frame);
}
new IntersectionObserver(entries=>{visible=entries[0].isIntersecting;sync();}).observe(host);
document.addEventListener("visibilitychange",sync);
reduced.addEventListener("change",sync);
canvas.addEventListener("webglcontextlost",()=>{cancelAnimationFrame(raf);canvas.hidden=true;});
} catch(error) {
 canvas.hidden=true;
 console.warn("Fundo animado indisponível; usando fundo estático.",error);
}
});
})();
