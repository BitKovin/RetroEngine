#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float screenWidth = 1280; // Change to your actual screen width
float screenHeight = 720; // Change to your actual screen height

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

// Parameters for bilateral filter
const float sigmaColor = 0.1; // Adjust based on the color range of your texture
const float sigmaSpace = 5.0; // Adjust based on the spatial range of your texture

// Helper function to calculate the Gaussian weight
float Gaussian(float x, float sigma)
{
    return exp(-0.5 * (x * x) / (sigma * sigma));
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 texCoord = input.TextureCoordinates;
    float4 centerColor = tex2D(SpriteTextureSampler, texCoord);

    float4 finalColor = 0;
    float totalWeight = 0;

    return tex2D(SpriteTextureSampler, texCoord);

    int radius = 3; // Adjust the radius as needed
    for (int y = -radius; y <= radius; ++y)
    {
        for (int x = -radius; x <= radius; ++x)
        {
            float2 offset = float2(x, y) / float2(screenWidth, screenHeight);
            float4 sampleColor = tex2D(SpriteTextureSampler, texCoord + offset);

            // Calculate the spatial weight
            float spatialWeight = Gaussian(length(offset * float2(screenWidth, screenHeight)), sigmaSpace);

            // Calculate the color weight
            float colorWeight = Gaussian(distance(centerColor.rgb, sampleColor.rgb), sigmaColor);

            // Combine weights
            float weight = spatialWeight * colorWeight;

            finalColor += sampleColor * weight;
            totalWeight += weight;
        }
    }

    finalColor /= totalWeight;
    return finalColor;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};