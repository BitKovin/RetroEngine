#define NO_SPECULAR
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


half3 CalculateDirectionalVertexLight(half3 tangentNormal)
{

    float shadow = 0;

    float shadowed = shadow;

	shadow = lerp(shadow, 1, 1 - max(0, dot(tangentNormal, normalize(-LightDirection))));


	//shadow = saturate(shadow);

	float3 lightDir = normalize(-LightDirection);

	// Calculate specular reflection


	//float3 globalSpecularDir = normalize(-normal + float3(0, -5, 0) + LightDirection);
	//specular += CalculateSpecular(input.MyPosition, normal, globalSpecularDir, roughness, metallic, albedo) * 0.02;

	// Direct light contribution
	float3 light = DirectBrightness * GlobalLightColor;
	light *= (1.0f - shadow);


	float3 globalLightColor = lerp(GlobalLightColor, SkyColor, shadowed);

	// Global ambient light
	float3 globalLight = GlobalBrightness * globalLightColor * lerp(1.0f, 0.1f, (dot(tangentNormal, float3(0, -1, 0)) + 1) / 2);

	light = max(light, 0.0f);
	light += globalLight;

    return light;

}

half3 CalculateSimplePointLight(int i, PixelInput pixelInput, half3 normal)
{
	float3 lightVector = LightPositions[i].xyz - pixelInput.MyPosition;
    float3 lightDir = normalize(lightVector);
	float distanceToLight = length(lightVector);

	if (distanceToLight > LightRadiuses[i])
		return half3(0, 0, 0);


	if (isParticle)
		normal = lightDir;

	// Calculate the dot product between the normalized light vector and light direction
	half lightDot = dot(-lightDir, normalize(LightDirections[i].xyz));

	// Define the inner and outer angles of the spotlight in radians
	half innerConeAngle = LightPositions[i].w;
	half outerConeAngle = LightDirections[i].w; // Adjust this value to control the smoothness

	// Calculate the smooth transition factor using smoothstep
	half dirFactor = smoothstep(outerConeAngle, innerConeAngle, lightDot);


	if (dirFactor <= 0.001)
		return 0;


	if (dot(normal, lightDir) < 0)
	{
		return float3(0, 0, 0);
	}


	float dist = max((LightRadiuses[i] - distanceToLight)/LightRadiuses[i],0);
	half intense = dist; //(1.0 - dist * dist);
	half distIntence = intense;

	intense *= dot(normal, lightDir);
	half3 specular = 0;

	intense = max(intense, 0);

    intense *= 1;
	half3 l = LightColors[i] * intense;

	return l * dirFactor;
}

half3 CalculateVertexLight(PixelInput input)
{


    half3 tangentNormal = normalize(GetTangentNormal(input.Normal, input.Tangent, input.BiTangent));

    half3 light = CalculateDirectionalVertexLight(tangentNormal);

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

PixelInput VertexShaderFunction(VertexInput input)
{

    PixelInput output = DefaultVertexShaderFunction(input);

    float3 light = CalculateVertexLight(output);

    output.Light = float4(light,1);

    return output;
}


PixelOutput PixelShaderFunction(PixelInput input)
{
    PixelOutput output = (PixelOutput)0;
    
    float Depth = input.MyPixelPosition.z;
    
    float4 ColorRGBA = tex2D(TextureSampler, input.TexCoord) * input.Color;
    float4 EmRGBT = tex2D(EmissiveTextureSampler, input.TexCoord);
    
    if (ColorRGBA.a < 0.001)
        discard;

    
    float3 textureColor = ColorRGBA.xyz;
	float textureAlpha = ColorRGBA.w;
    
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

#if OPENGL

#else
    if(LargeObject)
#endif
    for (int i = 0; i < min(MAX_POINT_LIGHTS, PointLightsNumber); i++)
	{
		light += CalculateSimplePointLight(i, input, input.Normal);
	}

    textureColor *= light;

    textureColor += EmRGBT * EmissionPower * EmRGBT.a;
    
    textureAlpha *= Transparency;
    
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