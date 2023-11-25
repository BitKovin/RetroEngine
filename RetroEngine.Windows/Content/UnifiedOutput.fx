#if OPENGL
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
sampler TextureSampler = sampler_state
{
    texture = <Texture>;
};
texture EmissiveTexture;
sampler EmissiveTextureSampler = sampler_state
{
    texture = <EmissiveTexture>;
};

texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
    texture = <ShadowMap>;
};

float DirectBrightness;
float GlobalBrightness;
float3 LightDirection;

float EmissionPower;
float ShadowBias;
float Transparency;
matrix ShadowMapViewProjection;

#define MAX_POINT_LIGHTS 2

float3 LightPositions[MAX_POINT_LIGHTS];
float3 LightColors[MAX_POINT_LIGHTS];
float LightRadiuses[MAX_POINT_LIGHTS];


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
    float4 lightPos : TEXCOORD3;
    float3 MyPosition : TEXCOORD4;
};

float3 normalize(float3 v)
{
    return rsqrt(dot(v, v)) * v;
}

PixelInput VertexShaderFunction(VertexInput input)
{
    PixelInput output;

    output.Position = mul(input.Position, World);
    output.MyPosition = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.TexCoord = input.TexCoord;

	// Pass the world space normal to the pixel shader
    output.Normal = mul(input.Normal, (float3x3) World);
    output.Normal = normalize(output.Normal);

    float lightingFactor = max(-0.0, dot(output.Normal, normalize(-LightDirection))) * DirectBrightness; // Example light direction

    output.light = lightingFactor;

    output.lightPos = mul(float4(mul(input.Position, World)), ShadowMapViewProjection);

    return output;
}

float GetShadow(float3 lightCoords)
{
    float shadow = 0;

    float3 centerCoords = lightCoords;
    if (lightCoords.x >= 0 && lightCoords.x <= 1 && lightCoords.y >= 0 && lightCoords.y <= 1)
    {

        float closestDepth = tex2D(ShadowMapSampler, lightCoords.xy).r;
        float currentDepth = lightCoords.z * 2 - 1;

        if (currentDepth > closestDepth + ShadowBias)
            shadow += 1;
    }

    return shadow;
}

float3 CalculatePointLight(int i, PixelInput PixelInput)
{

    if (LightRadiuses[i] <= 0)
        return 0;

    float intense = distance(LightPositions[i], PixelInput.MyPosition) / LightRadiuses[i];

    float3 dirToSurface = normalize(LightPositions[i] - PixelInput.MyPosition);

    intense = 1 - intense;
    intense *= clamp(dot(PixelInput.Normal, dirToSurface) * 5 + 2, 0, 1);
    intense = max(intense, 0);

    return LightColors[i] * intense;
}


float4 PixelShaderFunction(PixelInput input) : COLOR0
{

    float3 textureColor = tex2D(TextureSampler, input.TexCoord).xyz;
    float textureAlpha = tex2D(TextureSampler, input.TexCoord).w;

    textureAlpha *= Transparency;

    float3 lightCoords = input.lightPos.xyz / input.lightPos.w;

    float shadow = 0;

    lightCoords = (lightCoords + 1.0f) / 2.0f;

    lightCoords.y = 1.0f - lightCoords.y;

    shadow += GetShadow(lightCoords);
    
    float3 light = input.light;

    light -= shadow;
    light = max(light, 0);
    light += GlobalBrightness;


    for (int i = 0; i < MAX_POINT_LIGHTS; i++)
    {
        light += CalculatePointLight(i, input);
    }


    textureColor *= light;


    textureColor += tex2D(EmissiveTextureSampler, input.TexCoord).xyz * EmissionPower;

    //textureColor *= Transparency;
    
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