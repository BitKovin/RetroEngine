#define NO_SPECULAR

#include "../../../RetroEngine/Content/Shaders/ShaderLib/BasicShader.fx"

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

PixelInput VertexShaderFunction(VertexInput input)
{
    return DefaultVertexShaderFunction(input);
}

PixelOutput PixelShaderFunction(PixelInput input)
{
    
    PixelOutput output = (PixelOutput) 0;
    
    float Depth = input.MyPixelPosition.z;
    
    output.Position = float4(input.MyPosition - viewPos, 1);

    float3 textureNormal = tex2D(NormalTextureSampler, input.TexCoord).xyz;
    
    
    float3 textureColor = tex2D(TextureSampler, input.TexCoord).xyz;
    float textureAlpha = tex2D(TextureSampler, input.TexCoord).w;

    
    float3 pixelNormal = ApplyNormalTexture(textureNormal, input.Normal, input.Tangent);
    
    

    float3 light = CalculateLight(input, pixelNormal, 1, 0,1);
    
    textureColor *= light;
    
    light -= 1.1;
    light = saturate(light / 8);
    textureColor += light;
    
    textureColor += tex2D(EmissiveTextureSampler, input.TexCoord).rgb * EmissionPower * tex2D(EmissiveTextureSampler, input.TexCoord).a;
    
    textureAlpha *= Transparency;
    
    output.Color = float4(textureColor, textureAlpha);
    output.Reflectiveness = float4(0, 0, 0, 1);
    output.Normal = float4((pixelNormal + 1) / 2, 1);
    
    
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