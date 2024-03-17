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

    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;

};

Texture2D Bloom2Texture;

sampler2D Bloom2TextureSampler = sampler_state
{
    Texture = <Bloom2Texture>;

    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D Bloom3Texture;

sampler2D Bloom3TextureSampler = sampler_state
{
    Texture = <Bloom3Texture>;

    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;

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
	
    float3 bloomColor = tex2D(BloomTextureSampler, input.TextureCoordinates).rgb / 3 + tex2D(Bloom2TextureSampler, input.TextureCoordinates).rgb / 1 + tex2D(Bloom3TextureSampler, input.TextureCoordinates).rgb / 2;
	
    float bloomL = length(bloomColor);
	
    bloomColor = lerp(bloomColor, float3(bloomL, bloomL, bloomL), bloomL * 0.5);
	
    bloomL = length(bloomColor);
	
    bloomColor *= lerp(bloomL + 1, 1, 0.1);
	
    float ssao = tex2D(SSAOTextureSampler, input.TextureCoordinates).r;
	
    return float4((color + bloomColor)*ssao, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};