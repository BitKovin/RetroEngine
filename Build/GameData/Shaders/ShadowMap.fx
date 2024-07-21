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

float3 LightDirection;

#define BONE_NUM 128

matrix Bones[BONE_NUM];

float bias = 0.02;

float depthBias = 0;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    
    float3 Tangent : TANGENT0;
    
    float4 BlendIndices : BLENDINDICES0;
    float4 BlendWeights : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 myPosition : TEXCOORD1;
    //float3 normal : TEXCOORD2;
};

float DepthScale = 1;

float3 GetTangentNormal(float3 worldNormal, float3 worldTangent)
{
    
    float3 normalMapSample = float3(0, 0, 1);
    
    
    // Create the tangent space matrix as before
    float3 bitangent = cross(worldNormal, worldTangent);
    float3x3 tangentToWorld = float3x3(worldTangent, bitangent, worldNormal);

    // Transform the normal from tangent space to world space
    float3 worldNormalFromTexture = mul(normalMapSample, tangentToWorld);

    // Normalize the final normal
    worldNormalFromTexture = normalize(worldNormalFromTexture);

    return worldNormalFromTexture;
}

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
    
    //float3 normal = GetTangentNormal(input.Normal, input.Tangent);
    
    float4x4 BonesWorld = mul(boneTrans, World);

    //output.normal = mul(input.Normal, (float3x3)BonesWorld);

    input.Position-= float4(input.Normal*bias,0);
    
    // Transform the vertex position to world space
    output.Position = mul(input.Position, BonesWorld);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.Position.z -= depthBias;
    output.myPosition = output.Position;
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
/*
    if(dot(normalize(input.normal), -normalize(LightDirection))>0.7)
    {
        discard;
    }
*/
    // Retrieve the depth value from the depth buffer
    float depthValue = input.myPosition.z;
    
        
	//depthValue /= 20;

    return float4(depthValue, depthValue, depthValue, 1);
}

technique NormalColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
