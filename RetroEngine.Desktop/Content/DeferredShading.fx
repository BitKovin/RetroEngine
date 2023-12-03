#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

//directional light
float DirectBrightness;
float GlobalBrightness;
float3 LightDirection;


//point lights
#define MAX_POINT_LIGHTS 5

float3 LightPositions[MAX_POINT_LIGHTS];
float3 LightColors[MAX_POINT_LIGHTS];
float LightRadiuses[MAX_POINT_LIGHTS];



//shadow map
matrix ShadowMapViewProjection;
float ShadowMapResolution;
texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
    texture = <ShadowMap>;
};


//buffers
texture ColorTexture;
sampler ColorTextureSampler = sampler_state
{
    texture = <ColorTexture>;
};

texture EmissiveTexture;
sampler EmissiveTextureSampler = sampler_state
{
    texture = <EmissiveTexture>;
};

texture NormalTexture;
sampler NormalTextureSampler = sampler_state
{
    texture = <NormalTexture>;
};

texture PositionTexture;
sampler PositionTextureSampler = sampler_state
{
    texture = <PositionTexture>;
};


struct PixelShaderInput
{
    float2 TexCoord : TEXCOORD0;
};

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

float3 SolvePointLight(int i, float3 position, float3 normal)
{

    float3 lightVector = LightPositions[i] - position;
    float distanceToLight = length(lightVector);
    float intense = saturate(1.0 - distanceToLight / LightRadiuses[i]);
    float3 dirToSurface = normalize(lightVector);
    
    intense *= saturate(dot(normal, dirToSurface) * 5 + 2);

    return LightColors[i] * max(intense, 0);
}

float3 SolveDirectionalLight(float3 normal)
{
    float light = max(dot(normal, LightDirection * -1.0f),0) * DirectBrightness;
    light += GlobalBrightness;
    return float3(light,light,light);

}

float3 CalculateLight(float3 position, float3 normal)
{
    float3 light = float3(0, 0, 0);
	
    light += SolveDirectionalLight(normal);

    for (int i = 0; i < MAX_POINT_LIGHTS; i++)
    {
        light += SolvePointLight(i, position, normal);
    }
    
    return light;
}

float4 MainPS(PixelShaderInput input) : COLOR
{
	
    float3 color = tex2D(ColorTextureSampler, input.TexCoord).xyz;
    float3 emissive = tex2D(EmissiveTextureSampler, input.TexCoord).xyz;
    float3 normal = tex2D(NormalTextureSampler, input.TexCoord).xyz * 2.0f - 1.0f;
    float3 position = tex2D(PositionTextureSampler, input.TexCoord).xyz;
	
    color *= CalculateLight(position, normal);

    color += emissive;
    
    return float4(color,1);
}

technique DefferedShading
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};