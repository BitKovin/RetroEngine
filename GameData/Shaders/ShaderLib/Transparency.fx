float4 ApplyRefraction(float4 inColor, sampler2D s,float2 ScreenCoords, float2 RefractionOffset)
{

    if(inColor.a>0.95)
        return inColor;

    float3 frameColor = tex2D(s, ScreenCoords + RefractionOffset).rgb;

    float3 color = lerp(frameColor, inColor.rgb, inColor.a);

    return float4(color,1);
}
