//#include "ShaderLib/BasicShader.fx"

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

    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;

};

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

bool Enabled;

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
        for (float angle = 0.0; angle < 6.283; angle += 0.7853)
        {

            float sampleL = l * lerp(1, 0.1, depth/50);

            float2 offset = sampleL * float2(cos(angle), sin(angle));
            offset*= float2(1,1.5);
            float2 sampleCoord = texCoord + offset;

            if (sampleCoord.x > 1 || sampleCoord.y > 1 || sampleCoord.x < 0 || sampleCoord.y < 0)
                continue;

            float sampleDepth = tex2D(DepthTextureSampler, sampleCoord).r;

            float depthDifference = -0.76;// sampleDepth - sampleDepth + bias;

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

// Pixel shader
float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
// Sample depth, normal, and color
float depth = tex2D(DepthTextureSampler, texCoord).r;
float3 normal = DecodeNormal(tex2D(NormalTextureSampler, texCoord).xyz);
//float3 pos = tex2D(PosTextureSampler, texCoord).xyz + viewPos;

float ao = 0;

float sampleRadius = 2;

#if OPENGL == FALSE
if(Enabled)
    ao += CalculateSSAO(texCoord, depth, DecodeNormal(normal));
#endif

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