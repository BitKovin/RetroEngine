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

//shadow map
matrix ShadowMapViewProjection;
float ShadowMapResolution;
texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
    texture = <ShadowMap>;
};
float ShadowBias = 0.05f;


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

float GetShadow(float3 lightCoords, float3 position, float3 normal)
{
    float shadow = 0;

    if (lightCoords.x >= 0 && lightCoords.x <= 1 && lightCoords.y >= 0 && lightCoords.y <= 1)
    {
        float currentDepth = lightCoords.z * 2 - 1;

        float resolution = 1;
        

        int numSamples = 1; // Number of samples in each direction (total samples = numSamples^2)

        float bias = ShadowBias * abs(normal.z) + ShadowBias / 2.0f;
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
    return float3(light,light,light);

}

float3 CalculateLight(float3 position, float3 normal)
{
    float3 light = float3(0, 0, 0);
    
    float4 lightPos = mul(float4(position,1), ShadowMapViewProjection);
    
    float3 lightCoords = lightPos.xyz / lightPos.w;

    float shadow = 0;
    
    lightCoords = (lightCoords + 1.0f) / 2.0f;

    lightCoords.y = 1.0f - lightCoords.y;
    
    shadow += GetShadow(lightCoords, position, normal);
    light -= shadow;
    
    light += SolveDirectionalLight(normal);
    light = max(light, 0);
    light += GlobalBrightness;
    
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