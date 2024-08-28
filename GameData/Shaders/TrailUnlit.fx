#include "ShaderLib/BasicShader.fx"

texture Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;

    AddressU = Clamp;
    AddressV = Clamp;

};

PixelInput VertexShaderFunction(VertexInput input)
{
    return DefaultVertexShaderFunction(input);
}

PixelOutput PixelShaderFunction(PixelInput input)
{
    
    PixelOutput output = (PixelOutput) 0;
    
    float Depth = input.MyPixelPosition.z;
    

    
    
    float3 textureColor = tex2D(TextureSampler, input.TexCoord).xyz;
    float textureAlpha = tex2D(TextureSampler, input.TexCoord).w;
    
    
    textureAlpha *= Transparency;
    
    float pbs = 1;

    if (textureAlpha * input.Color.a<0.95)
        pbs = 0;

    output.Color = float4(textureColor, textureAlpha) * input.Color;
    
    output.Position = float4(input.MyPosition - viewPos, pbs);

    output.Normal = float4((input.Normal + 1) / 2, pbs);
    output.Reflectiveness = float4(0, 0, 0, pbs);
    
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