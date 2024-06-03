#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture ColorTexture;
sampler ColorSampler = sampler_state
{
    texture = <ColorTexture>;
};

bool Enabled;

struct PixelInput
{
    float2 TexCoord : TEXCOORD0;
};

float3 SamplePixelWithOffset(float2 TexCoord, float2 Offset)
{
    TexCoord.x = clamp(TexCoord.x + Offset.x, 0, 1);
    TexCoord.y = clamp(TexCoord.y + Offset.y, 0, 1);
    return tex2D(ColorSampler, TexCoord).xyz;
}

float4 PixelShaderFunction(PixelInput input) : COLOR0
{
    float3 color = tex2D(ColorSampler, input.TexCoord).xyz;

    if (false)
    {
    
        float3 bloom = float3(0, 0, 0);

        float n = 0;

        float radius = 0.0035;
        float step = radius / 4;

        float3 smpl = float3(0, 0, 0);

        for (float x = -radius; x <= radius; x += step)
        {
            for (float y = -radius; y <= radius; y += step)
            {

                if (distance(float2(x, y), float2(0, 0)) > radius)
                    continue;
			
                n++;
                smpl = SamplePixelWithOffset(input.TexCoord, float2(x, y)) - float3(0.8, 0.8, 0.8);
			
                bloom += max(smpl, 0);
            }
        }
        bloom /= n;

        bloom *= bloom;

        color += bloom;
    }
    return float4(color.xyz, 1);
}

technique PostProcessingTechnique
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}