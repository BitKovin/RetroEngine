#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D Texture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <Texture>;
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
	
    float3 color = tex2D(SpriteTextureSampler, input.TexCoord).rgb;
	
    float3 reflection = tex2D(ReflectionTextureSampler, input.TexCoord).rgb;
	
    reflection *= color;
	
    float factor = tex2D(FactorTextureSampler, input.TexCoord).r;
	
    float3 result = lerp(color, reflection, factor);
	
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