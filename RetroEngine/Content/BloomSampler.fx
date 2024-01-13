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

float offset = 0.8f;

float screenWidth = 1280; // Change to your actual screen width
float screenHeight = 720; // Change to your actual screen height

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	
    float3 color = 0;
	

    color += saturate(tex2D(SpriteTextureSampler, input.TextureCoordinates).rgb - offset);
	
	
    float sampleRadius = 10;
	
	
    for (int x = -1 * sampleRadius; x <= sampleRadius; x ++)
        for (int y = -1 * sampleRadius; y <= sampleRadius; y ++)
        {
            
            float2 TextureOffset = float2(x, y);
            
            if (length(TextureOffset) > sampleRadius)
                continue;
			
            float2 offsetCoords = TextureOffset / float2(screenWidth * 3, screenHeight * 3);
			
            color += saturate(tex2D(SpriteTextureSampler, input.TextureCoordinates + offsetCoords).rgb - offset) / (length(TextureOffset) + 1);
        }
	
    return float4(color/50.0f,1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};