using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class SoftBodyCell : MonoBehaviour
{
    public float radius;
    public int numPoints;
    public float springForce;
    public float damperForce;
    public bool isControlled;

    private Rigidbody2D rb;
    public float moveForce = 3f;

    private MeshFilter meshFilter;
    private Mesh mesh;

    public VerletSimulator2.Circle circle;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.drag = 0.2f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.radius = VerletSimulator2.Circle.minRadiusMultiplier;

        // Initialize Mesh components
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // Initialize the circle in the Verlet simulator
        circle = VerletSimulator2.instance.AddCircle(transform.position, radius, numPoints, springForce, damperForce, this);
    }

    void Update()
    {
        Vector2 pos = circle.center.position;
        gameObject.transform.position = new Vector3(pos.x, pos.y, 0);
        UpdateMesh();
    }

    void UpdateMesh()
    {
        // Update mesh vertices based on circle's ring positions
        Vector3[] vertices = new Vector3[circle.ring.Count];

        for (int i = 0; i < circle.ring.Count; i++)
        {
            // Calculate the direction from the center to the ring point
            Vector2 direction = circle.ring[i].position - circle.center.position;
            direction.Normalize();
            
            // Calculate the new position by moving the point outerRadius units further from the center
            Vector2 newPosition = circle.ring[i].position + direction * circle.outerRadius / 2;
            
            // Update vertex position
            vertices[i] = new Vector3(newPosition.x - circle.center.position.x, newPosition.y - circle.center.position.y, 0);
        }

        mesh.Clear();
        mesh.vertices = vertices;

        // Create triangles assuming the vertices form a simple polygon (winding order matters)
        int[] triangles = new int[(circle.ring.Count - 2) * 3];
        int index = 0;
        for (int i = 1; i < circle.ring.Count - 1; i++)
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

    void FixedUpdate()
    {
        // Check arrow key inputs for movement
         /*Vector2 force = Vector2.zero;

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
            // if (force.magnitude > 1f)
            //     force.Normalize();

            // Apply force to the Rigidbody
            rb.AddForce(force);
        }*/
    }
}
