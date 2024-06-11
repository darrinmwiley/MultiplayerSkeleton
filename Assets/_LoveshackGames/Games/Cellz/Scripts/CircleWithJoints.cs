using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class CircleWithJoints : MonoBehaviour
{
    public float radius = 5f; // Radius of the large circle
    public float smallRadius = 0.5f; // Radius of the small circles
    public int numberOfCircles = 6; // Number of small circles (modifiable in the inspector)
    public int connections = 2; // Number of neighbors to connect to each circle
    public GameObject circlePrefab; // Prefab for the circles
    public GameObject pushOutMarkerPrefab; // Prefab for the green push-out marker
    public float springFrequency = 10f; // Frequency for the spring joints
    public float springDampingRatio = 0.2f; // Damping ratio for the spring joints
    public float moveForce = 5f; // Force applied to move the center circle
    public bool controlled;

    public int textureSize = 32; // Public variable for the texture size

    private GameObject[] smallCircles;
    private Vector3[] previousPositions;
    private GameObject centerCircle;
    private Rigidbody2D centerRb;
    private LineRenderer lineRenderer;
    private bool isColliding;
    private List<GameObject> pushOutMarkers; // List to store push-out markers
    private GameObject quad;
    private Texture2D hullTexture; // Instance variable for the texture

    public Material material;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numberOfCircles * 2;
        lineRenderer.startWidth = 0.05f; // Thinner lines
        lineRenderer.endWidth = 0.05f; // Thinner lines
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        pushOutMarkers = new List<GameObject>(); // Initialize the list

        // Create center circle
        centerCircle = Instantiate(circlePrefab, transform.position, Quaternion.identity, transform);
        centerCircle.transform.localScale = Vector3.one * smallRadius * 2;

        // Ensure the center circle has a CircleCollider2D
        if (centerCircle.GetComponent<CircleCollider2D>() == null)
        {
            centerCircle.AddComponent<CircleCollider2D>().radius = smallRadius;
        }

        smallCircles = new GameObject[numberOfCircles];
        previousPositions = new Vector3[numberOfCircles];
        centerRb = centerCircle.AddComponent<Rigidbody2D>();
        centerRb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Create and place small circles
        for (int i = 0; i < numberOfCircles; i++)
        {
            float angle = i * Mathf.PI * 2f / numberOfCircles;
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            smallCircles[i] = Instantiate(circlePrefab, transform.position + (Vector3)pos, Quaternion.identity, transform);
            smallCircles[i].transform.localScale = Vector3.one * smallRadius * 2;

            // Ensure each small circle has a CircleCollider2D
            if (smallCircles[i].GetComponent<CircleCollider2D>() == null)
            {
                CircleCollider2D collider = smallCircles[i].AddComponent<CircleCollider2D>();
                collider.radius = smallRadius;
            }

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

        // Create and attach a quad to the center circle
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.SetParent(centerCircle.transform);
        quad.transform.localPosition = Vector3.zero;
        quad.transform.localScale = new Vector3(radius * 2.5f, radius * 2.5f, 1);

        // Create the texture once
        hullTexture = new Texture2D(numberOfCircles,1, TextureFormat.RGBA32, false, true);

        // Calculate the convex hull
        List<Vector2> pointPositions = new List<Vector2>();
        for (int i = 0; i < smallCircles.Length; i++)
        {
            pointPositions.Add(smallCircles[i].transform.position);
        }
        Vector2[] hull = QuickHull(pointPositions.ToArray());

        // Generate the initial texture based on the convex hull
        UpdateHullTexture(hull, quad);
        quad.GetComponent<MeshRenderer>().material = Instantiate(material);
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

        // Set the line renderer color to white at the start of each update
        isColliding = false;

        // Check for collisions with other CircleWithJoints
        CircleWithJoints[] allCircles = FindObjectsOfType<CircleWithJoints>();
        foreach (CircleWithJoints other in allCircles)
        {
            if (other != this)
            {
                //CheckCollisionWithOtherCircle(other);
            }
        }

        // Update line renderer color based on collision state
        if (isColliding)
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }
        else
        {
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
        }

        // Calculate convex hull and update line renderer
        List<Vector2> pointPositions = new List<Vector2>();
        for (int i = 0; i < smallCircles.Length; i++)
        {
            previousPositions[i] = smallCircles[i].transform.position;
            pointPositions.Add(smallCircles[i].transform.position);
        }

        Vector2[] hull = QuickHull(pointPositions.ToArray());
        UpdateLineRenderer(hull);

        // Update the existing texture based on the convex hull
        UpdateHullTexture(hull, quad);
    }

    //todo: compute hull outside of HLSL again
    void UpdateHullTexture(Vector2[] hull, GameObject quad)
    {
        Color32[] pixels = hullTexture.GetPixels32();
        float centerX = textureSize / 2;
        float centerY = textureSize / 2;
        for (int i = 0; i < numberOfCircles; i++)
        {
            // Calculate the position of the hull point in quad's local space
            Vector3 point = smallCircles[i].transform.position;
            Vector3 localPoint = quad.transform.InverseTransformPoint(new Vector3(point.x, point.y, 0));
            
            // Set the red and green channels based on the transformed local coordinates
            float localX = localPoint.x + 0.5f; // Transform to [0,1] range
            float localY = localPoint.y + 0.5f; // Transform to [0,1] range

            // Update the pixel color
            pixels[i] = new Color32(
                (byte)(localX * 255), // Red channel
                (byte)(localY * 255), // Green channel
                0,                   // Blue channel (set to 0 for simplicity)
                255                  // Alpha channel
            );
        }

        hullTexture.SetPixels32(pixels);
        hullTexture.Apply();
        material.SetTexture("_HullTex", hullTexture);
        material.SetFloat("_NumTexPoints", numberOfCircles);
        material.SetFloat("_TexWidth", numberOfCircles);
    }


    bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int intersections = 0;
        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 p1 = polygon[i];
            Vector2 p2 = polygon[(i + 1) % polygon.Length];

            if (RayIntersectsSegment(point, p1, p2))
            {
                intersections++;
            }
        }
        return (intersections % 2) != 0;
    }

    bool RayIntersectsSegment(Vector2 point, Vector2 p1, Vector2 p2)
    {
        if (p1.y > p2.y)
        {
            Vector2 temp = p1;
            p1 = p2;
            p2 = temp;
        }

        if (point.y == p1.y || point.y == p2.y)
        {
            point.y += 0.0001f;
        }

        if (point.y < p1.y || point.y > p2.y)
        {
            return false;
        }

        if (point.x > Mathf.Max(p1.x, p2.x))
        {
            return false;
        }

        if (point.x < Mathf.Min(p1.x, p2.x))
        {
            return true;
        }

        float m = (p2.x - p1.x) / (p2.y - p1.y);
        float xIntersect = p1.x + (point.y - p1.y) * m;

        return point.x < xIntersect;
    }

    void UpdateLineRenderer(Vector2[] hull)
    {
        if (hull.Length < 3)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = hull.Length + 1;
        for (int i = 0; i < hull.Length; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(hull[i].x, hull[i].y, 0));
        }
        lineRenderer.SetPosition(hull.Length, new Vector3(hull[0].x, hull[0].y, 0)); // Close the loop
    }

    Vector2[] QuickHull(Vector2[] points)
    {
        if (points.Length < 3) return points;

        List<Vector2> hull = new List<Vector2>();

        Vector2 leftmost = points[0];
        Vector2 rightmost = points[0];

        foreach (Vector2 p in points)
        {
            if (p.x < leftmost.x) leftmost = p;
            if (p.x > rightmost.x) rightmost = p;
        }

        hull.Add(leftmost);
        hull.Add(rightmost);

        List<Vector2> leftSet = new List<Vector2>();
        List<Vector2> rightSet = new List<Vector2>();

        foreach (Vector2 p in points)
        {
            if (p == leftmost || p == rightmost) continue;

            if (IsLeftOfLine(leftmost, rightmost, p))
                leftSet.Add(p);
            else
                rightSet.Add(p);
        }

        FindHull(hull, leftSet, leftmost, rightmost);
        FindHull(hull, rightSet, rightmost, leftmost);

        return hull.ToArray();
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

    void CheckCollisionWithOtherCircle(CircleWithJoints other)
    {
        Vector2[] polygon = new Vector2[other.smallCircles.Length];
        for (int i = 0; i < other.smallCircles.Length; i++)
        {
            polygon[i] = other.smallCircles[i].transform.position;
        }

        for (int i = 0; i < smallCircles.Length; i++)
        {
            GameObject point = smallCircles[i];
            Vector3 previousPosition = previousPositions[i]; // Assuming you have a previousPositions array
            SpriteRenderer spriteRenderer = point.GetComponent<SpriteRenderer>();
            if (IsPointInPolygon(point.transform.position, polygon, out Vector2 intersection))
            {
                spriteRenderer.color = Color.red;
                isColliding = true;

                // Find closest edge and move vertex there
                Vector2 closestPointOnEdge = FindClosestPointOnPolygonEdges(point.transform.position, polygon);
                point.transform.position = closestPointOnEdge;

                // Reset velocity
                Rigidbody2D rb = point.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero; // Stop all motion
                }

                //3) TODO redirect the kinetic energy into a collision force with the
                //two points touching that line
            }
            else
            {
                spriteRenderer.color = Color.white;
            }
        }
    }

    Vector2 FindClosestPointOnPolygonEdges(Vector2 point, Vector2[] polygon)
    {
        Vector2 closestPoint = Vector2.zero;
        float minDistance = float.MaxValue;

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 p1 = polygon[i];
            Vector2 p2 = polygon[(i + 1) % polygon.Length];
            Vector2 closestPointOnEdge = ClosestPointOnLineSegment(p1, p2, point);

            float distance = Vector2.Distance(point, closestPointOnEdge);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = closestPointOnEdge;
            }
        }

        return closestPoint;
    }

    Vector2 ClosestPointOnLineSegment(Vector2 p1, Vector2 p2, Vector2 point)
    {
        Vector2 lineVec = p2 - p1;
        Vector2 pointVec = point - p1;
        float lineLen = lineVec.magnitude;
        Vector2 lineDir = lineVec / lineLen;
        float projectedLength = Vector2.Dot(pointVec, lineDir);
        projectedLength = Mathf.Clamp(projectedLength, 0f, lineLen);
        return p1 + lineDir * projectedLength;
    }


    bool IsPointInPolygon(Vector2 point, Vector2[] polygon, out Vector2 intersection)
    {
        int intersections = 0;
        intersection = Vector2.zero;
        bool foundIntersection = false;

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 p1 = polygon[i];
            Vector2 p2 = polygon[(i + 1) % polygon.Length];

            if (RayIntersectsSegment(point, p1, p2, out Vector2 intersect))
            {
                intersections++;
                if (!foundIntersection)
                {
                    intersection = intersect;
                    foundIntersection = true;
                }
            }
        }

        return (intersections % 2) != 0;
    }

    bool RayIntersectsSegment(Vector2 point, Vector2 p1, Vector2 p2, out Vector2 intersection)
    {
        intersection = Vector2.zero;
        if (p1.y > p2.y)
        {
            Vector2 temp = p1;
            p1 = p2;
            p2 = temp;
        }

        if (point.y == p1.y || point.y == p2.y)
        {
            point.y += 0.0001f;
        }

        if (point.y < p1.y || point.y > p2.y)
        {
            return false;
        }

        if (point.x > Mathf.Max(p1.x, p2.x))
        {
            return false;
        }

        if (point.x < Mathf.Min(p1.x, p2.x))
        {
            intersection = point;
            return true;
        }

        float m = (p2.x - p1.x) / (p2.y - p1.y);
        float xIntersect = p1.x + (point.y - p1.y) * m;

        intersection = new Vector2(xIntersect, point.y);
        return point.x < xIntersect;
    }

    void ClearPushOutMarkers()
    {
        foreach (GameObject marker in pushOutMarkers)
        {
            Destroy(marker);
        }
        pushOutMarkers.Clear();
    }
}
