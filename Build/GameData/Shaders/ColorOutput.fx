#include "ShaderLib/BasicShader.fx"

texture Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;
};
texture EmissiveTexture;
sampler EmissiveTextureSampler = sampler_state
{
    texture = <EmissiveTexture>;
};

texture NormalTexture;
sampler NormalTextureSampler = sampler_state
{
    texture = <NormalTexture>;
};

texture ORMTexture;
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
    
    float3 textureNormal = tex2D(NormalTextureSampler, input.TexCoord).xyz;
    
    
    
    float3 textureColor = tex2D(TextureSampler, input.TexCoord).xyz;
	float textureAlpha = tex2D(TextureSampler, input.TexCoord).w;

    
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