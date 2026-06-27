#ifndef MORU_COMMON_INCLUDED
#define MORU_COMMON_INCLUDED

// ============================================================
// MoruToon Common Header
// 共通構造体・関数・マクロ定義
// ============================================================

// -------------------------------------
// Appdata (Vertex Input)
// -------------------------------------
struct appdata_moru
{
    float4 vertex   : POSITION;
    fixed4 color    : COLOR;
    float2 uv       : TEXCOORD0;
    float3 normal   : NORMAL;
};

// -------------------------------------
// V2F (Vertex to Fragment)
// -------------------------------------
struct v2f_moru
{
    float4 vertex       : SV_POSITION;
    fixed4 color        : COLOR;
    float2 uv           : TEXCOORD0;    // Main UV
    float  agePercent   : TEXCOORD1;    // Particle age (0-1), from Custom Vertex Streams or 0
    float4 screenPos    : TEXCOORD2;
#if defined(_SOFTPARTICLES_ON)
    float eyeDepth      : TEXCOORD3;
#endif
    float3 worldPos     : TEXCOORD4;
    UNITY_FOG_COORDS(5)
};

// -------------------------------------
// UV Scroll
// -------------------------------------
inline float2 moruUVScroll(float2 uv, float speedU, float speedV)
{
    return uv + float2(speedU, speedV) * _Time.y;
}

// -------------------------------------
// Rotation UV
// -------------------------------------
inline float2 moruRotateUV(float2 uv, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    float2 center = float2(0.5, 0.5);
    uv -= center;
    uv = float2(
        dot(float2(c, -s), uv),
        dot(float2(s, c), uv)
    );
    uv += center;
    return uv;
}

// -------------------------------------
// Dissolve
// -------------------------------------
inline void moruDissolve(
    inout fixed4 color,
    float2 uv,
    sampler2D dissolveTex,
    float dissolveAmount,
    float edgeWidth,
    fixed4 edgeColor)
{
    float noise = tex2D(dissolveTex, uv).r;
    clip(noise - dissolveAmount);

    float edge = smoothstep(dissolveAmount, dissolveAmount + edgeWidth, noise);
    color.rgb += edgeColor.rgb * (1.0 - edge) * edgeColor.a;
}

// -------------------------------------
// Flipbook
// -------------------------------------
inline float2 moruFlipbook(float2 uv, float tilesX, float tilesY, float frame)
{
    float2 tileUV = frac(uv * float2(tilesX, tilesY));
    float totalFrames = tilesX * tilesY;
    frame = fmod(frame, totalFrames);
    float2 tileOffset = float2(
        fmod(frame, tilesX) / tilesX,
        floor(frame / tilesX) / tilesY
    );
    return tileUV / float2(tilesX, tilesY) + tileOffset;
}

// -------------------------------------
// Flipbook with Blend (returns two UVs + blend factor)
// -------------------------------------
inline void moruFlipbookBlended(
    float2 uv,
    float tilesX,
    float tilesY,
    float frame,
    float blend,
    out float2 uvA,
    out float2 uvB,
    out float blendFactor)
{
    float totalFrames = tilesX * tilesY;
    frame = fmod(frame, totalFrames);

    float frameFrac = frac(frame);
    int frame0 = (int)frame;
    int frame1 = (int)(frame + 1.0) % (int)totalFrames;

    blendFactor = frameFrac * blend;

    float2 tileUV = frac(uv * float2(tilesX, tilesY));
    float2 scale = float2(1.0 / tilesX, 1.0 / tilesY);

    // frame0
    float2 offset0 = float2(fmod((float)frame0, tilesX), floor((float)frame0 / tilesX)) * scale;
    uvA = tileUV * scale + offset0;

    // frame1
    float2 offset1 = float2(fmod((float)frame1, tilesX), floor((float)frame1 / tilesX)) * scale;
    uvB = tileUV * scale + offset1;
}

// -------------------------------------
// Layer Blend Modes
// -------------------------------------
inline fixed3 moruBlendAdd(fixed3 base, fixed3 layer, float intensity)
{
    return base + layer * intensity;
}

inline fixed3 moruBlendMultiply(fixed3 base, fixed3 layer, float intensity)
{
    return lerp(base, base * layer, intensity);
}

inline fixed3 moruBlendScreen(fixed3 base, fixed3 layer, float intensity)
{
    return lerp(base, 1.0 - (1.0 - base) * (1.0 - layer), intensity);
}

inline fixed3 moruBlendOverlay(fixed3 base, fixed3 layer, float intensity)
{
    fixed3 overlay = base < 0.5 ? 2.0 * base * layer : 1.0 - 2.0 * (1.0 - base) * (1.0 - layer);
    return lerp(base, overlay, intensity);
}

// -------------------------------------
// Emission Pulse
// -------------------------------------
inline float moruEmissionPulse(float speed, float minVal, float maxVal)
{
    return lerp(minVal, maxVal, sin(_Time.y * speed) * 0.5 + 0.5);
}

// -------------------------------------
// HUE Shift
// -------------------------------------
inline fixed3 moruRGBtoHSV(fixed3 c)
{
    fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
    fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

inline fixed3 moruHSVtoRGB(fixed3 c)
{
    fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

inline fixed3 moruHueShift(fixed3 color, float shift)
{
    fixed3 hsv = moruRGBtoHSV(color);
    hsv.x = frac(hsv.x + shift);
    return moruHSVtoRGB(hsv);
}

// -------------------------------------
// Color Ramp
// -------------------------------------
inline fixed3 moruColorRamp(float t, sampler2D rampTex)
{
    return tex2D(rampTex, float2(saturate(t), 0.5)).rgb;
}

// -------------------------------------
// Distance Fade
// -------------------------------------
inline float moruDistanceFade(float3 worldPos, float3 cameraPos, float near, float far)
{
    float dist = distance(worldPos, cameraPos);
    return smoothstep(far, near, dist);
}

// -------------------------------------
// Particle Lifetime Alpha
// -------------------------------------
inline float moruLifetimeFade(float agePercent, float fadeInEnd, float fadeOutStart)
{
    float fadeIn = smoothstep(0.0, fadeInEnd, agePercent);
    float fadeOut = 1.0 - smoothstep(fadeOutStart, 1.0, agePercent);
    return fadeIn * fadeOut;
}

inline float moruLifetimeDissolveAmount(float agePercent, float startDelay, float speed)
{
    return saturate((agePercent - startDelay) * speed);
}

// -------------------------------------
// Mask (Visible / Hide)
// -------------------------------------
inline float moruApplyMask(float2 uv, sampler2D maskTex, float strength)
{
    float maskVal = tex2D(maskTex, uv).r;
    return lerp(1.0, maskVal, strength);
}

inline float moruApplyHideMask(float2 uv, sampler2D maskTex, float strength)
{
    float maskVal = tex2D(maskTex, uv).r;
    return lerp(1.0, 1.0 - maskVal, strength);
}

#endif // MORU_COMMON_INCLUDED
