﻿#if OPENGL
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

struct VertexShaderInput
{
    float4 Position : POSITION0;
    
    float4 BlendIndices : BLENDINDICES0;
    float4 BlendWeights : BLENDWEIGHT0;

    float2 TexCoords : TEXCOORD0;

};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 MyPosition : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float2 TexCoords : TEXCOORD2;
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
    
    output.WorldPos = output.Position;
    
    output.Position = mul(output.Position, ViewProjection);

    if (Viewmodel)
        output.Position.z *= 0.02;
    
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
    
    
    if (Masked && tex2D(TextureSampler, input.TexCoords).a < 0.99)
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