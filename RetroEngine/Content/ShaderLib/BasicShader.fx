#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

#define PI 3.1415f

matrix World;
matrix View;
matrix Projection;
matrix ProjectionViewmodel;

texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
    texture = <ShadowMap>;
};

texture ShadowMapClose;
sampler ShadowMapCloseSampler = sampler_state
{
    texture = <ShadowMapClose>;
};

texture DepthTexture;
sampler DepthTextureSampler = sampler_state
{
    texture = <DepthTexture>;
};

texture ReflectionCubemap;
sampler ReflectionCubemapSampler = sampler_state
{
    texture = <ReflectionCubemap>;
};


float FarPlane;
float3 viewDir;
float3 viewPos;

float DirectBrightness;
float GlobalBrightness;
float3 LightDirection;
float3 GlobalLightColor;

float EmissionPower;
float ShadowBias;
float Transparency;
matrix ShadowMapViewProjection;
float ShadowMapResolution;

bool Viewmodel = false;

matrix ShadowMapViewProjectionClose;
float ShadowMapResolutionClose;

#ifndef MAX_POINT_LIGHTS

#define MAX_POINT_LIGHTS 4

#endif

float3 LightPositions[MAX_POINT_LIGHTS];
float3 LightColors[MAX_POINT_LIGHTS];
float LightRadiuses[MAX_POINT_LIGHTS];

texture PointLightCubemap1;
sampler PointLightCubemap1Sampler = sampler_state
{
    texture = <PointLightCubemap1>;
};

texture PointLightCubemap2;
sampler PointLightCubemap2Sampler = sampler_state
{
    texture = <PointLightCubemap2>;
};

texture PointLightCubemap3;
sampler PointLightCubemap3Sampler = sampler_state
{
    texture = <PointLightCubemap3>;
};

texture PointLightCubemap4;
sampler PointLightCubemap4Sampler = sampler_state
{
    texture = <PointLightCubemap4>;
};


#define BONE_NUM 128

matrix Bones[BONE_NUM];

bool isParticle = false;

float depthScale = 1.0f;

float ScreenHeight;
float ScreenWidth;

struct VertexInput
{
    float4 Position : SV_POSITION0;
    float3 Normal : NORMAL0; // Add normal input
    float2 TexCoord : TEXCOORD0;
    float3 Tangent : TANGENT0;
    
    float4 BlendIndices : BLENDINDICES0;
    float4 BlendWeights : BLENDWEIGHT0;
};

struct PixelInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : TEXCOORD1; // Pass normal to pixel shader
    float3 light : TEXCOORD2;
    float4 lightPos : TEXCOORD3;
    float4 lightPosClose : TEXCOORD4;
    float3 MyPosition : TEXCOORD5;
    float4 MyPixelPosition : TEXCOORD6;
    float3 Tangent : TEXCOORD7;
    float3 TangentNormal : TEXCOORD8;
};

struct PBRData
{
    float3 specular;
    float3 lighting;
    float reflectiveness;
};

struct PixelOutput
{
    float4 Color : COLOR0;
    float4 Depth : COLOR1;
    float4 Normal : COLOR2;
};

float3 normalize(float3 v)
{
    return rsqrt(dot(v, v)) * v;
}

float4x4 GetBoneTransforms(VertexInput input)
{
    
    float4x4 identity = float4x4(
    1.0, 0.0, 0.0, 0.0,
    0.0, 1.0, 0.0, 0.0,
    0.0, 0.0, 1.0, 0.0,
    0.0, 0.0, 0.0, 1.0);
    
    float sum = input.BlendWeights.x + input.BlendWeights.y + input.BlendWeights.z + input.BlendWeights.w;
    
    if (sum < 0.05f)
        return identity;
    
    float4x4 mbones =
    Bones[input.BlendIndices.x] * (float) input.BlendWeights.x / sum +
    Bones[input.BlendIndices.y] * (float) input.BlendWeights.y / sum +
    Bones[input.BlendIndices.z] * (float) input.BlendWeights.z / sum +
    Bones[input.BlendIndices.w] * (float) input.BlendWeights.w / sum;
    
    return mbones;
}

