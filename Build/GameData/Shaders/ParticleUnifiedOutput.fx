#define NO_SPECULAR
#define MAX_POINT_LIGHTS_SHADOWS 3

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

texture OldFrameTexture;
sampler OldFrameTextureSampler = sampler_state
{
    texture = <OldFrameTexture>;
};




PixelInput VertexShaderFunction(VertexInput input, float4 row1: BLENDINDICES1, float4 row2 : BLENDINDICES2, float4 row3 : BLENDINDICES3, float4 row4 : BLENDINDICES4, float4 InstanceColor : Color2)
{

    float4x4 world = float4x4(row1,row2,row3,row4);

    PixelInput output;

    //float4x4 boneTrans = GetBoneTransforms(input);

    float4x4 BonesWorld = world;

    float4 worldPos = input.Position;

    worldPos = mul(worldPos, BonesWorld);

    output.Position = worldPos;
    output.MyPosition = output.Position.xyz;
    output.Position = mul(output.Position, View);

    //output.Position.z *= 0.5;

    
    output.Position = mul(output.Position, Projection);
    



    output.MyPixelPosition = output.Position;


    output.TexCoord = input.TexCoord;

    // Pass the world space normal to the pixel shader
    output.Normal = mul(input.Normal, (float3x3)BonesWorld);
    output.Normal = normalize(output.Normal);


    output.Tangent = mul(input.Tangent, (float3x3)BonesWorld);
    output.Tangent = normalize(output.Tangent);

    output.BiTangent = mul(input.BiTangent, (float3x3)BonesWorld);
    output.BiTangent = normalize(output.BiTangent);



    output.lightPos = mul(worldPos, ShadowMapViewProjection);
    output.lightPosClose = mul(worldPos, ShadowMapViewProjectionClose);
    output.lightPosVeryClose = mul(worldPos, ShadowMapViewProjectionVeryClose);

    output.TexCoord = input.TexCoord;
    output.Color = InstanceColor;

    return output;
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
    
    float3 orm;// = tex2D(ORMTextureSampler, input.TexCoord).rgb;
    
    orm = float3(1, 1, 0);

    float roughness =orm.g;
    float metalic = orm.b;
    float ao = orm.r;
    
    float4 tex = tex2D(TextureSampler, input.TexCoord);
    
    float3 textureColor = tex.xyz * input.Color.rgb;
	float textureAlpha = tex.w * input.Color.a;
    
    float3 pixelNormal = ApplyNormalTexture(textureNormal, input.Normal, input.Tangent, input.BiTangent);
    
    
    float3 albedo = textureColor;
    
    float3 light = CalculateLight(input, pixelNormal, roughness, metalic, ao, albedo);
    
    
	textureColor *= light;
    
    //textureColor = ApplyReflection(textureColor, albedo, input, pixelNormal, roughness, metalic);
    
    light -= 1.1;
    light = saturate(light/8);
    textureColor += light;
    
    textureColor += tex2D(EmissiveTextureSampler, input.TexCoord).rgb * EmissionPower * tex2D(EmissiveTextureSampler, input.TexCoord).a;
    
    textureAlpha *= Transparency;
    

    //textureColor = lerp(textureColor, oldFrame, 0.5);
    
    float3 vDir = normalize(input.MyPosition - viewPos);
    
    float pbs = 1;
    
    if (textureAlpha<0.95)
        pbs = 0;
    
    float3 reflection = reflect(vDir, pixelNormal);
    
    output.Color = float4(textureColor, textureAlpha);
    

    output.Normal = float4((pixelNormal + 1) / 2, pbs);
    output.Position = float4(input.MyPosition - viewPos, pbs);
    
    
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