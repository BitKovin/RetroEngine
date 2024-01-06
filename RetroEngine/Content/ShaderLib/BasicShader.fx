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

texture DepthMap;
sampler DepthMapSampler = sampler_state
{
    texture = <DepthMap>;
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

matrix ShadowMapViewProjectionClose;
float ShadowMapResolutionClose;

#define MAX_POINT_LIGHTS 15

float3 LightPositions[MAX_POINT_LIGHTS];
float3 LightColors[MAX_POINT_LIGHTS];
float LightRadiuses[MAX_POINT_LIGHTS];

#define BONE_NUM 128

matrix BoneTransforms[BONE_NUM];

bool isParticle = false;

float depthScale = 1.0f;

struct VertexInput
{
    float4 Position : SV_POSITION0;
    float3 Normal : NORMAL0; // Add normal input
    float2 TexCoord : TEXCOORD0;
    float3 Tangent : TANGENT0;
    
    float2 Bone1 : POSITION1;
    float2 Bone2 : POSITION2;
    float2 Bone3 : POSITION3;
    float2 Bone4 : POSITION4;
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
};

float3 normalize(float3 v)
{
    return rsqrt(dot(v, v)) * v;
}

float4 ApplyBoneTransformations(VertexInput input)
{
    float4 position = input.Position;
    
    position = lerp(position, mul(position, BoneTransforms[input.Bone1.x]), input.Bone1.y);
    position = lerp(position, mul(position, BoneTransforms[input.Bone2.x]), input.Bone2.y);
    position = lerp(position, mul(position, BoneTransforms[input.Bone3.x]), input.Bone3.y);
    position = lerp(position, mul(position, BoneTransforms[input.Bone4.x]), input.Bone4.y);
    
    return position;
}

PixelInput DefaultVertexShaderFunction(VertexInput input)
{
    PixelInput output;

    float4 vertexPos = ApplyBoneTransformations(input);
    
    output.Position = mul(vertexPos, World);
    output.MyPosition = mul(vertexPos, World).xyz;
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    
    
    output.Position.z *= depthScale;
    
    output.MyPixelPosition = output.Position;
    
    output.TexCoord = input.TexCoord;

	// Pass the world space normal to the pixel shader
    output.Normal = mul(input.Normal, (float3x3) World);
    output.Normal = normalize(output.Normal);
    
    output.Tangent = mul(input.Tangent, (float3x3) World);
    output.Tangent = normalize(output.Tangent);

    
    output.light = 0;

    output.lightPos = mul(float4(mul(input.Position, World)), ShadowMapViewProjection);
    output.lightPosClose = mul(float4(mul(input.Position, World)), ShadowMapViewProjectionClose);
    
    output.TexCoord = input.TexCoord;
    
    return output;
}

float3 ApplyNormalTexture(float3 sampledNormalColor, float3 worldNormal, float3 worldTangent)
{
    
    if (length(sampledNormalColor) < 0.001f)
        sampledNormalColor = float3(0.5, 0.5, 1);
    
    sampledNormalColor *= float3(1, 1, 0.8f);
    
    sampledNormalColor = normalize(sampledNormalColor);
    
    float3 normalMapSample = sampledNormalColor * 2.0 - 1.0;
    
    normalMapSample*=10;
    
    // Create the tangent space matrix as before
    float3 bitangent = cross(worldNormal, worldTangent);
    float3x3 tangentToWorld = float3x3(worldTangent, bitangent, worldNormal);

    // Transform the normal from tangent space to world space
    float3 worldNormalFromTexture = mul(normalMapSample, tangentToWorld);

    // Normalize the final normal
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

float SampleShadowMap(sampler2D shadowMap, float2 coords, float compare)
{
    return step(compare, tex2D(shadowMap, coords).r);
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

    if (lightCoords.x >= 0 && lightCoords.x <= 1 && lightCoords.y >= 0 && lightCoords.y <= 1)
    {
        float currentDepth = lightCoords.z * 2 - 1;

        float resolution = 1;
        

        int numSamples = 2; // Number of samples in each direction (total samples = numSamples^2)

        float bias = ShadowBias * abs(input.Normal.z) + ShadowBias / 2.0f;
        resolution = ShadowMapResolution;
            
        
        float texelSize = 1.0f / resolution; // Assuming ShadowMapSize is the size of your shadow map texture
        
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
    
    intense *= saturate(dot(normal, dirToSurface) * 5 + 2);

    return LightColors[i] * max(intense, 0);
}

float3 CalculateLight(PixelInput input, float3 normal)
{
    float3 lightCoords = input.lightPos.xyz / input.lightPos.w;

    float shadow = 0;

    lightCoords = (lightCoords + 1.0f) / 2.0f;

    lightCoords.y = 1.0f - lightCoords.y;
    
    float3 lightCoordsClose = input.lightPosClose.xyz / input.lightPosClose.w;
    lightCoordsClose = (lightCoordsClose + 1.0f) / 2.0f;
    lightCoordsClose.y = 1.0f - lightCoordsClose.y;

    if (lightCoordsClose.x >= 0.01f && lightCoordsClose.x <= 0.99f && lightCoordsClose.y >= 0.01f && lightCoordsClose.y <= 0.99f && false)
    {
        shadow += GetShadow(lightCoordsClose, input, true);
    }
    else
        shadow += GetShadow(lightCoords, input, false);
    
    
    if(isParticle)
        normal = -LightDirection;
    
    float3 light = max(0.0, dot(normal, normalize(-LightDirection))) * DirectBrightness * GlobalLightColor; // Example light direction;

    if (abs(dot(input.Normal, normalize(-LightDirection)))>0.01f)
        light -= shadow;
    
    light = max(light, 0);
    light += GlobalBrightness;


    for (int i = 0; i < MAX_POINT_LIGHTS; i++)
    {
        light += CalculatePointLight(i, input, normal);
    }
    
    return light;
    
}