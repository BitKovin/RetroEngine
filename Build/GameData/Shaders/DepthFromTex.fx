#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

texture Texture;
sampler DepthTextureSampler = sampler_state
{
    texture = <Texture>;

    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;

};

texture OldFrame;
sampler OldFrameSampler = sampler_state
{
    texture = <OldFrame>;

    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;

};


struct VertexInput
{
    float4 PositionPS : POSITION;
    float4 Diffuse    : COLOR0;
    float2 TexCoord   : TEXCOORD0;
};

// Vertex Shader Output Structure
struct VertexOutput
{
    float4 PositionPS : SV_Position0;
    float4 Diffuse    : COLOR0;
    float2 TexCoord   : TEXCOORD0;
};

// Vertex Shader
VertexOutput SimpleVertexShader(VertexInput input)
{
    VertexOutput output;

    // Pass the position directly to the pixel shader
    output.PositionPS = input.PositionPS;

    output.Diffuse = float4(1,1,1,1);

    // Pass the texture coordinates directly to the pixel shader
    output.TexCoord = input.TexCoord;

    return output;
}

struct PixelOut
{
    float4 color : COLOR0;
    float depth : SV_Depth;
};

// Pixel shader
PixelOut PixelShaderFunction(float4 position : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0)
{

    PixelOut output = (PixelOut)0;

    output.color = float4(tex2D(OldFrameSampler,texCoord).xyz,1);
    output.depth = tex2D(DepthTextureSampler, texCoord).x;

    return output;
}

// Technique
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
        VertexShader = compile VS_SHADERMODEL SimpleVertexShader();
    }
}