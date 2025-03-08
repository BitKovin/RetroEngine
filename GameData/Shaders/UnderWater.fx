#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

Texture2D Color;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <Color>;
};


Texture2D DepthTexture;

sampler2D DepthTextureSampler = sampler_state
{
    Texture = <DepthTexture>;
};

Texture2D PositionTexture;

sampler2D PositionTextureSampler = sampler_state
{
    Texture = <PositionTexture>;
};


float3 viewPos;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 SampleBlur(float2 screenCoords)
{
	int n = 4;

	float blurSize = 0.01;

	float3 col = 0;

	col += tex2D(SpriteTextureSampler, screenCoords + float2(0 , blurSize)).rgb;
	col += tex2D(SpriteTextureSampler, screenCoords + float2(0 , -blurSize)).rgb;
	col += tex2D(SpriteTextureSampler, screenCoords + float2(-blurSize , 0)).rgb;
	col += tex2D(SpriteTextureSampler, screenCoords + float2(blurSize , 0)).rgb;

	col/=4;

	return float4(col,0);

}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	
    float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	
	color*=1;

	color += SampleBlur(input.TextureCoordinates);
	color/=2;

	color += float4(0, 0, 0.02, 0);

    float depth = tex2D(DepthTextureSampler, input.TextureCoordinates).r;
	
    //float3 pos = tex2D(PositionTextureSampler, input.TextureCoordinates).rgb + viewPos;
	
    float factor = depth / 40;
	
	factor = saturate(factor);

	float3 resultColor = lerp(color.rgb*input.Color.rgb, float3(0.03,0.1,0.1),factor);

    return float4(resultColor, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};