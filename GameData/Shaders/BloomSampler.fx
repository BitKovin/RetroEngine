#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

Texture2D SpriteTexture; // screen texture

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

float offset = 0.95f; // brightness bloom offset
float screenWidth = 1280; // Change to your actual screen width
float screenHeight = 720; // Change to your actual screen height
float bloomRadius = 5.0f; // bloom radius
float bloomIntensity = 0.5; // bloom intensity

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float CalcLuminance(float3 color)
{
    return dot(color, float3(0.299f, 0.587f, 0.114f));
}


float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 texCoord = input.TextureCoordinates;
    float4 color = tex2D(SpriteTextureSampler, texCoord);

    float3 bloomColor = float4(0, 0, 0, 0);
    float totalWeight = 0.0f;

    // Sample the surrounding pixels
    for (int x = -int(bloomRadius); x <= int(bloomRadius); ++x)
    {
        for (int y = -int(bloomRadius); y <= int(bloomRadius); ++y)
        {
            float2 sampleCoord = texCoord + float2(x / screenWidth, y / screenHeight);
            float weight = exp(-(x * x + y * y) / (2 * bloomRadius * bloomRadius));
            bloomColor += max(tex2D(SpriteTextureSampler, sampleCoord).rgb - offset,0) * weight;
            totalWeight += weight;
        }
    }

    bloomColor /= totalWeight;

    bloomColor = pow(bloomColor, 0.7);

    bloomColor *= bloomIntensity;

    return float4(bloomColor,1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};