float3 GetTangentNormal(float3 worldNormal, float3 worldTangent)
{
    
    float3 normalMapSample = float3(0, 0, 1);
    
    
    // Create the tangent space matrix as before
    float3 bitangent = cross(worldNormal, worldTangent);
    float3x3 tangentToWorld = float3x3(worldTangent, bitangent, worldNormal);

    // Transform the normal from tangent space to world space
    float3 worldNormalFromTexture = mul(normalMapSample, tangentToWorld);

    // Normalize the final normal
    worldNormalFromTexture = normalize(worldNormalFromTexture);

    return worldNormalFromTexture;
}

PixelInput DefaultVertexShaderFunction(VertexInput input)
{
    PixelInput output;

    float4x4 boneTrans = GetBoneTransforms(input);
    
    

    output.Position = mul(mul(input.Position, boneTrans), World);
    output.MyPosition = output.Position.xyz;
    output.Position = mul(output.Position, View);
    
    
    
    if (Viewmodel)
    {
        output.Position = mul(output.Position, ProjectionViewmodel);
    }
    else
    {
        output.Position = mul(output.Position, Projection);
    }
    
    
    if (Viewmodel)
        output.Position.z *= 0.02;
    
    output.MyPixelPosition = output.Position;
    
    
    
    output.TexCoord = input.TexCoord;

	// Pass the world space normal to the pixel shader
    output.Normal = mul(mul(input.Normal, (float3x3) boneTrans), (float3x3) World);
    output.Normal = normalize(output.Normal);
    
    if (dot(output.Normal, normalize(output.MyPosition - viewPos)) > 0)
        output.Normal *= -1;
    
    output.Tangent = mul(mul(input.Tangent, (float3x3) boneTrans), (float3x3) World);
    output.Tangent = normalize(output.Tangent);

    output.TangentNormal = GetTangentNormal(output.Normal, output.Tangent);
    
    output.light = 0;

    output.lightPos = mul(float4(mul(mul(input.Position, boneTrans), World)), ShadowMapViewProjection);
    output.lightPosClose = mul(float4(mul(input.Position, World)), ShadowMapViewProjectionClose);
    
    output.TexCoord = input.TexCoord;
    
    return output;
}

void DepthDiscard(float depth, PixelInput input)
{
    if (depth < input.MyPixelPosition.z - 0.1)
        discard;
}

float SampleDepth(float2 coords)
{
    return tex2D(DepthTextureSampler, coords);

}

float SampleMaxDepth(float2 screenCoords)
{
    
    float2 texelSize = 0.3 / float2(ScreenWidth, ScreenHeight);
    
    float d = SampleDepth(screenCoords);
    float d1 = SampleDepth(screenCoords + texelSize);
    float d2 = SampleDepth(screenCoords - texelSize);
    
    float d3 = SampleDepth(screenCoords + texelSize * float2(2,0));
    float d4 = SampleDepth(screenCoords - texelSize * float2(0, 2));

    return max(d, max(d1, max(d2, max(d3, d4))));

}



float4 SampleCubemap(samplerCUBE s, float3 coords)
{
    return texCUBE(s, coords * float3(-1,1,1));
}

float3 ApplyNormalTexture(float3 sampledNormalColor, float3 worldNormal, float3 worldTangent)
{
    
    if (length(sampledNormalColor) < 0.1f)
        sampledNormalColor = float3(0.5, 0.5, 1);
    
    
    sampledNormalColor *= float3(1, 1, 1);
    
    worldNormal = normalize(worldNormal);
    worldTangent = normalize(worldTangent);

    float3 normalMapSample = sampledNormalColor * 2.0 - 1.0;
    
    normalMapSample *= float3(-1, -1, 1);
    
    normalMapSample *= 1;
    
    // Create the tangent space matrix as before
    float3 bitangent = cross(worldNormal, worldTangent);
    float3x3 tangentToWorld = float3x3(worldTangent, bitangent, worldNormal);

    // Transform the normal from tangent space to world space
    float3 worldNormalFromTexture = mul(normalMapSample, tangentToWorld);
    
    worldNormalFromTexture = normalize(worldNormalFromTexture);

    return worldNormalFromTexture;
}

