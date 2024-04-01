#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

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

    const float radius = 50;
    const float bias = ssaoBias;

    float rotation = texCoord.x + texCoord.y;

    for (float l = 0; l <= radius; l += radius/20)
        for (float angle = 0.0; angle < 6.283; angle += 0.5)
        {
            float2 offset = l * float2(cos(angle + rotation), sin(angle + rotation));
            float2 sampleCoord = texCoord + offset / 256 / depth;
       
            if (sampleCoord.x > 1 || sampleCoord.y > 1 || sampleCoord.x < 0 || sampleCoord.y < 0)
                continue;

            float3 sampleNormal = DecodeNormal(tex2D(NormalTextureSampler, sampleCoord).xyz);
            float sampleDepth = tex2D(DepthTextureSampler, sampleCoord).r;

            float depthDifference = depth - sampleDepth + bias;
            
            depthDifference = clamp(depthDifference, -4, 4);

            float factor = (radius - l) / radius;

            
            float normalDifference = 1 - dot(normal, sampleNormal);
       
            float occlusion = normalDifference * depthDifference;
       
            ao += occlusion / lerp(depth, 1, 0.995) * ssaoIntensity * factor;
        }

    ao /= 63.0 * 3;
    return ao ;
}

// Hermite function for smooth falloff (replace with your preferred falloff function)
float HermiteFunction(float value, float edge0, float edge1, float slope0)
{
    float t = clamp((value - edge0) / (edge1 - edge0), 0.0f, 1.0f);
    return (3.0f * t * (t - 1.0f) * (t - 1.0f)) * slope0;
}


// Pixel shader
float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample depth, normal, and color
    float depth = tex2D(DepthTextureSampler, texCoord).r;
    float3 normal = DecodeNormal(tex2D(NormalTextureSampler, texCoord).xyz);

    float ao = 0;
    
    float sampleRadius = 2;
    
    ao += CalculateSSAO(texCoord, depth, normal);

    
    // Apply AO to the final color
    float3 finalColor = 1;// -ao;

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