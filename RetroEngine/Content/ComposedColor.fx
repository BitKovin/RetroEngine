#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

Texture2D ColorTexture;

sampler2D ColorTextureSampler = sampler_state
{
	Texture = <ColorTexture>;
};

Texture2D SSAOTexture;

sampler2D SSAOTextureSampler = sampler_state
{
    Texture = <SSAOTexture>;
};

Texture2D BloomTexture;

sampler2D BloomTextureSampler = sampler_state
{
    Texture = <BloomTexture>;
};

Texture2D Bloom2Texture;

sampler2D Bloom2TextureSampler = sampler_state
{
    Texture = <Bloom2Texture>;
};

Texture2D Bloom3Texture;

sampler2D Bloom3TextureSampler = sampler_state
{
    Texture = <Bloom3Texture>;
};


struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	
    float3 color = tex2D(ColorTextureSampler, input.TextureCoordinates).rgb;
	
    float3 bloomColor = tex2D(BloomTextureSampler, input.TextureCoordinates).rgb + tex2D(Bloom2TextureSampler, input.TextureCoordinates).rgb / 1.5 + tex2D(Bloom3TextureSampler, input.TextureCoordinates).rgb / 2;
	
	
    float ssao = tex2D(SSAOTextureSampler, input.TextureCoordinates).r;
	
    return float4(color + bloomColor,1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};