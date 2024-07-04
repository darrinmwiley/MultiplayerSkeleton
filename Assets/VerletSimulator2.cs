using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletSimulator2 : MonoBehaviour
{
    public static VerletSimulator2 instance;
    void Awake() => instance = this;

    Vector2 gravity = new Vector2(0, -.01f);
    public int maxDistance = 3;
    public float moveForce = 1;
    public float drag = .01f;
    public int substeps = 50;

    particle controlledParticle = null;
    public GameObject particlePrefab;

    public class Circle
    {
        public int ID;
        public particle center;
        public List<particle> ring;
        public float hubForceMultiplier = 1f;

        public static float minRadiusMultiplier = .8f;
        public static float maxRadiusMultiplier = 1.4f;
 
        private float radius;
        public float outerRadius;

        List<Spring> springs = new List<Spring>();
        List<FixedAngleConstraint> fixedAngleConstraints = new List<FixedAngleConstraint>();
        List<MinDistanceConstraint> minDistanceConstraints = new List<MinDistanceConstraint>();
        List<MaxDistanceConstraint> maxDistanceConstraints = new List<MaxDistanceConstraint>();
        List<Edge> edges = new List<Edge>();
        public SoftBodyCell softBodyCell;
        //TODO: clean way to apply edge constraints against another circle

        public Circle(Vector2 centerPosition, float radius, int numPoints, float springForce, float damperForce, SoftBodyCell softBodyCell, int ID)
        {
            this.ID = ID;
            this.softBodyCell = softBodyCell;
            this.radius = radius;
            ring = new List<particle>();
            float angleStep = 2 * Mathf.PI / numPoints;

            Vector2 firstPosition = centerPosition + new Vector2(Mathf.Cos(angleStep), Mathf.Sin(angleStep)) * radius;
            float nextAngle = 2 * angleStep;
            Vector2 nextPosition = centerPosition + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * radius;
            outerRadius = Vector2.Distance(firstPosition, nextPosition);
            float colliderRadius = outerRadius / 2;

            // Create the center particle at the center
            center = instance.AddParticle(centerPosition);

            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * angleStep;
                Vector2 position = centerPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                particle p = instance.AddParticle(position, /*colliderRadius = */ colliderRadius, /*parentId = */ ID);
                p.parentId = ID;
                ring.Add(p);
            }

            for (int i = 0; i < numPoints; i++)
            {
                int nextIndex = (i + 1) % numPoints;
                // Add fixed angle constraints between the center and adjacent spoke particles
                fixedAngleConstraints.Add(new FixedAngleConstraint(center, ring[i]));
                springs.Add(new Spring(center, ring[i], Vector2.Distance(ring[i].position, center.position), springForce, damperForce, /*parentMode = */ false, /*visible = */ false));
                springs.Add(new Spring(ring[i], ring[nextIndex], Vector2.Distance(ring[i].position, ring[nextIndex].position), springForce*.2f, damperForce, /* parentMode = */ false, /*visible = */ false));
                minDistanceConstraints.Add(new MinDistanceConstraint(center, ring[i], Vector2.Distance(ring[i].position, center.position) * minRadiusMultiplier));
                maxDistanceConstraints.Add(new MaxDistanceConstraint(center, ring[i], Vector2.Distance(ring[i].position, center.position) * maxRadiusMultiplier));
            }
        }

        public void AccumulateForces()
        {
            if(softBodyCell.isControlled){
                ApplyForce(instance.GetInputForce());
            }
            foreach(Spring spring in springs){
                spring.Apply();
            }
        }

        public void SatisfyConstraints(){
            //Vector3 softBodyPosition = softBodyCell.transform.position;
            //center.position = new Vector2(softBodyPosition.x, softBodyPosition.y);
            //center.previous = center.position;
            foreach(Constraint constraint in minDistanceConstraints){
                constraint.SatisfyConstraint();
            }
            foreach(Constraint constraint in maxDistanceConstraints){
                constraint.SatisfyConstraint();
            }
            foreach(Constraint constraint in fixedAngleConstraints){
                constraint.SatisfyConstraint();
            }
        }

        public void ApplyForce(Vector2 force)
        {
            foreach (particle p in ring)
                p.acceleration += force;
            center.acceleration += force * hubForceMultiplier;
            // apply the force to each ring point, and the force * hubForceMultiplier to the center
        }

        public void ResolveCollisionsForParticle(particle particle)
        {
            // Define a ray in the +x direction from the particle's current position
            /*Vector2 rayOrigin = particle.position;
            Vector2 rayDirection = Vector2.right;

            int intersectCount = 0;

            // Check intersections with each edge
            foreach (Edge edge in edges)
            {
                Vector2 intersectionPoint;

                // Check if the ray intersects with the edge
                // TODO, use a more efficient intersection algo
                //if (LineUtil.IntersectLineSegments2D(rayOrigin, rayOrigin + rayDirection * 1000f, edge.p1.position, edge.p2.position, out intersectionPoint))
                //{
                    intersectCount++;
                //}
            }

            // Determine if particle is inside or outside based on intersect count
            bool isInside = (intersectCount % 2) == 1;

            if (isInside)
            {
                //HandleCollision(particle); // Stubbed out method to handle collision
            }*/
        }

        // Method to handle collision and project particle out of nearest edge
        private void HandleCollision(particle particle)
        {
            // Find the nearest edge
            Edge nearestEdge = FindNearestEdge(particle.position);

            if (nearestEdge != null)
            {
                // Project the particle out of the nearest edge
                Vector2 edgeDirection = (nearestEdge.p2.position - nearestEdge.p1.position).normalized;
                Vector2 particleToEdge = particle.position - nearestEdge.p1.position;
                float distanceAlongEdge = Vector2.Dot(particleToEdge, edgeDirection);
                Vector2 projectionPoint = nearestEdge.p1.position + Mathf.Clamp(distanceAlongEdge, 0f, Vector2.Distance(nearestEdge.p1.position, nearestEdge.p2.position)) * edgeDirection;

                // Apply a small offset to ensure the particle is outside the edge
                float offsetDistance = 0.01f;
                particle.position = projectionPoint + edgeDirection * offsetDistance;
                particle.previous = particle.position; // Update previous position to prevent re-collision
            }
        }

        // Method to find the nearest edge to a given point
        private Edge FindNearestEdge(Vector2 point)
        {
            Edge nearestEdge = null;
            float minDistance = float.MaxValue;

            foreach (Edge edge in edges)
            {
                float distance = LineUtil.DistancePointToLineSegment(point, edge.p1.position, edge.p2.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEdge = edge;
                }
            }

            return nearestEdge;
        }

        //call this only when two circle's bounding boxes intersect
        //we can approximate bounding box by using a circle of maxDistanceConstraint length
        public void ResolveCollisions(Circle circle)
        {
            foreach(particle p in circle.ring)
            {
                ResolveCollisionsForParticle(p);
            }
        }

        // Method to get the maximum bounding box
        //TODO: stop recreating this every frame, attach to circle GO intead since it's a fixed size
        public Rect GetMaxBoundingBox()
        {
            // Calculate the radius considering the max distance constraint
            float maxRadius = radius * maxRadiusMultiplier;

            // Create a bounding box based on the center and max radius
            Vector2 size = new Vector2(maxRadius * 2, maxRadius * 2);
            Rect boundingBox = new Rect(center.position - size / 2, size);

            // Visualize bounding box as a square
            Debug.DrawLine(new Vector3(boundingBox.xMin, boundingBox.yMin), new Vector3(boundingBox.xMax, boundingBox.yMin), Color.red);
            Debug.DrawLine(new Vector3(boundingBox.xMax, boundingBox.yMin), new Vector3(boundingBox.xMax, boundingBox.yMax), Color.red);
            Debug.DrawLine(new Vector3(boundingBox.xMax, boundingBox.yMax), new Vector3(boundingBox.xMin, boundingBox.yMax), Color.red);
            Debug.DrawLine(new Vector3(boundingBox.xMin, boundingBox.yMax), new Vector3(boundingBox.xMin, boundingBox.yMin), Color.red);

            return boundingBox;
        }
    }

    //change this to Entity
    List<particle> particles = new List<particle>();
    List<Circle> circles = new List<Circle>();
    List<Edge> edges = new List<Edge>();

    public Circle AddCircle(Vector2 center, float radius, int numPoints, float springForce, float damperForce, SoftBodyCell softBodyCell)
    {
        Circle c = new Circle(center, radius, numPoints, springForce, damperForce, softBodyCell, circles.Count);
        circles.Add(c);
        return c;
    }

    public particle AddParticle(Vector2 position, float colliderRadius = 0, int parentId = -1)
    {
        particle p = CreateParticle(position, colliderRadius, parentId);
        particles.Add(p);
        return p;
    }

    public particle CreateParticle(Vector2 position, float colliderRadius, int parentId)
    {
        GameObject particleObj = new GameObject();
        particleObj.AddComponent<ParticleClickHandler>();
        ParticleClickHandler handler = particleObj.GetComponent<ParticleClickHandler>();
        particle p = particleObj.AddComponent<particle>();
        p.Init(position, colliderRadius, parentId, particles.Count);
        handler.particle = p;
        handler.simulator = this;

        return p;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime / substeps;
        for (int i = 0; i < substeps; i++)
        {
            AccumulateForces();
            Integrate(dt);
            SatisfyConstraints();
        }
    }

    Vector2 GetInputForce()
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
        /*Vector2 force = GetInputForce();
        if (circles.Count != 0)
            circles[0].ApplyForce(force);
        else
        {
            controlledParticle.acceleration += force;
        }*/
        Vector2 input = GetInputForce();
        foreach (Circle circle in circles)
        {
            circle.AccumulateForces();
        }
    }

    void SatisfyConstraints()
    {
        //todo don't sort this every time, just maintain a sorted list
        //constraints.Sort();

        foreach (Circle circle in circles)
        {
            circle.SatisfyConstraints();
        }

        foreach(particle p in particles){
            foreach(int intersectingId in p.intersectingParticles){
                particle q = particles[intersectingId];
                if(p.parentId != q.parentId)
                {
                    PushOut(p, q);
                }
            }
        }
    }

    void PushOut(particle p, particle q)
    {
        // Calculate the direction vectors from each particle to the center of their own circles
        Vector2 directionP = p.position - circles[p.parentId].center.position;
        Vector2 directionQ = q.position - circles[q.parentId].center.position;

        // Calculate the distance between the two particles
        float distance = (p.position - q.position).magnitude;
        float overlap = p.size + q.size - distance;

        // If they are intersecting (overlap is positive)
        if (overlap > 0)
        {
            // Normalize the direction vectors
            directionP.Normalize();
            directionQ.Normalize();

            // Calculate the push distance
            Vector2 pushDistanceP = directionP * overlap / 2;
            Vector2 pushDistanceQ = directionQ * overlap / 2;

            // Move each particle away from the center of its parent circle
            p.position -= pushDistanceP;
            q.position -= pushDistanceQ;
            
            // Update previous positions to prevent re-collision
            p.previous = p.position;
            q.previous = q.position;
        }
    }

    public void SetParticleControlled(particle p)
    {
        if (controlledParticle != null)
        {
            //controlledParticle.gameObject.GetComponent<Renderer>().material.color = Color.gray;
        }

        controlledParticle = p;

        if (controlledParticle != null)
        {
            //controlledParticle.gameObject.GetComponent<Renderer>().material.color = Color.white;
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