float DistributionGGX(float3 N, float3 H, float a)
{
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;
	
    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return nom / denom;
}

PBRData CalculatePBR(float3 normal, float roughness, float metallic, float3 worldPos)
{
    PBRData output;
    
    roughness = clamp(roughness, 0.001f, 1);
    
    float3 reflectDir = reflect(normalize(viewPos - worldPos), normal);
    
    float3 viewDir = normalize(viewPos - worldPos);
    float3 halfwayDir = normalize(-LightDirection + viewDir);
    
    halfwayDir *= DistributionGGX(normal, halfwayDir, roughness);
    
    //float specular = saturate(pow(max(dot(halfwayDir, normal), 0.0), 32));

    float specular = pow(max(dot(normal, halfwayDir), 0.0), 1.5) / 4;
    
    float fresnelReflectance = metallic + (1.0 - metallic) * pow(1.0 - roughness, 5.0);
    float reflectionAmount = fresnelReflectance * pow(roughness, 4.0);
    
    output.reflectiveness = reflectionAmount;
    
    output.specular = specular * GlobalLightColor;
    
    
    return output;

}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = saturate(dot(N, V));
    float NdotL = saturate(dot(N, L));
    float GGXV = 2.0 * NdotV / (NdotV + sqrt(roughness * roughness + (1.0 - roughness * roughness) * NdotV * NdotV));
    float GGXL = 2.0 * NdotL / (NdotL + sqrt(roughness * roughness + (1.0 - roughness * roughness) * NdotL * NdotL));
    return GGXV * GGXL;
}

float FresnelSchlick(float cosTheta, float metallic)
{
    return metallic + (1.0 - metallic) * pow(1.0 - cosTheta, 5.0);
}

float CalculateSpecular(float3 worldPos,float3 normal, float3 lightDir, float roughness, float metallic)
{
    
    float3 viewDir = normalize(viewPos - worldPos);
    
    lightDir *= -1;
    
    float3 halfwayDir = normalize(viewDir + lightDir);
    float NdotH = saturate(dot(normal, halfwayDir));
    float NdotV = saturate(dot(normal, viewDir));

    float specular = 0.0;

    if (NdotH > 0.0)
    {
        float roughnessSq = lerp(roughness * roughness, roughness, 0.5);
        float D = DistributionGGX(normal, halfwayDir, roughnessSq);
        float G = GeometrySmith(normal, viewDir, lightDir, roughnessSq);
        float F = FresnelSchlick(NdotV, metallic);

        specular = D * G / (4 * NdotV * saturate(dot(normal, lightDir)) + 0.001) * lerp(F,1,0.3);
    }

    return specular * 1;
}

float SampleShadowMap(sampler2D shadowMap, float2 coords, float compare)
{
    
    float4 sample = tex2D(shadowMap, coords);
    
    return step(compare, sample.r);
}

float SampleShadowDif(sampler2D shadowMap, float2 coords, float compare)
{
    
    float sample = tex2D(shadowMap, coords).r - compare;
    
    return sample;
}

float SampleShadowMapLinear(sampler2D shadowMap, float2 coords, float compare, float2 texelSize)
{
    float2 pixelPos = coords / texelSize + float2(0.5, 0.5);
    float2 fracPart = frac(pixelPos);
    float2 startTexel = (pixelPos - fracPart) * texelSize;

    float blTexel = SampleShadowMap(shadowMap, startTexel, compare);
    float brTexel = SampleShadowMap(shadowMap, startTexel + float2(texelSize.x, 0.0), compare);
    float tlTexel = SampleShadowMap(shadowMap, startTexel + float2(0.0, texelSize.y), compare);
    float trTexel = SampleShadowMap(shadowMap, startTexel + texelSize, compare);

    float mixA = lerp(blTexel, tlTexel, fracPart.y);
    float mixB = lerp(brTexel, trTexel, fracPart.y);

    return lerp(mixA, mixB, fracPart.x);
}

