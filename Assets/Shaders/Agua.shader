Shader "Custom/Agua"
{
   Properties
   {
        _Color ("Color", Color) = (0,0.3,0.5,0.5)
        _NormalTex1 ("Normal texture 1", 2D) = "bump" {}
        _NormalTex2 ("Normal texture 2", 2D) = "bump" {}
        _NoiseTex ("Noise texture", 2D) = "white" {}

        _Scale ("Noise scale", Float) = 0.03
        _Amplitude ("Amplitude", Float) = 0.5
        _Speed ("Speed", Float) = 0.15
        _NormalStrength ("Normal Strength", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" "Queue"="Transparent" "RenderType"="Transparent"}

        Pass{
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            TEXTURE2D(_NormalTex1);
            SAMPLER(sampler_NormalTex1);

            TEXTURE2D(_NormalTex2);
            SAMPLER(sampler_NormalTex2);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            float4 _Color;
            float _Scale;
            float _Amplitude;
            float _Speed;
            float _NormalStrength;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;

                float2 noiseUV = (IN.uv + _Time.y * _Speed) * _Scale;
                float amp = _Amplitude * abs(sin(_Time.y * _Speed));

                float noise = SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_NoiseTex, noiseUV, 0).r;

                float3 pos = IN.positionOS;
                pos.y += noise * amp;

                OUT.positionCS = TransformObjectToHClip(pos);
                OUT.uv = IN.uv;

                return OUT;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float2 uv1 = IN.uv + float2(_Time.y * _Speed, 0);
                float2 uv2 = IN.uv + float2(0, _Time.y * _Speed);

                float3 n1 = SAMPLE_TEXTURE2D(_NormalTex1, sampler_NormalTex1, uv1).xyz * 2 - 1;
                float3 n2 = SAMPLE_TEXTURE2D(_NormalTex2, sampler_NormalTex2, uv2).xyz * 2 - 1;

                float3 normal = normalize((n1 + n2) * _NormalStrength);

                float3 lightDir = normalize(float3(0.4, 1, 0.3));
                float lighting = saturate(dot(normal, lightDir));

                float3 col = _Color.rgb * (0.4 + 0.6 * lighting);

                return float4(col, _Color.a);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
