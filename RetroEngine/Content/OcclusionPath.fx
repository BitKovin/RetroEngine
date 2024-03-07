#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

matrix World;
matrix View;
matrix Projection;

float3 CameraPos;

bool pointDistance;

#define BONE_NUM 128

matrix Bones[BONE_NUM];

struct VertexShaderInput
{
    float4 Position : POSITION0;
    
    float4 BlendIndices : BLENDINDICES0;
    float4 BlendWeights : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 MyPosition : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
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
    
    output.WorldPos = output.Position;
    
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);

    output.MyPosition = output.Position;
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    
    float depth = input.MyPosition.z;
    
    if (pointDistance)
        depth = distance(input.WorldPos, CameraPos);
    
    return float4(depth, 0, 0, 1);
}

float4 MainPSPoint(VertexShaderOutput input) : SV_TARGET
{
    
    float depth = input.MyPosition.z;
    
    depth = distance(input.WorldPos, CameraPos);
    
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
