#include "ShaderLib/BasicShader.fx"

texture NormalTexture;
sampler NormalTextureSampler = sampler_state
{
    texture = <NormalTexture>;
};

texture PosTexture;
sampler PosTextureSampler = sampler_state
{
    texture = <PosTexture>;
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

    const float radius = 0.06;
    const float bias = ssaoBias;

    const float steps = 10;

    float rotation = 1;

    if (depth > 1000)
        return 0;

    for (float l = 0; l <= radius; l += radius / steps)
        for (float angle = 0.0; angle < 6.283; angle += 0.5)
        {

            float sampleL = l * lerp(1, 0.1, depth/50);

            float2 offset = sampleL * float2(cos(angle), sin(angle));
            float2 sampleCoord = texCoord + offset;

            if (sampleCoord.x > 1 || sampleCoord.y > 1 || sampleCoord.x < 0 || sampleCoord.y < 0)
                continue;

            float sampleDepth = tex2D(DepthTextureSampler, sampleCoord).r;

            float depthDifference = -0.6;// sampleDepth - sampleDepth + bias;

            if (depth > sampleDepth + bias && (depth - sampleDepth) < 1)
                depthDifference = 1;

            float rangeCheck = smoothstep(1, 0, (depth - sampleDepth) / 2);

            depthDifference *= rangeCheck;

            depthDifference = clamp(depthDifference, -4, 4);

            float factor = (radius - l) / radius;

            float occlusion = depthDifference;

            ao += occlusion / lerp(depth, 1, 0.995) * ssaoIntensity * factor / steps * 30;
        }

    ao /= 63.0 * 3;
    return ao;
}

#define KERNEL_SIZE 16
#define RADIUS 0.5



float CalculateSSAOnew(float3 pos, float2 texCoord, float depth, float3 normal)
{
    float occlusion = 0.0;
    float3 random = float3(1, 1, 1);

float3 kernel[KERNEL_SIZE] = {
    float3(0.5381, 0.1856, 0.4319),
    float3(0.1379, 0.2486, 0.4430),
    float3(0.3371, 0.5679, 0.0057),
    float3(-0.6999, -0.0451, 0.0019),
    float3(0.0687, -0.1593, 0.8547),
    float3(0.4768, 0.0482, 0.6726),
    float3(0.5379, -0.4883, 0.6876),
    float3(0.2917, -0.2620, 0.0308),
    float3(-0.2158, -0.3910, 0.7594),
    float3(-0.6825, 0.3020, 0.3280),
    float3(-0.0271, 0.0790, 0.9444),
    float3(-0.3024, -0.9026, 0.2259),
    float3(0.0227, -0.4881, 0.2835),
    float3(-0.0989, -0.3277, 0.3645),
    float3(0.0978, -0.0402, 0.5017),
    float3(0.0534, 0.5204, 0.6209)
};

    // Calculate tangent and bitangent vectors
        float3 up = abs(normal.y) < 0.999 ? float3(0, 1, 0) : float3(1, 0, 0);
        float3 tangent = normalize(cross(up, normal));
        float3 bitangent = cross(normal, tangent);
    float3x3 TBN = float3x3(tangent, bitangent, normal);

    for (int i = 0; i < KERNEL_SIZE; ++i)
    {
        float3 samplePos = mul(TBN, kernel[i]);
        samplePos = pos + samplePos * RADIUS;

        float sampleDepth = SampleDepthWorldCoords(samplePos);
        float rangeCheck = smoothstep(0.0, 1.0, RADIUS / abs(depth - sampleDepth));
        occlusion += (sampleDepth >= depth + ssaoBias ? 1.0 : 0.0) * rangeCheck;
    }

    occlusion = (occlusion / KERNEL_SIZE);
    return occlusion;
}

// Pixel shader
float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
// Sample depth, normal, and color
float depth = tex2D(DepthTextureSampler, texCoord).r;
float3 normal = DecodeNormal(tex2D(NormalTextureSampler, texCoord).xyz);
float3 pos = tex2D(PosTextureSampler, texCoord).xyz + viewPos;

float ao = 0;

float sampleRadius = 2;

ao += CalculateSSAO(texCoord, depth, DecodeNormal(normal));


// Apply AO to the final color
float finalColor = 1 - ao;

return float4(finalColor, finalColor, finalColor, 1.0);
}

// Technique
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}