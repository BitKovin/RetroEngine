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
   
    float2 screenCoords = input.MyPixelPosition.xyz / input.MyPixelPosition.w;
    
    screenCoords = (screenCoords + 1.0f) / 2.0f;

    screenCoords.y = 1.0f - screenCoords.y;
    
    float depthIn = SampleMaxDepth(screenCoords);
    
    DepthDiscard(depthIn, input);
    
    
    PixelOutput output = (PixelOutput) 0;
    
    float Depth = input.MyPixelPosition.z;
   
    float3 reflection = normalize(input.MyPosition - viewPos);

    
    float3 textureColor = SampleCubemap(TextureSampler, reflection).xyz;
    
    
    output.Color = float4(textureColor, 1);
    
    output.Position = float4(input.MyPosition - viewPos, 1);
    output.Reflectiveness = float4(0,0,0,1);
    
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