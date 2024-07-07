using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Cell
{
    public float radius = .2f;
    public float maxRadius = 2.2f;
    public int numPoints = 8;
    public float springForce = 5;
    public float damperForce = .2f;
    public bool isControlled = false;
    public int detail = 1; // Default detail level

    public Rigidbody2D rb;
    public float moveForce = 3f;

    private MeshFilter meshFilter;
    public MeshRenderer renderer;
    private Mesh mesh;

    public VerletSoftBody softBody;
    CircleCollider2D circleCollider;

    float growthRatePerSecond = .05f;

    public GameObject gameObject;

    public void Init(GameObject gameObject, Material material){
        this.gameObject = gameObject;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = material; 
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.drag = 0.2f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        circleCollider = gameObject.AddComponent<CircleCollider2D>();
        softBody = VerletSimulator.instance.AddVerletSoftBody(gameObject.transform.position, radius, numPoints, springForce, damperForce, this);
    }

    public void Destroy(){
        if(isControlled)
            ControlledCellHandler.SetControlledCell(null);
        VerletSimulator.instance.RemoveSoftBody(softBody.ID);
        Object.Destroy(gameObject);
    }

    public void SetPosition(Vector2 pos){
        gameObject.transform.position = new Vector3(pos.x, pos.y, 0);
        softBody.SetPosition(pos);
    }

    public void SetRadius(float r)
    {
        circleCollider.radius = r;
        radius = r;
    }

    public void Update()
    {
        if(radius < maxRadius)
            SetRadius(radius + growthRatePerSecond * Time.deltaTime);
        Vector2 pos = softBody.center.position;
        UpdateMesh();
    }

    void UpdateMesh()
    {
        // Calculate the number of vertices considering the detail level
        int numVertices = softBody.ring.Count * (detail + 1);
        Vector3[] vertices = new Vector3[numVertices];

        int vertexIndex = 0;
        for (int i = 0; i < softBody.ring.Count; i++)
        {
            Vector2 p0 = softBody.ring[i].position;
            Vector2 p1 = softBody.ring[(i + 1) % softBody.ring.Count].position;
            Vector2 m0 = softBody.ring[(i - 1 + softBody.ring.Count) % softBody.ring.Count].position;
            Vector2 m1 = softBody.ring[(i + 2) % softBody.ring.Count].position;

            // Add the main point
            vertices[vertexIndex++] = new Vector3(p0.x - softBody.center.position.x, p0.y - softBody.center.position.y, 0);

            // Add detail points
            for (int j = 1; j <= detail; j++)
            {
                float t = j / (float)(detail + 1);
                Vector2 interpolatedPoint = CatmullRomSpline(p0, p1, m0, m1, t);
                vertices[vertexIndex++] = new Vector3(interpolatedPoint.x - softBody.center.position.x, interpolatedPoint.y - softBody.center.position.y, 0);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;

        // Create triangles assuming the vertices form a simple polygon (winding order matters)
        int[] triangles = new int[(numVertices - 2) * 3];
        int index = 0;
        for (int i = 1; i < numVertices - 1; i++)
        {
            triangles[index + 2] = 0;
            triangles[index + 1] = i;
            triangles[index] = i + 1;
            index += 3;
        }
        mesh.triangles = triangles;

        // Recalculate normals and other necessary attributes
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    Vector2 CatmullRomSpline(Vector2 p0, Vector2 p1, Vector2 m0, Vector2 m1, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ((2 * p0) +
                      (-m0 + p1) * t +
                      (2 * m0 - 5 * p0 + 4 * p1 - m1) * t2 +
                      (-m0 + 3 * p0 - 3 * p1 + m1) * t3);
    }

    public void FixedUpdate()
    {
        // Check arrow key inputs for movement
        Vector2 force = Vector2.zero;

        if (isControlled)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
                force += Vector2.left * moveForce;
            if (Input.GetKey(KeyCode.RightArrow))
                force += Vector2.right * moveForce;
            if (Input.GetKey(KeyCode.UpArrow))
                force += Vector2.up * moveForce;
            if (Input.GetKey(KeyCode.DownArrow))
                force += Vector2.down * moveForce;

            // You may want to normalize force to ensure consistent speed
            // This depends on your specific movement requirements
            if (force.magnitude > 1f)
                force.Normalize();

            // Apply force to the Rigidbody
            rb.AddForce(force * moveForce);
        }
    }

    public virtual void OnMouseDown(int mouseButton)
    {
        ControlledCellHandler.SetControlledCell(this);
    }
}
