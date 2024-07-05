// Function to find the convex hull using the Gift Wrapping algorithm
int FindConvexHull(float2 points[20], int numPoints, out float2 hull[20])
{
    if (numPoints < 3) return 0; // A convex hull is not possible with fewer than 3 points

    int hullCount = 0;

    // Find the leftmost point
    int leftmost = 0;
    for (int i = 1; i < numPoints; ++i)
    {
        if (points[i].x < points[leftmost].x)
        {
            leftmost = i;
        }
    }

    int p = leftmost;
    do
    {
        hull[hullCount++] = points[p];

        int q = (p + 1) % numPoints;
        for (int i = 0; i < numPoints; ++i)
        {
            // Check if point i is more counterclockwise than current point q
            float2 p1 = points[p];
            float2 p2 = points[q];
            float2 p3 = points[i];

            float crossProduct = (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);
            if (crossProduct < 0)
            {
                q = i;
            }
        }

        p = q;
    } while (p != leftmost); // While we don't return to the first point

    return hullCount;
}

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
void output_float(UnityTexture2D source, float2 uv, float numInputPoints, float numTexPoints, UnitySamplerState state, out float Mask)
{
    // Retrieve the dimensions of the input texture

    // Define maximum number of hull points
    float2 points[20];

    // Loop through the texture to find valid hull points
    for (int x = 0; x < numInputPoints; ++x)
    {
        // Calculate UV for the texture sampling
        float2 sampleUV = float2((x+.5) / numInputPoints, 0);
        half4 texColor = source.Sample(state, sampleUV);

        // Check if the pixel is not black (empty)
        if (texColor.r > 0 || texColor.g > 0 || texColor.b > 0)
        {
            float2 pt = float2(texColor.r, texColor.g);
            points[x] = pt; 
        }
    }

    float2 hull[20];
    int numHullPts = FindConvexHull(points, numInputPoints, hull);

    bool insideHull = IsPointInPolygon(uv, hull, numHullPts);
    
    Mask = insideHull ? 1 : 0;
}
