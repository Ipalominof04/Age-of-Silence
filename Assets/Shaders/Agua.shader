Shader "Custom/Agua"
{
   Properties
   {
        _Color ("Color", Color) = (0,0.3,0.5,0.5) // color base
        _NormalTex1 ("Normal texture 1", 2D) = "bump" {} // textura1 de normals per simular les ondulacions
        _NormalTex2 ("Normal texture 2", 2D) = "bump" {} // textura2 de normals per combinar moviments diferents
        _NoiseTex ("Noise texture", 2D) = "white" {} // textura de soroll per deformar la geometria

        _Scale ("Noise scale", Float) = 0.03 // escala aplicada a la textura de soroll
        _Amplitude ("Amplitude", Float) = 0.5 // intensitat max de les ones
        _Speed ("Speed", Float) = 0.15 // velocitat animacio de les ones i normals
        _NormalStrength ("Normal Strength", Float) = 0.5 // instensitat de efecte de les normals
    }

    SubShader
    {
        // config perque funcioni a HDRP i es renderitzi com a transparent
        Tags { "RenderPipeline"="HDRenderPipeline" "Queue"="Transparent" "RenderType"="Transparent"}

        Pass{
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off // evita escriure al buffer de profunditat

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5

            // llibreries de HDRP necessaries
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            // declaracio de les textures i els seus samplers
            TEXTURE2D(_NormalTex1);
            SAMPLER(sampler_NormalTex1);

            TEXTURE2D(_NormalTex2);
            SAMPLER(sampler_NormalTex2);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            // variables exposades a l'inspecotor
            float4 _Color;
            float _Scale;
            float _Amplitude;
            float _Speed;
            float _NormalStrength;

            // dades q arriben desde el mesh
            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            // dades que passen del vertex shader al fragment shader
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // vertex shader
            Varyings Vert(Attributes IN)
            {
                Varyings OUT;

                // desplacament de les UV e soroll al llarg del temps
                float2 noiseUV = (IN.uv + _Time.y * _Speed) * _Scale;
                // variacio aplitud 
                float amp = _Amplitude * abs(sin(_Time.y * _Speed));

                // mostra el valor de la textura de soroll
                float noise = SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_NoiseTex, noiseUV, 0).r;

                // copia la pos original del vertex
                float3 pos = IN.positionOS;
                // desplaça el vertex verticalment segons el soroll
                pos.y += noise * amp;

                // converteix la pos a espai de clip
                OUT.positionCS = TransformObjectToHClip(pos);
                // passa les UV al fragment shader
                OUT.uv = IN.uv;

                return OUT;
            }

            // fragment shader
            float4 Frag(Varyings IN) : SV_Target
            {
                float2 uv1 = IN.uv + float2(_Time.y * _Speed, 0); // mou normal1 cap a eix X
                float2 uv2 = IN.uv + float2(0, _Time.y * _Speed);// mou normal 2 cap a eix Y

                // llegeix normal1 i normal2 i la converteix a rang [-1,1]
                float3 n1 = SAMPLE_TEXTURE2D(_NormalTex1, sampler_NormalTex1, uv1).xyz * 2 - 1;
                float3 n2 = SAMPLE_TEXTURE2D(_NormalTex2, sampler_NormalTex2, uv2).xyz * 2 - 1;

                // combina les dues normals i ajusta la intensitat
                float3 normal = normalize((n1 + n2) * _NormalStrength);

                float3 lightDir = normalize(float3(0.4, 1, 0.3)); // direccio de llum simulada
                float lighting = saturate(dot(normal, lightDir)); // calcul i·lluminacio difusa

                // aplica la llum al color base de aigua
                float3 col = _Color.rgb * (0.4 + 0.6 * lighting);
                //retorna el color final mantenir alfa configurat
                return float4(col, _Color.a);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
