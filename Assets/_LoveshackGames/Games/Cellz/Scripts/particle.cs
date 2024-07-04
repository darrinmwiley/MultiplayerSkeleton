using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle : MonoBehaviour
{
    public Vector2 position;
    public Vector2 previous;
    public Vector2 acceleration;
    public List<Edge> connectedEdges = new List<Edge>();
    int id;
    public int parentId = 0;
    public float size;
    public HashSet<int> intersectingParticles = new HashSet<int>();
    Rigidbody2D rb;

    bool particlesVisible = false;

    public void Init(Vector2 pos, float size = 0, int parentId = -1, int id = -1)
    {
        this.parentId = parentId;
        this.id = id;
        position = pos;
        previous = pos;
        acceleration = Vector2.zero;
        gameObject.transform.position = new Vector3(pos.x, pos.y, 0);
        this.size = size;

        if (size != 0)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = size;
            CreateCircularMesh(gameObject, size);
        }
    }

    public void SetPosition(Vector2 location)
    {
        position = location;
        gameObject.transform.position = new Vector3(location.x, location.y, 0);
    }

    private void CreateCircularMesh(GameObject obj, float radius)
    {
        Mesh mesh = new Mesh();
        int segments = 36; // Number of segments for the circle
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        float angleIncrement = 360f / segments;

        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * angleIncrement * i;
            vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3 + 2] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3] = i == segments - 1 ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        if(particlesVisible){
            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));

            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        particle otherParticle = other.GetComponent<particle>();
        if (otherParticle != null)
        {
            if (otherParticle.id != id)
            {
                intersectingParticles.Add(otherParticle.id);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        particle otherParticle = other.GetComponent<particle>();
        if (otherParticle != null)
        {
            if (otherParticle.id != id)
            {
                intersectingParticles.Remove(otherParticle.id);
            }
        }
    }
}
