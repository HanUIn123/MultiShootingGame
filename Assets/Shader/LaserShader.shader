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

        // ������ Additive ���, �Ϲ� ������ ���� ���
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = TRANSFORM_TEX(v.uv, _NoiseTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _Speed;

                // �� ������ ������ � ��鸲
                float wave = sin(i.noiseUV.y * 60 + time * 4) * 0.5 +
                             cos(i.noiseUV.y * 30 + time * 2) * 0.5;
                float2 offset = float2(wave * _DistortAmount, 0);
                float2 uv = i.uv + offset;

                fixed4 texCol = tex2D(_MainTex, uv);
                fixed4 col = texCol * _Color;

                // ���� ���� ���� (�߾��� ���ϰ� ���� ������ fade-out)
                float baseAlpha = saturate(1.0 - i.uv.y) * 1.2;
                col.a = texCol.a * baseAlpha;

                // �߱� ������ ����
                float noise = tex2D(_NoiseTex, i.noiseUV + time * 0.2).r;
                float3 emission = _EmissionColor.rgb * noise * _Intensity;

                col.rgb = col.rgb + emission;

                return col;
            }
            ENDCG
        }
    }
}
