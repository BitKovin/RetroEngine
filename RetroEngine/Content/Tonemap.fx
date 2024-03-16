#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
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

float ColToneB(float hdrMax, float contrast, float shoulder, float midIn, float midOut)
{
    return
        -((-pow(midIn, contrast) + (midOut * (pow(hdrMax, contrast * shoulder) * pow(midIn, contrast) -
            pow(hdrMax, contrast) * pow(midIn, contrast * shoulder) * midOut)) /
            (pow(hdrMax, contrast * shoulder) * midOut - pow(midIn, contrast * shoulder) * midOut)) /
            (pow(midIn, contrast * shoulder) * midOut));
}

// General tonemapping operator, build 'c' term.
float ColToneC(float hdrMax, float contrast, float shoulder, float midIn, float midOut)
{
    return (pow(hdrMax, contrast * shoulder) * pow(midIn, contrast) - pow(hdrMax, contrast) * pow(midIn, contrast * shoulder) * midOut) /
           (pow(hdrMax, contrast * shoulder) * midOut - pow(midIn, contrast * shoulder) * midOut);
}

// General tonemapping operator, p := {contrast,shoulder,b,c}.
float ColTone(float x, float4 p)
{
    float z = pow(x, p.r);
    return z / (pow(z, p.g) * p.b + p.a);
}

float3 TimothyTonemapper(float3 color)
{
    static float hdrMax = 16.0; // How much HDR range before clipping. HDR modes likely need this pushed up to say 25.0.
    static float contrast = 2.0; // Use as a baseline to tune the amount of contrast the tonemapper has.
    static float shoulder = 1.0; // Likely don�t need to mess with this factor, unless matching existing tonemapper is not working well..
    static float midIn = 0.18; // most games will have a {0.0 to 1.0} range for LDR so midIn should be 0.18.
    static float midOut = 0.18; // Use for LDR. For HDR10 10:10:10:2 use maybe 0.18/25.0 to start. For scRGB, I forget what a good starting point is, need to re-calculate.

    float b = ColToneB(hdrMax, contrast, shoulder, midIn, midOut);
    float c = ColToneC(hdrMax, contrast, shoulder, midIn, midOut);

#define EPS 1e-6f
    float peak = max(color.r, max(color.g, color.b));
    peak = max(EPS, peak);

    float3 ratio = color / peak;
    peak = ColTone(peak, float4(contrast, shoulder, b, c));
    // then process ratio

    // probably want send these pre-computed (so send over saturation/crossSaturation as a constant)
    float crosstalk = 4.0; // controls amount of channel crosstalk
    float saturation = contrast; // full tonal range saturation control
    float crossSaturation = contrast * 16.0; // crosstalk saturation

    float white = 1.0;

    // wrap crosstalk in transform
    ratio = pow(abs(ratio), saturation / crossSaturation);
    ratio = lerp(ratio, white, pow(peak, crosstalk));
    ratio = pow(abs(ratio), crossSaturation);

    // then apply ratio to peak
    color = peak * ratio;
    return color;
}

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

    float exposure_bias = 2.0f + Exposure;
    float3 curr = tonemap_uncharted2(exposure_bias * color);

    float3 white_scale = 1.0f / tonemap_uncharted2(W);
    float3 ccolor = curr * white_scale;

    float3 ret = pow(abs(ccolor), Gamma); // gamma

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