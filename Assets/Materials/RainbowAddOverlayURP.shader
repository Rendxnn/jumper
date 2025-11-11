Shader "Custom/RainbowAddOverlayURP"
{
    Properties
    {
        _Effect ("Effect Amount", Range(0,1)) = 0
        _Opacity ("Max Opacity", Range(0,2)) = 1
        _HueSpeed ("Hue Speed", Float) = 1
        _RainbowScale ("Rainbow Scale", Float) = 1
        _FresnelStrength ("Fresnel Strength", Range(0,5)) = 1
        _FresnelPower ("Fresnel Power", Range(0.1,8)) = 3
    }

    SubShader
    {
        Tags{
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "UniversalForward"
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float2 uv          : TEXCOORD1;
                float3 viewDirWS   : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
            float _Effect;
            float _Opacity;
            float _HueSpeed;
            float _RainbowScale;
            float _FresnelStrength;
            float _FresnelPower;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                float3 camPosWS = GetCameraPositionWS();
                OUT.viewDirWS = normalize(camPosWS - posWS);
                return OUT;
            }

            float3 HSVtoRGB(float3 hsv)
            {
                float3 rgb = clamp(abs(frac(hsv.x + float3(0,2.0/6.0,4.0/6.0)) * 6.0 - 3.0) - 1.0, 0.0, 1.0);
                return lerp(float3(1,1,1), rgb, hsv.y) * hsv.z;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Rainbow hue cycles with time and varies across UVs
                float hue = frac(_Time.y * _HueSpeed + (IN.uv.x + IN.uv.y) * _RainbowScale);
                float3 rainbow = HSVtoRGB(float3(hue, 1.0, 1.0));

                // Fresnel factor to accentuate edges
                float NdotV = saturate(dot(normalize(IN.normalWS), normalize(IN.viewDirWS)));
                float fresnel = pow(1.0 - NdotV, _FresnelPower) * _FresnelStrength;

                float effect = saturate(_Effect);
                float alpha = saturate(effect) * _Opacity;

                float3 color = rainbow * (effect + fresnel);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

