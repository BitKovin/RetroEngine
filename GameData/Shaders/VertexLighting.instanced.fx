﻿#define NO_SPECULAR
#define SIMPLE_SHADOWS


texture Texture;
sampler TextureSampler = sampler_state
{
    texture = <Texture>;

    MinFilter = Anisotropic;
    MagFilter = Anisotropic;

    MipLODBias = -0.5;

    AddressU = Wrap;
    AddressV = Wrap;
};
texture EmissiveTexture;
sampler EmissiveTextureSampler = sampler_state
{
    texture = <EmissiveTexture>;

    MinFilter = Anisotropic;
    MagFilter = Anisotropic;

    

    AddressU = Wrap;
    AddressV = Wrap;
};

#include "ShaderLib/BasicShader.fx"

bool earlyZ;


half3 CalculateDirectionalVertexLight(half3 tangentNormal, PixelInput input)
{

    float4 lightPos = mul(float4(input.MyPosition,1), ShadowMapViewProjection);
	float4 lightClosePos = mul(float4(input.MyPosition,1), ShadowMapViewProjectionClose);
	float4 lightVeryClosePos = mul(float4(input.MyPosition,1), ShadowMapViewProjectionVeryClose);

	float3 lightCoords = lightPos.xyz / lightPos.w;
	lightCoords = (lightCoords + 1.0f) / 2.0f;
	lightCoords.y = 1.0f - lightCoords.y;

	float3 lightCoordsClose = lightClosePos.xyz / lightClosePos.w;
	lightCoordsClose = (lightCoordsClose + 1.0f) / 2.0f;
	lightCoordsClose.y = 1.0f - lightCoordsClose.y;

	float3 lightCoordsVeryClose = 0;

	float shadow = 0;



	if (isParticle)
		tangentNormal = -LightDirection;


	if (dot(tangentNormal, LightDirection) >= 0)
	{
		shadow += 1;
	}
	else
	{
		shadow += GetShadow(lightCoords, lightCoordsClose, lightCoordsVeryClose, input, tangentNormal);
    }
    //float shadowed = shadow;


	shadow = lerp(shadow, 1, max(0, 1 - dot(normalize(tangentNormal), normalize(-LightDirection))));



	//shadow = saturate(shadow);

	float3 lightDir = normalize(-LightDirection);

	// Calculate specular reflection


	//float3 globalSpecularDir = normalize(-normal + float3(0, -5, 0) + LightDirection);
	//specular += CalculateSpecular(input.MyPosition, normal, globalSpecularDir, roughness, metallic, albedo) * 0.02;

	// Direct light contribution
	float3 light = DirectBrightness * GlobalLightColor;
	light *= (1.0f - shadow);


	float3 globalLightColor = lerp(GlobalLightColor, SkyColor, 0);

	// Global ambient light
	float3 globalLight = GlobalBrightness * globalLightColor * lerp(1.0f, 0.2f, (dot(tangentNormal, float3(0, -1, 0)) + 1) / 2);

	light = max(light, 0.0f);
	light += globalLight;

    return light;

}

half3 CalculateSimplePointLight(int i, PixelInput pixelInput, half3 normal)
{
	float3 lightVector = LightPositions[i].xyz - pixelInput.MyPosition;
	float distanceToLight = length(lightVector);

	if (distanceToLight > LightRadiuses[i])
		return half3(0, 0, 0);


	if (isParticle)
		normal = normalize(lightVector);

	// Calculate the dot product between the normalized light vector and light direction
	half lightDot = dot(normalize(-lightVector), normalize(LightDirections[i].xyz));

	// Define the inner and outer angles of the spotlight in radians
	half innerConeAngle = LightPositions[i].w;
	half outerConeAngle = LightDirections[i].w; // Adjust this value to control the smoothness

	// Calculate the smooth transition factor using smoothstep
	half dirFactor = smoothstep(outerConeAngle, innerConeAngle, lightDot);


	if (dirFactor <= 0.001)
		return 0;


	if (dot(normal, normalize(lightVector)) < 0)
	{
		return float3(0, 0, 0);
	}


	float dist = saturate((LightRadiuses[i] - distanceToLight)/LightRadiuses[i]);
	half intense = dist; //(1.0 - dist * dist);
	half distIntence = intense;
	half3 dirToSurface = normalize(lightVector);

	intense *= saturate(dot(normal, dirToSurface));
	half3 specular = 0;


	half colorInstens = abs(max(LightColors[i].x, (max(LightColors[i].y, LightColors[i].z))));

	intense = max(intense, 0);


    intense *= colorInstens;
	half3 l = LightColors[i] * intense;

	return (l + distIntence * specular) * dirFactor;
}

