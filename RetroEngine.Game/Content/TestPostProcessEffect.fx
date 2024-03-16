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


struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	
    float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	
    float depth = tex2D(DepthTextureSampler, input.TextureCoordinates).r;
	
    float factor = 1 - depth/40;
	
    return color * input.Color * factor;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};