#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

float Exposure;
float Gamma;
float Saturation;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float3 ToneMap(float3 color, float gamma, float exposure, float Saturation)
{
    
    float Bleach = 0;
    
    color *= pow(2.0f, exposure); // Exposure
    color = pow(color, gamma); // Gamma

    const float3 coefLuma = float3(0.2126, 0.7152, 0.0722);
    float lum = dot(coefLuma, color);
	
    float L = saturate(10.0 * (lum - 0.45));
    float3 A2 = Bleach * color;

    float3 result1 = 2.0f * color * lum;
    float3 result2 = 1.0f - 2.0f * (1.0f - lum) * (1.0f - color);
	
    float3 newColor = lerp(result1, result2, L);
    float3 mixRGB = A2 * newColor;
    color += ((1.0f - A2) * mixRGB);
	
    float3 middlegray = dot(color, (1.0 / 3.0));
    float3 diffcolor = color - middlegray;
    color = (color + diffcolor * Saturation) / (1 + (diffcolor * Saturation)); // Saturation
    
    return color;
}

#define TONEMAP_GAMMA 1.0

// Reinhard Tonemapper
float4 tonemap_reinhard(in float3 color)
{
    color *= 16;
    color = color / (1 + color);
    float3 ret = pow(color, TONEMAP_GAMMA); // gamma
    return float4(ret, 1);
}

// Uncharted 2 Tonemapper
float3 tonemap_uncharted2(in float3 x)
{
    float A = 0.15;
    float B = 0.50;
    float C = 0.10;
    float D = 0.20;
    float E = 0.02;
    float F = 0.30;

    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

float3 tonemap_uc2(in float3 color)
{
    float W = 11.2;

    color *= 16; // Hardcoded Exposure Adjustment

    float exposure_bias = 2.0f;
    float3 curr = tonemap_uncharted2(exposure_bias * color);

    float3 white_scale = 1.0f / tonemap_uncharted2(W);
    float3 ccolor = curr * white_scale;

    float3 ret = pow(abs(ccolor), TONEMAP_GAMMA); // gamma

    return ret;
}

// Filmic tonemapper
float3 tonemap_filmic(float3 color)
{
    color = max(0, color - 0.004f);
    color = (color * (6.2f * color + 0.5f)) / (color * (6.2f * color + 1.7f) + 0.06f);

    // result has 1/2.2 baked in
    return pow(color, Gamma);
}


float4 MainPS(VertexShaderOutput input) : COLOR0
{
	
    float3 color = tex2D(SpriteTextureSampler, input.TextureCoordinates).rgb;
	
    color = ToneMap(tonemap_filmic(color), 1, Exposure, Saturation);
	
    
    return float4(color, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};