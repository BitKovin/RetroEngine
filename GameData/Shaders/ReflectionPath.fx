#include "ShaderLib/BasicShader.fx"


TextureCube ReflectionCubemap;
sampler ReflectionCubemapSampler = sampler_state
{
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D NormalTexture;
Texture2D PositionTexture;
Texture2D FactorTexture;

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
            return float4(0,0,0,0);
            Step == 0.02;
            factor = lerp(factor, 1, 0.5);
        }
        
        if (SampledDepth + 0.025 < dist && (SampledDepth > dist - 1 || facingCamera == false))
        {

            Step /= factor;
            factor = lerp(factor, 1, 0.5);

            outCoords = coords;
            
            weight += 1;
            
            if(factor < 1.01)
            break;
            
            continue;

        }

        Step *= factor;
        Step += 0.01;
    }
    
    weight = step(2,weight);

    //weight = saturate(weight);

    outColor = float4(SAMPLE_TEXTURE(FrameTexture,LinearSampler, coords).rgb,  weight);
    
    return outColor;
    
}

float4 MainPS(VertexShaderOutput input) : SV_Target0
{
	
    float3 normal = SAMPLE_TEXTURE(NormalTexture,LinearSampler, input.TextureCoordinates).rgb * 2 - 1;
    float depth = SampleDepth(input.TextureCoordinates).r;
	
    
    float3 worldPos = SAMPLE_TEXTURE(PositionTexture, LinearSampler, input.TextureCoordinates).xyz + viewPos;
    
    float3 vDir = normalize(worldPos - viewPos);
    
    float3 reflection = reflect(normalize(vDir), normal);
    
    
    
    float2 texel = float2(1.5/SSRWidth, 1.5/SSRHeight);
    float factor = SAMPLE_TEXTURE(FactorTexture,LinearSampler, input.TextureCoordinates).r;
    factor = max(SAMPLE_TEXTURE(FactorTexture,LinearSampler, input.TextureCoordinates + float2(texel.x,0)).rgb, reflection);
    factor = max(SAMPLE_TEXTURE(FactorTexture,LinearSampler, input.TextureCoordinates + float2(-texel.x,0)).rgb, reflection);
    factor = max(SAMPLE_TEXTURE(FactorTexture,LinearSampler, input.TextureCoordinates + float2(0,texel.y)).rgb, reflection);
    factor = max(SAMPLE_TEXTURE(FactorTexture,LinearSampler, input.TextureCoordinates + float2(0,texel.y)).rgb, reflection);


    reflection = normalize(reflection);
    
    float3 cube = SampleCubemap(ReflectionCubemap, reflection);
    
    if (enableSSR == false)
        return float4(cube, 1);
    
    float4 ssr = float4(cube, 1);
    
    if (factor > 0.1)
        ssr = SampleSSR(reflection, worldPos, depth, normal, vDir);
    
    float3 reflectionColor = lerp(cube, ssr.rgb, ssr.w);
    
    return float4(reflectionColor, 1);

}

// Vertex Shader
VertexShaderOutput SimpleVertexShader(VertexInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    // Pass the position directly to the pixel shader
    output.Position = input.Position;

    output.Color = float4(1,1,1,1);

    // Pass the texture coordinates directly to the pixel shader
    output.TextureCoordinates = input.TexCoord;

    return output;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();

		VertexShader = compile VS_SHADERMODEL SimpleVertexShader();

	}
};