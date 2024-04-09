#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

matrix World;
matrix ViewProjection;

bool Masked;

float3 CameraPos;

bool pointDistance;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoords : TEXCOORD0;
};

Texture2D Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 MyPosition : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float2 TexCoords : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    // Transform the vertex position to world space
    output.Position = mul(input.Position, World);

    output.WorldPos = output.Position;

    output.Position = mul(output.Position, ViewProjection);

    output.MyPosition = output.Position;

    output.TexCoords = input.TexCoords;

    return output;
}

struct PS_Out
{
    float4 occlusion;
    float4 depth;
};

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{

    PS_Out output = (PS_Out)0;

    float depth = input.MyPosition.z;

    if (pointDistance)
        depth = distance(input.WorldPos, CameraPos);

    if (tex2D(TextureSampler, input.TexCoords).a < 0.99&& Masked)
        discard;

    return float4(depth, 0, 0, 1);
}

technique NormalColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
