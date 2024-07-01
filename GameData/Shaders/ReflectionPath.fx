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

float DistanceToBorder(float2 uv)
{
    // Calculate distance to the four borders
    float left = uv.x;
    float right = 1.0 - uv.x;
    float top = uv.y;
    float bottom = 1.0 - uv.y;

    // Find the minimum distance to any border
    float minDistance = min(min(left, right), min(top, bottom));

    return minDistance;
}

float FadeDistance(float distance, float minDistance, float maxDistance)
{
    // Clamp the distance between the min and max distances
    float clampedDistance = clamp(distance, minDistance, maxDistance);
    
    // Normalize the clamped distance to a 0-1 range
    float normalizedDistance = (clampedDistance - minDistance) / (maxDistance - minDistance);
    
    // Use smoothstep to create a smooth transition between 0 and 1
    float fadeValue = smoothstep(0.0, 1.0, normalizedDistance);
    
    return fadeValue;
}

float4 SampleSSR(float3 direction, float3 position, float currentDepth, float3 normal, float3 vDir)
{
    
    float Step = 0.015;
    
    const int steps = 50;
    
    float4 outColor = float4(0, 0, 0, 0);
    
    float3 selectedCoords;
    
    float3 dir = normalize(direction);
    
    float3 pos = position;
    
    float2 coords;
    
    float2 outCoords = 0;
    
    float weight = -0.3;
   
    float factor = 1.4;
    
    bool facingCamera = false; dot(vDir, direction) < 0;
    
    
    float disToCamera = length(viewPos - position);
    
    float2 oldCoords = 0;
    float oldDepth = 0;

    float lastHitStep = 0.015;

    for (int i = 0; i < steps; i++)
    {
        
        float3 offset = dir * (Step) * disToCamera / 30 + dir * 0.02 * disToCamera;
        
        
        float dist = WorldToClip(pos + offset).z;
        
        
        selectedCoords = pos + offset;
        
        

        coords = WorldToScreen(selectedCoords);

        float SampledDepth = SampleDepth(coords);

        bool inScreen = coords.x > 0.001 && coords.x < 0.999 && coords.y > 0.001 && coords.y < 0.999;
        
        if(i<3)
        {
            oldCoords = coords;
            oldDepth = SampledDepth;
        }

        if (SampledDepth < currentDepth - 0.05 && facingCamera == false)
        {
            Step = lastHitStep;
            factor = lerp(factor, 1, 0.5);
            continue;

        }
        
        if (inScreen == false || SampledDepth>1000)
        {
            Step /= 3;
            factor = lerp(factor, 1, 0.5);
            continue;
        }

        if (SampledDepth + 0.015 < dist&& SampledDepth>0.3f)
        {

            if(distance(oldDepth, SampledDepth)<disToCamera/15)
                outCoords = lerp(coords, oldCoords,1);

            Step /= factor;
            lastHitStep = Step;
            factor = lerp(factor, 1, 0.5);

            weight += 1;
            
            //oldCoords = coords;
            //oldPos = pos + offset;

            continue;

        }
        oldCoords = coords;
        oldDepth = SampledDepth;
        Step *= factor;
    }
    
    weight = step(2,weight);

    weight *= FadeDistance(DistanceToBorder(outCoords), 0, 0.1);

    //weight = saturate(weight);

    outColor = float4(tex2D(FrameTextureSampler, outCoords).rgb,  weight);
    
    return outColor;
    
}

// Function to generate a random float based on the surface coordinates
float Random (float2 uv)
{
    return frac(sin(dot(uv,float2(12.9898,78.233)))*758.5453123);
}

// Function to generate a random vector based on the surface coordinates and roughness
float3 RandomVector(float2 uv, float roughness)
{
    float3 randomVec;
    randomVec.x = Random(uv + roughness);
    randomVec.y = Random(uv + roughness * 2.0);
    randomVec.z = Random(uv + roughness * 3.0);
    return normalize(randomVec * 2.0 - 1.0);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	
    float3 normal = tex2D(NormalTextureSampler, input.TextureCoordinates).rgb * 2 - 1;
    float depth = SampleDepth(input.TextureCoordinates).r;
	
    
    float3 worldPos = tex2D(PositionTextureSampler, input.TextureCoordinates).xyz + viewPos;
    
    float3 vDir = normalize(worldPos - viewPos);
    
    float3 reflection = reflect(normalize(vDir), normal);
    float3 reflectionBase = reflection;

    
    float2 texel = float2(1.5/SSRWidth, 1.5/SSRHeight);
    float3 factor = tex2D(FactorTextureSampler, input.TextureCoordinates).rgb;

    float roughness = saturate(factor.g/2 - 0.1);

    // Add noise to the reflection vector based on surface roughness
    float3 noise = RandomVector(input.TextureCoordinates, 1);
    noise *= (dot(noise, normal)<0) ? -1 : 1;
    
    reflection = normalize(reflection + noise * roughness);
    
    float3 cube = SampleCubemap(ReflectionCubemapSampler, reflection);
    cube += SampleCubemap(ReflectionCubemapSampler, normalize(reflectionBase + RandomVector(input.TextureCoordinates, 3) * roughness));
    cube += SampleCubemap(ReflectionCubemapSampler, normalize(reflectionBase + RandomVector(input.TextureCoordinates, 4) * roughness));
    cube += SampleCubemap(ReflectionCubemapSampler, normalize(reflectionBase + RandomVector(input.TextureCoordinates, 7) * roughness));
    cube += SampleCubemap(ReflectionCubemapSampler, normalize(reflectionBase + RandomVector(input.TextureCoordinates, 5) * roughness));
    cube += SampleCubemap(ReflectionCubemapSampler, normalize(reflectionBase + RandomVector(input.TextureCoordinates, 2) * roughness));
    cube/=6;

    if (enableSSR == false)
        return float4(cube, 1);
    
    float4 ssr = float4(cube, 1);
    
    if (factor.x > 0.1)
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