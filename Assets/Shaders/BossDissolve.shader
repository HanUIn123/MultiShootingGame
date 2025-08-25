Shader "Custom/BossDissolve"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _EdgeColor ("Edge Color", Color) = (1,0.5,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"       // ← 이게 반드시 필요함!

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float _DissolveAmount;
            float4 _EdgeColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // ← 오류 해결됨
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 texColor = tex2D(_MainTex, i.uv);
                float noise = tex2D(_NoiseTex, i.uv).r;

                if (noise < _DissolveAmount)
                    discard;

                float edge = smoothstep(_DissolveAmount - 0.05, _DissolveAmount, noise);
                texColor.rgb = lerp(_EdgeColor.rgb, texColor.rgb, edge);

                return texColor;
            }
            ENDCG
        }
    }
}
