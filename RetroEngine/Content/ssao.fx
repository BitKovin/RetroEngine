#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

texture ColorTexture;
sampler ColorTextureSampler = sampler_state
{
    texture = <ColorTexture>;
};

texture DepthTexture;
sampler DepthTextureSampler = sampler_state
{
    texture = <DepthTexture>;
};

texture NormalTexture;
sampler NormalTextureSampler = sampler_state
{
    texture = <NormalTexture>;
};

// Assuming these are your screen dimensions
float screenWidth = 1280; // Change to your actual screen width
float screenHeight = 720; // Change to your actual screen height

// Constants for SSAO
float ssaoRadius = 0.1;
float ssaoBias = 0.01;
float ssaoIntensity = 1.0;

// Function to convert normal from 0-1 to -1 to 1
float3 DecodeNormal(float3 normal)
{
    return normal * 2 - 1;
}



// SSAO function
float CalculateSSAO(float2 texCoord, float depth, float3 normal)
{
    float ao = 0.0;

    float radius = ssaoRadius;
    const float bias = 0.01;

    float rotation = texCoord.x + texCoord.y;

    for (float angle = 0.0; angle < 6.283; angle += 0.3)
    {
        float2 offset = radius * float2(cos(angle + rotation), sin(angle + rotation));
        float2 sampleCoord = texCoord + offset / float2(screenWidth, screenHeight);

        // Clamp sample coordinates to prevent sampling outside texture bounds
        sampleCoord = clamp(sampleCoord, 0.0, 1.0);

        float3 sampleNormal = DecodeNormal(tex2D(NormalTextureSampler, sampleCoord).xyz);
        float sampleDepth = tex2D(DepthTextureSampler, sampleCoord).r;

        float depthDifference = sampleDepth - depth + bias;

        if (depthDifference < 0)
        {
            continue;
        }

        // Adjust normals for smoother transitions at edges
        float3 adjustedNormal = lerp(normal, sampleNormal, smoothstep(0.0, 0.1, length(offset)));

        float normalDifference = dot(adjustedNormal, sampleNormal);
        float occlusion = 1.0 - smoothstep(0.0, radius, length(depthDifference) / (radius * 0.25) + normalDifference / radius);

        ao += occlusion;
    }

    ao /= 63.0;
    return ao * ssaoIntensity;
}


// Pixel shader
float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample depth, normal, and color
    float depth = tex2D(DepthTextureSampler, texCoord).r;
    float3 normal = DecodeNormal(tex2D(NormalTextureSampler, texCoord).xyz);
    float3 albedo = tex2D(ColorTextureSampler, texCoord).rgb;

    float ao = 0;
    
    float sampleRadius = 2;
    
    ao += CalculateSSAO(texCoord, depth, normal);
    
    for (int x = -1 * sampleRadius; x < 2 * sampleRadius; x+= sampleRadius)
        for (int y = -1 * sampleRadius; y < 2 * sampleRadius; y += sampleRadius)
        {
            
            float2 offset = float2(x,y);
            
            float2 offsetCoords = offset / float2(screenWidth, screenHeight);
            
            ao += CalculateSSAO(texCoord + offsetCoords, depth, normal) / (length(offset) + 1);
        }
    
    // Apply AO to the final color
    float3 finalColor = 1 * (1.0 - ao) + albedo*0.00000000000000001f;

    return float4(finalColor, 1.0);
}

// Technique
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}