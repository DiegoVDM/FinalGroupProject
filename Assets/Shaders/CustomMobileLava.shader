Shader "Custom/Mobile Lava Unlit"
{
    Properties
    {
        // Main lava/noise texture. The shader reads this twice with different
        // scrolling UVs so one small texture can create richer motion.
        _MainTex ("Lava / Noise Texture", 2D) = "white" {}

        // Three-color ramp used to turn the grayscale lava texture into
        // dark crust, molten orange, and hot yellow areas.
        _DarkColor ("Dark Color", Color) = (0.16, 0.01, 0.00, 1.0)
        _MidColor ("Mid Color", Color) = (1.00, 0.20, 0.02, 1.0)
        _HotColor ("Hot Color", Color) = (1.00, 0.78, 0.12, 1.0)

        // Kept as a single float for simple material tweaking. Larger values
        // repeat the texture more often across the overlay mesh.
        _Tiling ("Tiling", Float) = 6.0

        // Two independent flow directions. These are Vector properties so the
        // material can store an X/Y scroll direction and speed for each sample.
        _FlowSpeed1 ("Flow Speed 1", Vector) = (0.035, 0.020, 0.0, 0.0)
        _FlowSpeed2 ("Flow Speed 2", Vector) = (-0.020, 0.030, 0.0, 0.0)

        // Contrast pushes values away from mid gray, making hot veins pop.
        _Contrast ("Contrast", Range(0.5, 4.0)) = 2.0

        // Brightness is plain color intensity, useful even without Bloom.
        _Brightness ("Brightness", Range(0.25, 4.0)) = 1.25

        // EmissionStrength multiplies the final color so the lava reads as
        // glowing in an unlit shader before any post-processing is applied.
        _EmissionStrength ("Emission Strength", Range(0.0, 5.0)) = 2.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        // Opaque overlay material: writes depth, avoids sorting problems, and
        // stays cheaper than transparent/refraction-based lava effects.
        ZWrite On
        Cull Back

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // URP's Core.hlsl gives us TransformObjectToHClip, _Time, and the
            // common texture/sampler macros used by URP shaders.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _DarkColor;
                half4 _MidColor;
                half4 _HotColor;
                half _Tiling;
                half4 _FlowSpeed1;
                half4 _FlowSpeed2;
                half _Contrast;
                half _Brightness;
                half _EmissionStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                // Transform from object space to homogeneous clip space so URP
                // can place the mesh correctly on screen.
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);

                // TRANSFORM_TEX applies the material's built-in texture tiling
                // and offset. _Tiling is applied later for a simple "lava scale"
                // control that is easy to explain and tune.
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // _Time.y is elapsed time in seconds. Multiplying by two small
                // flow vectors scrolls the same texture in different directions.
                half time = (half)_Time.y;
                float2 baseUV = input.uv * _Tiling;
                float2 uv1 = baseUV + ((float2)_FlowSpeed1.xy * time);
                float2 uv2 = baseUV + ((float2)_FlowSpeed2.xy * time);

                // Two texture reads are a good mobile-friendly compromise:
                // enough movement to avoid obvious repetition, but still cheap.
                half lava1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1).r;
                half lava2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv2).r;
                half lavaValue = (lava1 + lava2) * 0.5h;

                // Center contrast around 0.5 so increasing contrast makes dark
                // crust darker and hot channels brighter without extra samples.
                lavaValue = saturate(((lavaValue - 0.5h) * _Contrast) + 0.5h);

                // Build a simple three-stop color ramp. The first lerp moves
                // from dark red to orange; the second adds yellow in the hottest
                // half of the value range.
                half3 darkToMid = lerp(_DarkColor.rgb, _MidColor.rgb, lavaValue);
                half hotMask = smoothstep(0.50h, 1.00h, lavaValue);
                half3 lavaColor = lerp(darkToMid, _HotColor.rgb, hotMask);

                // Unlit output: no scene lights are evaluated. Brightness and
                // emission keep the surface readable before optional Bloom.
                lavaColor *= (_Brightness * max(_EmissionStrength, 0.0h));

                return half4(lavaColor, 1.0h);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
