using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletSoftBody
{
    public int ID;
    public particle center;
    public List<particle> ring;
    public float hubForceMultiplier = 1f;

    public static float minRadiusMultiplier = .8f;
    public static float maxRadiusMultiplier = 1.4f;

    public float radius;
    public float outerRadius;

    List<Spring> spokeSprings = new List<Spring>();
    List<Spring> adjacentSprings = new List<Spring>();
    List<FixedAngleConstraint> fixedAngleConstraints = new List<FixedAngleConstraint>();
    List<MinDistanceConstraint> minDistanceConstraints = new List<MinDistanceConstraint>();
    List<MaxDistanceConstraint> maxDistanceConstraints = new List<MaxDistanceConstraint>();
    public Cell cell;
    int numPoints;

    public VerletSoftBody(Vector2 centerPosition, float radius, int numPoints, float springForce, float damperForce, Cell cell, int ID)
    {
        this.ID = ID;
        this.cell = cell;
        this.radius = radius;
        ring = new List<particle>();
        float angleStep = 2 * Mathf.PI / numPoints;
        this.numPoints = numPoints;

        Vector2 firstPosition = centerPosition + new Vector2(Mathf.Cos(angleStep), Mathf.Sin(angleStep)) * radius;
        float nextAngle = 2 * angleStep;
        Vector2 nextPosition = centerPosition + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * radius;
        outerRadius = Vector2.Distance(firstPosition, nextPosition);
        float colliderRadius = outerRadius / 2;

        // Create the center particle at the center
        center = VerletSimulator.instance.AddParticle(centerPosition);

        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * angleStep;
            Vector2 position = centerPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            particle p = VerletSimulator.instance.AddParticle(position, /*colliderRadius = */ colliderRadius, /*parentId = */ ID);
            p.parentId = ID;
            ring.Add(p);
        }

        for (int i = 0; i < numPoints; i++)
        {
            int nextIndex = (i + 1) % numPoints;
            // Add fixed angle constraints between the center and adjacent spoke particles
            fixedAngleConstraints.Add(new FixedAngleConstraint(center, ring[i]));
            spokeSprings.Add(new Spring(center, ring[i], Vector2.Distance(ring[i].position, center.position), springForce, damperForce, /*parentMode = */ false, /*visible = */ false));
            adjacentSprings.Add(new Spring(ring[i], ring[nextIndex], Vector2.Distance(ring[i].position, ring[nextIndex].position), springForce*.2f, damperForce, /* parentMode = */ false, /*visible = */ false));
            minDistanceConstraints.Add(new MinDistanceConstraint(center, ring[i], Vector2.Distance(ring[i].position, center.position) * minRadiusMultiplier));
            maxDistanceConstraints.Add(new MaxDistanceConstraint(center, ring[i], Vector2.Distance(ring[i].position, center.position) * maxRadiusMultiplier));
        }
    }

    //corresponds to center pos
    public void SetPosition(Vector2 pos){
        center.position = pos;
        center.previous = pos;
        float angleStep = 2 * Mathf.PI / numPoints;
        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * angleStep;
            ring[i].position = ring[i].previous = center.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
    }

    public void SetRadius(float r)
    {
        this.radius = r;
        float angleStep = 2 * Mathf.PI / numPoints;

        Vector2 firstPosition = new Vector2(Mathf.Cos(angleStep), Mathf.Sin(angleStep)) * radius;
        float nextAngle = 2 * angleStep;
        Vector2 nextPosition = new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * radius;
        outerRadius = Vector2.Distance(firstPosition, nextPosition);

        for (int i = 0; i < numPoints; i++)
        {
            int nextIndex = (i + 1) % numPoints;
            // Add fixed angle constraints between the center and adjacent spoke particles
            spokeSprings[i].SetDistance(r);
            adjacentSprings[i].SetDistance(outerRadius);
            minDistanceConstraints[i].SetDistance(r * minRadiusMultiplier);
            maxDistanceConstraints[i].SetDistance(r * maxRadiusMultiplier);
        }
    }

    public void AccumulateForces()
    {
        if(cell.isControlled){
            ApplyForce(VerletSimulator.instance.GetInputForce());
        }
        foreach(Spring spring in spokeSprings){
            spring.Apply();
        }
        foreach(Spring spring in adjacentSprings){
            spring.Apply();
        }
    }

    public void SatisfyConstraints(){
        Vector3 softBodyPosition = cell.gameObject.transform.position;
        center.position = new Vector2(softBodyPosition.x, softBodyPosition.y);
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
    }
}