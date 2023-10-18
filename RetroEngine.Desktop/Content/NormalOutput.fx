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


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0; // Add normal input
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 Normal : TEXCOORD0; // Pass normal to pixel shader
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    // Transform the vertex position to world space
    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.Color = input.Color;

    // Pass the world space normal to the pixel shader
    output.Normal = mul(input.Normal, (float3x3)World);

    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    // Use the world space normal as color
    float3 color = (input.Normal) * 0.5 + 0.5; // Map normal to [0,1] range

    return float4(color, 1.0f);
}

technique NormalColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
