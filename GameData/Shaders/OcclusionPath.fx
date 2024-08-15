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

bool Viewmodel = false;

bool Masked;

float3 CameraPos;

bool pointDistance;

#define BONE_NUM 128

matrix Bones[BONE_NUM];

float NormalBias = 0;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    
    float4 BlendIndices : BLENDINDICES0;
    float4 BlendWeights : BLENDWEIGHT0;

    float4 Color : COLOR0;
    float3 SmoothNormal : NORMAL1;

    float2 TexCoords : TEXCOORD0;

};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 MyPosition : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float2 TexCoords : TEXCOORD2;
    float4 Color : COLOR0;
};

Texture2D Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4x4 GetBoneTransforms(VertexShaderInput input)
{
    
    float4x4 identity = float4x4(
    1.0, 0.0, 0.0, 0.0,
    0.0, 1.0, 0.0, 0.0,
    0.0, 0.0, 1.0, 0.0,
    0.0, 0.0, 0.0, 1.0);
    
    float sum = input.BlendWeights.x + input.BlendWeights.y + input.BlendWeights.z + input.BlendWeights.w;
    
    if (sum < 0.05f)
        return identity;
    
    float4x4 mbones =
    Bones[input.BlendIndices.x] * (float) input.BlendWeights.x / sum +
    Bones[input.BlendIndices.y] * (float) input.BlendWeights.y / sum +
    Bones[input.BlendIndices.z] * (float) input.BlendWeights.z / sum +
    Bones[input.BlendIndices.w] * (float) input.BlendWeights.w / sum;
    
    return mbones;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;

    float4x4 boneTrans = GetBoneTransforms(input);
    

    // Transform the vertex position to world space
    output.Position = mul(mul(input.Position, boneTrans), World);
    

    input.Position -= float4(normalize((mul(mul(float4(input.SmoothNormal,0), boneTrans), World)*NormalBias).xyz),0);

    output.WorldPos = output.Position;
    
    output.Position = mul(output.Position, ViewProjection);

    if (Viewmodel)
        output.Position.z *= 0.02;
    
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
    
    PS_Out output = (PS_Out)0;
    
    float depth = input.MyPosition.z;
    
    if (pointDistance)
        depth = distance(input.WorldPos, CameraPos);
    
    
    if (Masked && tex2D(TextureSampler, input.TexCoords).a*input.Color.a < 0.99)
        discard;

    const float a = 1;

    output.depth = float4(depth, 0, 0, a);
    output.depthHomo = float4(((input.MyPosition.z+(Viewmodel? +0.000013 : 0.001))/input.MyPosition.w), 0, 0, a);

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
