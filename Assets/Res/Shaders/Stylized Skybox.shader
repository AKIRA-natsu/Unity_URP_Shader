Shader "Unlit/Stylized Skybox"
{
    Properties
    {
        [Header(Stars Settings)]
        _Stars("Stars Texture", 2D) = "black" {}

        [Header(Horizon Settings)]
        _HorizonColorDay("Day Horizon Color", Color) = (0,0.8,1,1)
        _HorizonColorNight("Night Horizon Color", Color) = (0,0.8,1,1)

        [Header(Sun Settings)]
		_SunColor("Sun Color", Color) = (1,1,1,1)
		_SunRadius("Sun Radius",  Range(0, 2)) = 0.1

		[Header(Moon Settings)]
		_MoonColor("Moon Color", Color) = (1,1,1,1)
		_MoonRadius("Moon Radius",  Range(0, 2)) = 0.15
		_MoonOffset("Moon Crescent",  Range(-1, 1)) = -0.1

        [Header(Day Sky Settings)]
        _DayTopColor("Day Sky Color Top", Color) = (0.4,1,1,1)
        _DayBottomColor("Day Sky Color Bottom", Color) = (0,0.8,1,1)

        [Header(Night Sky Settings)]
        _NightTopColor("Night Sky Color Top", Color) = (0,0,0,1)
        _NightBottomColor("Night Sky Color Bottom", Color) = (0,0,0.2,1)

        [Header(Main Cloud Settings)]
        _BaseNoise("Base Noise", 2D) = "black" {}
        _Distort("Distort", 2D) = "black" {}
        _SecNoise("Sec Noise", 2D) = "black" {}
        _BaseNoiseScale("Base Noise Scale",  Range(0, 1)) = 0.2
        _DistortScale("Distort Noise Scale",  Range(0, 1)) = 0.06
        _SecNoiseScale("Secondary Noise Scale",  Range(0, 1)) = 0.05
        _CloudCutoff("Cloud Cutoff",  Range(0, 1)) = 0.3
        _Fuzziness("Cloud Fuzziness",  Range(0, 1)) = 0.04

        [Header(Day Clouds Settings)]
        _CloudColorDayEdge("Clouds Edge Day", Color) = (1,1,1,1)
        _CloudColorDayMain("Clouds Main Day", Color) = (0.8,0.9,0.8,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature FUZZY
            // // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            // skys
            sampler2D _Stars;
            
            // sun and moon
            float _SunRadius, _MoonRadius, _MoonOffset;
            float4 _SunColor, _MoonColor;

            // sky color
            float4 _DayTopColor, _DayBottomColor;
            float4 _NightTopColor, _NightBottomColor;
            
            // horizon
            // float _OffsetHorizon, _HorizonIntensity;
            float4 _HorizonColorDay, _HorizonColorNight;

            // cloud
            sampler2D _BaseNoise, _Distort, _SecNoise;
            float _BaseNoiseScale, _DistortScale, _SecNoiseScale;
            float _CloudCutoff, _Fuzziness;
            // day cloud
            float4 _CloudColorDayEdge, _CloudColorDayMain;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // UNITY_TRANSFER_FOG(o,o.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // horizon
                float horizon = abs(i.uv.y);
                
                // uv for the sky
                float2 skyUV = i.worldPos.xz / i.worldPos.y;

                // moving clouds
                float baseNoise = tex2D(_BaseNoise, (skyUV - _Time.x) * _BaseNoiseScale);
                float noise1 = tex2D(_Distort, ((skyUV + baseNoise) - _Time.x) * _DistortScale);
                float noise2 = tex2D(_SecNoise, ((skyUV + noise1) - _Time.x) * _SecNoiseScale);
                float finalNoise = saturate(noise1 * noise2) * 3 * saturate(i.worldPos.y);

                // clouds
                float clouds = saturate(smoothstep(_CloudCutoff, _CloudCutoff + _Fuzziness, finalNoise));
                float3 cloudsColored = lerp(_CloudColorDayEdge, _CloudColorDayMain, clouds) * clouds;

                // sun
                float sun = distance(i.uv.xyz, _WorldSpaceLightPos0);
                float sunDisc = 1 - (sun / _SunRadius);                                                                 // saturate 颜色规划到 0 - 1 之间
                sunDisc = saturate(sunDisc * 50);

                // moon
                float moon = distance(i.uv.xyz, -_WorldSpaceLightPos0);                                                 // 两点之间的距离
                float crescentMoon = distance(float3(i.uv.x + _MoonOffset, i.uv.yz), -_WorldSpaceLightPos0);
                float crescentMoonDisc = 1 - (crescentMoon / _MoonRadius);
                crescentMoonDisc = saturate(crescentMoonDisc * 50);
                float moonDisc = 1 - (moon / _MoonRadius);
                moonDisc = saturate(moonDisc * 50);
                moonDisc = saturate(moonDisc - crescentMoonDisc);                                                       // 计算月牙缺损

				float3 SunAndMoon = (sunDisc * _SunColor) + (moonDisc * _MoonColor);

                // stars
                float3 stars = tex2D(_Stars, skyUV - _Time.x);
                stars *= 1 - saturate(_WorldSpaceLightPos0.x);

                // gradient day sky
                float3 gradientDay = lerp(_DayBottomColor, _DayTopColor, saturate(i.uv.xyz));

                // gradient night sky
                float3 gradientNight = lerp(_NightBottomColor, _NightTopColor, saturate(i.uv.xyz));

                float3 skyGradients = lerp(gradientNight, gradientDay, saturate(_WorldSpaceLightPos0.y));

                float horizonGlow = saturate((1 - horizon) * saturate(_WorldSpaceLightPos0.y));
                float4 horizonGlowDay = horizonGlow * _HorizonColorDay;

                // combined all effects
                float3 combined = SunAndMoon + skyGradients + horizonGlowDay + stars + cloudsColored;

                return float4(combined, 1);
            }
            ENDCG
        }
    }
}
