#include "ShaderLib/BasicShader.fx"

Texture2D NormalTexture;

sampler2D NormalTextureSampler = sampler_state
{
    Texture = <NormalTexture>;
};

Texture2D PositionTexture;

sampler2D PositionTextureSampler = sampler_state
{
    Texture = <PositionTexture>;
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
	
    float3 normal = tex2D(NormalTextureSampler, input.TextureCoordinates).rgb * 2 - 1;
    float depth = SampleDepth(input.TextureCoordinates).r;
	
    
    float3 worldPos = tex2D(PositionTextureSampler, input.TextureCoordinates).xyz + viewPos;
    
    float3 vDir = normalize(worldPos - viewPos);
    
    float3 reflection = reflect(normalize(vDir), normal);
    
    float factor = tex2D(FactorTextureSampler, input.TextureCoordinates).r;
    
    reflection = normalize(reflection);
    
    float3 cube = SampleCubemap(ReflectionCubemapSampler, reflection);
    
    float4 ssr = float4(cube, 1);
    
    if (factor>0.1)
        ssr = SampleSSR(reflection, worldPos, depth, normal, vDir);
    
    
    
    float3 reflectionColor = lerp(cube, ssr.rgb, ssr.w);
    
    return float4(reflectionColor, 1);

}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};