Shader "Custom/LaserShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (1, 0.5, 0, 1)
        _Intensity ("Emission Intensity", Float) = 2
        _Speed ("Noise Speed", Float) = 5
        _DistortAmount ("Wave Distortion", Float) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        // 투명 블렌딩 설정
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex VS_MAIN
            #pragma fragment PS_MAIN
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float4 _Color;
            float4 _EmissionColor;
            float _Intensity;
            float _Speed;
            float _DistortAmount;

            // 정점 셰이더 입력
            struct VS_IN
            {
                float4 vPos : POSITION;
                float2 vUV  : TEXCOORD0;
            };

            // 픽셀 셰이더 입력 
            struct PS_IN
            {
                float2 vUV       : TEXCOORD0;
                float2 vNoiseUV  : TEXCOORD1;
                float4 vClipPos  : SV_POSITION;
            };

            // 정점 셰이더
            PS_IN VS_MAIN(VS_IN In)
            {
                PS_IN Out;
                Out.vClipPos = UnityObjectToClipPos(In.vPos);
                Out.vUV = TRANSFORM_TEX(In.vUV, _MainTex);
                Out.vNoiseUV = TRANSFORM_TEX(In.vUV, _NoiseTex);
                return Out;
            }

            // 픽셀 셰이더
            fixed4 PS_MAIN(PS_IN In) : SV_Target
            {
                float fTime = _Time.y * _Speed;

                // 레이저 출렁임 (sin + cos 파형)
                float fWave = sin(In.vNoiseUV.y * 60 + fTime * 4) * 0.5 +
                              cos(In.vNoiseUV.y * 30 + fTime * 2) * 0.5;

                float2 vOffset = float2(fWave * _DistortAmount, 0);
                float2 vUV = In.vUV + vOffset;

                // 메인 텍스처 색상
                fixed4 texCol = tex2D(_MainTex, vUV);
                fixed4 col = texCol * _Color;

                // 중앙 진하게, 위로 갈수록 옅게
                float fBaseAlpha = saturate(1.0 - In.vUV.y) * 1.2;
                col.a = texCol.a * fBaseAlpha;

                // 발광 노이즈
                float fNoise = tex2D(_NoiseTex, In.vNoiseUV + fTime * 0.2).r;
                float3 vEmission = _EmissionColor.rgb * fNoise * _Intensity;

                col.rgb = col.rgb + vEmission;

                return col;
            }
            ENDCG
        }
    }
}
