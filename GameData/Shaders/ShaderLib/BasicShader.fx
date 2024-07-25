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
    MinFilter = Point;
    MagFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture ShadowMapClose;
sampler ShadowMapCloseSampler = sampler_state
{
    texture = <ShadowMapClose>;
    MinFilter = Point;
    MagFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture ShadowMapVeryClose;
sampler ShadowMapVeryCloseSampler = sampler_state
{
    texture = <ShadowMapVeryClose>;
    MinFilter = Point;
    MagFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
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

    MinFilter = Point;
    MagFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture ReflectionTexture;
sampler ReflectionTextureSampler = sampler_state
{
    texture = <ReflectionTexture>;

    MinFilter = Linear;
    MagFilter = Linear;
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


float DitherDisolve;

float FarPlane;
float3 viewDir;
float3 viewPos;

matrix InverseViewProjection;

float LightDistanceMultiplier;
float DirectBrightness;
float GlobalBrightness;
float3 LightDirection;
float3 GlobalLightColor;
float3 SkyColor;

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

bool depthTestEqual;

#ifndef MAX_POINT_LIGHTS

#define MAX_POINT_LIGHTS 20

bool skeletalMesh;

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
float4 LightDirections[MAX_POINT_LIGHTS];
 
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

float SSRHeight;
float SSRWidth;

bool ViewmodelShadowsEnabled;

bool Masked;

struct VertexInput
{
    float4 Position : SV_POSITION0;
    float3 Normal : NORMAL0;

    float3 SmoothNormal : NORMAL1;

    float2 TexCoord : TEXCOORD0;
    float3 Tangent : TANGENT0;
    float3 BiTangent : BINORMAL0;
    
    float4 BlendIndices : BLENDINDICES0;
    float4 BlendWeights : BLENDWEIGHT0;

    float4 Color : COLOR0;

};

struct PixelInput //only color and texcoords or opengl might freak out
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : TEXCOORD8; 
    float4 lightPos : TEXCOORD1;
    float4 lightPosClose : TEXCOORD2;
    float3 MyPosition : TEXCOORD3;
    float4 MyPixelPosition : TEXCOORD4;
    float3 Tangent : TEXCOORD5;
    float4 lightPosVeryClose : TEXCOORD6;
    float3 BiTangent : TEXCOORD7;
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

float3 ApplyNormalTexture(float3 sampledNormalColor, float3 worldNormal, float3 worldTangent, float3 bitangent)
{
    
    if (length(sampledNormalColor) < 0.1f)
        sampledNormalColor = float3(0.5, 0.5, 1);
    
    
    sampledNormalColor *= float3(1, 1, 1);
    
    worldNormal = normalize(worldNormal);
    worldTangent = normalize(worldTangent);

    float3 normalMapSample = sampledNormalColor * 2.0 - 1.0;
    
    normalMapSample *= float3(-1, -1, 1);
    
    normalMapSample *= 1;
    
    
    float3x3 tangentToWorld = float3x3(worldTangent, bitangent, worldNormal);

    // Transform the normal from tangent space to world space
    float3 worldNormalFromTexture = mul(normalMapSample, tangentToWorld);
    
    worldNormalFromTexture = normalize(worldNormalFromTexture);

    return worldNormalFromTexture;
}

float3 GetTangentNormal(float3 worldNormal, float3 worldTangent, float3 bitangent)
{
    return ApplyNormalTexture(float3(0.5,0.5,1),worldNormal, worldTangent, bitangent);
}

PixelInput DefaultVertexShaderFunction(VertexInput input)
{
    PixelInput output;

    float4x4 boneTrans = GetBoneTransforms(input);
    
    float4x4 BonesWorld = mul(boneTrans, World);
    
    //input.Position += float4(input.SmoothNormal*0.2,0);

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

    output.BiTangent = mul(input.BiTangent, (float3x3) BonesWorld);
    output.BiTangent = normalize(output.BiTangent);
    

    output.lightPos = mul(worldPos, ShadowMapViewProjection);
    output.lightPosClose = mul(worldPos, ShadowMapViewProjectionClose);
    output.lightPosVeryClose = mul(worldPos, ShadowMapViewProjectionVeryClose);
    
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;

    return output;
}

void DepthDiscard(float depth, PixelInput input)
{

    float b = 0.01;
    if(Viewmodel)
    b = 0.000005f;

    if (depth < input.MyPixelPosition.z - b && depthTestEqual == false)
        discard;
}

float SampleDepth(float2 coords)
{
    return tex2D(DepthTextureSampler, coords);

}

void MaskedDiscard(float alpha)
{
    if(alpha<0.95 && Masked)
        discard;
}

float SampleMaxDepth(float2 screenCoords)
{
    
    float2 texelSize = 1.1 / float2(ScreenWidth, ScreenHeight);
    
    float d = SampleDepth(screenCoords);
    float d1 = SampleDepth(screenCoords + texelSize);
    float d2 = SampleDepth(screenCoords - texelSize);
    float d3 = SampleDepth(screenCoords + texelSize*float2(1,-1));
    float d4 = SampleDepth(screenCoords - texelSize*float2(-1,1));

    return max(d, max(d1, max(d2,max(d3,d4))));

}



float4 SampleCubemap(samplerCUBE s, float3 coords)
{
    return texCUBE(s, coords * float3(-1,1,1));
}


// Bayer matrix (4x4) for dithering
static const float4x4 BayerMatrix = {
    0.0f / 16.0f,  8.0f / 16.0f,  2.0f / 16.0f, 10.0f / 16.0f,
   12.0f / 16.0f,  4.0f / 16.0f, 14.0f / 16.0f,  6.0f / 16.0f,
    3.0f / 16.0f, 11.0f / 16.0f,  1.0f / 16.0f,  9.0f / 16.0f,
   15.0f / 16.0f,  7.0f / 16.0f, 13.0f / 16.0f,  5.0f / 16.0f
};

// Function to get the texel size based on the screen resolution
float2 GetTexelSize(float2 screenResolution) {
    return 1.0f / screenResolution;
}

// Dithering function
bool Dither(float2 screenCoords, float amount, float2 screenResolution) {
    float2 texelSize = GetTexelSize(screenResolution);

    // Calculate the Bayer matrix index
    int2 index = int2(screenCoords / texelSize) % 4;

    // Get the Bayer matrix value
    float threshold = BayerMatrix[index.x][index.y];

    // Return the dithering result
    return amount > threshold;
}


float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = roughness + 1.0f;
    float k = (r * r) / 8.0f;

    float num = NdotV;
    float denom = NdotV * (1.0f - k) + k;

    return num / denom;
}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0f);
    float NdotL = max(dot(N, L), 0.0f);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

float3 FresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0f - F0) * pow(1.0f - cosTheta, 5.0f);
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

