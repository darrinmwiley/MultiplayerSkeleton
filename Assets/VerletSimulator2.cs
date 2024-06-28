using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletSimulator2 : MonoBehaviour
{
    static VerletSimulator2 instance;
    void Awake() => instance = this;

    Vector2 gravity = new Vector2(0, -.01f);
    public int maxDistance = 3;
    public float moveForce = 1;
    public float drag = .01f;
    public int substeps = 5;

    particle controlledParticle = null;
    public GameObject particlePrefab;

    public class Circle
    {
        public particle center;
        public List<particle> ring;
        public float hubForceMultiplier = 1.5f;

        public Circle(particle center, List<particle> ring)
        {
            this.center = center;
            this.ring = ring;
        }

        public void ApplyForce(Vector2 force)
        {
            foreach (particle p in ring)
                p.acceleration += force;
            center.acceleration += force * hubForceMultiplier;
            // apply the force to each ring point, and the force * hubForceMultiplier to the center
        }
    }

    //change this to Entity
    List<particle> particles = new List<particle>();
    List<Constraint> constraints = new List<Constraint>();
    //change this to Force once we have more than one force
    List<Spring> springs = new List<Spring>();
    List<Circle> circles = new List<Circle>();

    void Start()
    {
        int numberOfCircles = 50; // Number of small circles to spawn
        float circleRadius = .4f; // Radius of each small circle
        int numPoints = 8; // Number of points (particles) in each circle
        float springForce = 1f; // Spring force
        float damperForce = 0.1f; // Damper force

        for (int i = 0; i < numberOfCircles; i++)
        {
            Vector2 randomPosition = new Vector2(Random.Range(-5f, 5f), Random.Range(-3f, 3f));
            AddCircle(randomPosition, circleRadius, numPoints, springForce, damperForce);
        }
    }

    public void AddCircle(Vector2 center, float radius, int numPoints, float springForce, float damperForce)
    {
        List<particle> circleParticles = new List<particle>();
        float angleStep = 2 * Mathf.PI / numPoints;

        // Create the hub particle at the center
        particle hub = AddParticle(center);

        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * angleStep;
            Vector2 position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            particle p = AddParticle(position);
            circleParticles.Add(p);
        }

        for (int i = 0; i < numPoints; i++)
        {
            int nextIndex = (i + 1) % numPoints;
            // Add fixed angle constraints between the hub and adjacent spoke particles
            AddConstraint(new FixedAngleConstraint(hub, circleParticles[i]));
            AddSpring(hub, circleParticles[i], Vector2.Distance(circleParticles[i].position, hub.position), springForce, damperForce);
            AddSpring(circleParticles[i], circleParticles[nextIndex], Vector2.Distance(circleParticles[i].position, circleParticles[nextIndex].position), springForce, damperForce);
            AddConstraint(new MinDistanceConstraint(hub, circleParticles[i], Vector2.Distance(circleParticles[i].position, hub.position) * .8f));
            AddConstraint(new MaxDistanceConstraint(hub, circleParticles[i], Vector2.Distance(circleParticles[i].position, hub.position) * 1.25f));
        }
        circles.Add(new Circle(hub, circleParticles));
    }

    public particle AddParticle(Vector2 position)
    {
        particle p = CreateParticle(position);
        particles.Add(p);
        return p;
    }

    public particle CreateParticle(Vector2 position)
    {
        GameObject particleObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        particleObj.transform.localScale = new Vector3(.1f, .1f, .1f);
        particleObj.AddComponent<ParticleClickHandler>();
        ParticleClickHandler handler = particleObj.GetComponent<ParticleClickHandler>();
        handler.particle = new particle(position, particleObj);
        handler.simulator = this;

        return handler.particle;
    }

    public void AddSpring(particle p1, particle p2, float distance, float springForce, float damperForce)
    {
        springs.Add(new Spring(p1, p2, distance, springForce, damperForce));
    }

    public void AddConstraint(Constraint constraint)
    {
        constraints.Add(constraint);
    }

    void FixedUpdate()
    {
        HandleInput();
        float dt = Time.fixedDeltaTime / substeps;
        for (int i = 0; i < substeps; i++)
        {
            AccumulateForces();
            Integrate(dt);
            SatisfyConstraints();
        }
    }

    void HandleInput()
    {
        if (controlledParticle != null)
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

            if (circles.Count != 0)
                circles[0].ApplyForce(force);
            else
            {
                controlledParticle.acceleration += force;
            }
        }
    }

    void Integrate(float timestep)
    {
        foreach (particle p in particles)
        {
            Vector2 loc = p.position;
            Vector2 newPrev = loc;
            p.position = loc * (2 - drag) - p.previous * (1 - drag) + p.acceleration * timestep * timestep;
            p.previous = newPrev;
            p.gameObject.transform.position = new Vector3(p.position.x, p.position.y, 0);
            p.acceleration = Vector2.zero; // Reset acceleration after applying it
        }
    }

    void AccumulateForces()
    {
        foreach (Spring spring in springs)
        {
            spring.Apply();
        }
    }

    void SatisfyConstraints()
    {
        constraints.Sort();

        foreach (var constraint in constraints)
        {
            constraint.SatisfyConstraint();
        }
    }

    public void SetParticleControlled(particle p)
    {
        if (controlledParticle != null)
        {
            controlledParticle.gameObject.GetComponent<Renderer>().material.color = Color.gray;
        }

        controlledParticle = p;

        if (controlledParticle != null)
        {
            controlledParticle.gameObject.GetComponent<Renderer>().material.color = Color.white;
        }
    }
}

public class ParticleClickHandler : MonoBehaviour
{
    public particle particle;
    public VerletSimulator2 simulator;

    void OnMouseDown()
    {
        simulator.SetParticleControlled(particle);
    }
}