float GetShadow(float3 lightCoords, PixelInput input, bool close = false)
{
    float shadow = 0;
    
    float dist = distance(viewPos, input.MyPosition);
    
    if (dist > 100)
        return 0;
    
    if (tex2D(ShadowMapSampler,lightCoords.xy).r<0.01)
        return 0;
    
    if (lightCoords.x >= 0 && lightCoords.x <= 1 && lightCoords.y >= 0 && lightCoords.y <= 1)
    {
        float currentDepth = lightCoords.z * 2 - 1;

        float resolution = 1;
        

        int numSamples = 1; // Number of samples in each direction (total samples = numSamples^2)

        float bias = ShadowBias * (1 - saturate(dot(input.Normal, -LightDirection))) + ShadowBias / 2.0f;
        resolution = ShadowMapResolution;
            
        if (dist > 40)
        {
            return 1 - SampleShadowMap(ShadowMapSampler, lightCoords.xy, currentDepth - bias);
        }
        
        
        float size = 1;
        
        
        float texelSize = size / resolution; // Assuming ShadowMapSize is the size of your shadow map texture
        
        for (int i = -numSamples; i <= numSamples; ++i)
        {
            for (int j = -numSamples; j <= numSamples; ++j)
            {
                float2 offsetCoords = lightCoords.xy + float2(i, j) * texelSize;
                float closestDepth;
                closestDepth = SampleShadowMapLinear(ShadowMapSampler, offsetCoords, currentDepth - bias, float2(texelSize, texelSize));

                shadow += closestDepth;

            }
        }

        // Normalize the accumulated shadow value
        shadow /= ((2 * numSamples + 1) * (2 * numSamples + 1));
        
        return (1 - shadow) * (1 - shadow);
    }
    return 0;
    
}

float GetPointLightDepth(int i, float3 worldPos)
{
    
    if (i>=4)
        return 10000000;

    // Get the direction from the world position to the light position
        float3 lightDir = LightPositions[i] - worldPos;

    // Normalize the light direction
    lightDir = normalize(lightDir);

    float depth = 0.05;
    
    lightDir *= float3(1, -1, -1);
    
    if (i == 0)
        depth = texCUBE(PointLightCubemap1Sampler, lightDir).r;
    else if (i == 1)
        depth = texCUBE(PointLightCubemap2Sampler, lightDir).r;
    else if (i == 2)
        depth = texCUBE(PointLightCubemap3Sampler, lightDir).r;
    else if (i == 3)
        depth = texCUBE(PointLightCubemap4Sampler, lightDir).r;
    
    depth += 0.02;

    return depth;
}


float3 CalculatePointLight(int i, PixelInput pixelInput, float3 normal, float roughness, float metalic)
{
    
    float3 lightVector = LightPositions[i] - pixelInput.MyPosition;
    float distanceToLight = length(lightVector);
    
    float ShadowDistance = GetPointLightDepth(i, pixelInput.MyPosition);
    
    if (distanceToLight>ShadowDistance)
        return float3(0, 0, 0);
    
    float intense = saturate(1.0 - distanceToLight / LightRadiuses[i]);
    float3 dirToSurface = normalize(lightVector);
    
    
    if (isParticle)
        dirToSurface = normal;
    
    
    intense *= saturate(dot(normal, dirToSurface) * 1.1 + 0.4);

    float3 specular = CalculateSpecular(pixelInput.MyPosition, normal, -dirToSurface, roughness, metalic);
    
    intense = max(intense, 0);
    
    float3 l = LightColors[i] * intense;
    
    return l + intense * specular;
}

