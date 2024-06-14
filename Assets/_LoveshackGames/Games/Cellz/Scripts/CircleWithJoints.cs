using UnityEngine;
using System.Collections.Generic;

public class CircleWithJoints : MonoBehaviour
{
    public float radius = 5f; // Radius of the large circle
    private float smallRadius = 0.5f; // Radius of the small circles
    public int numberOfCircles = 12; // Number of small circles (modifiable in the inspector)
    private int connections; // Number of neighbors to connect to each circle
    public float springFrequency = 1f; // Frequency for the spring joints
    public float springDampingRatio = 0; // Damping ratio for the spring joints
    public float moveForce = 50f; // Force applied to move the center circle
    public int detail;
    public bool controlled;

    private GameObject[] smallCircles;
    private Vector3[] previousPositions;
    private GameObject centerCircle;
    private Rigidbody2D centerRb;
    private bool isColliding;

    // Mesh for the hull
    private Mesh hullMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public Material coloredMaterial;

    void Start()
    {
        connections = numberOfCircles - 1;
        // Initialize the mesh
        hullMesh = new Mesh();

        // Ensure the GameObject has a MeshFilter and MeshRenderer
        meshFilter = gameObject.GetComponent<MeshFilter>();
        if (!meshFilter)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = hullMesh;

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (!meshRenderer)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (coloredMaterial)
        {
            meshRenderer.material = Instantiate(coloredMaterial);
        }

        // Create center circle
        centerCircle = CreateCircle(Vector2.zero, smallRadius, "CenterCircle");
        centerRb = centerCircle.GetComponent<Rigidbody2D>();
        centerRb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Create and place small circles
        smallCircles = new GameObject[numberOfCircles];
        previousPositions = new Vector3[numberOfCircles];
        for (int i = 0; i < numberOfCircles; i++)
        {
            float angle = i * Mathf.PI * 2f / numberOfCircles;
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            smallCircles[i] = CreateCircle(pos, smallRadius, "SmallCircle" + i);

            // Create and configure spring joint between the small circle and center circle
            SpringJoint2D spring = smallCircles[i].AddComponent<SpringJoint2D>();
            spring.connectedBody = centerRb;
            spring.autoConfigureDistance = false;
            spring.distance = radius;
            spring.frequency = springFrequency;
            spring.dampingRatio = springDampingRatio;
        }

        // Connect small circles with spring joints to their neighbors
        for (int i = 0; i < numberOfCircles; i++)
        {
            for (int j = 1; j <= connections; j++)
            {
                int neighborIndex = (i + j) % numberOfCircles;
                SpringJoint2D spring = smallCircles[i].AddComponent<SpringJoint2D>();
                spring.connectedBody = smallCircles[neighborIndex].GetComponent<Rigidbody2D>();
                spring.autoConfigureDistance = false;
                spring.distance = Vector2.Distance(smallCircles[i].transform.position, smallCircles[neighborIndex].transform.position);
                spring.frequency = springFrequency;
                spring.dampingRatio = springDampingRatio;
            }
        }

        // Calculate the convex hull
        List<Vector2> pointPositions = new List<Vector2>();
        for (int i = 0; i < smallCircles.Length; i++)
        {
            pointPositions.Add(smallCircles[i].transform.localPosition);
        }
        List<Vector2> hull = QuickHull(pointPositions);

        // Generate the initial texture based on the convex hull
        UpdateMesh(hull);
    }

