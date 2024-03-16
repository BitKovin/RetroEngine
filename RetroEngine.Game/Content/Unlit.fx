#include "../../RetroEngine/Content/ShaderLib/BasicShader.fx"

texture Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;
};

PixelInput VertexShaderFunction(VertexInput input)
{
    return DefaultVertexShaderFunction(input);
}

PixelOutput PixelShaderFunction(PixelInput input)
{
    
    PixelOutput output = (PixelOutput) 0;
    
    float Depth = input.MyPixelPosition.z;
    
    output.Position = float4(input.MyPosition, 1);
    
    
    float3 textureColor = tex2D(TextureSampler, input.TexCoord).xyz;
    float textureAlpha = tex2D(TextureSampler, input.TexCoord).w;
    
    
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