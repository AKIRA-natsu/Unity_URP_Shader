// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Flares/Textured Flare Shader"
{
	Properties 
   	{
    	_MainTex ( "Texture", 2D )	= "white" {}   
  	}
    
	SubShader 
	{
		Tags { "Queue"="Transparent+100" "IgnoreProjector"="True" "RenderType"="Transparent" }

    	Pass 
		{    
			ZWrite Off
      	 	ZTest Always 
      	 	Blend One One
     		
			CGPROGRAM
			
 			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

            #define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

			sampler2D _MainTex;
            float4 _MainTex_ST;
 
           	struct VertInput
            {
                half4 vertex	: POSITION;
                half2 texcoord	: TEXCOORD0;
			  	fixed4 color	: COLOR;
            };

           	struct Verts
            {
                half4 pos		: SV_POSITION;
                half2 uv		: TEXCOORD0;
			  	fixed4 _color   : COLOR;
            };

			Verts vert ( VertInput vert )
			{
				Verts v;

				v._color		= vert.color*(vert.color.a*3);
    			v.pos			= UnityObjectToClipPos ( vert.vertex );
   				v.uv 	  		= TRANSFORM_TEX(vert.texcoord, _MainTex);
   				
				return v;
			}
 	
			fixed4 frag ( Verts v ):COLOR
			{
    			return tex2D ( _MainTex, v.uv ) * v._color;
			}

			ENDCG
		}
 	}
}


