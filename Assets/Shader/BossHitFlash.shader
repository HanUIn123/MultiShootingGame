Shader "Custom/BossHitFlash"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _WhiteAmount ("White Flash", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex VS_MAIN
            #pragma fragment PS_MAIN
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WhiteAmount;

            // Á¤Á¡ ¼ÎÀÌ´õ ÀÔ·Â
            struct VS_IN
            {
                float4 vPos : POSITION;
                float2 vUV  : TEXCOORD0;
            };

            // ÇÈ¼¿ ¼ÎÀÌ´õ ÀÔ·Â 
            struct PS_IN
            {
                float2 vUV      : TEXCOORD0;
                float4 vClipPos : SV_POSITION;
            };

            // Á¤Á¡ ¼ÎÀÌ´õ
            PS_IN VS_MAIN(VS_IN In)
            {
                PS_IN Out;
                Out.vClipPos = UnityObjectToClipPos(In.vPos);
                Out.vUV = TRANSFORM_TEX(In.vUV, _MainTex);
                return Out;
            }

            // ÇÈ¼¿ ¼ÎÀÌ´õ
            fixed4 PS_MAIN(PS_IN In) : SV_Target
            {
                fixed4 colBase = tex2D(_MainTex, In.vUV);                       
                fixed4 colFlash = fixed4(1, 1, 1, colBase.a);                   
                fixed4 colFinal = lerp(colBase, colFlash, _WhiteAmount);        
                return colFinal;
            }
            ENDCG
        }
    }
}
