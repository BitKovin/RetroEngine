#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

#define MAX_POINT_LIGHTS 6

Texture2D ColorTexture;

sampler2D ColorTextureSampler = sampler_state
{
    Texture = <ColorTexture>;
};

Texture2D PositionTexture;

sampler2D PositionTextureSampler = sampler_state
{
    Texture = <PositionTexture>;
};

Texture2D NormalTexture;

sampler2D NormalTextureSampler = sampler_state
{
    Texture = <NormalTexture>;
};

#define POINT_LIGHT_SAMPLER_PARAMS MinFilter = Point; MipFilter = Point; MagFilter = Point ; MipLODBias = 0; AddressU = Mirror; AddressV = Mirror; AddressW = Mirror;

TextureCube PointLightCubemap1;
samplerCUBE ShadowCaster1Sampler = sampler_state
{
    Texture = <PointLightCubemap1>;
    POINT_LIGHT_SAMPLER_PARAMS
};

TextureCube PointLightCubemap2;
samplerCUBE ShadowCaster2Sampler = sampler_state
{
    Texture = <PointLightCubemap2>;
    POINT_LIGHT_SAMPLER_PARAMS
};

TextureCube PointLightCubemap3;
samplerCUBE ShadowCaster3Sampler = sampler_state
{
    Texture = <PointLightCubemap3>;
    POINT_LIGHT_SAMPLER_PARAMS
};

TextureCube PointLightCubemap4;
samplerCUBE ShadowCaster4Sampler = sampler_state
{
    Texture = <PointLightCubemap4>;
    POINT_LIGHT_SAMPLER_PARAMS
};

TextureCube PointLightCubemap5;
samplerCUBE ShadowCaster5Sampler = sampler_state
{
    Texture = <PointLightCubemap5>;
    POINT_LIGHT_SAMPLER_PARAMS
};

TextureCube PointLightCubemap6;
samplerCUBE ShadowCaster6Sampler = sampler_state
{
    Texture = <PointLightCubemap6>;
    POINT_LIGHT_SAMPLER_PARAMS
};

int PointLightsNumber;
float3 LightPositions[MAX_POINT_LIGHTS];
float LightRadiuses[MAX_POINT_LIGHTS];
float LightResolutions[MAX_POINT_LIGHTS];
float4 LightDirections[MAX_POINT_LIGHTS];

float3 viewPos;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


float SampleCubemapDepth(int i, float3 dir)
{

    dir *= float3(-1, 1, 1);

    float depth = 0;

    if(i == 0)
        depth = texCUBE(ShadowCaster1Sampler,dir).r;
    else if(i == 1)
        depth = texCUBE(ShadowCaster2Sampler,dir).r;
    else if(i == 2)
        depth = texCUBE(ShadowCaster3Sampler,dir).r;
    else if(i == 3)
        depth = texCUBE(ShadowCaster4Sampler,dir).r;
    else if(i == 4)
        depth = texCUBE(ShadowCaster5Sampler,dir).r;
    else if(i == 5)
        depth = texCUBE(ShadowCaster6Sampler,dir).r;

    if(depth < 0.001)
        depth = 10000000;

    return depth;

}

float GetShadowFromSource(int i, float3 worldPos, float3 worldNormal)
{



    float3 lightPos = LightPositions[i];
    float3 dir = normalize(worldPos - lightPos);

    float dirFactor = dot(dir, -worldNormal);



    if(dirFactor<=0)
        return 0;

    float depth = SampleCubemapDepth(i, dir);

    float compareDistance = distance(lightPos, worldPos);

    float bias = compareDistance / LightResolutions[i];
    bias*=15;

    float shadow = step(depth, compareDistance - bias);

    float dist = (compareDistance / LightRadiuses[i]);
    float intense = saturate(1.0 - dist * dist);




    return shadow * intense * LightDirections[i].w * dirFactor;

}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	
    float3 color = tex2D(ColorTextureSampler, input.TextureCoordinates).rgb;
	
    float3 pos = tex2D(PositionTextureSampler, input.TextureCoordinates).rgb + viewPos;

    float3 normal = tex2D(NormalTextureSampler, input.TextureCoordinates).rgb;

    normal = normalize(normal * 2 - 1);
	

    float factor = 1;

    for(int i = 0; i < 6; i++)
    {
        factor -= GetShadowFromSource(i, pos, normal);
    }

    color*= factor;

    return float4(factor,factor,factor, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};