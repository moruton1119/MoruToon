// ============================================================
// MoruToon - Lightweight Gimmick & Particle Shader
// Version: 0.3.0
// License: MIT
// ============================================================

Shader "MoruToon/Particle"
{
    Properties
    {
        [HideInInspector] _TemplateMode ("Template Mode", Float) = 0

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
        _DissolveSoftness ("Dissolve Softness", Range(0.001,1)) = 0.2

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
        // Mask
        // ============================================
        [Toggle] _MASK_ON ("Mask", Float) = 0
        _VisibleMaskTex ("Visible Mask (White=Show)", 2D) = "white" {}
        _VisibleMaskStrength ("Mask Strength", Range(0,1)) = 1.0
        _HideMaskTex ("Hide Mask (White=Hide)", 2D) = "black" {}
        _HideMaskStrength ("Hide Mask Strength", Range(0,1)) = 1.0

        // ============================================
        // Fresnel / フレネル（輪郭光・正面透け）
        // ============================================
        [Toggle] _FRESNEL_ON ("Fresnel", Float) = 0
        _FresnelPower ("Fresnel Power", Range(0.1,10)) = 3.0
        _FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
        _FresnelAlpha ("Fresnel Alpha (正面の透け)", Range(0,1)) = 0.0

        // ============================================
        // Normal Map / ノーマルマップ
        // ============================================
        [Toggle] _NORMALMAP_ON ("Normal Map", Float) = 0
        [Normal] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScrollU ("Normal Scroll U", Float) = 0.1
        _NormalScrollV ("Normal Scroll V", Float) = 0.1
        _NormalStrength ("Normal Strength", Range(0,2)) = 1.0

        // ============================================
        // Cubemap Reflection / キューブマップ反射
        // ============================================
        [Toggle] _CUBEMAP_ON ("Cubemap", Float) = 0
        [NoScaleOffset] _Cubemap ("Cubemap", Cube) = "_Skybox" {}
        [HDR] _ReflectColor ("Reflect Color", Color) = (1,1,1,1)

        // ============================================
        // Parallax / パララックス（視差奥行き）
        // ============================================
        [Toggle] _PARALLAX_ON ("Parallax", Float) = 0
        _ParallaxDepth ("Parallax Depth", Range(-0.1, 0.1)) = 0.02

        // ============================================
        // UV Distortion / UV歪み（水流ゆらぎ）
        // ============================================
        [Toggle] _DISTORTION_ON ("UV Distortion", Float) = 0
        _DistortionTex ("Distortion Texture", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0, 0.5)) = 0.05
        _DistortScrollU ("Distortion Scroll U", Float) = -0.05
        _DistortScrollV ("Distortion Scroll V", Float) = 0.07

        // ============================================
        // GrabPass Refraction / 背景屈折
        // ============================================
        [Toggle] _REFRACTION_ON ("Refraction (GrabPass)", Float) = 0
        _RefractionStrength ("Refraction Strength", Range(0, 0.5)) = 0.05

        // ============================================
        // Black Transparency / 黒透過
        // ============================================
        [Toggle] _BLACKTRANSPARENCY_ON ("Black Transparency", Float) = 0
        _BlackThreshold ("Threshold", Range(0,1)) = 0.1
        _BlackSoftness ("Softness", Range(0,1)) = 0.3

        // ============================================
        // Stencil
        // ============================================
        _StencilRef ("Stencil Reference", Range(0,255)) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Compare", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Stencil Pass", Float) = 2

        // ============================================
        // Rendering
        // ============================================
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10
        [Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 0
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

        Stencil
        {
            Ref [_StencilRef]
            Comp [_StencilComp]
            Pass [_StencilPass]
        }

        GrabPass
        {
            Tags { "LightMode" = "Always" }
            "_MoruGrabTexture"
        }

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
            #pragma shader_feature _ _FRESNEL_ON
            #pragma shader_feature _ _NORMALMAP_ON
            #pragma shader_feature _ _CUBEMAP_ON
            #pragma shader_feature _ _PARALLAX_ON
            #pragma shader_feature _ _DISTORTION_ON
            #pragma shader_feature _ _REFRACTION_ON
            #pragma shader_feature _ _BLACKTRANSPARENCY_ON

            #include "UnityCG.cginc"
            #include "Includes/moru_common.hlsl"

            // ============================================
            // Properties
            // ============================================
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Brightness;

            float _ScrollSpeedU, _ScrollSpeedV, _RotationSpeed;

            sampler2D _EmissionMap;
            fixed4 _EmissionColor;
            float _PulseSpeed, _PulseMin, _PulseMax;

            sampler2D _DissolveTex;
            float4 _DissolveTex_ST;
            float _DissolveAmount, _DissolveEdgeWidth, _DissolveSoftness;
            fixed4 _DissolveEdgeColor;
            float _DissolveDelay, _DissolveSpeed;

            float _FlipbookTilesX, _FlipbookTilesY, _FlipbookFPS, _FlipbookBlend;

            sampler2D _SubTex;
            float _BlendMode, _SubIntensity;
            float _SubScrollSpeedU, _SubScrollSpeedV;

            sampler2D_float _CameraDepthTexture;
            float _SoftDistance;

            sampler2D _ColorRampTex;
            float _ColorRampIntensity;

            float _HueShift, _HueShiftSpeed;

            float _FadeNear, _FadeFar;

            float _LifetimeFadeIn, _LifetimeFadeOut;

            sampler2D _VisibleMaskTex;
            float _VisibleMaskStrength;
            sampler2D _HideMaskTex;
            float _HideMaskStrength;

            // Fresnel
            float _FresnelPower;
            fixed4 _FresnelColor;
            float _FresnelAlpha;

            // Normal Map
            sampler2D _NormalMap;
            float4 _NormalMap_ST;
            float _NormalScrollU, _NormalScrollV, _NormalStrength;

            // Cubemap
            samplerCUBE _Cubemap;
            fixed4 _ReflectColor;

            // Parallax
            float _ParallaxDepth;

            // Distortion
            sampler2D _DistortionTex;
            float _DistortionStrength;
            float _DistortScrollU, _DistortScrollV;

            // Refraction
            sampler2D _MoruGrabTexture;
            float _RefractionStrength;

            // Black Transparency
            float _BlackThreshold, _BlackSoftness;

            // ============================================
            // Vertex
            // ============================================
            struct v2f_moru_full
            {
                float4 vertex       : SV_POSITION;
                fixed4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float  agePercent   : TEXCOORD1;
                float4 screenPos    : TEXCOORD2;
#if defined(_SOFTPARTICLES_ON)
                float eyeDepth      : TEXCOORD3;
#endif
                float3 worldPos     : TEXCOORD4;
                float3 worldNormal  : TEXCOORD5;
                float3 worldViewDir : TEXCOORD6;
                float3 tangentViewDir : TEXCOORD7;
                UNITY_FOG_COORDS(8)
            };

            v2f_moru_full vert(appdata_full v)
            {
                v2f_moru_full o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.agePercent = 0.0;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldViewDir = WorldSpaceViewDir(v.vertex);

                // Tangent space view dir for parallax
                float3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
                float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                float3 wBitangent = cross(o.worldNormal, wTangent) * tangentSign;
                o.tangentViewDir = float3(
                    dot(wTangent, o.worldViewDir),
                    dot(wBitangent, o.worldViewDir),
                    dot(o.worldNormal, o.worldViewDir)
                );

#if defined(_SOFTPARTICLES_ON)
                COMPUTE_EYEDEPTH(o.eyeDepth);
#endif
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            // ============================================
            // Fragment
            // ============================================
            fixed4 frag(v2f_moru_full i) : SV_Target
            {
                float2 uv = i.uv;
                float agePercent = i.agePercent;
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.worldViewDir);

                // --- UV Scroll ---
                #if defined(_UVSCROLL_ON)
                    uv = moruUVScroll(uv, _ScrollSpeedU, _ScrollSpeedV);
                #endif

                // --- Rotation ---
                #if defined(_ROTATION_ON)
                    uv = moruRotateUV(uv, _Time.y * _RotationSpeed);
                #endif

                // --- UV Distortion ---
                #if defined(_DISTORTION_ON)
                {
                    float2 distortUV = uv + float2(_DistortScrollU, _DistortScrollV) * _Time.y;
                    float noiseX = tex2D(_DistortionTex, distortUV).r;
                    float noiseY = tex2D(_DistortionTex, distortUV + float2(0.3, 0.7)).r;
                    float2 distortVec = (float2(noiseX, noiseY) * 2.0 - 1.0) * _DistortionStrength;
                    uv += distortVec;
                }
                #endif

                // --- Parallax ---
                #if defined(_PARALLAX_ON)
                {
                    float3 tViewDir = normalize(i.tangentViewDir);
                    uv += tViewDir.xy * _ParallaxDepth;
                }
                #endif

                // --- Normal Map ---
                float3 normalMapVal = float3(0,0,1);
                #if defined(_NORMALMAP_ON)
                {
                    float2 nrmUV = TRANSFORM_TEX(uv, _NormalMap) + float2(_NormalScrollU, _NormalScrollV) * _Time.y;
                    normalMapVal = UnpackNormal(tex2D(_NormalMap, nrmUV));
                    normalMapVal.xy *= _NormalStrength;
                }
                #endif

                // --- Refraction (GrabPass) ---
                fixed3 refractionColor = fixed3(0,0,0);
                #if defined(_REFRACTION_ON)
                {
                    float2 screenUV = i.screenPos.xy / max(i.screenPos.w, 0.001);
                    float2 refractOffset = normalMapVal.xy * _RefractionStrength;
                    refractionColor = tex2D(_MoruGrabTexture, screenUV + refractOffset).rgb;
                }
                #endif

                // --- Flipbook ---
                #if defined(_FLIPBOOK_ON)
                {
                    float frame = _Time.y * _FlipbookFPS;
                    if (_FlipbookBlend > 0.001)
                    {
                        float2 uvA, uvB;
                        float blendFactor;
                        moruFlipbookBlended(uv, _FlipbookTilesX, _FlipbookTilesY, frame, _FlipbookBlend, uvA, uvB, blendFactor);
                        col = lerp(tex2D(_MainTex, uvA), tex2D(_MainTex, uvB), blendFactor);
                    }
                    else
                    {
                        uv = moruFlipbook(uv, _FlipbookTilesX, _FlipbookTilesY, floor(frame));
                    }
                }
                #endif

                // --- Main Texture ---
                fixed4 col = tex2D(_MainTex, uv);

                // --- Refraction blend ---
                #if defined(_REFRACTION_ON)
                    col.rgb = lerp(refractionColor, col.rgb, col.a);
                #endif

                // --- Layer Blend ---
                #if defined(_LAYERBLEND_ON)
                {
                    float2 subUV = moruUVScroll(uv, _SubScrollSpeedU, _SubScrollSpeedV);
                    fixed4 subCol = tex2D(_SubTex, subUV);
                    if (_BlendMode < 0.5)
                        col.rgb = moruBlendAdd(col.rgb, subCol.rgb, _SubIntensity);
                    else if (_BlendMode < 1.5)
                        col.rgb = moruBlendMultiply(col.rgb, subCol.rgb, _SubIntensity);
                    else if (_BlendMode < 2.5)
                        col.rgb = moruBlendScreen(col.rgb, subCol.rgb, _SubIntensity);
                    else
                        col.rgb = moruBlendOverlay(col.rgb, subCol.rgb, _SubIntensity);
                }
                #endif

                // --- Mask ---
                #if defined(_MASK_ON)
                {
                    float visMask = moruApplyMask(uv, _VisibleMaskTex, _VisibleMaskStrength);
                    float hideMask = moruApplyHideMask(uv, _HideMaskTex, _HideMaskStrength);
                    col.a *= visMask * hideMask;
                }
                #endif

                // --- Black Transparency ---
                #if defined(_BLACKTRANSPARENCY_ON)
                {
                    float blackAlpha = moruBlackTransparency(col.rgb, _BlackThreshold, _BlackSoftness);
                    col.a *= 1.0 - blackAlpha;
                }
                #endif

                // --- Apply Vertex Color ---
                col *= i.color;
                col *= _Color;
                col *= _Brightness;

                // --- Fresnel ---
                #if defined(_FRESNEL_ON)
                {
                    float ndotv = saturate(dot(normal, viewDir));
                    float fresnel = pow(1.0 - ndotv, _FresnelPower);
                    col.rgb += _FresnelColor.rgb * fresnel * _FresnelColor.a;
                    col.a = lerp(col.a, col.a * fresnel, _FresnelAlpha);
                }
                #endif

                // --- Cubemap Reflection ---
                #if defined(_CUBEMAP_ON)
                {
                    float3 reflectDir = reflect(-viewDir, normal);
                    float3 envColor = texCUBE(_Cubemap, reflectDir).rgb;
                    float ndotv = saturate(dot(normal, viewDir));
                    float fresnel = pow(1.0 - ndotv, 3.0);
                    col.rgb += envColor * _ReflectColor.rgb * _ReflectColor.a * fresnel;
                }
                #endif

                // --- Color Ramp ---
                #if defined(_COLORRAMP_ON)
                {
                    float rampT = col.a;
                    fixed3 rampColor = moruColorRamp(rampT, _ColorRampTex);
                    col.rgb = lerp(col.rgb, col.rgb * rampColor, _ColorRampIntensity);
                }
                #endif

                // --- HUE Shift ---
                #if defined(_HUESHIFT_ON)
                {
                    float hueOffset = _HueShift;
                    if (_HueShiftSpeed > 0.0)
                        hueOffset += frac(_Time.y * _HueShiftSpeed);
                    col.rgb = moruHueShift(col.rgb, hueOffset);
                }
                #endif

                // --- Emission ---
                #if defined(_EMISSION_ON)
                {
                    fixed3 emis = tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
                    #if defined(_EMISSION_PULSE)
                        float pulse = moruEmissionPulse(_PulseSpeed, _PulseMin, _PulseMax);
                        emis *= pulse;
                    #endif
                    col.rgb += emis;
                }
                #endif

                // --- Lifetime Fade ---
                #if defined(_LIFETIMEFADE_ON)
                {
                    float lifeFade = moruLifetimeFade(agePercent, _LifetimeFadeIn, _LifetimeFadeOut);
                    col.a *= lifeFade;
                }
                #endif

                // --- Dissolve ---
                #if defined(_DISSOLVE_ON)
                {
                    float dissAmount = _DissolveAmount;
                    #if defined(_DISSOLVE_LIFETIME)
                        dissAmount = moruLifetimeDissolveAmount(agePercent, _DissolveDelay, _DissolveSpeed);
                    #endif
                    moruDissolveSoft(col, uv, _DissolveTex, dissAmount, _DissolveEdgeWidth, _DissolveEdgeColor, _DissolveSoftness);
                }
                #endif

                // --- Distance Fade ---
                #if defined(_DISTANCEFADE_ON)
                {
                    float distFade = moruDistanceFade(i.worldPos, _WorldSpaceCameraPos, _FadeNear, _FadeFar);
                    col.a *= distFade;
                }
                #endif

                // --- Soft Particle ---
                #if defined(_SOFTPARTICLES_ON)
                {
                    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                    float partZ = i.eyeDepth;
                    float fade = saturate(_SoftDistance / (sceneZ - partZ));
                    col.a *= fade;
                }
                #endif

                // --- Fog ---
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }

    FallBack "Transparent/VertexLit"

    CustomEditor "MoruToon.Editor.MoruToonGUI"
}
