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
};

float screenWidth = 1280; // Change to your actual screen width
float screenHeight = 720; // Change to your actual screen height

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

// Function to calculate Gaussian weight
float Gaussian(float x, float y, float sigma) {
    return exp(-((x * x + y * y) / (2.0 * sigma * sigma))) / (2.0 * 3.14159265358979323846 * sigma * sigma);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	
    float3 color = 0;
	
float n = 0;
float sigma = 2.0; // Standard deviation for Gaussian distribution
float2 texelSize = 1 / float2(screenWidth, screenHeight);
float weightSum = 0.0;
color = float3(0, 0, 0);

float2 blurRadius = float2(2,2);

for (int x = -blurRadius.x; x <= blurRadius.x; x++) {
    for (int y = -blurRadius.y; y <= blurRadius.y; y++) {

        float2 TextureOffset = float2(x, y);
        float2 offsetCoords = TextureOffset * texelSize;
        float weight = Gaussian(x, y, sigma);
        weightSum += weight;
        color += tex2D(SpriteTextureSampler, input.TextureCoordinates + offsetCoords).rgb * weight;
    }
}

// Normalize the color by the sum of the weights
color = color / weightSum;
	
    //color = length(color)*lerp(normalize(color), length(color), lerp(length(color),1,0.5));
	
    //float l = CalcLuminance(color) * 2;

    return float4(color,1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};