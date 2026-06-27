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
    float4 screenPos    : TEXCOORD1;    // Screen position (for soft particle)
#if defined(_SOFTPARTICLES_ON)
    float eyeDepth      : TEXCOORD2;    // Eye depth for soft particles
#endif
    UNITY_FOG_COORDS(3)
};

// -------------------------------------
// Blend Mode Helpers
// -------------------------------------
// PropertiesでBlend Modeを切り替えるためのenum定義
// [Enum(UnityEngine.Rendering.BlendMode)] を使用してインスペクターから選択

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

// -------------------------------------
// Emission Pulse
// -------------------------------------
inline float moruEmissionPulse(float speed, float minVal, float maxVal)
{
    return lerp(minVal, maxVal, sin(_Time.y * speed) * 0.5 + 0.5);
}

#endif // MORU_COMMON_INCLUDED
