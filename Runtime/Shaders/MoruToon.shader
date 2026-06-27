// ============================================================
// MoruToon - Lightweight Gimmick & Particle Shader
// Version: 0.1.0
// License: MIT
//
// Features:
//   [Particle] UV Scroll, Emission, Flipbook, Soft Particle,
//              Color Ramp, HUE Shift, Distance Fade, Lifetime Fade
//   [Gimmick]  Dissolve, Layer Blend, Stencil, Mask, HUE Shift,
//              Distance Fade
//   [System]   Template Tab System, Blend/Cull/ZWrite切替
// ============================================================

Shader "MoruToon/Particle"
{
    Properties
    {
        // ============================================
        // Template Selector (Tab System)
        // ============================================
        [HideInInspector] _TemplateMode ("Template Mode", Float) = 0
        // 0: Basic, 1: UV Scroll, 2: Dissolve, 3: Flipbook,
        // 4: Layer Blend, 5: Soft Particle, 6: Stencil Portal,
        // 7: Particle FX, 8: Custom

        // ============================================
        // Main
        // ============================================
        [HDR] _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _Brightness ("Brightness", Float) = 1.0

        // ============================================
        // UV Scroll
        // ============================================
        [Toggle] _UVSCROLL_ON ("UV Scroll", Float) = 0
        _ScrollSpeedU ("Scroll Speed U", Float) = 0.1
        _ScrollSpeedV ("Scroll Speed V", Float) = 0.0
        [Toggle] _ROTATION_ON ("Rotation", Float) = 0
        _RotationSpeed ("Rotation Speed", Float) = 0.5

        // ============================================
        // Emission
        // ============================================
        [Toggle] _EMISSION_ON ("Emission", Float) = 0
        _EmissionMap ("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        [Toggle] _EMISSION_PULSE ("Pulse", Float) = 0
        _PulseSpeed ("Pulse Speed", Float) = 2.0
        _PulseMin ("Pulse Min", Range(0,1)) = 0.5
        _PulseMax ("Pulse Max", Range(0,3)) = 1.5

        // ============================================
        // Dissolve
        // ============================================
        [Toggle] _DISSOLVE_ON ("Dissolve", Float) = 0
        _DissolveTex ("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0.0
        _DissolveEdgeWidth ("Edge Width", Range(0,0.5)) = 0.05
        _DissolveEdgeColor ("Edge Color", Color) = (1,0.5,0,1)
        [Toggle] _DISSOLVE_LIFETIME ("Lifetime Dissolve", Float) = 0
        _DissolveDelay ("Dissolve Delay", Range(0,1)) = 0.3
        _DissolveSpeed ("Dissolve Speed", Range(0.1,10)) = 1.5

        // ============================================
        // Flipbook
        // ============================================
        [Toggle] _FLIPBOOK_ON ("Flipbook", Float) = 0
        _FlipbookTilesX ("Tiles X", Float) = 4
        _FlipbookTilesY ("Tiles Y", Float) = 4
        _FlipbookFPS ("FPS", Float) = 15
        _FlipbookBlend ("Frame Blend", Range(0,1)) = 0.0

        // ============================================
        // Layer Blend
        // ============================================
        [Toggle] _LAYERBLEND_ON ("Layer Blend", Float) = 0
        _SubTex ("Sub Texture", 2D) = "black" {}
        [Enum(Add,0,Multiply,1,Screen,2,Overlay,3)] _BlendMode ("Blend Mode", Float) = 0
        _SubIntensity ("Sub Intensity", Range(0,3)) = 1.0
        _SubScrollSpeedU ("Sub Scroll U", Float) = 0.0
        _SubScrollSpeedV ("Sub Scroll V", Float) = 0.0

        // ============================================
        // Soft Particle
        // ============================================
        [Toggle] _SOFTPARTICLES_ON ("Soft Particle", Float) = 0
        _SoftDistance ("Soft Distance", Float) = 0.5

        // ============================================
        // Color Ramp
        // ============================================
        [Toggle] _COLORRAMP_ON ("Color Ramp", Float) = 0
        _ColorRampTex ("Color Ramp", 2D) = "white" {}
        _ColorRampIntensity ("Ramp Intensity", Range(0,1)) = 1.0

        // ============================================
        // HUE Shift
        // ============================================
        [Toggle] _HUESHIFT_ON ("HUE Shift", Float) = 0
        _HueShift ("Hue Shift", Range(0,1)) = 0.0
        _HueShiftSpeed ("Auto Shift Speed", Float) = 0.0

        // ============================================
        // Distance Fade
        // ============================================
        [Toggle] _DISTANCEFADE_ON ("Distance Fade", Float) = 0
        _FadeNear ("Fade Near", Float) = 2.0
        _FadeFar ("Fade Far", Float) = 10.0

        // ============================================
        // Particle Lifetime
        // ============================================
        [Toggle] _LIFETIMEFADE_ON ("Lifetime Fade", Float) = 0
        _LifetimeFadeIn ("Fade In End", Range(0,1)) = 0.1
        _LifetimeFadeOut ("Fade Out Start", Range(0,1)) = 0.7

        // ============================================
        // Mask (Visible / Hide)
        // ============================================
        [Toggle] _MASK_ON ("Mask", Float) = 0
        _VisibleMaskTex ("Visible Mask (White=Show)", 2D) = "white" {}
        _VisibleMaskStrength ("Mask Strength", Range(0,1)) = 1.0
        _HideMaskTex ("Hide Mask (White=Hide)", 2D) = "black" {}
        _HideMaskStrength ("Hide Mask Strength", Range(0,1)) = 1.0

        // ============================================
        // Stencil
        // ============================================
        [Toggle] _STENCIL_ON ("Stencil", Float) = 0
        [Enum(Draw,0,Mask,1,Cutout,2)] _StencilMode ("Stencil Mode", Float) = 0
        _StencilRef ("Stencil Reference", Range(0,255)) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Compare", Float) = 8    // Always
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Stencil Pass", Float) = 2            // Replace

        // ============================================
        // Rendering Options
        // ============================================
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5    // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10   // One (Additive)
        [Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 0          // Off
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }
        LOD 100

        // Stencil設定（グローバル）
        Stencil
        {
            Ref [_StencilRef]
            Comp [_StencilComp]
            Pass [_StencilPass]
        }

        Pass
        {
            Name "Forward"
            Blend [_SrcBlend] [_DstBlend]
            Cull [_Cull]
            ZWrite [_ZWrite]
            Lighting Off
            ZTest LEqual

            // Stencil Cutout / Mask時は色を描かない
            ColorMask [_ColorMask]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            // Feature toggles
            #pragma shader_feature _ _UVSCROLL_ON
            #pragma shader_feature _ _ROTATION_ON
            #pragma shader_feature _ _EMISSION_ON
            #pragma shader_feature _ _EMISSION_PULSE
            #pragma shader_feature _ _DISSOLVE_ON
            #pragma shader_feature _ _DISSOLVE_LIFETIME
            #pragma shader_feature _ _FLIPBOOK_ON
            #pragma shader_feature _ _LAYERBLEND_ON
            #pragma shader_feature _ _SOFTPARTICLES_ON
            #pragma shader_feature _ _COLORRAMP_ON
            #pragma shader_feature _ _HUESHIFT_ON
            #pragma shader_feature _ _DISTANCEFADE_ON
            #pragma shader_feature _ _LIFETIMEFADE_ON
            #pragma shader_feature _ _MASK_ON

            #include "UnityCG.cginc"
            #include "Includes/moru_common.hlsl"

            // ============================================
            // Properties
            // ============================================
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Brightness;

            // UV Scroll / Rotation
            float _ScrollSpeedU, _ScrollSpeedV;
            float _RotationSpeed;

            // Emission
            sampler2D _EmissionMap;
            float4 _EmissionMap_ST;
            fixed4 _EmissionColor;
            float _PulseSpeed, _PulseMin, _PulseMax;

            // Dissolve
            sampler2D _DissolveTex;
            float4 _DissolveTex_ST;
            float _DissolveAmount, _DissolveEdgeWidth;
            fixed4 _DissolveEdgeColor;
            float _DissolveDelay, _DissolveSpeed;

            // Flipbook
            float _FlipbookTilesX, _FlipbookTilesY, _FlipbookFPS, _FlipbookBlend;

            // Layer Blend
            sampler2D _SubTex;
            float4 _SubTex_ST;
            float _BlendMode, _SubIntensity;
            float _SubScrollSpeedU, _SubScrollSpeedV;

            // Soft Particle
            sampler2D_float _CameraDepthTexture;
            float _SoftDistance;

            // Color Ramp
            sampler2D _ColorRampTex;
            float _ColorRampIntensity;

            // HUE Shift
            float _HueShift, _HueShiftSpeed;

            // Distance Fade
            float _FadeNear, _FadeFar;

            // Particle Lifetime
            float _LifetimeFadeIn, _LifetimeFadeOut;

            // Mask
            sampler2D _VisibleMaskTex;
            float4 _VisibleMaskTex_ST;
            float _VisibleMaskStrength;
            sampler2D _HideMaskTex;
            float4 _HideMaskTex_ST;
            float _HideMaskStrength;

            // Stencil
            float _StencilMode;

            // ============================================
            // Vertex
            // ============================================
            v2f_moru vert(appdata_moru v)
            {
                v2f_moru o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

#if defined(_SOFTPARTICLES_ON)
                COMPUTE_EYEDEPTH(o.eyeDepth);
#endif

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            // ============================================
            // Fragment
            // ============================================
            fixed4 frag(v2f_moru i) : SV_Target
            {
                float2 uv = i.uv;

                // --- UV Scroll ---
                #if defined(_UVSCROLL_ON)
                    uv = moruUVScroll(uv, _ScrollSpeedU, _ScrollSpeedV);
                #endif

                // --- Rotation ---
                #if defined(_ROTATION_ON)
                    uv = moruRotateUV(uv, _Time.y * _RotationSpeed);
                #endif

                // --- Flipbook ---
                #if defined(_FLIPBOOK_ON)
                    float frame = _Time.y * _FlipbookFPS;
                    #if defined(_FLIPBOOK_BLEND)
                        float2 uvNext;
                        float blendFactor;
                        uv = moruFlipbookBlended(uv, _FlipbookTilesX, _FlipbookTilesY, frame, _FlipbookBlend, uvNext, blendFactor);
                    #else
                        uv = moruFlipbook(uv, _FlipbookTilesX, _FlipbookTilesY, floor(frame));
                    #endif
                #endif

                // --- Main Texture ---
                fixed4 col = tex2D(_MainTex, uv);

                // --- Layer Blend ---
                #if defined(_LAYERBLEND_ON)
                    float2 subUV = uv;
                    subUV = moruUVScroll(subUV, _SubScrollSpeedU, _SubScrollSpeedV);
                    fixed4 subCol = tex2D(_SubTex, subUV);
                    if (_BlendMode < 0.5)
                        col.rgb = moruBlendAdd(col.rgb, subCol.rgb, _SubIntensity);
                    else if (_BlendMode < 1.5)
                        col.rgb = moruBlendMultiply(col.rgb, subCol.rgb, _SubIntensity);
                    else if (_BlendMode < 2.5)
                        col.rgb = moruBlendScreen(col.rgb, subCol.rgb, _SubIntensity);
                    else
                        col.rgb = moruBlendOverlay(col.rgb, subCol.rgb, _SubIntensity);
                #endif

                // --- Mask (Visible / Hide) ---
                #if defined(_MASK_ON)
                    float visMask = moruApplyMask(uv, _VisibleMaskTex, _VisibleMaskStrength);
                    float hideMask = moruApplyHideMask(uv, _HideMaskTex, _HideMaskStrength);
                    col.a *= visMask * hideMask;
                #endif

                // --- Apply Vertex Color ---
                col *= i.color;
                col *= _Color;
                col *= _Brightness;

                // --- Color Ramp ---
                #if defined(_COLORRAMP_ON)
                    float rampT = col.a;
                    fixed3 rampColor = moruColorRamp(rampT, _ColorRampTex);
                    col.rgb = lerp(col.rgb, col.rgb * rampColor, _ColorRampIntensity);
                #endif

                // --- HUE Shift ---
                #if defined(_HUESHIFT_ON)
                    float hueOffset = _HueShift;
                    if (_HueShiftSpeed > 0.0)
                        hueOffset += frac(_Time.y * _HueShiftSpeed);
                    col.rgb = moruHueShift(col.rgb, hueOffset);
                #endif

                // --- Emission ---
                #if defined(_EMISSION_ON)
                    fixed3 emis = tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
                    #if defined(_EMISSION_PULSE)
                        float pulse = moruEmissionPulse(_PulseSpeed, _PulseMin, _PulseMax);
                        emis *= pulse;
                    #endif
                    col.rgb += emis;
                #endif

                // --- Particle Lifetime Fade ---
                #if defined(_LIFETIMEFADE_ON)
                    float agePercent = i.uv.z; // TEXCOORD0.z from Custom Vertex Streams
                    float lifeFade = moruLifetimeFade(agePercent, _LifetimeFadeIn, _LifetimeFadeOut);
                    col.a *= lifeFade;
                #endif

                // --- Dissolve ---
                #if defined(_DISSOLVE_ON)
                    float dissAmount = _DissolveAmount;
                    #if defined(_DISSOLVE_LIFETIME)
                        float agePercent = i.uv.z;
                        dissAmount = moruLifetimeDissolveAmount(agePercent, _DissolveDelay, _DissolveSpeed);
                    #endif
                    moruDissolve(col, uv, _DissolveTex, dissAmount, _DissolveEdgeWidth, _DissolveEdgeColor);
                #endif

                // --- Distance Fade ---
                #if defined(_DISTANCEFADE_ON)
                    float distFade = moruDistanceFade(i.worldPos, _WorldSpaceCameraPos, _FadeNear, _FadeFar);
                    col.a *= distFade;
                #endif

                // --- Soft Particle ---
                #if defined(_SOFTPARTICLES_ON)
                    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                    float partZ = i.eyeDepth;
                    float fade = saturate(_SoftDistance / (sceneZ - partZ));
                    col.a *= fade;
                #endif

                // --- Fog ---
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }

    // Stencil Mask専用（色を描かない）
    SubShader
    {
        Tags
        {
            "Queue" = "Geometry-1"
            "RenderType" = "Opaque"
        }
        LOD 100

        Pass
        {
            Name "StencilMask"
            ColorMask 0
            ZWrite Off

            Stencil
            {
                Ref [_StencilRef]
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert_mask
            #pragma fragment frag_mask
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata_mask
            {
                float4 vertex : POSITION;
            };

            struct v2f_mask
            {
                float4 vertex : SV_POSITION;
            };

            v2f_mask vert_mask(appdata_mask v)
            {
                v2f_mask o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag_mask(v2f_mask i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }
    }

    FallBack "Transparent/VertexLit"
}
