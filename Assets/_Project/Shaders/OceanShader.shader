Shader "OceanBattleRoyale/OceanShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Water Color", Color) = (0.0, 0.3, 0.5, 1.0)
        _DeepColor ("Deep Water Color", Color) = (0.0, 0.15, 0.3, 1.0)
        _ShallowColor ("Shallow Water Color", Color) = (0.1, 0.4, 0.6, 1.0)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 0.8)
        _FoamTex ("Foam Texture", 2D) = "white" {}
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveScale ("Wave Scale", Float) = 0.1
        _WaveHeight ("Wave Height", Float) = 2.0
        _FoamThreshold ("Foam Threshold", Range(0, 1)) = 0.5
        _FresnelPower ("Fresnel Power", Float) = 3.0
        _ReflectionStrength ("Reflection Strength", Float) = 0.3
        _CausticsTex ("Caustics Texture", 2D) = "white" {}
        _CausticsSpeed ("Caustics Speed", Float) = 0.5
        _CausticsScale ("Caustics Scale", Float) = 10.0
        _LODDistance ("LOD Distance", Float) = 200.0
        _MobileFallback ("Mobile Fallback", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "DisableBatching" = "True" }
        LOD 200

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #pragma multi_compile _ MOBILE_FALLBACK_ON

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float fogCoord : TEXCOORD3;
                float2 causticsUV : TEXCOORD4;
            };

            sampler2D _MainTex;
            sampler2D _FoamTex;
            sampler2D _CausticsTex;
            float4 _MainTex_ST;
            float4 _FoamTex_ST;
            float4 _Color;
            float4 _DeepColor;
            float4 _ShallowColor;
            float4 _FoamColor;
            float _WaveSpeed;
            float _WaveScale;
            float _WaveHeight;
            float _FoamThreshold;
            float _FresnelPower;
            float _ReflectionStrength;
            float _CausticsSpeed;
            float _CausticsScale;
            float _LODDistance;
            float _MobileFallback;

            float4 _WorldSpaceCameraPos;
            float4 _Time;
            float4 _ProjectionParams;

            float3 GerritsenWave(float2 uv, float time, float scale, float height, out float3 normal)
            {
                float2 uv1 = uv * scale * 1.0 + time * _WaveSpeed * 0.5;
                float2 uv2 = uv * scale * 2.0 + time * _WaveSpeed * 0.7 + 17.3;
                float2 uv3 = uv * scale * 4.0 + time * _WaveSpeed * 1.1 + 43.7;
                float2 uv4 = uv * scale * 8.0 + time * _WaveSpeed * 1.5 + 91.1;

                float h1 = sin(uv1.x + uv1.y) * 0.5;
                float h2 = sin(uv2.x - uv2.y) * 0.25;
                float h3 = sin(uv3.x * 1.5 + uv3.y * 0.5) * 0.125;
                float h4 = sin(uv4.x * 0.5 + uv4.y * 2.0) * 0.0625;

                float heightSum = (h1 + h2 + h3 + h4) * height;

                float eps = 0.01;
                float hx1 = sin((uv1.x + eps) + uv1.y) * 0.5;
                float hx2 = sin((uv2.x + eps) - uv2.y) * 0.25;
                float hx3 = sin((uv3.x + eps) * 1.5 + uv3.y * 0.5) * 0.125;
                float hx4 = sin((uv4.x + eps) * 0.5 + uv4.y * 2.0) * 0.0625;
                float dh_dx = (hx1 + hx2 + hx3 + hx4 - (h1 + h2 + h3 + h4)) / eps * scale * height;

                float hy1 = sin(uv1.x + (uv1.y + eps)) * 0.5;
                float hy2 = sin(uv2.x - (uv2.y + eps)) * 0.25;
                float hy3 = sin(uv3.x * 1.5 + (uv3.y + eps) * 0.5) * 0.125;
                float hy4 = sin(uv4.x * 0.5 + (uv4.y + eps) * 2.0) * 0.0625;
                float dh_dy = (hy1 + hy2 + hy3 + hy4 - (h1 + h2 + h3 + h4)) / eps * scale * height;

                normal = normalize(float3(-dh_dx, 1.0, -dh_dy));
                return float3(0, heightSum, 0);
            }

            v2f vert(appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float2 uv = v.uv;

                float3 normal;
                float3 displacement = GerritsenWave(uv, _Time.y, _WaveScale, _WaveHeight, normal);

                float3 displacedPos = worldPos + displacement;

                float dist = distance(_WorldSpaceCameraPos.xyz, displacedPos);
                float lodFactor = saturate(dist / _LODDistance);

#if MOBILE_FALLBACK_ON
                if (_MobileFallback > 0.5 || lodFactor > 0.8)
                {
                    displacedPos = worldPos;
                    normal = float3(0, 1, 0);
                }
#endif

                o.vertex = UnityWorldToClipPos(displacedPos);
                o.worldPos = displacedPos;
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, normal));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.causticsUV = displacedPos.xz * _CausticsScale + _Time.y * _CausticsSpeed;
                UNITY_TRANSFER_FOG(o, o.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float3 normal = i.worldNormal;

                float depth = i.worldPos.y;
                float depthFactor = saturate(-depth / 50.0);

                float4 baseColor = lerp(_ShallowColor, _DeepColor, depthFactor);

                float fresnel = pow(1.0 - saturate(dot(viewDir, normal)), _FresnelPower);
                float4 reflection = lerp(0, _FoamColor, fresnel * _ReflectionStrength);

                float slope = 1.0 - normal.y;
                float foam = smoothstep(_FoamThreshold, _FoamThreshold + 0.2, slope);
                foam *= saturate(length(normal.xz) * 2.0);

                float4 foamColor = _FoamColor * foam;
                foamColor.a *= 0.7;

                float4 caustics = tex2D(_CausticsTex, i.causticsUV);
                baseColor.rgb += caustics.rgb * 0.1 * (1.0 - depthFactor);

                float4 finalColor = baseColor + reflection + foamColor;
                finalColor.a = saturate(baseColor.a + foamColor.a);

                UNITY_APPLY_FOG(i.fogCoord, finalColor);

                return finalColor;
            }
            ENDCG
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert_shadow
            #pragma fragment frag_shadow
            #pragma target 3.0
            #pragma multi_compile_shadowcaster
            #pragma multi_compile _ MOBILE_FALLBACK_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
            };

            float _WaveSpeed;
            float _WaveScale;
            float _WaveHeight;
            float _MobileFallback;
            float4 _Time;

            float3 GerritsenWaveSimple(float2 uv, float time, float scale, float height, out float3 normal)
            {
                float2 uv1 = uv * scale * 1.0 + time * _WaveSpeed * 0.5;
                float h1 = sin(uv1.x + uv1.y) * 0.5;
                float h2 = sin(uv1.x * 2.0 - uv1.y) * 0.25;

                float heightSum = (h1 + h2) * height;

                float eps = 0.01;
                float hx1 = sin((uv1.x + eps) + uv1.y) * 0.5;
                float hx2 = sin((uv1.x + eps) * 2.0 - uv1.y) * 0.25;
                float dh_dx = (hx1 + hx2 - (h1 + h2)) / eps * scale * height;

                float hy1 = sin(uv1.x + (uv1.y + eps)) * 0.5;
                float hy2 = sin(uv1.x * 2.0 - (uv1.y + eps)) * 0.25;
                float dh_dy = (hy1 + hy2 - (h1 + h2)) / eps * scale * height;

                normal = normalize(float3(-dh_dx, 1.0, -dh_dy));
                return float3(0, heightSum, 0);
            }

            v2f vert_shadow(appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float2 uv = v.uv;

                float3 normal;
                float3 displacement = GerritsenWaveSimple(uv, _Time.y, _WaveScale, _WaveHeight, normal);
                float3 displacedPos = worldPos + displacement;

                float dist = distance(unity_WorldSpaceCameraPos.xyz, displacedPos);
                float lodFactor = saturate(dist / 200.0);

#if MOBILE_FALLBACK_ON
                if (_MobileFallback > 0.5 || lodFactor > 0.8)
                {
                    displacedPos = worldPos;
                }
#endif

                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.pos = UnityWorldToClipPos(displacedPos);
                o.uv = v.uv;

                return o;
            }

            float4 frag_shadow(v2f i) : SV_TARGET
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
