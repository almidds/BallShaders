Shader "Hidden/AddShader"{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader{
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct meshData{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct interpolator{
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Sample;

            interpolator vert (meshData v){
                interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (interpolator i) : SV_Target{
                return float4(tex2D(_MainTex, i.uv).rgb, 1.0f / (_Sample+1.0f));
            }
            ENDCG
        }
    }
}
