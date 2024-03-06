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

#define BONE_NUM 128

matrix Bones[BONE_NUM];

bool isParticle = false;

float depthScale = 1.0f;

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

float CalculateSpecular(float roughness, float3 worldPos, float3 normal, float3 lightDir)
{
    
#ifdef NO_SPECULAR
    
    return float(0);
    
#endif

    roughness = clamp(roughness, 0.001f, 1);
    
    float3 reflectDir = reflect(normalize(viewPos - worldPos), normal);
    
    float3 viewDir = normalize(viewPos - worldPos);
    float3 halfwayDir = normalize(lightDir + viewDir);
    
    halfwayDir *= DistributionGGX(normal, halfwayDir, roughness);
    
    float temp = max(dot(normal, halfwayDir), 0.0);
    temp = temp * temp;

    float specular = temp / 10;
    
    
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


float3 CalculatePointLight(int i, PixelInput pixelInput, float3 normal)
{
    
    float3 lightVector = LightPositions[i] - pixelInput.MyPosition;
    float distanceToLight = length(lightVector);
    
    
    float intense = saturate(1.0 - distanceToLight / LightRadiuses[i]);
    float3 dirToSurface = normalize(lightVector);
    
    
    if (isParticle)
        dirToSurface = normal;
    
    if (Viewmodel == false)
        if (dot(dirToSurface, pixelInput.Normal) < 0)
            return float3(0, 0, 0);
    
    intense *= saturate(dot(normal, dirToSurface) * 1.1 + 0.4);

    return LightColors[i] * max(intense, 0);
}

float3 CalculatePointLightSpeculars(int i, PixelInput pixelInput, float3 normal, float roughness)
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

    float3 specular = CalculateSpecular(roughness, pixelInput.MyPosition, normal, dirToSurface);
    
    return LightColors[i] * max(intense, 0) * specular;
}

float3 CalculateLight(PixelInput input, float3 normal, float roughness)
{
    float3 lightCoords = input.lightPos.xyz / input.lightPos.w;

    float shadow = 0;

    lightCoords = (lightCoords + 1.0f) / 2.0f;

    lightCoords.y = 1.0f - lightCoords.y;
    
    float3 lightCoordsClose = input.lightPosClose.xyz / input.lightPosClose.w;
    lightCoordsClose = (lightCoordsClose + 1.0f) / 2.0f;
    lightCoordsClose.y = 1.0f - lightCoordsClose.y;

    
    if (dot(input.TangentNormal, LightDirection) < 0.0f)
    {
        shadow += GetShadow(lightCoords, input, false);
    }
    else if (Viewmodel == true)
    {
        shadow += GetShadow(lightCoords, input, false);
    }
    else
    {
        shadow += 1;
    }
    
    shadow = saturate(shadow);
    
    
    
    float specular = 0;
    
    specular = CalculateSpecular(roughness, input.MyPosition, normal, -LightDirection);
    
    
    specular *= 1.05- shadow;
        
    
    if (isParticle)
        normal = -LightDirection;
    
    float3 light = max(0, dot(normal, normalize(-LightDirection)+0.2)) * DirectBrightness * GlobalLightColor; // Example light direction;
    
    light -= shadow;
    
    
    
    float3 globalLight = GlobalBrightness * GlobalLightColor * lerp(max(dot(normal, float3(0, 1, 0)), -1), 1, 0.7);
    
    light = max(light, 0);
    light += globalLight;
    
    if (!isParticle)
    {
        specular *= light;
    }

    for (int i = 0; i < MAX_POINT_LIGHTS; i++)
    {
        light += CalculatePointLight(i, input, normal);

    }
    
    for (int s = 0; s < 2; s++)
    {
        specular += CalculatePointLightSpeculars(s, input, normal, roughness);

    }
    
    light += specular;
    
    return light;
    
}

float determinant(float3 a, float3 b, float3 c)
{
    return dot(a, cross(b, c));
}

