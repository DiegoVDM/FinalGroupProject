Shader "Custom/LavaShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _Spread ("Spread Amount", Range(0,1)) = 0
        _Softness ("Edge Softness", Range(0.001, 0.5)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

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

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END
            float _Spread;
            float _Softness;
            Varyings vert(Attributes IN)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(IN.positionOS);
                o.uv = IN.uv;
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
    float2 uv = IN.uv;

    // Direction + distance from center
    float2 dir = uv - center;
    float dist = length(dir);

    // Normalize direction safely
    dir = dist > 0 ? dir / dist : float2(0,0);

    // Flow speed
    float speed = 0.3;

    // Move outward but LOOP using frac
    float flow = frac(dist + _Time.y * -speed);

    // Reconstruct UV from center outward
    uv = center + dir * flow;

    half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;

    return color;
                return color;
            }
            ENDHLSL
        }
    }
}
