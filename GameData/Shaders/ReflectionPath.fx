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

bool enableSSR;

float4 SampleSSR(float3 direction, float3 position, float currentDepth, float3 normal, float3 vDir)
{
    
    float Step = 0.015;
    
    const int steps = 40;
    
    float4 outColor = float4(0, 0, 0, 0);
    
    float3 selectedCoords;
    
    float3 dir = normalize(direction);
    
    float3 pos = position;
    
    float2 coords;
    
    float2 outCoords;
    
    float weight = -0.3;
   
    float factor = 1.4;
    
    bool facingCamera = false; dot(vDir, direction) < 0;
    
    
    float disToCamera = length(viewPos - position);
    
    for (int i = 0; i < steps; i++)
    {
        
        float3 offset = dir * (Step) * disToCamera / 30 + dir * 0.02 * disToCamera;
        
        
        float dist = WorldToClip(pos + offset).z;
        
        
        selectedCoords = pos + offset;
        
        coords = WorldToScreen(selectedCoords);

        float SampledDepth = SampleMaxDepth(coords);

        bool inScreen = coords.x > 0.001 && coords.x < 0.999 && coords.y > 0.001 && coords.y < 0.999;
        
        

        if (SampledDepth < currentDepth - 0.25 && facingCamera == false)
        {
            return float4(0, 0, 0, 0);

            Step /= factor;
            factor = lerp(factor, 1, 0.5);
            weight-=3;

        }
        
        if (inScreen == false || SampledDepth>10000)
        {
            Step == 0.02;
            factor = lerp(factor, 1, 0.5);
        }
        
        if (SampledDepth + 0.025 < dist && (SampledDepth > dist - 1 || facingCamera == false))
        {

            Step /= factor;
            factor = lerp(factor, 1, 0.5);

            outCoords = coords;
            
            weight += 1;
            
            
            continue;

        }

        Step *= factor;
        Step += 0.01;
    }
    
    weight = step(2,weight);

    //weight = saturate(weight);

    outColor = float4(tex2D(FrameTextureSampler, coords).rgb,  weight);
    
    return outColor;
    
}

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
    
    if (enableSSR == false)
        return float4(cube, 1);
    
    float4 ssr = float4(cube, 1);
    
    if (factor > 0.0)
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