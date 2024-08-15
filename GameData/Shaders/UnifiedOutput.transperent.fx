//#define SIMPLE_SHADOWS

#include "ShaderLib/BasicShader.fx"

texture Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;
    AddressU = Wrap;
    AddressV = Wrap;
};
texture EmissiveTexture;
sampler EmissiveTextureSampler = sampler_state
{
    texture = <EmissiveTexture>;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture NormalTexture;
sampler NormalTextureSampler = sampler_state
{
    texture = <NormalTexture>;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture ORMTexture;
sampler ORMTextureSampler = sampler_state
{
    texture = <ORMTexture>;
    AddressU = Wrap;
    AddressV = Wrap;
};


bool earlyZ;

PixelInput VertexShaderFunction(VertexInput input)
{
    //return (PixelInput)0;
    return DefaultVertexShaderFunction(input);
}

PixelOutput PixelShaderFunction(PixelInput input)
{
    
    half2 screenCoords = (input.MyPixelPosition.xyz / input.MyPixelPosition.w).xy;
    
    screenCoords = (screenCoords + 1.0f) / 2.0f;

    screenCoords.y = 1.0f - screenCoords.y;
    
    float depthIn = SampleDepth(screenCoords);

    if(earlyZ)
    {
    
        DepthDiscard(depthIn,input);
    }

    PixelOutput output = (PixelOutput)0;
    
    
    half4 ColorRGBTA = tex2D(TextureSampler, input.TexCoord) * input.Color;
    
    if(Masked && DitherDisolve>0)
        ColorRGBTA.a *= Dither(screenCoords, 1.0 - DitherDisolve, float2(ScreenWidth, ScreenHeight));
    
    MaskedDiscard(ColorRGBTA.a);

    if (ColorRGBTA.a < 0.001)
        discard;

    half3 textureNormal = tex2D(NormalTextureSampler, input.TexCoord).rgb;
    
    half3 orm = tex2D(ORMTextureSampler, input.TexCoord).rgb;
    
    half roughness = orm.g;
    half metalic = orm.b;
    half ao = orm.r;
    
    
    half3 textureColor = ColorRGBTA.xyz;
	half textureAlpha = ColorRGBTA.a;

    half3 pixelNormal = ApplyNormalTexture(textureNormal, input.Normal, input.Tangent, input.BiTangent);
    
    
    half3 albedo = textureColor;
    
    
    half3 TangentNormal = GetTangentNormal(input.Normal, input.Tangent, input.Tangent);

    half3 light = CalculateLight(input, pixelNormal, roughness, metalic, ao, albedo, TangentNormal);
    
    
	textureColor *= light;
    
    
    //textureColor = ApplyReflection(textureColor, albedo, input, pixelNormal, roughness, metalic);
    
    light -= 1.1;
    light = saturate(light/30);
    textureColor += light;
    
    textureColor += tex2D(EmissiveTextureSampler, input.TexCoord).rgb * EmissionPower * 2;
    
    textureAlpha *= Transparency;
    

    //textureColor = lerp(textureColor, oldFrame, 0.5);
    
    float3 vDir = normalize(input.MyPosition - viewPos);
    
    float pbs = 1;
    
    if (textureAlpha * input.Color.a<0.95)
        pbs = 0;
    
    float3 reflection = reflect(vDir, pixelNormal);
    
    
    
    output.Normal = float4((pixelNormal + 1) / 2, pbs);


    output.Position = float4((input.MyPosition - viewPos), pbs);

    
    float reflectiveness = CalculateReflectiveness(roughness, metalic, vDir / 3, pixelNormal);
    
    reflectiveness = saturate(reflectiveness);
    
    output.Reflectiveness = float4(reflectiveness, roughness, 0, pbs);

    if(pbs>0.5)
        textureColor = ApplyReflectionCubemapOnSurface(textureColor,albedo, reflectiveness, metalic, roughness, screenCoords, reflection);
    output.Color = float4(textureColor, textureAlpha);

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