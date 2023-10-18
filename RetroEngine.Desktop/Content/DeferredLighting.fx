#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// LightingEffect.fx

texture ColorTexture;
sampler ColorSampler = sampler_state { texture = <ColorTexture>; };

texture NormalTexture;
sampler NormalSampler = sampler_state { texture = <NormalTexture>; };

struct PixelInput
{
    float2 TexCoord : TEXCOORD0;
};

float4 PixelShaderFunction(PixelInput input) : COLOR0
{
    float4 color = tex2D(ColorSampler, input.TexCoord);
    float3 normal = 2.0 * tex2D(NormalSampler, input.TexCoord).rgb - 1.0;

    // Example lighting calculation (dot product of normal and a light direction)
    float lightingFactor = max(0, dot(normal, normalize(float3(0, 1, 0.2)))) * 0.5; // Example light direction

	lightingFactor += 0.6;

    // Apply lighting to color
    color.rgb *= lightingFactor;

    return color;
}

technique LightingTechnique
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
