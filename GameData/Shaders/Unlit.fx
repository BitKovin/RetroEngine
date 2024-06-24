#include "ShaderLib/BasicShader.fx"

Texture2D Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;

    AddressU = Wrap;
    AddressV = Wrap;

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
    
    float4 ColorRGBTA = SAMPLE_TEXTURE(Texture, TextureSampler, input.TexCoord) * input.Color;
    
    float3 textureColor = ColorRGBTA.xyz;
    float textureAlpha = ColorRGBTA.w;
    
    
    textureAlpha *= Transparency;
    
    output.Color = float4(textureColor, textureAlpha);
    
    output.Normal = float4((input.Normal + 1) / 2, 1);
    output.Reflectiveness = float4(0, 0, 0, 1);
    
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