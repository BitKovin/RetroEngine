#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

Texture2D Texture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <Texture>;
};

float offset = 0.8f;

float screenWidth = 1280; // Change to your actual screen width
float screenHeight = 720; // Change to your actual screen height

struct VertexInput
{
    float4 PositionPS : POSITION;
    float4 Diffuse    : COLOR0;
    float2 TexCoord   : TEXCOORD0;
};

// Vertex Shader Output Structure
struct VertexOutput
{
    float4 PositionPS : SV_Position0;
    float4 Diffuse    : COLOR0;
    float2 TexCoord   : TEXCOORD0;
};

// Vertex Shader
VertexOutput SimpleVertexShader(VertexInput input)
{
    VertexOutput output;

    // Pass the position directly to the pixel shader
    output.PositionPS = input.PositionPS;

    output.Diffuse = float4(1,1,1,1);

    // Pass the texture coordinates directly to the pixel shader
    output.TexCoord = input.TexCoord;

    return output;
}

float4 MainPS(VertexOutput input) : COLOR
{
	
    float3 color = 0;
	

    color += saturate(tex2D(SpriteTextureSampler, input.TexCoord).rgb - offset);
	
	
    float sampleRadius = 6;
	
	
    for (int x = -1 * sampleRadius; x <= sampleRadius; x ++)
        for (int y = -1 * sampleRadius; y <= sampleRadius; y ++)
        {
            
            float2 TextureOffset = float2(x, y);
            
            if (length(TextureOffset) > sampleRadius)
                continue;
			
            float2 offsetCoords = TextureOffset / float2(screenWidth * 3, screenHeight * 3);
			
            color += saturate(tex2D(SpriteTextureSampler, input.TexCoord + offsetCoords).rgb - offset) / (length(TextureOffset) + 1);
        }
	
    color = color / 30.0f;
	
    color = length(color)*lerp(normalize(color), length(color), length(color)*2);
	
    return float4(color,1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
        VertexShader = compile VS_SHADERMODEL SimpleVertexShader();
	}
};