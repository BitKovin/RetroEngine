#include "ShaderLib/BasicShader.fx"

texture Texture;
samplerCUBE TextureSampler = sampler_state
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
   
    float3 reflection = reflect(normalize(input.MyPosition - viewPos),input.TangentNormal);

    
    float3 textureColor = SampleCubemap(TextureSampler, reflection).xyz;
    
    output.Depth = float4(Depth, 0, 0, 1);
    
    output.Color = float4(textureColor, 1);
    
    
    
    output.Normal = float4((input.TangentNormal + 1) / 2, 1);
    
    
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