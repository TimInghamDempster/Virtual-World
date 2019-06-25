RWTexture2D<float4> outputImage : register(t0);

[numthreads(#threadCountX#, #threadCountY#, 1)]
void DrawGlobe(uint3 threadID : SV_DispatchThreadID)
{
	uint2 pos = threadID.xy;

	outputImage[pos] = float4(1.0f, 0.0f, 0.0f, 1.0f);
}