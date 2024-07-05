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

// Function to check if a point is near any hull point
bool IsNearHullPoint(float2 pt, float2 hullPoints[20], int pointCount, float threshold)
{
    for (int i = 0; i < pointCount; ++i)
    {
        float2 hullPoint = hullPoints[i];
        float distance = length(pt - hullPoint);
        if (distance <= threshold)
        {
            return true;
        }
    }
    return false;
}

// Main HLSL function to create a mask
void output_float(UnityTexture2D source, float2 uv, float numHullPts, float textureWidth, UnitySamplerState state, out float4 output)
{
    // Retrieve the dimensions of the input texture

    // Define maximum number of hull points
    float2 hullPoints[20];

    // Loop through the texture to find valid hull points
    for (int x = 0; x < numHullPts; ++x)
    {
        // Calculate UV for the texture sampling
        float2 sampleUV = float2((x / textureWidth), 0);
        float4 texColor = source.Sample(state, sampleUV);

        //output = texColor;

        // Check if the pixel is not black (empty)
        if (texColor.r > 0 || texColor.g > 0 || texColor.b > 0)
        {
            // Decode the point from red and green channels (normalized)
            float2 pt = float2(texColor.r, texColor.g) * 2;
            hullPoints[x] = pt;
        }
    }

    //hullPoints[0] = float2(.5,.5);

    // Check if the current pixel is inside the hull
    //bool insideHull = IsPointInPolygon(pixelPosition, hullPoints, numHullPts);
    bool nearHullPoint = IsNearHullPoint(uv, hullPoints, numHullPts, 0.01);

    // Output a mask based on whether the pixel is near any hull point
    if(nearHullPoint)
        output = float4(1,1,1,1);
    else
        output = float4(0,0,0,1);

    //output = (uv.y > .5) ? float4(1, 1, 1, 1) : float4(0, 0, 0, 1);
    //output = source.Sample(state, uv);
}
