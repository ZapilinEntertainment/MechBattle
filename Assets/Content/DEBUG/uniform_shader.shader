Shader "Custom/UniformMeshUV" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _TexScale ("Texture Scale", Float) = 1.0
        [Toggle] _USE_WORLD_POS ("Use World Position", Float) = 1
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _USE_WORLD_POS_ON

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                #ifndef _USE_WORLD_POS_ON
                float2 uv : TEXCOORD0;
                #endif
            };

            struct v2f {
                #ifdef _USE_WORLD_POS_ON
                float3 worldPos : TEXCOORD0;
                #else
                float2 uv : TEXCOORD0;
                #endif
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TexScale;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                #ifdef _USE_WORLD_POS_ON
                // Используем мировые координаты для текстурных координат
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                #else
                // Используем обычные UV, но масштабируем их
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) * _TexScale;
                #endif
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                #ifdef _USE_WORLD_POS_ON
                // Создаем UV на основе мировых координат
                float2 uv = i.worldPos.xz * _TexScale;
                #else
                float2 uv = i.uv;
                #endif
                
                fixed4 col = tex2D(_MainTex, uv);
                return col;
            }
            ENDCG
        }
    }
}