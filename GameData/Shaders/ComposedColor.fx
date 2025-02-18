#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

Texture2D ColorTexture;

sampler2D ColorTextureSampler = sampler_state
{
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;

	Texture = <ColorTexture>;
};

Texture2D SSAOTexture;

sampler2D SSAOTextureSampler = sampler_state
{
    Texture = <SSAOTexture>;

    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float2 ssaoResolution;

Texture2D BloomTexture;

sampler2D BloomTextureSampler = sampler_state
{
    Texture = <BloomTexture>;

    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;

};

Texture2D Bloom2Texture;

sampler2D Bloom2TextureSampler = sampler_state
{
    Texture = <Bloom2Texture>;

    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D Bloom3Texture;

sampler2D Bloom3TextureSampler = sampler_state
{
    Texture = <Bloom3Texture>;

    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;

};

Texture2D Bloom4Texture;

sampler2D Bloom4TextureSampler = sampler_state
{
    Texture = <Bloom3Texture>;

    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;

};

Texture2D LutTexture;

sampler2D LutTextureSampler = sampler_state
{
    Texture = <LutTexture>;

    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;

};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

int lutSize;

// Function to apply the LUT
float3 GetFromLUT(float3 color)
{

    color = saturate(color);

	float COLORS = lutSize;
	float2  _LUT_TexelSize = float2(lutSize* lutSize, lutSize);
	float maxColor = COLORS - 1.0;
	float3 col = color;
	float halfColX = 0.5 / _LUT_TexelSize.x;
	float halfColY = 0.5 / _LUT_TexelSize.y;
	float threshold = maxColor / COLORS;

	float xOffset = halfColX + col.r * threshold / COLORS;
	float yOffset = halfColY + col.g * threshold;
	float cell = floor(col.b * maxColor);

	float2 lutPos = float2(cell / COLORS + xOffset, yOffset);
	float3 gradedCol = tex2D(LutTextureSampler, lutPos).rgb;

	return gradedCol;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	
    float3 color = tex2D(ColorTextureSampler, input.TextureCoordinates).rgb;
	
    float3 bloomColor = tex2D(BloomTextureSampler, input.TextureCoordinates).rgb / 2 + tex2D(Bloom2TextureSampler, input.TextureCoordinates).rgb / 2 + tex2D(Bloom3TextureSampler, input.TextureCoordinates).rgb / 2 + tex2D(Bloom4TextureSampler, input.TextureCoordinates).rgb / 3;
	
    float bloomL = length(bloomColor);
	
    bloomColor = lerp(bloomColor, float3(bloomL, bloomL, bloomL), bloomL * 0.5);
	
    bloomColor*=3;
    
    bloomL = length(bloomColor);
	
    //bloomColor *= lerp(bloomL + 1, 1, 0.1);
	
    float ssao = tex2D(SSAOTextureSampler, input.TextureCoordinates).r;
	
    float2 ssaoTexel = 1/ssaoResolution;

    int numSamples = 1;

    int n = 1;

    for (int i = -numSamples; i <= numSamples; ++i)
    {
        for (int j = -numSamples; j <= numSamples; ++j)
        {

            float2 offset = float2(i,j) * ssaoTexel;

            ssao += tex2D(SSAOTextureSampler, input.TextureCoordinates + offset).r;
            n++;
        }
    }

    ssao/=n;

    ssao = 1 - ssao;
    ssao*= 1.5;
    ssao = 1 - ssao;

    float3 result = (color + bloomColor)*ssao;

    float3 lutResult = result;

    if(lutSize>1)
        lutResult = GetFromLUT(result);

    return float4(lutResult, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};