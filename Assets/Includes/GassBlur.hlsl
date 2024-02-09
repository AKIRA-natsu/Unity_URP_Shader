#ifndef GASSBLUR_INCLUDED
#define GASSBLUR_INCLUDED

void GassBlur_float(float2 UV, float BlurRadius , Texture2D MainTex , SamplerState Sampler  ,float TextureSize ,float DepthValue , out float4 BlurColor)
{
	//float2 blurUV = 0;
	float4 col = 0;
#ifdef SHADERGRAPH_PREVIEW
	//BlurUV = 0;
	BlurColor = 0;
#else

	
//	float sigma2 = pow(sigma, 2.0f);
//	float left = 1 / (2 * sigma2 * 3.1415926f);
	//TEXTURE2D(MainTex);
//	SAMPLER(sampler_MainTex);
//	TEXTURE2D(_MainTex);
//	SAMPLER(sampler_MainTex);
	//sampler2D sampler_MainTex;

	//float sigma3 = left * right;
		float sigma = BlurRadius / 3.0f;
		float sigma2 = sigma * sigma;
		float left = 1 / (2 * sigma2 * 3.1415926f);
		//float right = exp(-(x * x + y * y) / (2 * sigma2));
	//	return left * right;
	

	
	
	//float4 col = float4(0, 0, 0, 0);

	for (int x = -BlurRadius; x <= BlurRadius; ++x)
	{
		for (int y = -BlurRadius; y <= BlurRadius; ++y)
		{
			//��ȡ��Χ���ص���ɫ
			//��Ϊuv��0-1��һ��ֵ�����������������Σ�����Ҫȡ���ʶ�Ӧλ���ϵ���ɫ����Ҫ�����ε���������
			//תΪuv�ϵ�����ֵ
		    float4 color = SAMPLE_TEXTURE2D(MainTex, Sampler, (UV + float2(x / TextureSize, y / TextureSize)* DepthValue));
		//	float2 blurUV = UV + XY;
			//��ȡ�����ص�Ȩ��
			float weight = left * exp(-(x * x + y * y) / (2 * sigma2));
			//����˵��������ɫ		
			//float3 col = TextureColor + TextureColor * weight;
			col += color * weight;
		}
	}
	//BlurUV = blurUV;
	BlurColor = col;
#endif

	//	BlurColor = col;

}

#endif
