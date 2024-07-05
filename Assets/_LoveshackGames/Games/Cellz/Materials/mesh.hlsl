SamplerState samp; // "sampler" + “_MainTex”
// Function to check if a point is inside a convex polygon
bool IsPointInPolygon(float2 pt, float2 hullPoints[20], int pointCount)
{
    bool inside = false;

    for (int i = 0, j = pointCount - 1; i < pointCount; j = i++)
    {
        float2 p1 = hullPoints[i];
        float2 p2 = hullPoints[j];

        if (((p1.y > pt.y) != (p2.y > pt.y)) &&
            (pt.x < (p2.x - p1.x) * (pt.y - p1.y) / (p2.y - p1.y) + p1.x))
        {
            inside = !inside;
        }
    }

    return inside;
}

// Main HLSL function to create a mask
half4 output_float(float2 uv : TEXCOORD0) : SV_Target
{
    // Retrieve the dimensions of the input texture

    // Define maximum number of hull points
    #define MAX_HULL_POINTS 20
    float2 hullPoints[MAX_HULL_POINTS];
    int pointCount = 0;

    // Loop through the texture to find valid hull points
    for (int x = 0; x < _TexWidth; ++x)
    {
        if (pointCount >= MAX_HULL_POINTS)
            break; // Avoid exceeding the max array size

        // Calculate UV for the texture sampling
        float2 sampleUV = float2(x / _TexWidth, 0);
        half4 texColor = _HullTex.Sample(samp, uv);

        // Check if the pixel is not black (empty)
        if (texColor.r > 0 || texColor.g > 0 || texColor.b > 0)
        {
            // Decode the point from red and green channels (normalized)
            float2 pt = float2(texColor.r, texColor.g);
            hullPoints[pointCount++] = pt * 2.0 - 1.0; // Transform to [-1, 1] range
        }
    }

    // Check if the current pixel is inside the hull
    float2 pixelPosition = uv * 2.0 - 1.0; // Transform UV to [-1, 1] space
    bool insideHull = IsPointInPolygon(pixelPosition, hullPoints, pointCount);

    // Output a mask based on whether the pixel is inside the hull
    return insideHull ? half4(1, 1, 1, 1) : half4(0, 0, 0, 1);
}
