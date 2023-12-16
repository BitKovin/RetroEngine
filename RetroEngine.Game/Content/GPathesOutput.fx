#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;
matrix World;

float DepthScale = 1;

texture ColorTexture;
sampler ColorTextureSampler = sampler_state
{
    texture = <ColorTexture>;
};

texture EmissiveTexture;
sampler EmissiveTextureSampler = sampler_state
{
    texture = <EmissiveTexture>;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float3 WorldPosition : COLOR1;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Emissive : COLOR1;
    float4 Normal : COLOR2;
    float4 Position : COLOR3;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
    output.Position.z *= DepthScale;
    output.Normal = normalize(mul(input.Normal, (float3x3) World));
    output.TexCoord = input.TexCoord;
    output.WorldPosition = mul(input.Position, World).xyz;

	return output;
}

PixelShaderOutput MainPS(VertexShaderOutput input)
{
	
    PixelShaderOutput output = (PixelShaderOutput) 0;
	
    if (tex2D(ColorTextureSampler, input.TexCoord).a >= 0.5f)
    {
        output.Color = float4(tex2D(ColorTextureSampler, input.TexCoord).rgb, 1);
        output.Emissive = float4(tex2D(EmissiveTextureSampler, input.TexCoord).rgb, 1);
        output.Normal = float4(input.Normal / 2.0f + 0.5f, 1);
        output.Position = float4(input.WorldPosition, 1);
    }
    else
    {
        output.Color = output.Emissive = output.Normal = output.Position = float4(0, 0, 0, 0);

    }
	return output;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};