﻿#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix View;
matrix Projection;
texture Texture;
sampler TextureSampler = sampler_state { texture = <Texture>; };
texture EmissiveTexture;
sampler EmissiveTextureSampler = sampler_state { texture = <EmissiveTexture>; };

texture ShadowMap;
sampler ShadowMapSampler = sampler_state { texture = <ShadowMap>; };

float DirectBrightness;
float GlobalBrightness;
float3 LightDirection;

float EmissionPower;

float ShadowBias;

bool test;

matrix ShadowMapViewProjection;

struct VertexInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0; // Add normal input
    float2 TexCoord : TEXCOORD0;
	
};

struct PixelInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
	float3 Normal : TEXCOORD1; // Pass normal to pixel shader
    float light : TEXCOORD2;
    float4 lightPos :TEXCOORD3;
};

float3 normalize(float3 v)
{
  return rsqrt(dot(v,v))*v;
}

PixelInput VertexShaderFunction(VertexInput input)
{
    PixelInput output;

    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.TexCoord = input.TexCoord;

	// Pass the world space normal to the pixel shader
    output.Normal = mul(input.Normal, (float3x3)World);
    output.Normal = normalize(output.Normal);

    float lightingFactor = max(-0.0, dot(output.Normal, normalize(-LightDirection))) * DirectBrightness; // Example light direction

    output.light = lightingFactor;

    output.lightPos = mul(float4(mul(input.Position, World)),ShadowMapViewProjection);

    return output;
}

float GetShadow(float3 lightCoords)
{

    float shadow = 0;

    float3 centerCoords = lightCoords;

    


            if(lightCoords.x>=0 && lightCoords.x <=1 && lightCoords.y>=0 && lightCoords.y <=1)
            {

                float closestDepth = tex2D(ShadowMapSampler,lightCoords.xy).r;
                float currentDepth = lightCoords.z * 2 - 1;

                if(currentDepth > closestDepth + ShadowBias)
                    shadow +=1;
            }
        
    

    return shadow;
}

float4 PixelShaderFunction(PixelInput input) : COLOR0
{

	float3 textureColor = tex2D(TextureSampler, input.TexCoord).xyz;
	float textureAlpha = tex2D(TextureSampler, input.TexCoord).w;

    float3 lightCoords = input.lightPos.xyz / input.lightPos.w;

    float shadow = 0;

    lightCoords = (lightCoords + 1.0f) / 2.0f;

    lightCoords.y = 1.0f - lightCoords.y;

    shadow += GetShadow(lightCoords);
    
    float light = input.light;

    light *= 1.0f - shadow;
    light += GlobalBrightness;

	textureColor *= light;

    textureColor /= 4.0f;

    textureColor += tex2D(EmissiveTextureSampler, input.TexCoord).xyz / 4.0f * EmissionPower;

    return float4(textureColor, textureAlpha);
}



technique BasicTechnique
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}