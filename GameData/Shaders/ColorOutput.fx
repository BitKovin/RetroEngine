#include "ShaderLib/BasicShader.fx"

Texture2D Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;
};
Texture2D EmissiveTexture;
sampler EmissiveTextureSampler = sampler_state
{
    texture = <EmissiveTexture>;
};

Texture2D NormalTexture;
sampler NormalTextureSampler = sampler_state
{
    texture = <NormalTexture>;
};

Texture2D ORMTexture;
sampler ORMTextureSampler = sampler_state
{
    texture = <ORMTexture>;
};

PixelInput VertexShaderFunction(VertexInput input)
{
    return DefaultVertexShaderFunction(input);
}

PixelOutput PixelShaderFunction(PixelInput input)
{
    
    PixelOutput output = (PixelOutput)0;
    
    float Depth = input.MyPixelPosition.z;
    
    float3 textureNormal = SAMPLE_TEXTURE(NormalTexture,NormalTextureSampler, input.TexCoord).rgb;
    
    
    float4 ColorRGBTA = SAMPLE_TEXTURE(Texture, TextureSampler, input.TexCoord) * input.Color;
    float3 textureColor = ColorRGBTA.xyz;
	float textureAlpha = ColorRGBTA.w;

    
    float3 pixelNormal = ApplyNormalTexture(textureNormal, input.Normal, input.Tangent, input.BiTangent);
    
    
    textureAlpha *= Transparency;
    
    output.Color = float4(textureColor, textureAlpha);
    
    output.Normal = float4((pixelNormal + 1) / 2,1);
    
    return output;
}


technique BasicTechnique
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();

    }
}