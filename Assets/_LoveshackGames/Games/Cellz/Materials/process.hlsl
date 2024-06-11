// Main HLSL function
void process_float(Texture2D source, float2 uv, SamplerState state, out float4 output)
{
    // Sample the texture at the given UV coordinates
    float4 texColor = source.Sample(state, uv);

    // Return the sampled color as a half4
    output = half4(texColor.rgb, texColor.a);
}