float3 CalculatePointLightSpeculars(int i, PixelInput pixelInput, float3 normal, float roughness, float metalic)
{
    
#ifdef NO_SPECULAR
    
    return float3(0,0,0);
    
#endif

    float3 lightVector = LightPositions[i] - pixelInput.MyPosition;
    float distanceToLight = length(lightVector);
    float intense = saturate(1.0 - distanceToLight / LightRadiuses[i]);
    float3 dirToSurface = normalize(lightVector);

    
    if (isParticle)
        dirToSurface = normal;
    
    if (Viewmodel == false)
        if (dot(dirToSurface, pixelInput.Normal) < 0)
            return float3(0, 0, 0);
    
    intense *= 1;

    float3 specular = CalculateSpecular(pixelInput.MyPosition,normal, -dirToSurface, roughness, metalic);
    
    return LightColors[i] * max(intense, 0) * specular;
}

float3 CalculateLight(PixelInput input, float3 normal, float roughness, float metalic, float ao)
{
    float3 lightCoords = input.lightPos.xyz / input.lightPos.w;

    float shadow = 0;

    lightCoords = (lightCoords + 1.0f) / 2.0f;

    lightCoords.y = 1.0f - lightCoords.y;
    
    float3 lightCoordsClose = input.lightPosClose.xyz / input.lightPosClose.w;
    lightCoordsClose = (lightCoordsClose + 1.0f) / 2.0f;
    lightCoordsClose.y = 1.0f - lightCoordsClose.y;

    
    shadow += GetShadow(lightCoords, input, false);
    
    shadow += 1 - max(0, dot(normal, normalize(-LightDirection) * 1));
    

    
    shadow = saturate(shadow);
    
    
    float specular = 0;
    
    specular = CalculateSpecular(input.MyPosition, normal, normalize(LightDirection), roughness, metalic);
    
    
    specular *= 1 - shadow;
    
    float3 globalSpecularDir = normalize(-normal + float3(0,-5,0) + LightDirection);
    

    specular += CalculateSpecular(input.MyPosition, normal, globalSpecularDir, roughness, metalic) * 0.02;
    
    if (isParticle)
        normal = -LightDirection;
    
    float3 light = DirectBrightness * GlobalLightColor; // Example light direction;
    
    light *= (1.0) - shadow;
    
    
    
    float3 globalLight = GlobalBrightness * GlobalLightColor * lerp(1, 0.5, dot(normal, float3(0, -1, 0)));
    
    light = max(light, 0);
    light += globalLight;
    
    if (!isParticle)
    {
        specular *= light;
    }

    for (int i = 0; i < MAX_POINT_LIGHTS; i++)
    {
        light += CalculatePointLight(i, input, normal, roughness, metalic);

    }
    
    light -= 1 - ao;
    
    light += specular;
    
    return light;
    
}

float determinant(float3 a, float3 b, float3 c)
{
    return dot(a, cross(b, c));
}

float CalculateReflectiveness(float roughness, float metallic, float3 viewDir, float3 normal)
{
    // Calculate the base reflectiveness based on metallic
    float baseReflectiveness = metallic * 0.5;

    // Calculate the Fresnel factor using the Schlick approximation
    float F0 = lerp(0.01, 0.5, metallic);
    float F = F0 + (1.0 - F0) * pow(1.0 - abs(dot(viewDir, normal)), 5.0);

    // Adjust the base reflectiveness based on roughness
    float reflectiveness = lerp(baseReflectiveness, 0.01, roughness);

    // Modulate reflectiveness by the Fresnel factor
    reflectiveness *= F;

    reflectiveness -= 0.08;
    reflectiveness = saturate(reflectiveness);
    
    reflectiveness *= 1.3;
    
    return reflectiveness;
}

float3 ApplyReflection(float3 inColor, PixelInput input,float3 normal, float roughness, float metallic)
{
    
    float3 viewDir = normalize(viewPos - input.MyPosition);
    
    float3 reflection = reflect(normalize(input.MyPosition - viewPos), normalize(lerp(normal, input.TangentNormal, 0.85)));
    
    
    float3 reflectionColor = SampleCubemap(ReflectionCubemapSampler, reflection);

    float reflectiveness = CalculateReflectiveness(roughness, metallic, viewDir, normal);
    
    reflectiveness = saturate(reflectiveness);
    
    
    return lerp(inColor, reflectionColor, reflectiveness);
}