    void Update()
    {
        // Apply force to the center circle based on arrow key input
        Vector2 force = Vector2.zero;
        if (controlled)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                force += Vector2.up;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                force += Vector2.down;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                force += Vector2.left;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                force += Vector2.right;
            }
        }

        centerRb.AddForce(force * moveForce);

        // Calculate convex hull and update mesh
        List<Vector2> pointPositions = new List<Vector2>();
        for (int i = 0; i < smallCircles.Length; i++)
        {
            pointPositions.Add(smallCircles[i].transform.position);
        }

        List<Vector2> hull = QuickHull(pointPositions);

        // Update the existing texture based on the convex hull
        UpdateMesh(hull);
    }

    GameObject CreateCircle(Vector2 position, float radius, string name)
    {
        GameObject circle = new GameObject(name);
        circle.transform.position = position;
        circle.transform.localScale = Vector3.one * radius * 2;

        CircleCollider2D collider = circle.AddComponent<CircleCollider2D>();
        collider.radius = radius;

        Rigidbody2D rb = circle.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        circle.transform.parent = transform;

        return circle;
    }

    void UpdateMesh(List<Vector2> hull)
    {
        // Check if hull is not null and contains points
        if (hull == null || hull.Count == 0)
        {
            Debug.LogError("Hull points are null or empty");
            return;
        }

        // List to hold interpolated points
        List<Vector2> detailedHull = new List<Vector2>();

        // Add interpolated points
        int hullCount = hull.Count;
        for (int i = 0; i < hullCount; i++)
        {
            Vector2 p0 = hull[(i - 1 + hullCount) % hullCount];
            Vector2 p1 = hull[i];
            Vector2 p2 = hull[(i + 1) % hullCount];
            Vector2 p3 = hull[(i + 2) % hullCount];

            detailedHull.Add(p1);

            for (int j = 1; j <= detail; j++)
            {
                float t = j / (float)(detail + 1);
                Vector2 newPoint = CatmullRom(p0, p1, p2, p3, t);
                detailedHull.Add(newPoint);
            }
        }

        // Set vertices from detailed hull points
        Vector3[] vertices = new Vector3[detailedHull.Count];
        int[] triangles = new int[(detailedHull.Count - 2) * 3];

        for (int i = 0; i < detailedHull.Count; i++)
        {
            vertices[i] = new Vector3(detailedHull[i].x, detailedHull[i].y, 0);
        }

        for (int i = 0; i < detailedHull.Count - 2; i++)
        {
            triangles[i * 3 + 2] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3] = i + 2;
        }

        // Update the existing mesh properties
        hullMesh.Clear();
        hullMesh.vertices = vertices;
        hullMesh.triangles = triangles;
        hullMesh.RecalculateNormals();
    }

    Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector2 result = 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);

        return result;
    }

    // QuickHull implementation
    public static List<Vector2> QuickHull(List<Vector2> points)
    {
        // Step 1: Sort points lexicographically by x and then by y
        points.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
        
        int n = points.Count;
        List<Vector2> hull = new List<Vector2>();
        
        // Step 2: Construct lower hull
        for (int i = 0; i < n; i++)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(points[i]);
        }
        
        // Step 3: Construct upper hull
        int t = hull.Count + 1;
        for (int i = n - 1; i >= 0; i--)
        {
            while (hull.Count >= t && Cross(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(points[i]);
        }

        // Remove the last point because it's the same as the first point
        hull.RemoveAt(hull.Count - 1);

        // Step 4: Sort counter-clockwise
        Vector2 centroid = CalculateCentroid(hull);
        hull.Sort((a, b) => PolarAngle(centroid, a).CompareTo(PolarAngle(centroid, b)));

        return hull;
    }

    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    private static Vector2 CalculateCentroid(List<Vector2> points)
    {
        float xSum = 0;
        float ySum = 0;
        foreach (Vector2 point in points)
        {
            xSum += point.x;
            ySum += point.y;
        }
        return new Vector2(xSum / points.Count, ySum / points.Count);
    }

    private static float PolarAngle(Vector2 origin, Vector2 point)
    {
        return Mathf.Atan2(point.y - origin.y, point.x - origin.x);
    }

    bool IsLeftOfLine(Vector2 a, Vector2 b, Vector2 p)
    {
        return ((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x)) > 0;
    }

    float DistanceFromLine(Vector2 a, Vector2 b, Vector2 p)
    {
        return Mathf.Abs((b.x - a.x) * (a.y - p.y) - (a.x - p.x) * (b.y - a.y)) / (b - a).magnitude;
    }

    void FindHull(List<Vector2> hull, List<Vector2> points, Vector2 a, Vector2 b)
    {
        if (points.Count == 0) return;

        Vector2 farthestPoint = points[0];
        float maxDistance = DistanceFromLine(a, b, farthestPoint);

        foreach (Vector2 p in points)
        {
            float distance = DistanceFromLine(a, b, p);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestPoint = p;
            }
        }

        hull.Insert(hull.IndexOf(b), farthestPoint);
        List<Vector2> leftSetAP = new List<Vector2>();
        List<Vector2> leftSetPB = new List<Vector2>();

        foreach (Vector2 p in points)
        {
            if (p == farthestPoint) continue;

            if (IsLeftOfLine(a, farthestPoint, p))
                leftSetAP.Add(p);
            else if (IsLeftOfLine(farthestPoint, b, p))
                leftSetPB.Add(p);
        }

        FindHull(hull, leftSetAP, a, farthestPoint);
        FindHull(hull, leftSetPB, farthestPoint, b);
    }
}