﻿#define NO_SPECULAR
//#define SIMPLE_SHADOWS


texture Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;

    MinFilter = Point;
    MagFilter = Point;

    MipLODBias = -0.5;

    AddressU = Wrap;
    AddressV = Wrap;
};
texture EmissiveTexture;
sampler EmissiveTextureSampler = sampler_state
{
    texture = <EmissiveTexture>;

    MinFilter = Point;
    MagFilter = Point;

    

    AddressU = Wrap;
    AddressV = Wrap;
};

#include "ShaderLib/BasicShader.fx"

bool earlyZ;


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

    if(earlyZ)
    {
        DepthDiscard(depthIn,input);
    }
    
    PixelOutput output = (PixelOutput)0;
    
    float Depth = input.MyPixelPosition.z;
    
    float4 ColorRGBTA = tex2D(TextureSampler, input.TexCoord) * input.Color;
    
    if (ColorRGBTA.a < 0.001)
        discard;

    //float3 textureNormal = tex2D(NormalTextureSampler, input.TexCoord).rgb;
    
    float3 orm = float3(1,1,0);
    
    float roughness =orm.g;
    float metalic = orm.b;
    float ao = orm.r;
    
    
    float3 textureColor = ColorRGBTA.xyz;
	float textureAlpha = tex2D(TextureSampler, input.TexCoord).w;
    
    if (textureAlpha < 0.01)
        discard;
    
    
    output.Normal = float4((normalize(input.Normal) + 1) / 2, 1);
    output.Position = float4(input.MyPosition - viewPos, 1);
    
    
    
    //float reflectiveness = CalculateReflectiveness(roughness, metalic, vDir / 3, pixelNormal);
    
    //reflectiveness = saturate(reflectiveness);
    
    output.Reflectiveness = float4(0, 0, 0, 1);
    
    //textureColor = ApplyReflectionOnSurface(textureColor,albedo, screenCoords, 0);
    output.Color = float4(float3(1,1,1), 0.1f);

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