using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Cell
{
    Behaviour behaviour;

    public float radius = .2f;
    public float maxRadius = 2.2f;
    public int numPoints = 12;
    public float springForce = 5;
    public float damperForce = .2f;
    public bool isControlled = false;
    public int detail = 1; // Default detail level

    public Rigidbody2D rb;
    public float moveForce = 3f;

    private MeshFilter meshFilter;
    public MeshRenderer renderer;
    public Mesh mesh;

    public VerletSoftBody softBody;
    CircleCollider2D circleCollider;
    public float softBodyRadiusMultiplier = 1.2f;
    CircleCollider2D softBodyCollider;

    float growthRatePerSecond = .05f;

    bool showOutline = true;
    public GameObject gameObject;
    private LineRenderer lineRenderer;
    public Color outlineColor = new Color(25 / 255f, 0, 45 / 255f); // Configurable outline color

    public HashSet<int> overlappingSoftBodyIds = new HashSet<int>();

    public void Init(GameObject gameObject, Material material){
        SetBehaviour(new IdleBehaviour());
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
        softBodyCollider = gameObject.AddComponent<CircleCollider2D>();
        softBodyCollider.isTrigger = true;
        softBodyCollider.radius = radius * softBodyRadiusMultiplier;
        softBody = VerletSimulator.instance.AddVerletSoftBody(gameObject.transform.position, radius, numPoints, springForce, damperForce, this);

        // Initialize LineRenderer
        if(showOutline){
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = numPoints;
            lineRenderer.startWidth = Mathf.Min(radius / 10f, 0.1f);
            lineRenderer.endWidth = Mathf.Min(radius / 10f, 0.1f);
            lineRenderer.loop = true;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = outlineColor;
            lineRenderer.endColor = outlineColor;
        }
    }

    public void SetBehaviour(Behaviour behaviour)
    {
        this.behaviour = behaviour;
        behaviour.cell = this;
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
        softBodyCollider.radius = r * softBodyRadiusMultiplier;
        radius = r;
    }

    public void Update()
    {
        if(radius < maxRadius)
            SetRadius(radius + growthRatePerSecond * Time.deltaTime);
        UpdateMesh();
    }

    void UpdateMesh()
    {
        // Calculate the number of vertices considering the detail level
        int numVertices = softBody.ring.Count * (detail + 1);
        Vector3[] vertices = new Vector3[numVertices];
        Vector3[] lineVertices = new Vector3[numVertices];


        int vertexIndex = 0;
        for (int i = 0; i < softBody.ring.Count; i++)
        {
            Vector2 p0 = softBody.ring[i].position;
            Vector2 p1 = softBody.ring[(i + 1) % softBody.ring.Count].position;
            Vector2 m0 = softBody.ring[(i - 1 + softBody.ring.Count) % softBody.ring.Count].position;
            Vector2 m1 = softBody.ring[(i + 2) % softBody.ring.Count].position;

            // Add the main point
            vertices[vertexIndex] = new Vector3(p0.x - gameObject.transform.position.x, p0.y - gameObject.transform.position.y, 0);
            lineVertices[vertexIndex++] = new Vector3(p0.x, p0.y, 0);
            // Add detail points
            for (int j = 1; j <= detail; j++)
            {
                float t = j / (float)(detail + 1);
                Vector2 interpolatedPoint = CatmullRomSpline(p0, p1, m0, m1, t);
                vertices[vertexIndex] = new Vector3(interpolatedPoint.x - gameObject.transform.position.x, interpolatedPoint.y - gameObject.transform.position.y, 0);
                lineVertices[vertexIndex++] = new Vector3(interpolatedPoint.x, interpolatedPoint.y, 0);
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

        if(showOutline)
            UpdateLineRenderer(lineVertices);
    }

    void UpdateLineRenderer(Vector3[] vertices)
    {
        if (lineRenderer != null && vertices != null)
        {
            lineRenderer.startWidth = Mathf.Min(radius / 10f, 0.1f);
            lineRenderer.endWidth = Mathf.Min(radius / 10f, 0.1f);
            lineRenderer.positionCount = vertices.Length;
            lineRenderer.SetPositions(vertices);
        }
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

    public void Move(Vector2 direction)
    {
        rb.AddForce(direction.normalized * moveForce);
    }

    public void MoveTowards(float x, float y)
    {
        // Get vector between gameobject and (x, y)
        Vector2 direction = new Vector2(x, y) - (Vector2)gameObject.transform.position;
        
        // Normalize the direction to get a unit vector
        direction.Normalize();
        
        // Apply moveForce in that direction
        rb.AddForce(direction * moveForce);
    }

    public void FixedUpdate()
    {
        if(behaviour != null)
            behaviour.FixedUpdate();
    }

    public virtual void OnMouseDown(int mouseButton)
    {
        ControlledCellHandler.SetControlledCell(this);
    }
}
