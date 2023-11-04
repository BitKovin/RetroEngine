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
struct VertexInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct PixelInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

PixelInput VertexShaderFunction(VertexInput input)
{
    PixelInput output;

    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.TexCoord = input.TexCoord;

    return output;
}

float4 PixelShaderFunction(PixelInput input) : COLOR0
{
    return tex2D(TextureSampler, input.TexCoord);
}

technique BasicTechnique
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
