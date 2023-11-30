#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
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
    
    radius = min(radius, 0.99f);
    
    // Sample surrounding points in a hemisphere
    for (float angle = 0; angle < 6.283; angle += 0.1)  // Full circle in steps of 0.1 radians
    {
        float2 offset = radius * float2(cos(angle), sin(angle));
        float2 sampleCoord = texCoord + offset / float2(screenWidth, screenHeight);

        if (sampleCoord.x < 0 || sampleCoord.x > 1 || sampleCoord.y < 0 || sampleCoord.y > 1)
        {
            continue;
        }
            
        
        // Decode normal from 0-1 to -1 to 1
            float3 sampleNormal = DecodeNormal(tex2D(NormalTextureSampler, sampleCoord).xyz);

        // Sample depth from the depth buffer
        float sampleDepth = tex2D(DepthTextureSampler, sampleCoord).r;
        
        if (depth - sampleDepth> 0.0002f)
            continue;
        
        // Calculate occlusion factor based on depth and normal differences
            float depthDifference = max(sampleDepth - depth, 0);
        float normalDifference = dot(normal, sampleNormal);
        
        float occlusion = 1.0 - smoothstep(0.0, radius, length(depthDifference) / (radius * 0.25) + dot(normal, sampleNormal) / radius);
        
        
        occlusion *= 1;
        
        ao += occlusion;
    }


    // Average the occlusion values
    ao /= 63.0;

    // Output the depth value for debugging (visualize it in the final color)
    return ao * ssaoIntensity;
}


// Pixel shader
float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample depth, normal, and color
    float depth = tex2D(DepthTextureSampler, texCoord).r;
    float3 normal = DecodeNormal(tex2D(NormalTextureSampler, texCoord).xyz);
    float3 albedo = tex2D(ColorTextureSampler, texCoord).rgb;

    // Calculate SSAO
    float ao = CalculateSSAO(texCoord, depth, normal);

    // Apply AO to the final color
    float3 finalColor = albedo * (1.0 - ao);

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