Shader"URP/Post/RadialBrul" {
    Properties
    {
        _MainTex("tex",2D)="Wwhite"{}
//        [int]_Loop("_Loop",range(1,10))=1
//        [int]_Blur("_Blur",range(0,2))=1
//        _Y("_Y",float)=0.5
//        _X("_X",float)=0.5
    }

    SubShader
    {
        Tags{
            "RenderPipeline" = "UniversalRenderPipeline"
        }
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        float4 _MainTex_ST;
        float _Loop;
        float _Blur;
        float _Y;
        float _X;
        float _Instensity;
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_SourceTex);
        SAMPLER(sampler_SourceTex);

        struct a2v
        {
            float4 positionOS:POSITION;
            float2 texcoord:TEXCOORD;
        };

        struct v2f
        {
            float4 positionCS:SV_POSITION;
            float2 texcoord:TEXCOORD;
        };

        ENDHLSL

        pass
        {
            HLSLPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment

            v2f vertex(a2v i)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.texcoord = i.texcoord;
                return o;
            }

            half4 fragment(v2f i) :SV_TARGET
            {
                float4 col = 0;
                float2 dir = (float2(_X,_Y)-i.texcoord ) * _Blur * 0.01;
                for (int t = 0; t < _Loop; t++)
                {
                    col += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.texcoord + dir * t) / _Loop;
                }
                return col;
            }
            ENDHLSL
        }
    }
} 