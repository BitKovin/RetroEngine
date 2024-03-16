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

texture OldFrameTexture;
sampler OldFrameTextureSampler = sampler_state
{
    texture = <OldFrameTexture>;
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
    
    DepthDiscard(depthIn,input);
    
    PixelOutput output = (PixelOutput)0;
    
    float Depth = input.MyPixelPosition.z;
    
    
    

    float3 textureNormal = tex2D(NormalTextureSampler, input.TexCoord).rgb;
    
    float3 orm = tex2D(ORMTextureSampler, input.TexCoord).rgb;
    
    float roughness =orm.g;
    float metalic = orm.b;
    float ao = orm.r;
    
    
    float3 textureColor = tex2D(TextureSampler, input.TexCoord).xyz;
	float textureAlpha = tex2D(TextureSampler, input.TexCoord).w;
    
    float3 pixelNormal = ApplyNormalTexture(textureNormal, input.Normal, input.Tangent);
    
    
    float3 albedo = textureColor;
    
    float3 light = CalculateLight(input, pixelNormal, roughness, metalic, ao);
    
    
	textureColor *= light;
    
    //textureColor = ApplyReflection(textureColor, albedo, input, pixelNormal, roughness, metalic);
    
    light -= 1.1;
    light = saturate(light/8);
    textureColor += light;
    
    textureColor += tex2D(EmissiveTextureSampler, input.TexCoord).rgb * EmissionPower * tex2D(EmissiveTextureSampler, input.TexCoord).a;
    
    textureAlpha *= Transparency;
    

    //textureColor = lerp(textureColor, oldFrame, 0.5);
    
    float pbs = 1;
    
    if (textureAlpha<0.95)
        pbs = 0;
    
    
    output.Color = float4(textureColor, textureAlpha);
    
    output.Normal = float4((pixelNormal + 1) / 2, pbs);
    output.Position = float4(input.MyPosition, pbs);
    
    float3 vDir = normalize(input.MyPosition - viewPos);
    float reflectiveness = CalculateReflectiveness(roughness, metalic, vDir / 3, pixelNormal);
    
    reflectiveness = saturate(reflectiveness);
    
    output.Reflectiveness = float4(reflectiveness, reflectiveness, reflectiveness, pbs);
    
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