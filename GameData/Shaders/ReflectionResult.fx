#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

Texture2D ReflectionTexture;

sampler2D ReflectionTextureSampler = sampler_state
{
    Texture = <ReflectionTexture>;
};

Texture2D FactorTexture;

sampler2D FactorTextureSampler = sampler_state
{
    Texture = <FactorTexture>;
};

int screenWidth;
int screenHeight;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

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

float2 blurRadius = float2(1,1);

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