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

texture ShadowMapVeryClose;
sampler ShadowMapVeryCloseSampler = sampler_state
{
    texture = <ShadowMapVeryClose>;
};

texture DepthTexture;
sampler DepthTextureSampler = sampler_state
{
    texture = <DepthTexture>;

    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;

};

texture FrameTexture;
sampler FrameTextureSampler = sampler_state
{
    texture = <FrameTexture>;

    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture ReflectionTexture;
sampler ReflectionTextureSampler = sampler_state
{
    texture = <ReflectionTexture>;

    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture ReflectionCubemap;
sampler ReflectionCubemapSampler = sampler_state
{
    texture = <ReflectionCubemap>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};


float FarPlane;
float3 viewDir;
float3 viewPos;

matrix InverseViewProjection;

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

matrix ShadowMapViewProjectionVeryClose;
float ShadowMapResolutionVeryClose;



#ifndef MAX_POINT_LIGHTS

#define MAX_POINT_LIGHTS 20

#endif

#ifndef MAX_POINT_LIGHTS_SHADOWS

#define MAX_POINT_LIGHTS_SHADOWS 6

#endif

#ifdef OPENGL
#define MAX_POINT_LIGHTS 6
#endif

float3 LightPositions[MAX_POINT_LIGHTS];
float3 LightColors[MAX_POINT_LIGHTS];
float LightRadiuses[MAX_POINT_LIGHTS];
float LightResolutions[MAX_POINT_LIGHTS];

texture PointLightCubemap1;
sampler PointLightCubemap1Sampler = sampler_state
{
    texture = <PointLightCubemap1>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture PointLightCubemap2;
sampler PointLightCubemap2Sampler = sampler_state
{
    texture = <PointLightCubemap2>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;

};

texture PointLightCubemap3;
sampler PointLightCubemap3Sampler = sampler_state
{
    texture = <PointLightCubemap3>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture PointLightCubemap4;
sampler PointLightCubemap4Sampler = sampler_state
{
    texture = <PointLightCubemap4>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture PointLightCubemap5;
sampler PointLightCubemap5Sampler = sampler_state
{
    texture = <PointLightCubemap5>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture PointLightCubemap6;
sampler PointLightCubemap6Sampler = sampler_state
{
    texture = <PointLightCubemap6>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture PointLightCubemap7;
sampler PointLightCubemap7Sampler = sampler_state
{
    texture = <PointLightCubemap7>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture PointLightCubemap8;
sampler PointLightCubemap8Sampler = sampler_state
{
    texture = <PointLightCubemap8>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture PointLightCubemap9;
sampler PointLightCubemap9Sampler = sampler_state
{
    texture = <PointLightCubemap9>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture PointLightCubemap10;
sampler PointLightCubemap10Sampler = sampler_state
{
    texture = <PointLightCubemap10>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
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

    float4 Color : COLOR0;

};

struct PixelInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : TEXCOORD1; // Pass normal to pixel shader
    float4 lightPos : TEXCOORD2;
    float4 lightPosClose : TEXCOORD3;
    float3 MyPosition : TEXCOORD4;
    float4 MyPixelPosition : TEXCOORD5;
    float3 Tangent : TEXCOORD6;
    float3 TangentNormal : TEXCOORD7;
    float4 lightPosVeryClose : TEXCOORD8;
    float4 Color : COLOR0;
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
    float4 Normal : COLOR1;
    float4 Reflectiveness : COLOR2;
    float4 Position : COLOR3;
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
    
    float4x4 BonesWorld = mul(boneTrans, World);
    
    float4 worldPos = mul(input.Position, BonesWorld);

    
    output.Position = worldPos;
    output.MyPosition = output.Position.xyz;
    output.Position = mul(output.Position, View);
    
    
    
    if (Viewmodel)
    {
        output.Position = mul(output.Position, ProjectionViewmodel);
        output.Position.z *= 0.02;
    }
    else
    {
        output.Position = mul(output.Position, Projection);
    }
    
        
    
    output.MyPixelPosition = output.Position;
    
    
    output.TexCoord = input.TexCoord;

	// Pass the world space normal to the pixel shader
    output.Normal = mul(input.Normal, (float3x3)BonesWorld);
    output.Normal = normalize(output.Normal);
    
    
    output.Tangent = mul(input.Tangent, (float3x3) BonesWorld);
    output.Tangent = normalize(output.Tangent);

    output.TangentNormal = GetTangentNormal(output.Normal, output.Tangent);
    
    if (dot(output.TangentNormal, normalize(output.MyPosition - viewPos)) > 0)
        output.TangentNormal *= -1;
    

    output.lightPos = mul(worldPos, ShadowMapViewProjection);
    output.lightPosClose = mul(worldPos, ShadowMapViewProjectionClose);
    output.lightPosVeryClose = mul(worldPos, ShadowMapViewProjectionVeryClose);
    
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;

    return output;
}

void DepthDiscard(float depth, PixelInput input)
{
    if (depth < input.MyPixelPosition.z - 0.015)
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

    return max(d, max(d1, d2));

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

float CalculateSpecular(float3 worldPos, float3 normal, float3 lightDir, float roughness, float metallic) {
    // Common calculations
    float3 vDir = normalize(viewPos - worldPos);
    lightDir *= -1.0f;
    float3 halfwayDir = normalize(vDir + lightDir);
    float NdotH = saturate(dot(normal, halfwayDir));
    float NdotV = saturate(dot(normal, vDir));
    float NdotL = saturate(dot(normal, lightDir));

    // GGX BRDF (distribution and visibility)
    float roughnessSq = roughness * roughness;
    float D = DistributionGGX(normal, halfwayDir, roughnessSq);
    float G = GeometrySmith(normal, vDir, lightDir, roughnessSq);

    // Fresnel term (reflectivity based on metallic and viewing angle)
    float F = FresnelSchlick(NdotV, metallic);

    // Calculate specular based on BRDF components and energy conservation
    float specular = D * G * F / (4.0f * NdotV * NdotL + 0.001f);

    // Clamp specular to prevent negative values
    specular = max(specular, 0.0f);

    return specular;
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

float GetShadowClose(float3 lightCoords, PixelInput input)
{
    float shadow = 0;
    
    float dist = distance(viewPos, input.MyPosition);
    
    if (lightCoords.x >= 0 && lightCoords.x <= 1 && lightCoords.y >= 0 && lightCoords.y <= 1)
    {
        float currentDepth = lightCoords.z * 2 - 1;

        float resolution = 1;
        

        int numSamples = 1; // Number of samples in each direction (total samples = numSamples^2)

        float bias = ShadowBias * (1 - saturate(dot(input.Normal, -LightDirection))) + ShadowBias / 2.0f;
        resolution = ShadowMapResolutionClose;
        
        
        float size = 1;
        
        bias *= 1;
        
        if(abs(dot(input.Normal, -LightDirection)) <= 0.3)
        return 1 - SampleShadowMap(ShadowMapCloseSampler, lightCoords.xy, currentDepth + bias);

        float texelSize = size / resolution; // Assuming ShadowMapSize is the size of your shadow map texture
        
        for (int i = -numSamples; i <= numSamples; ++i)
        {
            for (int j = -numSamples; j <= numSamples; ++j)
            {
                float2 offsetCoords = lightCoords.xy + float2(i, j) * texelSize;
                float closestDepth;
                closestDepth = SampleShadowMapLinear(ShadowMapCloseSampler, offsetCoords, currentDepth + bias, float2(texelSize, texelSize));

                shadow += closestDepth;

            }
        }

        // Normalize the accumulated shadow value
        shadow /= ((2 * numSamples + 1) * (2 * numSamples + 1));
        
        return (1 - shadow) * (1 - shadow);
    }
    return 0;
    
}

float GetShadowVeryClose(float3 lightCoords, PixelInput input)
{
    float shadow = 0;
    
    float dist = distance(viewPos, input.MyPosition);
    
    if (lightCoords.x >= 0 && lightCoords.x <= 1 && lightCoords.y >= 0 && lightCoords.y <= 1)
    {
        float currentDepth = lightCoords.z * 2 - 1;

        float resolution = 1;
        

        int numSamples = 1; // Number of samples in each direction (total samples = numSamples^2)

        float b = 0.0004;
        
        float bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;
        resolution = ShadowMapResolutionClose;
        
        //bias -= max(dot(input.Normal, float3(0,1,0)),0) * b/2;
        
        float size = (abs(dot(input.Normal, -LightDirection))-0.5)*2;
        
        size = 1; max(size, 0.001);
        
        float texelSize = size / resolution; // Assuming ShadowMapSize is the size of your shadow map texture
        
        for (int i = -numSamples; i <= numSamples; ++i)
        {
            for (int j = -numSamples; j <= numSamples; ++j)
            {
                float2 offsetCoords = lightCoords.xy + float2(i, j) * texelSize;
                float closestDepth;
                closestDepth = SampleShadowMapLinear(ShadowMapVeryCloseSampler, offsetCoords, currentDepth - bias, float2(texelSize, texelSize));

                shadow += closestDepth;

            }
        }

        // Normalize the accumulated shadow value
        shadow /= ((2 * numSamples + 1) * (2 * numSamples + 1));
        
        return (1 - shadow) * (1 - shadow);
    }
    return 0;
    
}

float GetShadow(float3 lightCoords,float3 lightCoordsClose,float3 lightCoordsVeryClose, PixelInput input)
{
    float shadow = 0;
    
    float dist = distance(viewPos, input.MyPosition);
    
    if (dist > 200)
        return 0;
    
    if (tex2D(ShadowMapSampler,lightCoords.xy).r<0.01)
        return 0;
    

    if (lightCoords.x >= 0 && lightCoords.x <= 1 && lightCoords.y >= 0 && lightCoords.y <= 1)
    {
        
            
        if (dist < 8 ) //&& abs(dot(input.TangentNormal, -LightDirection))>0.3
        {
            if (lightCoordsVeryClose.x >= 0 && lightCoordsVeryClose.x <= 1 && lightCoordsVeryClose.y >= 0 && lightCoordsVeryClose.y <= 1)
            {
                return GetShadowVeryClose(lightCoordsVeryClose, input);
            }
        }
        
        if (dist < 31)
        {
            if (lightCoordsClose.x >= 0 && lightCoordsClose.x <= 1 && lightCoordsClose.y >= 0 && lightCoordsClose.y <= 1)
            {
                return GetShadowClose(lightCoordsClose, input);
            }
        }
        
        float currentDepth = lightCoords.z * 2 - 1;

        float resolution = 1;
        

        int numSamples = 1; // Number of samples in each direction (total samples = numSamples^2)

        float bias = ShadowBias * (1 - saturate(dot(input.Normal, -LightDirection))) + ShadowBias / 2.0f;
        resolution = ShadowMapResolution;
        
        bias *= 1;

        return 1 - SampleShadowMap(ShadowMapSampler, lightCoords.xy, currentDepth + bias);
        
        float size = 1;
        
        
        float texelSize = size / resolution; // Assuming ShadowMapSize is the size of your shadow map texture
        
        for (int i = -numSamples; i <= numSamples; ++i)
        {
            for (int j = -numSamples; j <= numSamples; ++j)
            {
                float2 offsetCoords = lightCoords.xy + float2(i, j) * texelSize;
                float closestDepth;
                closestDepth = SampleShadowMapLinear(ShadowMapSampler, offsetCoords, currentDepth + bias, float2(texelSize, texelSize));

                shadow += closestDepth;

            }
        }

        // Normalize the accumulated shadow value
        shadow /= ((2 * numSamples + 1) * (2 * numSamples + 1));
        
        return (1 - shadow) * (1 - shadow);
    }
    return 0;
    
}


float GetPointLightDepth(int i, float3 lightDir)
{
    if (i >= MAX_POINT_LIGHTS_SHADOWS)
        return 10000;

    float depth = 0.00;

    lightDir *= float3(1, -1, -1);

    if (i == 0)
        depth = texCUBE(PointLightCubemap1Sampler, lightDir).r;
    else if (i == 1)
        depth = texCUBE(PointLightCubemap2Sampler, lightDir).r;
    else if (i == 2)
        depth = texCUBE(PointLightCubemap3Sampler, lightDir).r;
    else if (i == 3)
        depth = texCUBE(PointLightCubemap4Sampler, lightDir).r;
    else if (i == 4)
        depth = texCUBE(PointLightCubemap5Sampler, lightDir).r;
    else if (i == 5)
        depth = texCUBE(PointLightCubemap6Sampler, lightDir).r;

    if (depth == 0)
        return 10000;

    depth += depth / (LightResolutions[i] * 3) + 0.04;

    return depth;
}


float3 CalculatePointLight(int i, PixelInput pixelInput, float3 normal, float roughness, float metalic)
{
    float3 lightVector = LightPositions[i] - pixelInput.MyPosition;
    float distanceToLight = length(lightVector);

    if(distanceToLight> LightRadiuses[i])
        return float3(0,0,0);

    

    float offsetScale = 1 / (LightResolutions[i] / 40) / lerp(distanceToLight,1, 0.7);
    offsetScale *= lerp(abs(dot(normal, normalize(lightVector))), 0.7, 1);
    float notShadow = 1;

    if(dot(normal, normalize(lightVector))<0.01)
    {
        notShadow = 0;
    }

    if (LightResolutions[i] > 10 && notShadow>0)
    {
        float3 lightDir = normalize(lightVector);
        float shadowBias = 0.05;  // Adjust this bias for your specific scene

        // Calculate tangent and bitangent vectors
        float3 up = abs(normal.y) < 0.999 ? float3(0, 1, 0) : float3(1, 0, 0);
        float3 tangent = normalize(cross(up, normal));
        float3 bitangent = cross(normal, tangent);

        // PCF sampling
        int samples = 0;
        float shadowFactor = 0.0;

        const int radius = 2;

        const float step = 0.66666666666665;

        for (float x = -radius; x <= radius; x+=step)
        {
            for (float y = -radius; y <= radius; y+=step)
            {
                float3 offset = (tangent * x + bitangent * y) * shadowBias * offsetScale;
                float shadowDepth = GetPointLightDepth(i, lightDir + offset);
                shadowFactor += distanceToLight < shadowDepth ? 1.0 : 0.0;
                samples++;
            }
        }

        shadowFactor /= samples;
        notShadow = shadowFactor;
    }

    float dist = (distanceToLight / LightRadiuses[i]);
    float intense = saturate(1.0 - dist * dist);
    float3 dirToSurface = normalize(lightVector);

    if (isParticle)
        dirToSurface = normal;

    intense *= saturate(dot(normal, dirToSurface));
    float3 specular = CalculateSpecular(pixelInput.MyPosition, normal, -dirToSurface, roughness, metalic);

    intense = max(intense, 0);
    float3 l = LightColors[i] * intense;

    return (l + intense * specular) * notShadow;
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
    
    float3 lightCoordsVeryClose = input.lightPosVeryClose.xyz / input.lightPosVeryClose.w;
    lightCoordsVeryClose = (lightCoordsVeryClose + 1.0f) / 2.0f;
    lightCoordsVeryClose.y = 1.0f - lightCoordsVeryClose.y;

    
    
    if(dot(normal, -LightDirection)<0.01)
    {
        shadow+=1;
    }else
    {
    shadow += GetShadow(lightCoords,lightCoordsClose,lightCoordsVeryClose, input);
    }
    
    
    shadow += 1 - max(0, dot(normal, normalize(-LightDirection) * 1));
    

    
    shadow = saturate(shadow);
    
    
    float specular = 0;
    
    specular = CalculateSpecular(input.MyPosition, normal, normalize(LightDirection), roughness, metalic) * DirectBrightness;
    
    specular *= max(1 - shadow, 0);
    
    float3 globalSpecularDir = normalize(-normal + float3(0,-5,0) + LightDirection);
    

    specular += CalculateSpecular(input.MyPosition, normal, globalSpecularDir, roughness, metalic) * 0.02 ;
    
    if (isParticle)
        normal = -LightDirection;
    
    float3 light = DirectBrightness * GlobalLightColor; // Example light direction;
    
    light *= (1.0) - shadow;
    
    
    
    float3 globalLight = GlobalBrightness * GlobalLightColor * lerp(1, 0.6, max(dot(normal, float3(0, -1, 0)),0));
    globalLight*=ao;
    
    light = max(light, 0);
    
    
    

    for (int i = 0; i < MAX_POINT_LIGHTS; i++)
    {
        light += CalculatePointLight(i, input, normal, roughness, metalic);

    }
    
    //light -= (1 - ao);
    
    light += specular;
    
    light = max(light, 0);
    
    light += globalLight;

    return light;
    
}

float2 WorldToScreen(float3 pos)
{
    float4 position = float4(pos, 1);
    
    
    float4 projection = mul(mul(position, View), Projection);
    
    float2 screenCoords = projection.xyz / projection.w;
    
    screenCoords = (screenCoords + 1.0f) / 2.0f;

    screenCoords.y = 1.0f - screenCoords.y;
    
    
    return screenCoords;
}

float4 WorldToClip(float3 pos)
{
    float4 position = float4(pos, 1);
    
    
    float4 projection = mul(mul(position, View), Projection);
    
    return projection;
}

float SampleDepthWorldCoords(float3 pos)
{
    float2 screenCoords = WorldToScreen(pos);
    
    return SampleDepth(screenCoords);
}


float3 SampleColorWorldCoords(float3 pos)
{
    
    float2 screenCoords = WorldToScreen(pos);
    
    return tex2D(FrameTextureSampler, screenCoords).rgb;
}

float3 GetPosition(float2 UV, float depth)
{
    float4 position = 1.0f;
 
    position.x = UV.x * 2.0f - 1.0f;
    position.y = -(UV.y * 2.0f - 1.0f);

    position.z = depth;
 
    position = mul(position, InverseViewProjection);
 
    position /= position.w;

    return position.xyz;
}


float4 SampleSSR(float3 direction, float3 position, float currentDepth, float3 normal, float3 vDir)
{
    
    float step = 0.012;
    
    const int steps = 120;
    
    float4 outColor = float4(0, 0, 0, 0);
    
    float3 selectedCoords;
    
    float3 dir = direction;
    
    float3 pos = position;
    
    float2 coords;
    
    float2 outCoords;
    
    float weight = -0.3;
   
    float factor = 1.25;
    
    bool facingCamera = false; dot(vDir, direction) < 0;
    
    
    float disToCamera = length(viewPos - position);
    
    for (int i = 0; i < steps; i++)
    {
        
        float3 offset = dir * (step) * disToCamera / 30 + dir * 0.02 * disToCamera;
        
        coords = WorldToScreen(pos + offset);
        
        float dist = WorldToClip(pos + offset).z;
        
        float SampledDepth = SampleDepthWorldCoords(pos + offset);
        
        selectedCoords = pos + offset;
        
        bool inScreen = coords.x > 0.001 && coords.x < 0.999 && coords.y > 0.001 && coords.y < 0.999;
        
        weight = clamp(weight, -500000, 5);
        

        if (SampledDepth < currentDepth - 0.025 && facingCamera == false)
        {
            return float4(0, 0, 0, 0);

        }
        
        if (inScreen == false || SampledDepth>10000)
        {
            step == 0.02;
            factor = lerp(factor, 1, 0.5);
        }
        
        if (SampledDepth < dist && (SampledDepth > dist - 1 || facingCamera == false))
        {

            outCoords = coords;
            step /= 1.3;
            factor = lerp(factor, 1, 0.5);
            weight += 1;
            
            
            continue;

        }

        step *= factor;
        
    }
    
    weight = saturate(weight);
    
    outColor = float4(tex2D(FrameTextureSampler, coords).rgb,  weight);
    
    return outColor;
    
}

float ReflectionMapping(float x)
{
    
    const float n = -0.066;
    
    const float v = x / 3;
    
    return v / ((x * 10 + 1 / n)*n);

}

float CalculateReflectiveness(float roughness, float metallic, float3 vDir, float3 normal)
{
    // Calculate the base reflectiveness based on metallic
    float baseReflectiveness = metallic * 0.5;

    // Calculate the Fresnel factor using the Schlick approximation
    float F0 = lerp(0.01, 0.5, metallic);
    float F = 1; // F0 + (1.0 - F0) * pow(1.0 - abs(dot(vDir, normal)), 5.0);

    // Adjust the base reflectiveness based on roughness
    float reflectiveness = lerp(baseReflectiveness, 0.01, roughness);

    // Modulate reflectiveness by the Fresnel factor
    reflectiveness *= F;

    reflectiveness = saturate(reflectiveness);
    
    reflectiveness -= 0.1;
    
    reflectiveness *= 2.6;
    
    return ReflectionMapping(saturate(reflectiveness));
}

float CalcLuminance(float3 color)
{
    return dot(color, float3(0.299f, 0.587f, 0.114f));
}

float3 ApplyReflection(float3 inColor, float3 albedo, PixelInput input,float3 normal, float roughness, float metallic)
{
    
    
    float3 WorldPos = input.MyPosition;
    
    float3 vDir = normalize(input.MyPosition - viewPos);
    
    float3 reflection = reflect(normalize(input.MyPosition - viewPos), normalize(lerp(normal, input.TangentNormal, 0.4)));
    
    
    float4 ssr = SampleSSR(reflection, input.MyPosition, input.MyPixelPosition.z, normal, vDir);
    
    float3 cube = SampleCubemap(ReflectionCubemapSampler, reflection);
    
    float3 reflectionColor = lerp(cube, ssr.rgb, ssr.w);
    

    float reflectiveness = CalculateReflectiveness(roughness, metallic, normal, normal);
    
    reflectiveness = saturate(reflectiveness);
    
    reflectionColor *= lerp(float3(1, 1, 1), albedo, metallic);
    
    return lerp(inColor, reflectionColor, reflectiveness);
}

float3 ApplyReflectionOnSurface(float3 color,float2 screenCoords, float reflectiveness)
{

    float3 reflection = tex2D(ReflectionTextureSampler, screenCoords).rgb;

    float lum = CalcLuminance(reflection);

    return lerp(color, reflection * color, saturate(reflectiveness/2 * lum + reflectiveness));
}