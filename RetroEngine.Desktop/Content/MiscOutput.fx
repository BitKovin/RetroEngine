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
float3 CameraPosition;


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
    float4 myPosition : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    // Transform the vertex position to world space
    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.Color = input.Color;
    output.myPosition = mul(input.Position, World);

    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{

    // Retrieve the depth value from the depth buffer
    float depthValue = length(input.myPosition.xyz - CameraPosition);



	depthValue /= 100;

	depthValue = pow(depthValue, 0.5);

    return float4(depthValue,0,0,1);
}

technique NormalColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