float4x4 inverse(float4x4 m)
{
    float4 c0 = m[0];
    float4 c1 = m[1];
    float4 c2 = m[2];
    float4 c3 = m[3];

    float3 t0 = c1.yzx * c2.zxy - c1.zxy * c2.yzx;
    float3 t1 = c0.zxy * c2.yzx - c0.yzx * c2.zxy;
    float3 t2 = c0.yzx * c1.zxy - c0.zxy * c1.yzx;

    float invDet = 1.0 / determinant(c0, c1, c2);

    return float4x4(
        float4(t0 * invDet, 0),
        float4(t1 * invDet, 0),
        float4(t2 * invDet, 0),
        float4(-(c3.xyz * t0 + c3.yzx * t1 + c3.zxy * t2) * invDet, 1)
    );
}

float3 GetPosition(float2 UV, float depth)
{
    float4 position = 1.0f;
 
    position.x = UV.x * 2.0f - 1.0f;
    position.y = -(UV.y * 2.0f - 1.0f);

    position.z = depth;
 
    position = mul(position, inverse(View * Projection));
 
    position /= position.w;

    return position.xyz;
}

float3 GetUV(float3 position)
{
    
    position = mul(position, (float3x3)View);
    position = mul(position, (float3x3) Projection);
    
    float3 screenCoords = (position + 1.0f) / 2.0f;

    screenCoords.y = 1.0f - screenCoords.y;
    
    return screenCoords;

}

float GetDepth(float2 coords)
{
    return tex2D(DepthTextureSampler, coords).r;
}

float random(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
}

// Assuming 'proj' and 'invProj' are constant buffer matrices
float3 generatePositionFromDepth(float2 texturePos, float depth)
{
    float4 ndc = float4((texturePos - 0.5) * 2.0f, depth, 1.0f);
    float4 inversed = mul(Projection, ndc);
    inversed.w = 1.0f / inversed.w;
    return inversed.xyz;
}

float2 generateProjectedPosition(float3 pos)
{
    float4 samplePosition = mul(Projection, float4(pos, 1.0f));
    samplePosition.xy = (samplePosition.xy / samplePosition.w) * 0.5f + 0.5f;
    return samplePosition.xy;
}

float3 SSR(float3 position, float3 reflection, sampler textureFrame)
{
    
    
    
    float3 step = 1 * reflection;
    float3 marchingPosition = position + step;
    float delta;
    float depthFromScreen;
    float2 screenPosition;
  
    int iterationCount = 30;
    
    float distanceBias = 0.01;
    
    bool isAdaptiveStepEnabled = true;
    
    bool isBinarySearchEnabled = true;
    
    bool isExponentialStepEnabled = true;
    
    float rayStep = 1;
    
    int i = 0;
    for (; i < iterationCount; i++)
    {
        screenPosition = generateProjectedPosition(marchingPosition);
        depthFromScreen = abs(generatePositionFromDepth(screenPosition, tex2D(DepthTextureSampler, screenPosition).x).z);
        delta = abs(marchingPosition.z) - depthFromScreen;
        if (abs(delta) < distanceBias)
        {
            float3 color = float3(1,1,1);
            return tex2D(textureFrame, screenPosition).xyz * color;
        }
        if (isBinarySearchEnabled && delta > 0)
        {
            break;
        }
        if (isAdaptiveStepEnabled)
        {
            float directionSign = sign(abs(marchingPosition.z) - depthFromScreen);
      //this is sort of adapting step, should prevent lining reflection by doing sort of iterative converging
      //some implementation doing it by binary search, but I found this idea more cheaty and way easier to implement
            step = step * (1.0f - rayStep * max(directionSign, 0.0f));
            marchingPosition += step * (-directionSign);
        }
        else
        {
            marchingPosition += step;
        }
        if (isExponentialStepEnabled)
        {
            step *= 1.05f;
        }
    }
    if (isBinarySearchEnabled)
    {
        for (; i < iterationCount; i++)
        {
 
            step *= 0.5f;
            marchingPosition = marchingPosition - step * sign(delta);
 
            screenPosition = generateProjectedPosition(marchingPosition);
            depthFromScreen = abs(generatePositionFromDepth(screenPosition, tex2D(DepthTextureSampler, screenPosition).x).z);
            delta = abs(marchingPosition.z) - depthFromScreen;
 
            if (abs(delta) < distanceBias)
            {
                float3 color = float3(1,1,1);
                return tex2D(textureFrame, screenPosition).xyz * color;
            }
        }
    }
  
    return float3(1,1,1);
}