half3 CalculateVertexLight(PixelInput input)
{


    half3 tangentNormal = GetTangentNormal(input.Normal, input.Tangent, input.BiTangent);

    half3 light = 0;

#if OPENGL
#else
if(LargeObject == false)
    for (int i = 0; i < min(MAX_POINT_LIGHTS, PointLightsNumber); i++)
	{
		light += CalculateSimplePointLight(i, input, tangentNormal);
	}
#endif

    return light;

}

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



    //output.lightPos = mul(worldPos, ShadowMapViewProjection);
    //output.lightPosClose = mul(worldPos, ShadowMapViewProjectionClose);
    //output.lightPosVeryClose = mul(worldPos, ShadowMapViewProjectionVeryClose);

    output.TexCoord = input.TexCoord;
    output.Color = InstanceColor;
    output.Light = float4(0, 0, 0, 0);

    float3 light = CalculateVertexLight(output);

    output.Light = float4(light,1);

    return output;
}

PixelOutput PixelShaderFunction(PixelInput input)
{
    
    float2 screenCoords = input.MyPixelPosition.xyz / input.MyPixelPosition.w;
    
    screenCoords = (screenCoords + 1.0f) / 2.0f;

    screenCoords.y = 1.0f - screenCoords.y;
    

#if OPENGL
#else
    if(Decal)
    {

        float3 viewDir = normalize(input.MyPosition - viewPos);

        float sampleDepth = SampleDepth(screenCoords);
    
        float bias = 0.5;

        float dist = distance(input.MyPosition, viewPos);

        if(sampleDepth > input.MyPixelPosition.z + bias * dist/7)
        {
            discard;
        }

    }
#endif

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
    
    
    float3 textureColor = ColorRGBTA.xyz * input.Color.rgb;
	float textureAlpha = ColorRGBTA.w * input.Color.a;
    
    if (textureAlpha < 0.01)
        discard;

    float3 pixelNormal = input.Normal;//ApplyNormalTexture(textureNormal, input.Normal, input.Tangent, input.BiTangent);
    
    
    float3 albedo = textureColor;
    
    
    float3 TangentNormal = GetTangentNormal(input.Normal, input.Tangent, input.Tangent);

    //float3 light = CalculateLight(input, pixelNormal, roughness, metalic, ao, albedo, TangentNormal);
    
    
	//textureColor *= light;
    
    //textureColor = ApplyReflection(textureColor, albedo, input, pixelNormal, roughness, metalic);
    
    //light -= 1.1;
    //light = saturate(light/30);
    //textureColor += light;
    
    half3 light = input.Light;

    light += CalculateDirectionalVertexLight(TangentNormal, input);

#if OPENGL

#else
    if(LargeObject)
#endif
    for (int i = 0; i < MAX_POINT_LIGHTS; i++)
	{
		light += CalculateSimplePointLight(i, input, input.Normal);
	}

    textureColor *= light;

    textureColor += tex2D(EmissiveTextureSampler, input.TexCoord).rgb * EmissionPower * tex2D(EmissiveTextureSampler, input.TexCoord).a;
    
    textureAlpha *= Transparency;
    

    //textureColor = lerp(textureColor, oldFrame, 0.5);
    
    float3 vDir = normalize(input.MyPosition - viewPos);
    
    float pbs = 1;
    
    if (textureAlpha<0.95)
        pbs = 0;
    
    float3 reflection = reflect(vDir, pixelNormal);
    
    
    output.Normal = float4((normalize(lerp(pixelNormal, TangentNormal, 0.0)) + 1) / 2, pbs);
    output.Position = float4(input.MyPosition - viewPos, pbs);
    
    
    
    //float reflectiveness = CalculateReflectiveness(roughness, metalic, vDir / 3, pixelNormal);
    
    //reflectiveness = saturate(reflectiveness);
    
    output.Reflectiveness = float4(0, 0, 0, pbs);
    
    //textureColor = ApplyReflectionOnSurface(textureColor,albedo, screenCoords, 0);
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