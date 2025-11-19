Shader "Custom/UniformWorldSpaceTriplanarAO"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _TextureScale ("Texture Scale", Float) = 1.0
        _BlendSharpness ("Blend Sharpness", Range(1, 10)) = 4
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }

        // Основной пасс с освещением
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 blendWeights : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _TextureScale;
                float _BlendSharpness;
                float _Smoothness;
                float _Metallic;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                
                // Вычисляем веса для трипланарного проецирования
                float3 absNormal = abs(normalInput.normalWS);
                absNormal = pow(absNormal, _BlendSharpness);
                output.blendWeights = absNormal / (absNormal.x + absNormal.y + absNormal.z);
                
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            // Функция для трипланарного текстурирования
            float4 TriplanarSample(float3 position, float3 weights)
            {
                // Текстурируем по трем осям
                float4 xProjection = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, position.zy * _TextureScale);
                float4 yProjection = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, position.xz * _TextureScale);
                float4 zProjection = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, position.xy * _TextureScale);
                
                // Смешиваем проекции по весам
                return xProjection * weights.x + yProjection * weights.y + zProjection * weights.z;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Нормализуем нормаль
                float3 normalWS = normalize(input.normalWS);
                
                // Трипланарное текстурирование
                half4 albedo = TriplanarSample(input.positionWS, input.blendWeights) * _Color;

                // Освещение
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                float3 lighting = mainLight.color * mainLight.shadowAttenuation * saturate(dot(normalWS, mainLight.direction));

                // Добавляем ambient
                lighting += SampleSH(normalWS);

                // Применяем освещение
                half3 color = albedo.rgb * lighting;

                // Применяем туман
                color = MixFog(color, input.fogCoord);

                return half4(color, albedo.a);
            }
            ENDHLSL
        }

        // Пасс для теней
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // Пасс для Depth Only - нужен для SSAO
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // Пасс для Depth Normals - КРИТИЧЕСКИ ВАЖЕН для SSAO
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On

            HLSLPROGRAM
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            ENDHLSL
        }
    }

    // Fallback на стандартный шейдер если что-то пойдет не так
    FallBack "Universal Render Pipeline/Lit"
}