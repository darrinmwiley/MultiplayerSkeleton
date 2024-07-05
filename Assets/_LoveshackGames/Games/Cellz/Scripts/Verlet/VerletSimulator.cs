using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletSimulator : MonoBehaviour
{
    public static VerletSimulator instance;
    void Awake() => instance = this;

    public float moveForce = 8;
    public float drag = .01f;
    public int substeps = 3;

    private int nextId = 0;
    Dictionary<int, VerletSoftBody> circles = new Dictionary<int, VerletSoftBody>();
    private int nextParticleId = 0;
    Dictionary<int, particle> particles = new Dictionary<int, particle>();

    public VerletSoftBody AddVerletSoftBody(Vector2 center, float radius, int numPoints, float springForce, float damperForce, Cell cell)
    {
        VerletSoftBody c = new VerletSoftBody(center, radius, numPoints, springForce, damperForce, cell, nextId);
        circles.Add(nextId++, c);
        return c;
    }

    public void RemoveParticle(int id){
        particles.Remove(id);
    }

    public void RemoveSoftBody(int id){
        VerletSoftBody sb = circles[id];
        RemoveParticle(sb.center.id);
        foreach(particle p in sb.ring)
            RemoveParticle(p.id);
        circles.Remove(id);
    }

    public particle AddParticle(Vector2 position, float colliderRadius = 0, int parentId = -1)
    {
        particle p = CreateParticle(position, colliderRadius, parentId);
        particles.Add(p.id, p);
        return p;
    }

    public particle CreateParticle(Vector2 position, float colliderRadius, int parentId)
    {
        particle p = new particle(position, colliderRadius, parentId, nextParticleId++);
        return p;
    }

    void FixedUpdate()
    {
        foreach(VerletSoftBody softBody in circles.Values)
        {
            softBody.SetRadius(softBody.cell.radius);
        }
        float dt = Time.fixedDeltaTime / substeps;
        for (int i = 0; i < substeps; i++)
        {
            AccumulateForces();
            Integrate(dt);
            SatisfyConstraints();
        }
    }

    public Vector2 GetInputForce()
    {
        Vector2 force = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            force.y += moveForce;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            force.y -= moveForce;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            force.x -= moveForce;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            force.x += moveForce;
        }
        return force;
    }

    void Integrate(float timestep)
    {
        foreach (particle p in particles.Values)
        {
            Vector2 loc = p.position;
            Vector2 newPrev = loc;
            p.position = loc * (2 - drag) - p.previous * (1 - drag) + p.acceleration * timestep * timestep;
            p.previous = newPrev;
            p.acceleration = Vector2.zero; // Reset acceleration after applying it
        }
    }

    void AccumulateForces()
    {
        foreach (VerletSoftBody circle in circles.Values)
        {
            circle.AccumulateForces();
        }
    }

    void SatisfyConstraints()
    {
        foreach (VerletSoftBody circle in circles.Values)
        {
            circle.SatisfyConstraints();
        }
    }
}
