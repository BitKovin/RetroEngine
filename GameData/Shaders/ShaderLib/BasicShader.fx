#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

#include "GraphicsSettings.fx"

#define PI 3.1415f

matrix World;
matrix View;
matrix Projection;
matrix ProjectionViewmodel;

#include "EngineConstants.fx"

texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
	texture = <ShadowMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture ShadowMapClose;
sampler ShadowMapCloseSampler = sampler_state
{
	texture = <ShadowMapClose>;
	MinFilter = Linear;
	MagFilter = Linear;
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

texture FrameTexture; //ssr
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
	MipFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

texture ReflectionCubemap; //ssr
sampler ReflectionCubemapSampler = sampler_state
{
	texture = <ReflectionCubemap>;
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float3 ReflectionCubemapMin;
float3 ReflectionCubemapMax;
float3 ReflectionCubemapPosition;


float DitherDisolve;

float FarPlane;
float3 viewDirForward;
float3 viewDirRight;
float3 viewDirUp;
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

bool Decal;

#ifndef MAX_POINT_LIGHTS

#define MAX_POINT_LIGHTS 20

bool skeletalMesh;

#endif

#ifndef MAX_POINT_LIGHTS_SHADOWS

#define MAX_POINT_LIGHTS_SHADOWS 7

#endif

#ifdef OPENGL
#define MAX_POINT_LIGHTS 6
#endif

int PointLightsNumber;
float4 LightPositions[MAX_POINT_LIGHTS];
float3 LightColors[MAX_POINT_LIGHTS];
float LightRadiuses[MAX_POINT_LIGHTS];
float LightResolutions[MAX_POINT_LIGHTS];
float4 LightDirections[MAX_POINT_LIGHTS];

#define POINT_LIGHT_SAMPLER_PARAMS MinFilter = Linear; MipFilter = Linear; MagFilter = Linear ; MipLODBias = 0; AddressU = Clamp; AddressV = Clamp; AddressW = Clamp;

texture PointLightCubemap1;
sampler2D PointLightCubemap1Sampler = sampler_state
{
	texture = <PointLightCubemap1>;

	POINT_LIGHT_SAMPLER_PARAMS

};

texture PointLightCubemap2;
sampler2D PointLightCubemap2Sampler = sampler_state
{
	texture = <PointLightCubemap2>;
POINT_LIGHT_SAMPLER_PARAMS

};

texture PointLightCubemap3;
sampler2D PointLightCubemap3Sampler = sampler_state
{
	texture = <PointLightCubemap3>;
POINT_LIGHT_SAMPLER_PARAMS
};

texture PointLightCubemap4;
sampler2D PointLightCubemap4Sampler = sampler_state
{
	texture = <PointLightCubemap4>;
POINT_LIGHT_SAMPLER_PARAMS
};

texture PointLightCubemap5;
sampler2D PointLightCubemap5Sampler = sampler_state
{
	texture = <PointLightCubemap5>;
POINT_LIGHT_SAMPLER_PARAMS
};

texture PointLightCubemap6;
sampler2D PointLightCubemap6Sampler = sampler_state
{
	texture = <PointLightCubemap6>;
POINT_LIGHT_SAMPLER_PARAMS
};
texture PointLightCubemap7;
sampler2D PointLightCubemap7Sampler = sampler_state
{
	texture = <PointLightCubemap7>;
POINT_LIGHT_SAMPLER_PARAMS
};
texture PointLightCubemap8;
sampler2D PointLightCubemap8Sampler = sampler_state
{
	texture = <PointLightCubemap8>;
POINT_LIGHT_SAMPLER_PARAMS
};
texture PointLightCubemap9;
sampler2D PointLightCubemap9Sampler = sampler_state
{
	texture = <PointLightCubemap9>;
POINT_LIGHT_SAMPLER_PARAMS
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

bool LargeObject;

bool Masked;

struct VertexInput
{
	float4 Position : SV_POSITION0;
	float3 Normal : NORMAL0;

	float3 SmoothNormal : NORMAL1;

	float2 TexCoord : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 BiTangent : BINORMAL0;

	half4 BlendIndices : BLENDINDICES0;
	half4 BlendWeights : BLENDWEIGHT0;

	float4 Color : COLOR0;

};

struct PixelInput //only color and texcoords or opengl might freak out
{
	float4 Position : SV_POSITION;
	half2 TexCoord : TEXCOORD0;
	half3 Normal : TEXCOORD8;
	float3 MyPosition : TEXCOORD3;
	half4 MyPixelPosition : TEXCOORD4;
	half3 Tangent : TEXCOORD5;
	half3 BiTangent : TEXCOORD7;
	half4 Color : COLOR0;
	half4 Light : COLOR1;
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

half4x4 GetBoneTransforms(VertexInput input)
{

	half4x4 identity = half4x4(
		1.0, 0.0, 0.0, 0.0,
		0.0, 1.0, 0.0, 0.0,
		0.0, 0.0, 1.0, 0.0,
		0.0, 0.0, 0.0, 1.0);

	half sum = input.BlendWeights.x + input.BlendWeights.y + input.BlendWeights.z + input.BlendWeights.w;

	if (sum < 0.05f)
		return identity;

	half4x4 mbones =
		Bones[input.BlendIndices.x] * (half)input.BlendWeights.x / sum +
		Bones[input.BlendIndices.y] * (half)input.BlendWeights.y / sum +
		Bones[input.BlendIndices.z] * (half)input.BlendWeights.z / sum +
		Bones[input.BlendIndices.w] * (half)input.BlendWeights.w / sum;

	return mbones;
}

half3 ApplyNormalTexture(half3 sampledNormalColor, half3 worldNormal, half3 worldTangent, half3 bitangent)
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

half3 GetTangentNormal(float3 worldNormal, float3 worldTangent, float3 bitangent)
{
	return ApplyNormalTexture(float3(0.5, 0.5, 1), worldNormal, worldTangent, bitangent);
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


	output.Tangent = mul(input.Tangent, (float3x3)BonesWorld);
	output.Tangent = normalize(output.Tangent);

	output.BiTangent = mul(input.BiTangent, (float3x3)BonesWorld);
	output.BiTangent = normalize(output.BiTangent);

	output.Light = float4(0,0,0,0);

	//output.lightPos = mul(worldPos, ShadowMapViewProjection);
	//output.lightPosClose = mul(worldPos, ShadowMapViewProjectionClose);
	//output.lightPosVeryClose = mul(worldPos, ShadowMapViewProjectionVeryClose);

	output.TexCoord = input.TexCoord;
	output.Color = input.Color;

	return output;
}

void DepthDiscard(float depth, PixelInput input)
{

	float b = 0.005;
	if (Viewmodel)
		b = 0.000004f;

	if (depth < input.MyPixelPosition.z - b && depthTestEqual == false)
		discard;
}

float SampleDepth(float2 coords)
{
	return tex2D(DepthTextureSampler, coords);

}

void MaskedDiscard(float alpha)
{
	if (alpha < 0.95 && Masked)
		discard;
}

float SampleMaxDepth(float2 screenCoords)
{

	float2 texelSize = 2 / float2(ScreenWidth, ScreenHeight);

	float d = SampleDepth(screenCoords);
	float d1 = SampleDepth(screenCoords + texelSize);
	float d2 = SampleDepth(screenCoords - texelSize);
	float d3 = SampleDepth(screenCoords + texelSize * float2(1, -1));
	float d4 = SampleDepth(screenCoords - texelSize * float2(-1, 1));

	return max(d, max(d1, max(d2, max(d3, d4))));

}

float SampleMinDepth(float2 screenCoords)
{

	float2 texelSize = 2 / float2(ScreenWidth, ScreenHeight);

	float d = SampleDepth(screenCoords);
	float d1 = SampleDepth(screenCoords + texelSize);
	float d2 = SampleDepth(screenCoords - texelSize);
	float d3 = SampleDepth(screenCoords + texelSize * float2(1, -1));
	float d4 = SampleDepth(screenCoords - texelSize * float2(-1, 1));

	return min(d, min(d1, min(d2, min(d3, d4))));

}

float Gaussian(const float x, const float y, const float sigma) {
	return exp(-((x * x + y * y) / (2.0 * sigma * sigma))) / (2.0 * 3.141 * sigma * sigma);
}

float Gaussian(const float3 v, const float sigma) {
	return exp(-((v.x * v.x + v.y * v.y + v.z * v.z) / (2.0 * sigma * sigma))) / (2.0 * 3.141 * sigma * sigma);
}

half4 SampleCubemap(samplerCUBE s, float3 coords)
{
	return texCUBE(s, coords * float3(-1, 1, 1));
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


half GeometrySchlickGGX(float NdotV, float roughness)
{
	float r = roughness + 1.0f;
	float k = (r * r) / 8.0f;

	float num = NdotV;
	float denom = NdotV * (1.0f - k) + k;

	return num / denom;
}

half GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
	float NdotV = max(dot(N, V), 0.0f);
	float NdotL = max(dot(N, L), 0.0f);
	float ggx2 = GeometrySchlickGGX(NdotV, roughness);
	float ggx1 = GeometrySchlickGGX(NdotL, roughness);

	return ggx1 * ggx2;
}

half3 FresnelSchlick(float cosTheta, float3 F0)
{
	return F0 + (1.0f - F0) * pow(1.0f - cosTheta, 5.0f);
}

half DistributionGGX(float3 N, float3 H, float a)
{
	float a2 = a * a;
	float NdotH = max(dot(N, H), 0.0);
	float NdotH2 = NdotH * NdotH;

	float nom = a2;
	float denom = (NdotH2 * (a2 - 1.0) + 1.0);
	denom = PI * denom * denom;

	return nom / denom;
}

half CalculateSpecular(float3 worldPos, half3 normal, half3 lightDir, half roughness, half metallic, float3 albedo)
{
#ifdef NO_SPECULAR
    return 0;
#endif

    // Avoid light behind the surface
    if (dot(normal, lightDir) < 0)
        return 0;

    // View direction
    half3 vDir = normalize(viewPos - worldPos);

    // Light direction
    lightDir = normalize(lightDir);
    
    // Halfway vector
    half3 halfwayDir = normalize(vDir + lightDir);

    // NdotH, NdotV, NdotL calculations (saturated to avoid negative values)
    half NdotH = saturate(dot(normal, halfwayDir));
    half NdotV = saturate(dot(normal, vDir));
    half NdotL = saturate(dot(normal, lightDir));

    // GGX Normal Distribution Function (NDF)
    half roughnessSq = roughness * roughness;
    half D = DistributionGGX(normal, halfwayDir, roughnessSq);

    // Geometry function using Smith's method
    half G = GeometrySmith(normal, vDir, lightDir, roughnessSq);

    // Fresnel-Schlick approximation (with energy conservation)
    half F0 = lerp(0.04, 1.0, metallic);  // F0 is 0.04 for dielectrics, 1.0 for metals
    half3 F = F0 + (1.0 - F0) * pow(1.0 - NdotV, 5.0);  // Fresnel-Schlick approximation

    // Specular BRDF (Fresnel * NDF * Geometry) / (4 * NdotV * NdotL)
    half3 specular = (F * D * G) / max(4.0 * NdotV * NdotL, 0.001f);  // Avoid divide by zero
    
    // Energy conservation factor: Scale specular to avoid overwhelming brightness
    half energyFactor = saturate(1.0 - roughness * 0.5);  // More roughness = less specular
    specular *= energyFactor;

    // Cap the specular contribution to prevent brightness overload
    return specular * 0.01;
}


float3 offset_lookup(sampler2D map, float4 loc, float2 offset, float texelSize)
{
	return tex2Dproj(map, float4(loc.xy + offset * texelSize * loc.w, loc.z, loc.w));
}

half SampleShadowMap(sampler2D shadowMap, float2 coords, float compare)
{

	float4 sample = tex2D(shadowMap, coords);

	return step(compare, sample.r);
}

half SampleShadowMapLinear(sampler2D shadowMap, float2 coords, float compare, half2 texelSize)
{
	float2 pixelPos = coords / texelSize + float2(0.5, 0.5);
	float2 fracPart = frac(pixelPos);
	float2 startTexel = (pixelPos - fracPart) * texelSize;

	half blTexel = SampleShadowMap(shadowMap, startTexel, compare);
	half brTexel = SampleShadowMap(shadowMap, startTexel + half2(texelSize.x, 0.0), compare);
	half tlTexel = SampleShadowMap(shadowMap, startTexel + half2(0.0, texelSize.y), compare);
	half trTexel = SampleShadowMap(shadowMap, startTexel + texelSize, compare);

	half mixA = lerp(blTexel, tlTexel, fracPart.y);
	half mixB = lerp(brTexel, trTexel, fracPart.y);

	return lerp(mixA, mixB, fracPart.x);
}

float GetShadowClose(float3 lightCoords, PixelInput input, float3 TangentNormal)
{
	float shadow = 0;

	float dist = distance(viewPos, input.MyPosition);


	float currentDepth = lightCoords.z * 2 - 1 - DIRECTIONAL_LIGHT_DEPTH_OFFSET;

	float resolution = 1;


	int numSamples = 1; // Number of samples in each direction (total samples = numSamples^2)

	float b = -0.00005;

	float bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;

	float f = abs(dot(input.Normal, LightDirection));

	f *= lerp(f, 1, 0.5);

	bias *= lerp(15, 1, f);

	resolution = ShadowMapResolutionClose;

	bias -= 0.00002;

	float size = 1;



	bias *= (LightDistanceMultiplier + 1) / 2;



	//if(abs(dot(input.Normal, -LightDirection)) <= 0.3 && false)
	//return 1 - SampleShadowMap(ShadowMapCloseSampler, lightCoords.xy, currentDepth + bias);

	float texelSize = size / resolution; // Assuming ShadowMapSize is the size of your shadow map texture

	float forceShadow = 0;

	forceShadow = 0;// lerp(0, 1, saturate((dot(TangentNormal, LightDirection) + 0.4) * (10 / 4)));

	//bias *= lerp(1,5,saturate(forceShadow*1.75));



	//forceShadow*=forceShadow;
	//forceShadow*=forceShadow;


	//return 1 - SampleShadowMap(ShadowMapCloseSampler, lightCoords.xy, currentDepth + bias)* (1 - forceShadow);


	int n = 0;

	for (int i = -numSamples; i <= numSamples; ++i)
	{
		for (int j = -numSamples; j <= numSamples; ++j)
		{

			float2 offsetCoords = lightCoords.xy + float2(i, j) * texelSize;
			float closestDepth;
			//closestDepth = SampleShadowMap(ShadowMapCloseSampler, offsetCoords / float2(2, 1), currentDepth + bias * (lerp(length(float2(i, j)), 1, 0.5)));
			closestDepth = SampleShadowMapLinear(ShadowMapCloseSampler, offsetCoords / float2(2, 1), currentDepth + bias * (lerp(length(float2(i, j)), 1, 0.5)), float2(texelSize / 2, texelSize)/size);

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
		float currentDepth = lightCoords.z * 2 - 1 - DIRECTIONAL_LIGHT_DEPTH_OFFSET;

		float resolution = 1;


		int numSamples = 2; // Number of samples in each direction (total samples = numSamples^2)

		float b = 0.00001;

		float bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;

		float f = abs(dot(input.Normal, LightDirection));

		f *= lerp(f, 1, 0.5);

		bias *= lerp(12, 1, f);

		//bias += 0.00016;

		bias *= (LightDistanceMultiplier + 1) / 2;


		float forceShadow = 0;

		//if(Viewmodel == false)
		forceShadow = lerp(0, 1, saturate((dot(TangentNormal, LightDirection) + 0.3) * (10 / 3)));

		bias *= lerp(1, 2, saturate(forceShadow * 1.5));

		resolution = ShadowMapResolutionVeryClose;

		//bias -= max(dot(input.Normal, float3(0,1,0)),0) * b/2;

		float size = (abs(dot(input.Normal, -LightDirection)) - 0.5) * 2;

		size = 1; max(size, 0.001);

		float texelSize = size / resolution; // Assuming ShadowMapSize is the size of your shadow map texture

		//return 1 - SampleShadowMapLinear(ShadowMapVeryCloseSampler, lightCoords.xy, currentDepth - bias, float2(texelSize, texelSize));

#ifdef SIMPLE_SHADOWS
		return 1 - SampleShadowMapLinear(ShadowMapCloseSampler, lightCoords.xy / float2(2, 1) + float2(0.5, 0), currentDepth - bias, float2(texelSize / 2, texelSize));
#endif



		numSamples = 2;

		if (forceShadow > 0)
			numSamples = 1;

		int n = 0;

		for (int i = -numSamples; i <= numSamples; ++i)
		{
			for (int j = -numSamples; j <= numSamples; ++j)
			{

				if (length(float2(i, j)) > numSamples * 1.1)
					continue;

				float2 offsetCoords = lightCoords.xy + float2(i, j) * texelSize;
				float closestDepth;

				if(DirectionalLightShadowQuality<3)
				{
					closestDepth = SampleShadowMap(ShadowMapCloseSampler, offsetCoords / float2(2, 1) + float2(0.5, 0), currentDepth - bias);
					
				}else
				{
					closestDepth = SampleShadowMapLinear(ShadowMapCloseSampler, offsetCoords / float2(2, 1) + float2(0.5, 0), currentDepth - bias, float2(texelSize / 2, texelSize));
				}
					

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

float GetShadow(float3 lightCoords, float3 lightCoordsClose, float3 lightCoordsVeryClose, PixelInput input, float3 TangentNormal)
{


	float shadow = 0;

	if (DirectBrightness == 0)
		return 1;

if(DirectionalLightShadowQuality<1)
	return 0;

	float dist = distance(viewPos, input.MyPosition);

	if (dist > 160)
		return 0;

	float b = 0.0001;



	if (lightCoords.x >= 0 && lightCoords.x <= 1 && lightCoords.y >= 0 && lightCoords.y <= 1 || Viewmodel)
	{


		float currentDepth = lightCoords.z * 2 - 1 - DIRECTIONAL_LIGHT_DEPTH_OFFSET;

#if OPENGL
#else

		
		if (lightCoordsClose.x >= 0 && lightCoordsClose.x <= 1 && lightCoordsClose.y >= 0 && lightCoordsClose.y <= 1)
		{

			if(DirectionalLightShadowQuality>1)
			if (lightCoordsVeryClose.x >= 0 && lightCoordsVeryClose.x <= 1 && lightCoordsVeryClose.y >= 0 && lightCoordsVeryClose.y <= 1)

				if (dist > 7 && dist < 10)
				{
					
					return lerp(GetShadowVeryClose(lightCoordsVeryClose, input, TangentNormal), GetShadowClose(lightCoordsClose, input, TangentNormal), (dist - 7) / 3);
				}


			if (dist > 26 && dist < 30 && false)
			{
				float close = GetShadowClose(lightCoordsClose, input, TangentNormal);

				float bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;
				bias *= (LightDistanceMultiplier + 1) / 2;
				float far = 1 - SampleShadowMap(ShadowMapSampler, lightCoords.xy, currentDepth - bias);

				return lerp(close, far, (dist - 26) / 4);

			}
		}
#endif

		if(DirectionalLightShadowQuality>1)
		if (dist < 8.0)
		{



			if (lightCoordsVeryClose.x >= 0 && lightCoordsVeryClose.x <= 1 && lightCoordsVeryClose.y >= 0 && lightCoordsVeryClose.y <= 1)
			{
				return GetShadowVeryClose(lightCoordsVeryClose, input, TangentNormal);
			}
		}

		if (dist < 27)
		{
			if (lightCoordsClose.x >= 0 && lightCoordsClose.x <= 1 && lightCoordsClose.y >= 0 && lightCoordsClose.y <= 1)
			{

				//return 1;
				return GetShadowClose(lightCoordsClose, input, TangentNormal);
			}
		}

		if ((tex2D(ShadowMapSampler, lightCoords.xy).r + DIRECTIONAL_LIGHT_DEPTH_OFFSET) < 0.01)
			return 0;


		float resolution = 1;


		int numSamples = 1; // Number of samples in each direction (total samples = numSamples^2)



		half bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;
		resolution = ShadowMapResolution;

		bias *= (LightDistanceMultiplier + 1) / 2;

		half f = abs(dot(input.Normal, LightDirection));

		f *= lerp(f, 1, 0.5);

		bias *= lerp(10, 1, f);

		return 1 - SampleShadowMap(ShadowMapSampler, lightCoords.xy, currentDepth - bias);

		float size = 0.4;


		float texelSize = size / resolution; // Assuming ShadowMapSize is the size of your shadow map texture

		for (int i = -numSamples; i <= numSamples; ++i)
		{
			for (int j = -numSamples; j <= numSamples; ++j)
			{
				float2 offsetCoords = lightCoords.xy + float2(i, j) * texelSize;
				float closestDepth;
				closestDepth = SampleShadowMap(ShadowMapSampler, offsetCoords, currentDepth + bias * (lerp(length(float2(i, j)), 1, 0.5)));

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
	half shadow = 0;

	float currentDepth = lightCoords.z * 2 - 1 - DIRECTIONAL_LIGHT_DEPTH_OFFSET;
	currentDepth -= DIRECTIONAL_LIGHT_DEPTH_OFFSET;

	int numSamples = 1; // Number of samples in each direction (total samples = numSamples^2)

	half b = -0.0003;

	half bias = b * (1 - saturate(dot(input.Normal, -LightDirection))) + b / 2.0f;

	bias *= lerp(3, 1, abs(dot(input.Normal, -LightDirection)));

	bias += -0.0005;

	half forceShadow = lerp(0, 1, saturate((dot(TangentNormal, LightDirection) + 0.3) * (10 / 3)));

	bias *= lerp(1, 1, saturate(forceShadow * 1.5));

	resolution = ShadowMapResolution;

	half texelSize = 1 / resolution; // Assuming ShadowMapSize is the size of your shadow map texture

	return 1 - SampleShadowMapLinear(ShadowMapSampler, lightCoords.xy, currentDepth + bias, float2(texelSize, texelSize));

	half size = 1;


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

float2 GetCubeSampleCoordinate(float3 vec3)
{
	float2 texCoord;
	int slice = 0;


    float3 absVec = abs(vec3);

	vec3 *= -1;
	vec3.y *= -1;

	const float sixth = 1.0f / 3.0f;

	if (absVec.x >= absVec.y && absVec.x >= absVec.z)
	{
		if (vec3.x > 0) //Positive X
		{
			texCoord = vec3.zy / vec3.x;
			slice = 0;
		}
		else
		{
			vec3.y *= -1;
			texCoord = vec3.zy / vec3.x;
			slice = 1;
		}
	}
    else if (absVec.z >= absVec.y && absVec.z >= absVec.x)
	{
		if (vec3.z > 0) //Positive Z
		{
            vec3.x *= -1;
			texCoord = vec3.xy / vec3.z;
			slice = 4;
		}
		else
		{
            vec3.xy*=-1;
			texCoord = vec3.xy / vec3.z;
			slice = 5;
		}
	}else if (absVec.y >= abs(vec3.x) && absVec.y >= abs(vec3.z))
	{

        vec3.y*=-1;

		if (vec3.y > 0) //Positive X
		{
            vec3.x *= -1;
			texCoord = vec3.xz / vec3.y;
			slice = 2;
		}
		else
		{
			texCoord = vec3.xz / vec3.y;
			slice = 3;
		}
	}

	texCoord += 1;
	texCoord /= 2;

    texCoord.x/=2;

	int addSlice = 0;

    if(slice>2)
    {

        slice -=3;
        texCoord.x +=0.5;

    }

	texCoord.y *= sixth;
	texCoord += float2(0, slice) * sixth;

	return texCoord;
}

float SamplePointLightCubemap(sampler2D s, float2 coords, float depth)
{
	return step(depth, tex2D(s, coords).r);
}

float2 SnapToSlice(float2 coord, int slice, float texelSize)
{

	texelSize.x *= 2;

	float2 slices;
	if(slice > 2)
	{
		slices = float2(1, slice - 3);
	}else
	{
		slices = float2(0, slice);
	}

    //return coord;
    coord.y*=3;
	coord.x*=2;

    coord-=slices;

    coord = clamp(coord,texelSize,1-texelSize);

    coord+=slices;

	coord.x/=2;
    coord.y/=3;

    return coord;
}

half SamplePointShadowMapLinear(sampler2D shadowMap, float2 coords, float compare, float2 texelSize)
{

    texelSize = 0.5/texelSize;
    texelSize.y/=1.5;

    int slice = floor(coords.y*3);
	if(coords.x>0.5)
		slice += 3;


	float2 pixelPos = coords / texelSize + float2(0.5, 0.5);
	float2 fracPart = frac(pixelPos);
	float2 startTexel = (pixelPos - fracPart) * texelSize;

    float2 blCoord =  startTexel;
    float2 brCoord = startTexel + half2(texelSize.x, 0.0);
    float2 tlCoord = startTexel + half2(0.0, texelSize.y);
    float2 trCoord = startTexel + texelSize;

	blCoord = SnapToSlice(blCoord, slice,texelSize.x);
    brCoord = SnapToSlice(brCoord, slice,texelSize.x);
    tlCoord = SnapToSlice(tlCoord, slice,texelSize.x);
    trCoord = SnapToSlice(trCoord, slice,texelSize.x);


	half blTexel = SamplePointLightCubemap(shadowMap, blCoord, compare);
	half brTexel = SamplePointLightCubemap(shadowMap, brCoord, compare);
	half tlTexel = SamplePointLightCubemap(shadowMap, tlCoord, compare);
	half trTexel = SamplePointLightCubemap(shadowMap, trCoord, compare);

	half mixA = lerp(blTexel, tlTexel, fracPart.y);
	half mixB = lerp(brTexel, trTexel, fracPart.y);

	return lerp(mixA, mixB, fracPart.x);
}

half SamplePointShadowMap(sampler2D shadowMap, float2 coords, float compare, float2 texelSize)
{

    texelSize = 0.5/texelSize;
    texelSize.y/=1.5;

    int slice = floor(coords.y*3);
	if(coords.x>0.5)
		slice += 3;


	float2 pixelPos = coords / texelSize;
	float2 fracPart = frac(pixelPos);
	float2 startTexel = (pixelPos - fracPart) * texelSize;

    float2 blCoord =  startTexel;

	blCoord = SnapToSlice(blCoord, slice,texelSize.x);


	half blTexel = SamplePointLightCubemap(shadowMap, blCoord, compare);

	return blTexel;
}

float GetPointLightDepthLinear(int i, float3 lightDir, float d)
{
	if (i >= MAX_POINT_LIGHTS_SHADOWS)
		return 1;

	float depth = 0.00;

	lightDir *= float3(1, 1, 1);

	float2 sampleCoords = GetCubeSampleCoordinate(lightDir);

	//lightDir = normalize(lightDir);

	if (i == 0)
		depth = SamplePointShadowMapLinear(PointLightCubemap1Sampler, sampleCoords, d, LightResolutions[i]);
	else if (i == 1)
		depth = SamplePointShadowMapLinear(PointLightCubemap2Sampler, sampleCoords, d, LightResolutions[i]);
	else if (i == 2)
		depth = SamplePointShadowMapLinear(PointLightCubemap3Sampler, sampleCoords, d, LightResolutions[i]);
	else if (i == 3)
		depth = SamplePointShadowMapLinear(PointLightCubemap4Sampler, sampleCoords, d, LightResolutions[i]);
	else if (i == 4)
		depth = SamplePointShadowMapLinear(PointLightCubemap5Sampler, sampleCoords, d, LightResolutions[i]);
	else if (i == 5)
		depth = SamplePointShadowMapLinear(PointLightCubemap6Sampler, sampleCoords, d, LightResolutions[i]);
	else if (i == 6)
		depth = SamplePointShadowMapLinear(PointLightCubemap7Sampler, sampleCoords, d, LightResolutions[i]);
	else if (i == 7)
		depth = SamplePointShadowMapLinear(PointLightCubemap8Sampler, sampleCoords, d, LightResolutions[i]);
	else if (i == 8)
		depth = SamplePointShadowMapLinear(PointLightCubemap9Sampler, sampleCoords, d, LightResolutions[i]);



	//depth += depth / (LightResolutions[i] * 3) + 0.04;

	return depth;
}

float GetPointLightDepth(int i, float3 lightDir, float d)
{
	if (i >= MAX_POINT_LIGHTS_SHADOWS)
		return 1;

	float2 sampleCoords = GetCubeSampleCoordinate(lightDir);
	int slice = floor(sampleCoords.y*3);

	if(sampleCoords.x>0.5)
		slice += 3;

	float2 texelSize = LightResolutions[i];

	texelSize = 0.5/texelSize;
    texelSize.y/=1.5;

	float2 pixelPos = sampleCoords / texelSize;
	float2 fracPart = frac(pixelPos);
	float2 startTexel = (pixelPos - fracPart) * texelSize;

    float2 blCoord =  startTexel;

	blCoord = SnapToSlice(blCoord, slice,texelSize.x);
	sampleCoords=blCoord;

	if (i == 0)
		return SamplePointLightCubemap(PointLightCubemap1Sampler, sampleCoords, d);
	else if (i == 1)
		return SamplePointLightCubemap(PointLightCubemap2Sampler, sampleCoords, d);
	else if (i == 2)
		return SamplePointLightCubemap(PointLightCubemap3Sampler, sampleCoords, d);
	else if (i == 3)
		return SamplePointLightCubemap(PointLightCubemap4Sampler, sampleCoords, d);
	else if (i == 4)
		return SamplePointLightCubemap(PointLightCubemap5Sampler, sampleCoords, d);
	else if (i == 5)
		return SamplePointLightCubemap(PointLightCubemap6Sampler, sampleCoords, d);
	else if (i == 6)
		return SamplePointLightCubemap(PointLightCubemap7Sampler, sampleCoords, d);
	else if (i == 7)
		return SamplePointLightCubemap(PointLightCubemap8Sampler, sampleCoords, d);
	else if (i == 8)
		return SamplePointLightCubemap(PointLightCubemap9Sampler, sampleCoords, d);
	//depth += depth / (LightResolutions[i] * 3) + 0.04;

	return 1;
}
/*
sampler2D GetPointLightSampler(int i)
{
	if (i >= MAX_POINT_LIGHTS_SHADOWS)
		return PointLightCubemap1Sampler;

	//lightDir = normalize(lightDir);

	if (i == 0)
		return PointLightCubemap1Sampler;
	else if (i == 1)
		return PointLightCubemap2Sampler;
	else if (i == 2)
		return PointLightCubemap3Sampler;
	else if (i == 3)
		return PointLightCubemap4Sampler;
	else if (i == 4)
		return PointLightCubemap5Sampler;
	else if (i == 5)
		return PointLightCubemap6Sampler;
	else if (i == 6)
		return PointLightCubemap7Sampler;
	else if (i == 7)
		return PointLightCubemap8Sampler;
	else if (i == 8)
		return PointLightCubemap9Sampler;
	//depth += depth / (LightResolutions[i] * 3) + 0.04;

	return PointLightCubemap1Sampler;
}
*/

float SamplePointLightPCFSample(sampler2D s ,int i, float3 tangent, float3 bitangent, float3 lightDir, float distanceToLight, float bias, bool smooth)
{

    float shadowFactor = 0;
    float weightSum = 0;

    float offsetSize = (1/(LightResolutions[i]))*2;



#if OPENGL

	smooth = false;

#endif

#ifdef SIMPLE_SHADOWS
	smooth = false;
#endif


    for (float x = -1; x <= 1; x += 1)
			{
				for (float y = -1; y <= 1; y += 1)
				{


					float weight = 1;

					float3 offset = (tangent * x + bitangent * y)*offsetSize;

                    float2 TextureCoordinates = GetCubeSampleCoordinate(lightDir + offset);



                    if(smooth)
                    {
                        shadowFactor += SamplePointShadowMapLinear(s,TextureCoordinates, distanceToLight + bias*lerp(1,2,length(float2(x,y))), LightResolutions[i]);
                    }
                    else
                    {

						int slice = floor(TextureCoordinates.y*3);

						if(TextureCoordinates.x>0.5)
							slice += 3;

						float2 texelSize = LightResolutions[i];

			    		texelSize = 0.5/texelSize;
    					texelSize.y/=1.5;

						float2 pixelPos = TextureCoordinates / texelSize;
						float2 fracPart = frac(pixelPos);
						float2 startTexel = (pixelPos - fracPart) * texelSize;

    					float2 blCoord =  startTexel;

						blCoord = SnapToSlice(blCoord, slice,texelSize.x);
						TextureCoordinates=blCoord;

                        shadowFactor += SamplePointLightCubemap(s, TextureCoordinates, distanceToLight + bias*lerp(1,2,length(float2(x,y))));
                    }
					

					weightSum += weight;
				}

			}


    return shadowFactor/=weightSum;

            
}

float SamplePointLightPCF(int i, float3 tangent, float3 bitangent, float3 lightDir, float distanceToLight, float bias, bool smooth)
{
	if (i >= MAX_POINT_LIGHTS_SHADOWS)
		return 1;

	float2 sampleCoords = GetCubeSampleCoordinate(lightDir);

	//lightDir = normalize(lightDir);

	if (i == 0)
		return SamplePointLightPCFSample(PointLightCubemap1Sampler, i, tangent, bitangent, lightDir, distanceToLight, bias, smooth);
	else if (i == 1)
		return SamplePointLightPCFSample(PointLightCubemap2Sampler, i, tangent, bitangent, lightDir, distanceToLight, bias, smooth);
	else if (i == 2)
		return SamplePointLightPCFSample(PointLightCubemap3Sampler, i, tangent, bitangent, lightDir, distanceToLight, bias, smooth);
	else if (i == 3)
		return SamplePointLightPCFSample(PointLightCubemap4Sampler, i, tangent, bitangent, lightDir, distanceToLight, bias, smooth);
	else if (i == 4)
		return SamplePointLightPCFSample(PointLightCubemap5Sampler, i, tangent, bitangent, lightDir, distanceToLight, bias, smooth);
	else if (i == 5)
		return SamplePointLightPCFSample(PointLightCubemap6Sampler, i, tangent, bitangent, lightDir, distanceToLight, bias, smooth);
	else if (i == 6)
		return SamplePointLightPCFSample(PointLightCubemap7Sampler, i, tangent, bitangent, lightDir, distanceToLight, bias, smooth);
	else if (i == 7)
		return SamplePointLightPCFSample(PointLightCubemap8Sampler, i, tangent, bitangent, lightDir, distanceToLight, bias, smooth);
	else if (i == 8)
		return SamplePointLightPCFSample(PointLightCubemap9Sampler, i, tangent, bitangent, lightDir, distanceToLight, bias, smooth);
	//depth += depth / (LightResolutions[i] * 3) + 0.04;

	return 1;
}


half3 CalculatePointLight(int i, PixelInput pixelInput, half3 normal, half roughness, half metalic, half3 albedo, out float3 SpecularOut)
{

	SpecularOut = 0;

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

	half offsetScale = 1 / (LightResolutions[i] / 30);// / lerp(distanceToLight,1, 0.7);
	offsetScale *= lerp(abs(dot(normal, normalize(lightVector))), 0.6, 1);


	half notShadow = 1;

	if (dot(normal, normalize(lightVector)) < 0)
	{
		return float3(0, 0, 0);
	}

	float distFactor = 1;

	distFactor = lerp(distFactor, 1.02, abs(dot(normal, normalize(lightVector)))*abs(dot(normal, normalize(lightVector))));

    //distFactor *= 500 / LightResolutions[i];

    distFactor = saturate(distFactor);

    distFactor = 1;

	if (LightResolutions[i] > 10 && notShadow > 0)
	{
		float3 lightDir = normalize(lightVector);
		float shadowBias = 0.05;  // Adjust this bias for your specific scene


		float3 tangent = pixelInput.Tangent;
		float3 bitangent = pixelInput.BiTangent;

		// PCF sampling
		int samples = 0;
		float shadowFactor = 0.0;

		const int radius = 1;

		float step = 1;

		bool simpleShadows = false;


#ifdef SIMPLE_SHADOWS

	simpleShadows = true;

#endif

#if OPENGL

	simpleShadows = true;

#endif


		float bias = -5 / LightResolutions[i] * distanceToLight;

        float slopeFactor = abs(dot(pixelInput.Normal, lightDir));
        slopeFactor*=slopeFactor;
        slopeFactor*=slopeFactor;

		bias = lerp(bias, -2 / LightResolutions[i] * distanceToLight, slopeFactor);

		float weightSum = 0;

		bool breakLoop = false;

		float pixelSize = ((1.0f/LightResolutions[i]) * distanceToLight) / distance(viewPos, pixelInput.MyPosition);

		#if OPENGL

		notShadow = 1;// SamplePointLightPCF(i, tangent, bitangent, lightDir, distanceToLight*distFactor, bias, false);
		
		#else

		if(PointLightShadowQuality == 0)
		{
			notShadow = 1;
		}else
        if(pixelSize < 0.0006 || PointLightShadowQuality == 1)
        {

            notShadow = GetPointLightDepthLinear(i, lightDir,distanceToLight*distFactor + bias);

        }else
		if(pixelSize < 0.002 || simpleShadows || PointLightShadowQuality == 2)
        {
            notShadow = SamplePointLightPCF(i, tangent, bitangent, lightDir, distanceToLight*distFactor, bias, false);

			//notShadow = GetPointLightDepthLinear(i, lightDir,distanceToLight*distFactor + bias*1.6);

        }
        else
        {

            notShadow = SamplePointLightPCF(i, tangent, bitangent, lightDir, distanceToLight*distFactor, bias, true);
		}
		
		#endif

	}

	float dist = (distanceToLight / LightRadiuses[i]);
	half intense = saturate(1.0 - dist * dist);
	half distIntence = intense;
	half3 dirToSurface = normalize(lightVector);

	intense *= saturate(dot(normal, dirToSurface));
	half3 specular = CalculateSpecular(pixelInput.MyPosition, normal, dirToSurface, roughness, metalic, albedo);


	half colorInstens = abs(max(LightColors[i].x, (max(LightColors[i].y, LightColors[i].z))));

	intense = max(intense, 0);

    intense = lerp(0,notShadow, intense);

    intense *= colorInstens;
	half3 l = LightColors[i] * intense;

	if (dot(l, half3(1, 1, 1)) < 0)
		specular = 0;

	SpecularOut = distIntence * specular * notShadow * dirFactor;

	return l * dirFactor;
}

float3 CalculateSsrSpecular(PixelInput input, float3 normal, float roughness, float metalic, float3 albedo)
{
	return float3(0, 0, 0);

	float3 vDir = normalize(input.MyPosition - viewPos);

	float lightDir = -reflect(vDir, normal);

	float intens = CalculateSpecular(input.MyPosition, normal, lightDir, roughness + 0.1, metalic, albedo);



	float2 screenCoords = input.MyPixelPosition.xyz / input.MyPixelPosition.w;

	screenCoords = (screenCoords + 1.0f) / 2.0f;

	screenCoords.y = 1.0f - screenCoords.y;

	float2 texel = 1 / float2(SSRWidth, SSRHeight);

	float3 color = tex2D(ReflectionTextureSampler, screenCoords).rgb - 0.9;

	return saturate(color * intens * dot(lightDir, -normal));
}

half3 CalculateLight(PixelInput input, float3 normal, float roughness, float metallic, float ao, float3 albedo, float3 TangentNormal, out float3 Specular)
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

	float3 lightCoordsVeryClose = lightVeryClosePos.xyz / lightVeryClosePos.w;
	lightCoordsVeryClose = (lightCoordsVeryClose + 1.0f) / 2.0f;
	lightCoordsVeryClose.y = 1.0f - lightCoordsVeryClose.y;

	float shadow = 0;



	if (isParticle)
		normal = -LightDirection;


	if (dot(normal, LightDirection) >= 0)
	{
		shadow += 1;
	}
	else
	{

#if OPENGL
		shadow += GetShadow(lightCoords, lightCoordsClose, lightCoordsVeryClose, input, TangentNormal);
#else

		if (Viewmodel && ViewmodelShadowsEnabled)
		{
			if (lightCoordsVeryClose.x > 0 && lightCoordsVeryClose.x < 1 && lightCoordsVeryClose.y>0 && lightCoordsVeryClose.y < 1)
			{
				shadow += GetShadowVeryClose(lightCoordsVeryClose, input, TangentNormal);
			}
			else
			{
				shadow += GetShadowClose(lightCoordsClose, input, TangentNormal);
			}
			shadow += GetShadowViewmodel(lightCoords, input, TangentNormal);
		}
		else
		{
			shadow += GetShadow(lightCoords, lightCoordsClose, lightCoordsVeryClose, input, TangentNormal);
		}

		shadow = saturate(shadow);

#endif
	}

	float shadowed = shadow;

	shadow = lerp(shadow, 1, 1 - max(0, dot(normal, normalize(-LightDirection))));


	//shadow = saturate(shadow);

	float3 vDir = normalize(viewPos - input.MyPosition);
	float3 lightDir = normalize(-LightDirection);

	// Calculate specular reflection


	//float3 globalSpecularDir = normalize(-normal + float3(0, -5, 0) + LightDirection);
	//specular += CalculateSpecular(input.MyPosition, normal, globalSpecularDir, roughness, metallic, albedo) * 0.02;

	// Direct light contribution
	float3 light = DirectBrightness * GlobalLightColor;
	light *= (1.0f - shadow);


	float3 globalLightColor = lerp(GlobalLightColor, SkyColor, shadowed);

	// Global ambient light
	float3 globalLight = GlobalBrightness * globalLightColor * lerp(1.0f, 0.1f, (dot(normal, float3(0, -1, 0)) + 1) / 2);

	float3 specular = CalculateSpecular(input.MyPosition, normal, lightDir, roughness, metallic, albedo) * DirectBrightness * globalLightColor;
	specular *= max(1 - shadowed, 0);

	if (Viewmodel)
	{
		//globalLight += GlobalBrightness * lerp(1.0f, 0.1f, (dot(normal, float3(0,-1,0))+1)/2)/3;
	}

	globalLight *= ao;

	//light += specular;
	light = max(light, 0.0f);
	light += globalLight;

	// Accumulate point light contributions
	for (int i = 0; i < MAX_POINT_LIGHTS; i++)
	{

		float3 spec = 0;
		
		light += CalculatePointLight(i, input, normal, roughness, metallic, albedo, spec);

		specular += spec;

	}

	// Combine contributions

	//light += CalculateSsrSpecular(input, normal, roughness, metallic, albedo);

	Specular = specular;

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

float3 GetPosition(float2 uv, float depth)
{
	float4 position = 1.0f;

	// Convert screen space UV to normalized device coordinates (NDC)
	float4 ndc = float4(uv * 2.0 - 1.0, depth, 1.0);

	// Transform NDC to world space using the inverse view projection matrix
	float4 worldPos = mul(ndc, InverseViewProjection);

	// Perform perspective divide
	worldPos /= worldPos.w;

	return worldPos.xyz;

	return position.xyz;
}


float ReflectionMapping(float x)
{

	const float n = -0.066;

	const float v = x / 3;

	return v / ((x * 10 + 1 / n) * n);

}

float CalculateReflectiveness(float roughness, float metallic, float3 vDir, float3 normal)
{

	return lerp(0.04, 1, metallic) * (lerp(0.2, 1, (1 - roughness) * (1 - roughness)));

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

float3 ApplyReflection(float3 inColor, float3 albedo, PixelInput input, float3 normal, float roughness, float metallic)
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

float3 ApplyReflectionOnSurface(float3 color, float3 albedo, float2 screenCoords, float reflectiveness, float metalic, float roughness)
{

	float3 reflection = tex2D(ReflectionTextureSampler, screenCoords).rgb;

	float2 texel = roughness / float2(SSRWidth, SSRHeight) * 1.5;


	reflection += tex2D(ReflectionTextureSampler, screenCoords + float2(texel.x, 0)).rgb / 2;
	reflection += tex2D(ReflectionTextureSampler, screenCoords + float2(-texel.x, 0)).rgb / 2;
	reflection += tex2D(ReflectionTextureSampler, screenCoords + float2(0, texel.y)).rgb / 2;
	reflection += tex2D(ReflectionTextureSampler, screenCoords + float2(0, texel.y)).rgb / 2;
	reflection /= 4.0 / 2.0 + 1;

	//reflection = saturate(reflection);

	float lum = 0;// saturate(CalcLuminance(reflection))/30;

	float3 reflectionIntens = lerp(0, reflectiveness, metalic);

	return lerp(color, reflection * albedo, reflectiveness);
}

// Function to generate a random float based on the surface coordinates
float Random(float2 uv)
{
	return frac(sin(dot(uv, float2(12.9898, 78.233) * 21)) * 31.5453123);
}

// Function to generate a random vector based on the surface coordinates and roughness
float3 RandomVector(float2 uv, float roughness)
{

	float3 randomVec;
	randomVec.x = Random(uv + roughness);
	randomVec.y = Random(uv + roughness * 2.0);
	randomVec.z = Random(uv + roughness * 3.0);
	return normalize(randomVec * 2.0 - 1.0);
}

float4 parallexCorrectedCubemap(float3 worldPos, float3 ray) 
{

	if(distance(ReflectionCubemapMax,ReflectionCubemapMin)<0.5)
		return SampleCubemap(ReflectionCubemapSampler, ray);

   // gets min / max intersections with ray and cube
   // (not sure about this vector division or how it works tbh)
   float3 planeIntersect1 = (ReflectionCubemapMax - worldPos) / ray;
   float3 planeIntersect2 = (ReflectionCubemapMin - worldPos) / ray;

   // pick the furthest intersection
   float3 furthestPlane = max(planeIntersect1, planeIntersect2);

   // get the distance to closest intersection on this cube plane
   float dist = min(min(furthestPlane.x, furthestPlane.y), furthestPlane.z);

   // use this to recover the final intersected world space
   float3 intersectedWorldSpacePos = worldPos + ray * dist;
   
   // now get the ray in cubemap coords
   ray = intersectedWorldSpacePos - ReflectionCubemapPosition;

   return SampleCubemap(ReflectionCubemapSampler, ray);
}

float Random(int seed)
{
    // Use bitwise operations and constants to generate a pseudo-random value
    seed = (seed << 31) ^ seed;
    seed = (seed * (seed * seed * 13 + 721) + 3789);

    // Normalize to [0, 1]
    return frac(sin(seed) * 4823.4215453123);
}

float3 ApplyReflectionCubemapOnSurface(float3 color, float3 albedo, float reflectiveness, float metalic, float roughness, float2 texCoord, float3 reflectionBase, float3 worldPos)
{

	float3 cube = 0;SampleCubemap(ReflectionCubemapSampler, reflectionBase);

	const int numSamples = 8;

	roughness = saturate((roughness * lerp(roughness, 1, 0.7)) - 0.03);

	float n = 0;

	for (int i = 0; i < numSamples; i++)
	{
		//cube += SampleCubemap(ReflectionCubemapSampler, normalize(reflectionBase + normalize(RandomVector(texCoord + float2((i * 3) % 3.123 + 0.1, i), (i * 3) % 3.12352 + 0.1)) * roughness));

		float3 randVec;

#if OPENGL
		randVec = RandomVector(texCoord * 1000 + float2((i * 3) % 3.123 + 0.1, i), (i * 10) % 3.12352);
#else
		randVec = float3(Random((i+3)*130), Random((i+1)*310), Random((i+4)*470)) * 2 - 1;
#endif

		//randVec = RandomVector(texCoord * 1000 + float2((i * 3) % 3.123 + 0.1, i), (i * 10) % 3.12352);


		cube += parallexCorrectedCubemap(worldPos, normalize(reflectionBase + normalize(randVec) * roughness));

		n++;
	}
	cube /= n;

	//reflection = saturate(reflection);

	float lum = 0;// saturate(CalcLuminance(reflection))/30;


	float3 reflectionIntens = lerp(0, reflectiveness, metalic);

	//return cube;

	return lerp(color, cube * albedo, reflectiveness);
}