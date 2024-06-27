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

    public class Spring
    {
        public particle p1;
        public particle p2;
        public float restDistance;
        public float springForce;
        public float damperForce;
        public LineRenderer lineRenderer;

        public Spring(particle p1, particle p2, float distance, float springForce, float damperForce)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.restDistance = distance;
            this.springForce = springForce;
            this.damperForce = damperForce;

            lineRenderer = new GameObject("SpringLine").AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(0, 0, 1, 0.5f); // Blue translucent
            lineRenderer.endColor = new Color(0, 0, 1, 0.5f); // Blue translucent
            lineRenderer.positionCount = 2;
        }

        public void ApplyForce()
        {
            Vector2 delta = p2.position - p1.position;
            float currentDistance = delta.magnitude;
            float displacement = currentDistance - restDistance;

            Vector2 force = (displacement * springForce) * (delta / currentDistance);
            p1.acceleration += force;
            p2.acceleration -= force;

            // Damping
            Vector2 relativeVelocity = (p2.position - p2.previous) - (p1.position - p1.previous);
            Vector2 dampingForce = relativeVelocity * damperForce;
            p1.acceleration += dampingForce;
            p2.acceleration -= dampingForce;

            // Update line positions
            lineRenderer.SetPosition(0, p1.gameObject.transform.position);
            lineRenderer.SetPosition(1, p2.gameObject.transform.position);
        }
    }

    public class Circle{

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
            foreach(particle p in ring)
                p.acceleration += force;
            center.acceleration += force * hubForceMultiplier;
            // apply the force to each ring point, and the force * hubForceMultiplier to the center
        }

    }

    List<particle> particles = new List<particle>();
    List<Spring> springs = new List<Spring>();
    List<FixedDistanceConstraint> fixedDistanceConstraints = new List<FixedDistanceConstraint>();
    List<FixedAngleConstraint> angleConstraints = new List<FixedAngleConstraint>();
    List<MinDistanceConstraint> minDistanceConstraints = new List<MinDistanceConstraint>();
    List<MaxDistanceConstraint> maxDistanceConstraints = new List<MaxDistanceConstraint>();
    List<Circle> circles = new List<Circle>();

    void Start()
    {
        int numberOfCircles = 500; // Number of small circles to spawn
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
            // Connect each spoke particle to the hub with springs and FixedDistanceConstraints
            //AddSpring(hub, p, radius, springForce, damperForce);
            //AddFixedDistanceConstraint(hub, p, radius);
        }

        for (int i = 0; i < numPoints; i++)
        {
            int nextIndex = (i + 1) % numPoints;
            // Connect each adjacent pair of spoke particles with springs and FixedDistanceConstraints
            //AddSpring(circleParticles[i], circleParticles[nextIndex], Vector2.Distance(circleParticles[i].position, circleParticles[nextIndex].position), springForce, damperForce);
            //AddFixedDistanceConstraint(circleParticles[i], circleParticles[nextIndex], Vector2.Distance(circleParticles[i].position, circleParticles[nextIndex].position));
        }

        for (int i = 0; i < numPoints; i++)
        {
            int nextIndex = (i + 1) % numPoints;
            // Add fixed angle constraints between the hub and adjacent spoke particles
            AddAngleConstraint(hub, circleParticles[i]);
            AddSpring(hub, circleParticles[i], Vector2.Distance(circleParticles[i].position, hub.position), springForce, damperForce);
            AddSpring(circleParticles[i], circleParticles[nextIndex], Vector2.Distance(circleParticles[i].position, circleParticles[nextIndex].position), springForce, damperForce);
            AddMinDistanceConstraint(hub, circleParticles[i], Vector2.Distance(circleParticles[i].position, hub.position) * .8f);
            AddMaxDistanceConstraint(hub, circleParticles[i], Vector2.Distance(circleParticles[i].position, hub.position) * 1.25f);
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

    public void AddFixedDistanceConstraint(particle p1, particle p2, float distance)
    {
        fixedDistanceConstraints.Add(new FixedDistanceConstraint(p1, p2, distance));
    }

    public void AddAngleConstraint(particle parent, particle child)
    {
        angleConstraints.Add(new FixedAngleConstraint(parent, child));
    }

    public void AddMinDistanceConstraint(particle parent, particle child, float distance, bool parentMode = true)
    {
        minDistanceConstraints.Add(new MinDistanceConstraint(parent, child, distance, parentMode));
    }

    public void AddMaxDistanceConstraint(particle parent, particle child, float distance, bool parentMode = true)
    {
        maxDistanceConstraints.Add(new MaxDistanceConstraint(parent, child, distance, parentMode));
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

            if(circles.Count != 0)
                circles[0].ApplyForce(force);
            else{
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
            spring.ApplyForce();
        }
    }

    void SatisfyConstraints()
    {
        foreach (FixedDistanceConstraint fixedDistanceConstraint in fixedDistanceConstraints)
        {
            fixedDistanceConstraint.SatisfyConstraint();
        }
        foreach (FixedAngleConstraint angleConstraint in angleConstraints)
        {
            angleConstraint.SatisfyConstraint();
        }
        foreach(MinDistanceConstraint minDistanceConstraint in minDistanceConstraints)
        {
            minDistanceConstraint.SatisfyConstraint();
        }
        foreach(MaxDistanceConstraint maxDistanceConstraint in maxDistanceConstraints)
        {
            maxDistanceConstraint.SatisfyConstraint();
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
