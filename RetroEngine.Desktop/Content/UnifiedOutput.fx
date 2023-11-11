#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix View;
matrix Projection;
texture Texture;
sampler TextureSampler = sampler_state { texture = <Texture>; };

float DirectBrightness;
float GlobalBrightness;
float3 LightDirection;

struct VertexInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0; // Add normal input
    float2 TexCoord : TEXCOORD0;
	
};

struct PixelInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1; // Pass normal to pixel shader
};

float3 normalize(float3 v)
{
  return rsqrt(dot(v,v))*v;
}

PixelInput VertexShaderFunction(VertexInput input)
{
    PixelInput output;

    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.TexCoord = input.TexCoord;

	// Pass the world space normal to the pixel shader
    output.Normal = mul(input.Normal, (float3x3)World);
    output.Normal = normalize(output.Normal);

    return output;
}

float4 PixelShaderFunction(PixelInput input) : COLOR0
{

	float3 textureColor = tex2D(TextureSampler, input.TexCoord).xyz;
	float textureAlpha = tex2D(TextureSampler, input.TexCoord).w;

	float lightingFactor = max(-0.3, dot(input.Normal, normalize(-LightDirection))) * DirectBrightness; // Example light direction

	lightingFactor += GlobalBrightness;

	textureColor *= lightingFactor;

    return float4(textureColor, textureAlpha);
}

technique BasicTechnique
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
