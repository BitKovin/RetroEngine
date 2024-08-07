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

bool black = false;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
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
    float4 Color : COLOR0;
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

    output.Color = input.Color;

    return output;
}

struct PS_Out
{
    float4 depth : COLOR0;
    float4 depthHomo: COLOR1; //this value is gay for gpu (stores depth in same homogeneous way)
};

PS_Out MainPS(VertexShaderOutput input)
{
    float depth = input.MyPosition.z;

    if (pointDistance)
        depth = distance(input.WorldPos, CameraPos);
    
#if OPENGL
    if (Masked && tex2D(TextureSampler, input.TexCoords).a * input.Color.a < 0.99)
        discard;
#else 

    if(Masked)
        {
            if(tex2D(TextureSampler, input.TexCoords).a* input.Color.a < 0.99)
                discard;
        }
#endif

    float a = black? 0:1;

    PS_Out output;
    output.depth = float4(depth, 0, 0, a);
    output.depthHomo = float4((input.MyPosition.z+0.01)/input.MyPosition.w, 0, 0, a);

    return output;
}

technique NormalColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
