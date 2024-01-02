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
    
    output.Depth = float4(Depth, 0, 0, 1);

    float3 textureNormal = tex2D(NormalTextureSampler, input.TexCoord).xyz;
    
    float roughness = tex2D(ORMTextureSampler, input.TexCoord).g;
    float metalic = tex2D(ORMTextureSampler, input.TexCoord).b;
    
    
    float3 textureColor = tex2D(TextureSampler, input.TexCoord).xyz;
	float textureAlpha = tex2D(TextureSampler, input.TexCoord).w;

    float3 light = CalculateLight(input);

    float3 pixelNormal = ApplyNormalTexture(textureNormal, input.Normal);
    
    PBRData pbrData = CalculatePBR(textureColor, pixelNormal, roughness, metalic, input.MyPosition);

	textureColor *= light;

    textureColor += pbrData.specular;
    
    textureColor += tex2D(EmissiveTextureSampler, input.TexCoord).rgb * EmissionPower * tex2D(EmissiveTextureSampler, input.TexCoord).a;


    textureAlpha *= Transparency;
    
    output.Color = float4(pixelNormal, textureAlpha);
    
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