#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

Texture2D ReflectionTexture;

sampler2D ReflectionTextureSampler = sampler_state
{
    Texture = <ReflectionTexture>;
};

Texture2D FactorTexture;

sampler2D FactorTextureSampler = sampler_state
{
    Texture = <FactorTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	
    float3 color = tex2D(SpriteTextureSampler, input.TextureCoordinates).rgb;
	
    float3 reflection = tex2D(ReflectionTextureSampler, input.TextureCoordinates).rgb;
	
    reflection *= color;
	
    float factor = tex2D(FactorTextureSampler, input.TextureCoordinates).r;
	
    float3 result = lerp(color, reflection, factor);
	
    return float4(result,1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};