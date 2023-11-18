#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

texture ColorTexture;
sampler ColorSampler = sampler_state { texture = <ColorTexture>; };

struct PixelInput
{
    float2 TexCoord : TEXCOORD0;
};

float4 SamplePixelWithOffset(float2 TexCoord, float2 Offset)
{

	TexCoord.x = clamp(TexCoord.x + Offset.x, 0,1);
	TexCoord.y = clamp(TexCoord.y + Offset.y, 0,1);

	return tex2D(ColorSampler, TexCoord);
}

float4 PixelShaderFunction(PixelInput input) : COLOR0
{
    float4 color = tex2D(ColorSampler, input.TexCoord);

	color *= 4; // restoring color from 0 to 1 for not emissive and 0 to 4 for

	float4 bloom;

	float n = 0;

	float radius = 0.005;
	float step = radius/5;

	for(float x = -radius; x<=radius; x+=step){
		for(float y = -radius; y<=radius; y+=step)
		{

			float dist = distance(float2(x,y),float2(0,0));

			dist /= radius;

			if(dist > 1) continue;
			
			n++;

			float smpl = SamplePixelWithOffset(input.TexCoord, float2(x,y)) * 4.0f - 0.8f;
			bloom += clamp(smpl,0,5);
		}
	}
	bloom /= n;

	bloom*=bloom;

	color += bloom;

    return float4(color.xyz,1);
}

technique PostProcessingTechnique
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}