float CalculateSpecular(float3 worldPos, float3 normal, float3 lightDir, float roughness, float metallic, float3 albedo)
{
#ifdef NO_SPECULAR
    return 0;
#endif

    if(dot(normal, lightDir)<0)
        return 0;

    float3 vDir = normalize(viewPos - worldPos);
    lightDir = normalize(lightDir);
    float3 halfwayDir = normalize(vDir + lightDir);
    
    float NdotH = saturate(dot(normal, halfwayDir));
    float NdotV = saturate(dot(normal, vDir));
    float NdotL = saturate(dot(normal, lightDir));

    // GGX Normal Distribution Function (NDF)
    float roughnessSq = roughness * roughness;
    float D = DistributionGGX(normal, halfwayDir, roughnessSq);

    // Geometry function using Smith's method
    float G = GeometrySmith(normal, vDir, lightDir, roughnessSq);

    // Fresnel term using Schlick's approximation
    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), albedo, metallic);
    float3 F = FresnelSchlick(NdotH, F0);

    // Specular BRDF
    float3 specular = (D * G) / (4.0f * NdotV * NdotL + 0.001f);

    return specular * 0.75;
}


float3 offset_lookup(sampler2D map, float4 loc, float2 offset, float texelSize)
{
    return tex2Dproj(map, float4(loc.xy+offset*texelSize * loc.w,loc.z, loc.w));
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

float GetShadowClose(float3 lightCoords, PixelInput input, float3 TangentNormal)
{
    float shadow = 0;
    
    float dist = distance(viewPos, input.MyPosition);
    
    
        float currentDepth = lightCoords.z * 2 - 1;

        float resolution = 1;
        

        int numSamples = 2; // Number of samples in each direction (total samples = numSamples^2)

        float b = -0.00005;
        
        float bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;

        float f = abs(dot(input.Normal, LightDirection));

        f*=lerp(f,1,0.5); 

        bias*= lerp(15,1, f);

        resolution = 2048;
        
        bias -= 0.00002;
        
        float size = 0.7;
   
        

        bias *= (LightDistanceMultiplier+1)/2;
        


        //if(abs(dot(input.Normal, -LightDirection)) <= 0.3 && false)
        //return 1 - SampleShadowMap(ShadowMapCloseSampler, lightCoords.xy, currentDepth + bias);

        float texelSize = size / resolution; // Assuming ShadowMapSize is the size of your shadow map texture

        float forceShadow = 0;

        forceShadow = lerp(0, 1, saturate((dot(TangentNormal, LightDirection)+0.4)*(10/4)));
        
        //bias *= lerp(1,5,saturate(forceShadow*1.75));



        //forceShadow*=forceShadow;
        //forceShadow*=forceShadow;
        

        //return 1 - SampleShadowMap(ShadowMapCloseSampler, lightCoords.xy, currentDepth + bias)* (1 - forceShadow);
    

        int n = 0;
        
        for (int i = -numSamples; i <= numSamples; ++i)
        {
            for (int j = -numSamples; j <= numSamples; ++j)
            {

                if(length(float2(i,j)) > numSamples*1.1) continue;

                float2 offsetCoords = lightCoords.xy + float2(i, j) * texelSize;
                float closestDepth;
                closestDepth = SampleShadowMap(ShadowMapCloseSampler, offsetCoords, currentDepth + bias*(lerp(length(float2(i,j)),1,0.5)));

                closestDepth = saturate(closestDepth);

                shadow += closestDepth;

                n++;

            }
        }

        //return saturate(shadow);

        // Normalize the accumulated shadow value
        shadow /= n;
        
        return (1 - shadow) * (1 - shadow);
    
    
}

float GetShadowVeryClose(float3 lightCoords, PixelInput input, float3 TangentNormal)
{
    float shadow = 0;
    
    float dist = distance(viewPos, input.MyPosition);


    if (lightCoords.x >= 0 && lightCoords.x <= 1 && lightCoords.y >= 0 && lightCoords.y <= 1)
    {
        float currentDepth = lightCoords.z * 2 - 1;

        float resolution = 1;
        

        int numSamples = 2; // Number of samples in each direction (total samples = numSamples^2)

        float b = 0.00003;
        
        float bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;

        float f = abs(dot(input.Normal, LightDirection));

        f*=lerp(f,1,0.5); 

        bias*= lerp(12,1, f);

        bias += 0.00016;

        bias *= (LightDistanceMultiplier+1)/2;


        float forceShadow = 0;
        
        //if(Viewmodel == false)
        forceShadow = lerp(0, 1, saturate((dot(TangentNormal, LightDirection)+0.3)*(10/3)));
        
        bias *= lerp(1,2,saturate(forceShadow*1.5));

        resolution = ShadowMapResolutionVeryClose;
        
        //bias -= max(dot(input.Normal, float3(0,1,0)),0) * b/2;
        
        float size = (abs(dot(input.Normal, -LightDirection))-0.5)*2;
        
        size = 1; max(size, 0.001);
        
        float texelSize = size / resolution; // Assuming ShadowMapSize is the size of your shadow map texture
        
        //return 1 - SampleShadowMapLinear(ShadowMapVeryCloseSampler, lightCoords.xy, currentDepth - bias, float2(texelSize, texelSize));

        #ifdef SIMPLE_SHADOWS
        return 1 - SampleShadowMapLinear(ShadowMapVeryCloseSampler, lightCoords.xy, currentDepth - bias, float2(texelSize, texelSize));
        #endif


        numSamples = 2;

        if(forceShadow>0)
        numSamples = 1;

        int n = 0;

        for (int i = -numSamples; i <= numSamples; ++i)
        {
            for (int j = -numSamples; j <= numSamples; ++j)
            {

                if(length(float2(i,j))> numSamples*1.1)
                    continue;

                float2 offsetCoords = lightCoords.xy + float2(i, j) * texelSize;
                float closestDepth;
                closestDepth = SampleShadowMapLinear(ShadowMapVeryCloseSampler, offsetCoords, currentDepth - bias, float2(texelSize, texelSize));

                shadow += closestDepth;
                n++;
            }
        }

        // Normalize the accumulated shadow value
        shadow /= n;
        
        return lerp((1 - shadow) * (1 - shadow), 1, 0);
    }
    return 0;
    
}

float GetShadow(float3 lightCoords,float3 lightCoordsClose,float3 lightCoordsVeryClose, PixelInput input, float3 TangentNormal)
{


    float shadow = 0;
    
    if(DirectBrightness == 0)
        return 0;


    float dist = distance(viewPos, input.MyPosition);

    if (dist > 150)
        return 0;
    
        float b = 0.0002;

    

    if (lightCoords.x >= 0 && lightCoords.x <= 1 && lightCoords.y >= 0 && lightCoords.y <= 1 || Viewmodel)
    {
        
        
        float currentDepth = lightCoords.z * 2 - 1;
            
        #if OPENGL
        #else

if (lightCoordsClose.x >= 0 && lightCoordsClose.x <= 1 && lightCoordsClose.y >= 0 && lightCoordsClose.y <= 1)
{
if (lightCoordsVeryClose.x >= 0 && lightCoordsVeryClose.x <= 1 && lightCoordsVeryClose.y >= 0 && lightCoordsVeryClose.y <= 1)

        if(dist>6 && dist<8)
        {
            return lerp(GetShadowVeryClose(lightCoordsVeryClose, input, TangentNormal), GetShadowClose(lightCoordsClose, input, TangentNormal), (dist - 6)/2);
        }


    if(dist>22 && dist<25)
    {
        float close = GetShadowClose(lightCoordsClose, input, TangentNormal);

        float bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;
        bias *= (LightDistanceMultiplier+1)/2;
        float far = 1 - SampleShadowMap(ShadowMapSampler, lightCoords.xy, currentDepth - bias);

        return lerp(close, far, (dist - 22)/3);
        
    }
}
        #endif
        
        if (dist < 7.0)
        {

            

            if (lightCoordsVeryClose.x >= 0 && lightCoordsVeryClose.x <= 1 && lightCoordsVeryClose.y >= 0 && lightCoordsVeryClose.y <= 1)
            {
                return GetShadowVeryClose(lightCoordsVeryClose, input, TangentNormal);
            }
        }
        
        if (dist < 25)
        {
            if (lightCoordsClose.x >= 0 && lightCoordsClose.x <= 1 && lightCoordsClose.y >= 0 && lightCoordsClose.y <= 1)
            {

                //return 1;
                return GetShadowClose(lightCoordsClose, input, TangentNormal);
            }
        }
        
    if (tex2D(ShadowMapSampler,lightCoords.xy).r<0.01)
        return 0;


        float resolution = 1;
        

        int numSamples = 2; // Number of samples in each direction (total samples = numSamples^2)



        float bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;
        resolution = ShadowMapResolution*2;
        
        bias *= (LightDistanceMultiplier+1)/2;

        float f = abs(dot(input.Normal, LightDirection));

        f*=lerp(f,1,0.5); 

        bias*= lerp(10,1, f);

        return 1 - SampleShadowMap(ShadowMapSampler, lightCoords.xy, currentDepth - bias);
        
        float size = 0.7;
        
        
        float texelSize = size / resolution; // Assuming ShadowMapSize is the size of your shadow map texture
        
        for (int i = -numSamples; i <= numSamples; ++i)
        {
            for (int j = -numSamples; j <= numSamples; ++j)
            {
                float2 offsetCoords = lightCoords.xy + float2(i, j) * texelSize;
                float closestDepth;
                closestDepth = SampleShadowMap(ShadowMapSampler, offsetCoords, currentDepth + bias*(lerp(length(float2(i,j)),1,0.5)));

                shadow += closestDepth;

            }
        }

        // Normalize the accumulated shadow value
        shadow /= ((2 * numSamples + 1) * (2 * numSamples + 1));
        
        return (1 - shadow) * (1 - shadow);
    }
    return 0;
    
}

float GetShadowViewmodel(float3 lightCoords, PixelInput input, float3 TangentNormal)
{
    float resolution = 1;
    float shadow = 0;

    float currentDepth = lightCoords.z * 2 - 1;

    int numSamples = 1; // Number of samples in each direction (total samples = numSamples^2)

    float b = -0.0003;
        
    float bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;

    bias*= lerp(3,1, abs(dot(input.Normal, -LightDirection)));

    bias += -0.0005;

    float forceShadow = lerp(0, 1, saturate((dot(TangentNormal, LightDirection)+0.3)*(10/3)));
        
    bias *= lerp(1,1,saturate(forceShadow*1.5));

    resolution = ShadowMapResolution;

    float texelSize = 1 / resolution; // Assuming ShadowMapSize is the size of your shadow map texture

    return 1 - SampleShadowMapLinear(ShadowMapSampler, lightCoords.xy, currentDepth + bias, float2(texelSize, texelSize));
        
    float size = 1;
        
        
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


float3 CalculatePointLight(int i, PixelInput pixelInput, float3 normal, float roughness, float metalic, float3 albedo)
{
    float3 lightVector = LightPositions[i] - pixelInput.MyPosition;
    float distanceToLight = length(lightVector);

    if(distanceToLight> LightRadiuses[i])
        return float3(0,0,0);
    

    if(isParticle)
        normal = normalize(lightVector);

    // Calculate the dot product between the normalized light vector and light direction
    float lightDot = dot(normalize(-lightVector), normalize(LightDirections[i].xyz));

    // Define the inner and outer angles of the spotlight in radians
    float innerConeAngle = LightDirections[i].w;
    float outerConeAngle = innerConeAngle - 0.1; // Adjust this value to control the smoothness

    // Calculate the smooth transition factor using smoothstep
    float dirFactor = smoothstep(outerConeAngle, innerConeAngle, lightDot);


    if(dirFactor<=0.001)
        return 0;

    float offsetScale = 1 / (LightResolutions[i] / 40);// / lerp(distanceToLight,1, 0.7);
    offsetScale *= lerp(abs(dot(normal, normalize(lightVector))), 0.7, 1);
    float notShadow = 1;

    if(dot(normal, normalize(lightVector))<-0.01)
    {
        return float3(0,0,0);
    }

    float distFactor = 1; // 0.96

    distFactor = lerp(distFactor, 1, abs(dot(normal, normalize(lightVector))));

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

        float step = 0.66666666666665;

#if OPENGL
        step = radius;
#endif

            

        bool simpleShadows = false;

#ifdef SIMPLE_SHADOWS
        simpleShadows = true;
#endif

        float bias = -1/LightResolutions[i] * distanceToLight;

        if(simpleShadows)
            step = radius;

        if(simpleShadows&&false || isParticle)
        {

            float shadowDepth = GetPointLightDepth(i, lightDir);
            notShadow = distanceToLight * distFactor + bias < shadowDepth ? 1.0 : 0.0;

        }else
        {

        for (float x = -radius; x <= radius; x+=step)
        {
            for (float y = -radius; y <= radius; y+=step)
            {

                if(length(float2(x,y))>1.1*radius)
                    continue;

                float3 offset = (tangent * x + bitangent * y) * shadowBias * offsetScale;
                float shadowDepth = GetPointLightDepth(i, lightDir + offset);
                shadowFactor += distanceToLight * distFactor + bias < shadowDepth ? 1.0 : 0.0;
                samples++;
            }
        }
        
        shadowFactor /= samples;
        notShadow = shadowFactor;
        }
    }

    float dist = (distanceToLight / LightRadiuses[i]);
    float intense = saturate(1.0 - dist * dist);
    float distIntence = intense;
    float3 dirToSurface = normalize(lightVector);

    intense *= saturate(dot(normal, dirToSurface));
    float3 specular = CalculateSpecular(pixelInput.MyPosition, normal, dirToSurface, roughness, metalic, albedo);


    float colorInstens = abs( max(LightColors[i].x,(max(LightColors[i].y,LightColors[i].z))));

    intense = max(intense, 0) * colorInstens;
    float3 l = LightColors[i] * intense;

    if(dot(l, float3(1,1,1))<0)
        specular = 0;

    return (l + intense * specular) * notShadow * dirFactor;
}

float3 CalculateSsrSpecular(PixelInput input, float3 normal, float roughness, float metalic, float3 albedo)
{
    return float3(0,0,0);
    
    float3 vDir = normalize(input.MyPosition - viewPos);

    float lightDir = -reflect(vDir, normal);

    float intens = CalculateSpecular(input.MyPosition, normal, lightDir, roughness+0.1, metalic, albedo);

    

    float2 screenCoords = input.MyPixelPosition.xyz / input.MyPixelPosition.w;
    
    screenCoords = (screenCoords + 1.0f) / 2.0f;

    screenCoords.y = 1.0f - screenCoords.y;

    float2 texel = 1 / float2(SSRWidth, SSRHeight);

    float3 color = tex2D(ReflectionTextureSampler, screenCoords).rgb - 0.9;

    return saturate(color * intens * dot(lightDir, -normal));
}

float3 CalculateLight(PixelInput input, float3 normal, float roughness, float metallic, float ao, float3 albedo, float3 TangentNormal)
{
    float3 lightCoords = input.lightPos.xyz / input.lightPos.w;
    lightCoords = (lightCoords + 1.0f) / 2.0f;
    lightCoords.y = 1.0f - lightCoords.y;
    
    float3 lightCoordsClose = input.lightPosClose.xyz / input.lightPosClose.w;
    lightCoordsClose = (lightCoordsClose + 1.0f) / 2.0f;
    lightCoordsClose.y = 1.0f - lightCoordsClose.y;
    
    float3 lightCoordsVeryClose = input.lightPosVeryClose.xyz / input.lightPosVeryClose.w;
    lightCoordsVeryClose = (lightCoordsVeryClose + 1.0f) / 2.0f;
    lightCoordsVeryClose.y = 1.0f - lightCoordsVeryClose.y;

    float shadow = 0;

    

    if (isParticle)
        normal = -LightDirection;
    

    if (dot(normal, LightDirection) > 0.01)
    {
        shadow += 1;
    }
    else
    {
        
        #if OPENGL
            shadow += GetShadow(lightCoords, lightCoordsClose, lightCoordsVeryClose, input, TangentNormal);
        #else

        if(Viewmodel && ViewmodelShadowsEnabled)
        {
            shadow += GetShadowVeryClose(lightCoordsVeryClose, input, TangentNormal);
            shadow += GetShadowViewmodel(lightCoords, input, TangentNormal);
        }else
        {
            shadow += GetShadow(lightCoords, lightCoordsClose, lightCoordsVeryClose, input, TangentNormal);
        }

        shadow = saturate(shadow);
        
        #endif
    }
    
    shadow = lerp(shadow, 1, 1 - max(0, dot(normal, normalize(-LightDirection) * 1)));

    
    shadow = saturate(shadow);

    float3 vDir = normalize(viewPos - input.MyPosition);
    float3 lightDir = normalize(-LightDirection);

    // Calculate specular reflection
    float3 specular = CalculateSpecular(input.MyPosition, normal, lightDir, roughness, metallic, albedo) * DirectBrightness;
    specular *= max(1 - shadow, 0);

    //float3 globalSpecularDir = normalize(-normal + float3(0, -5, 0) + LightDirection);
    //specular += CalculateSpecular(input.MyPosition, normal, globalSpecularDir, roughness, metallic, albedo) * 0.02;

    // Direct light contribution
    float3 light = DirectBrightness * GlobalLightColor;
    light *= (1.0f - shadow);


    float3 globalLightColor = lerp(GlobalLightColor, SkyColor, shadow);

    // Global ambient light
    float3 globalLight = GlobalBrightness * globalLightColor * lerp(1.0f, 0.1f, (dot(normal, float3(0,-1,0))+1)/2);
    

    if(Viewmodel)
    {
        //globalLight += GlobalBrightness * lerp(1.0f, 0.1f, (dot(normal, float3(0,-1,0))+1)/2)/3;
    }

    globalLight *= ao;

    light += specular;
    light = max(light, 0.0f);
    light += globalLight;

    // Accumulate point light contributions
    for (int i = 0; i < MAX_POINT_LIGHTS; i++)
    {
        light += CalculatePointLight(i, input, normal, roughness, metallic, albedo);
    }

    // Combine contributions

    //light += CalculateSsrSpecular(input, normal, roughness, metallic, albedo);

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


float ReflectionMapping(float x)
{
    
    const float n = -0.066;
    
    const float v = x / 3;
    
    return v / ((x * 10 + 1 / n)*n);

}

float CalculateReflectiveness(float roughness, float metallic, float3 vDir, float3 normal)
{

    return lerp(0.04, 1, metallic) * (lerp(0.1, 1, (1-roughness)*(1-roughness)));

    // Calculate the base reflectiveness based on metallic
    float baseReflectiveness = metallic;

    // Calculate the Fresnel factor using the Schlick approximation
    float F0 = lerp(0.01, 1, metallic);
    float F = F0 + (1.0 - F0);// * pow(1.0 - abs(dot(vDir, normal)), 5.0);

    // Adjust the base reflectiveness based on roughness
    float reflectiveness = lerp(baseReflectiveness, 0.04, roughness);

    // Modulate reflectiveness by the Fresnel factor
    reflectiveness *= F;

    reflectiveness = saturate(reflectiveness);
    
    
    return ReflectionMapping(saturate(reflectiveness));
}

float CalcLuminance(float3 color)
{
    return dot(color, float3(0.299f, 0.587f, 0.114f));
}

float3 ApplyReflection(float3 inColor, float3 albedo, PixelInput input,float3 normal, float roughness, float metallic)
{
    
    return inColor;/*
    float3 WorldPos = input.MyPosition;
    
    float3 vDir = normalize(input.MyPosition - viewPos);
    
    float3 reflection = reflect(normalize(input.MyPosition - viewPos), normalize(lerp(normal, input.TangentNormal, 0.4)));
    
    
    float4 ssr = SampleSSR(reflection, input.MyPosition, input.MyPixelPosition.z, normal, vDir);
    
    float3 cube = SampleCubemap(ReflectionCubemapSampler, reflection);
    
    float3 reflectionColor = lerp(cube, ssr.rgb, ssr.w);
    

    float reflectiveness = CalculateReflectiveness(roughness, metallic, normal, normal);
    
    reflectiveness = saturate(reflectiveness);
    
    reflectionColor *= lerp(float3(1, 1, 1), albedo, metallic);
    
    return lerp(inColor, reflectionColor, reflectiveness);*/
}

float3 ApplyReflectionOnSurface(float3 color,float3 albedo,float2 screenCoords, float reflectiveness, float metalic)
{

    float3 reflection = tex2D(ReflectionTextureSampler, screenCoords).rgb;

    float2 texel = float2(1/SSRWidth, 1/SSRHeight);


    reflection += tex2D(ReflectionTextureSampler, screenCoords + float2(texel.x,0)).rgb/2;
    reflection += tex2D(ReflectionTextureSampler, screenCoords + float2(-texel.x,0)).rgb/2;
    reflection += tex2D(ReflectionTextureSampler, screenCoords + float2(0,texel.y)).rgb/2;
    reflection += tex2D(ReflectionTextureSampler, screenCoords + float2(0,texel.y)).rgb/2;
    reflection/=4.0/2.0 + 1;

    //reflection = saturate(reflection);

    float lum = 0;// saturate(CalcLuminance(reflection))/30;
    

    float3 reflectionIntens = lerp(0, reflectiveness, metalic);

    return lerp(color, reflection * albedo, reflectiveness);
}