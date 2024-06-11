using UnityEngine;

public class PointsOnMesh : MonoBehaviour
{
    // Static maximum points
    public static int MAX_PTS = 20;

    // Public properties
    public Material material; // Material to set on the mesh renderer
    public Texture2D pointTex; // Texture to store points
    public Vector2[] points = new Vector2[MAX_PTS]; // Array of points
    public float radius = 0.1f; // Radius of the points

    private MeshRenderer meshRenderer; // Reference to the mesh renderer

    void Start()
    {
        // Create the texture with the specified dimensions
        pointTex = new Texture2D(MAX_PTS, 1, TextureFormat.RGBA32, false, true);
        pointTex.filterMode = FilterMode.Point; // Set the filter mode to point for exact pixel values
        pointTex.wrapMode = TextureWrapMode.Clamp; // Ensure the texture doesn't wrap

        // Get or add a MeshRenderer component
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Instantiate a new material to avoid modifying the original one
        if (material != null)
        {
            meshRenderer.material = Instantiate(material);
        }
    }

    void Update()
    {
        // Ensure the points array is within the maximum bounds
        int numPoints = Mathf.Min(points.Length, MAX_PTS);

        // Encode the points into the texture
        for (int i = 0; i < MAX_PTS; i++)
        {
            if (i < numPoints)
            {
                // Encode the point's x and y coordinates into the red and green channels
                Color color = new Color(points[i].x, points[i].y, 0.0f, 1.0f);
                pointTex.SetPixel(i, 0, color);
            }
            else
            {
                // Set remaining pixels to black
                pointTex.SetPixel(i, 0, Color.black);
            }
        }

        // Apply the changes to the texture
        pointTex.Apply();

        // Update the material properties
        if (meshRenderer.material != null)
        {
            meshRenderer.material.SetTexture("_PointsTex", pointTex);
            meshRenderer.material.SetFloat("_NumPoints", numPoints);
            meshRenderer.material.SetFloat("_Radius", radius);
            meshRenderer.material.SetFloat("_TexWidth", MAX_PTS);
        }
    }
}
