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

float ScreenHeight;
float ScreenWidth;

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
    float2 TexCoords : TEXCOORD2;
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

    output.Position = mul(output.Position, ViewProjection);
    
    output.Position.z -= 0.0001;

    output.MyPosition = output.Position;
    
    output.TexCoords = input.TexCoords;


    return output;
}

// Bayer matrix (4x4) for dithering
static const float4x4 BayerMatrix = {
    0.0f / 16.0f,  8.0f / 16.0f,  2.0f / 16.0f, 10.0f / 16.0f,
   12.0f / 16.0f,  4.0f / 16.0f, 14.0f / 16.0f,  6.0f / 16.0f,
    3.0f / 16.0f, 11.0f / 16.0f,  1.0f / 16.0f,  9.0f / 16.0f,
   15.0f / 16.0f,  7.0f / 16.0f, 13.0f / 16.0f,  5.0f / 16.0f
};

// Function to get the texel size based on the screen resolution
float2 GetTexelSize(float2 screenResolution) {
    return 1.0f / screenResolution;
}

// Dithering function
bool Dither(float2 screenCoords, float amount, float2 screenResolution) {
    float2 texelSize = GetTexelSize(screenResolution);

    // Calculate the Bayer matrix index
    int2 index = int2(screenCoords / texelSize) % 4;

    // Get the Bayer matrix value
    float threshold = BayerMatrix[index.x][index.y];

    // Return the dithering result
    return amount > threshold;
}


float4 MainPS(VertexShaderOutput input) : SV_TARGET
{

    float2 screenCoords = input.MyPosition.xyz / input.MyPosition.w;
    
    screenCoords = (screenCoords + 1.0f) / 2.0f;

    screenCoords.y = 1.0f - screenCoords.y;

    if(Dither(screenCoords, 0.5, float2(ScreenWidth,ScreenHeight)))
        discard;

    const float b = 0.01f;

    return float4(b, b, b, 1);
}

technique NormalColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
