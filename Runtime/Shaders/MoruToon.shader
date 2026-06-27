// ============================================================
// MoruToon - Lightweight Gimmick & Particle Shader
// Version: 0.1.0
// License: MIT
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
        // 4: Layer Blend, 5: Soft Particle, 6: Custom

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
        [Enum(Add,0,Multiply,1,Screen,2)] _BlendMode ("Blend Mode", Float) = 0
        _SubIntensity ("Sub Intensity", Range(0,3)) = 1.0

        // ============================================
        // Soft Particle
        // ============================================
        [Toggle] _SOFTPARTICLES_ON ("Soft Particle", Float) = 0
        _SoftDistance ("Soft Distance", Float) = 0.5

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

        Pass
        {
            Name "Forward"
            Blend [_SrcBlend] [_DstBlend]
            Cull [_Cull]
            ZWrite [_ZWrite]
            Lighting Off
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            // Feature toggles
            #pragma shader_feature _ _UVSCROLL_ON
            #pragma shader_feature _ _EMISSION_ON
            #pragma shader_feature _ _EMISSION_PULSE
            #pragma shader_feature _ _DISSOLVE_ON
            #pragma shader_feature _ _FLIPBOOK_ON
            #pragma shader_feature _ _LAYERBLEND_ON
            #pragma shader_feature _ _SOFTPARTICLES_ON

            #include "UnityCG.cginc"
            #include "Includes/moru_common.hlsl"

            // ============================================
            // Properties
            // ============================================
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Brightness;

            // UV Scroll
            float _ScrollSpeedU, _ScrollSpeedV;

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

            // Flipbook
            float _FlipbookTilesX, _FlipbookTilesY, _FlipbookFPS, _FlipbookBlend;

            // Layer Blend
            sampler2D _SubTex;
            float4 _SubTex_ST;
            float _BlendMode, _SubIntensity;

            // Soft Particle
            sampler2D_float _CameraDepthTexture;
            float _SoftDistance;

#ifdef SOFTPARTICLES_ON
            // Unity built-in for soft particles
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
#endif

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

                // --- Flipbook ---
                #if defined(_FLIPBOOK_ON)
                    float frame = floor(_Time.y * _FlipbookFPS);
                    uv = moruFlipbook(uv, _FlipbookTilesX, _FlipbookTilesY, frame);
                #endif

                // --- Main Texture ---
                fixed4 col = tex2D(_MainTex, uv);

                // --- Layer Blend ---
                #if defined(_LAYERBLEND_ON)
                    fixed4 subCol = tex2D(_SubTex, uv);
                    if (_BlendMode < 0.5)
                        col.rgb = moruBlendAdd(col.rgb, subCol.rgb, _SubIntensity);
                    else if (_BlendMode < 1.5)
                        col.rgb = moruBlendMultiply(col.rgb, subCol.rgb, _SubIntensity);
                    else
                        col.rgb = moruBlendScreen(col.rgb, subCol.rgb, _SubIntensity);
                #endif

                // --- Apply Vertex Color ---
                col *= i.color;
                col *= _Color;
                col *= _Brightness;

                // --- Emission ---
                #if defined(_EMISSION_ON)
                    fixed3 emis = tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
                    #if defined(_EMISSION_PULSE)
                        float pulse = moruEmissionPulse(_PulseSpeed, _PulseMin, _PulseMax);
                        emis *= pulse;
                    #endif
                    col.rgb += emis;
                #endif

                // --- Dissolve ---
                #if defined(_DISSOLVE_ON)
                    moruDissolve(col, uv, _DissolveTex, _DissolveAmount, _DissolveEdgeWidth, _DissolveEdgeColor);
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

    FallBack "Transparent/VertexLit"
}
