using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Constraint : IComparable<Constraint>
{
    public abstract int GetOrder();
    public abstract void SatisfyConstraint();

    public int CompareTo(Constraint other)
    {
        return GetOrder().CompareTo(other.GetOrder());
    }
}

public class FixedDistanceConstraint : Constraint
{
    public particle p1;
    public particle p2;
    public float restDistance;
    public LineRenderer lineRenderer;

    public FixedDistanceConstraint(particle p1, particle p2, float distance)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.restDistance = distance;

        lineRenderer = new GameObject("FixedDistanceConstraintLine").AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(1, 0, 0, 0.5f); // Red translucent
        lineRenderer.endColor = new Color(1, 0, 0, 0.5f); // Red translucent
        lineRenderer.positionCount = 2;
    }

    public override int GetOrder() => 0;

    public override void SatisfyConstraint()
    {
        Vector2 delta = p2.position - p1.position;
        float currentDistance = delta.magnitude;
        float difference = (currentDistance - restDistance) / currentDistance;

        Vector2 offset = delta * 0.5f * difference;
        p1.position += offset;
        p2.position -= offset;

        // Update line positions
        lineRenderer.SetPosition(0, p1.gameObject.transform.position);
        lineRenderer.SetPosition(1, p2.gameObject.transform.position);
    }
}

public class FixedAngleConstraint : Constraint
{
    public particle parent;
    public particle child;
    private Vector2 direction;

    public override int GetOrder() => 0;

    public FixedAngleConstraint(particle parent, particle child)
    {
        this.parent = parent;
        this.child = child;
        Vector2 toChild = child.position - parent.position;
        direction = toChild.normalized;
    }

    public override void SatisfyConstraint()
    {
        Vector2 toChild = child.position - parent.position;
        float projectionLength = Vector2.Dot(toChild, direction);
        Vector2 projectedPoint = parent.position + direction * projectionLength;

        child.position = projectedPoint;
    }
}

public class MinDistanceConstraint : Constraint
{
    public particle p1;
    public particle p2;
    private float minDistance;

    public override int GetOrder() => 0;

    public MinDistanceConstraint(particle p1, particle p2, float minDistance)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.minDistance = minDistance;
    }

    public override void SatisfyConstraint()
    {
        Vector2 delta = p2.position - p1.position;
        float currentDistance = delta.magnitude;

        if (currentDistance < minDistance)
        {
            float difference = (minDistance - currentDistance) / currentDistance;
            Vector2 offset = delta * 0.5f * difference;
            p1.position -= offset;
            p2.position += offset;
        }
    }
}

public class MaxDistanceConstraint : Constraint
{
    public particle p1;
    public particle p2;
    private float maxDistance;

    public override int GetOrder() => 0;

    public MaxDistanceConstraint(particle p1, particle p2, float maxDistance)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.maxDistance = maxDistance;
    }

    public override void SatisfyConstraint()
    {
        Vector2 delta = p2.position - p1.position;
        float currentDistance = delta.magnitude;

        if (currentDistance > maxDistance)
        {
            float difference = (currentDistance - maxDistance) / currentDistance;
            Vector2 offset = delta * 0.5f * difference;
                p1.position += offset;
                p2.position -= offset;
        }
    }
}

public class FixedConstraint : Constraint
{
    public particle p;
    private Vector2 location;

    public override int GetOrder() => 2;

    public FixedConstraint(particle p)
    {
        this.p = p;
        this.location = p.position;
    }

    public override void SatisfyConstraint()
    {
        p.position = location;
    }
}

public class EdgeConstraint : Constraint
{
    private Edge edge;
    private particle particle;

    public EdgeConstraint(Edge edge, particle particle)
    {
        this.edge = edge;
        this.particle = particle;
    }

    public override int GetOrder() => 1;

    public override void SatisfyConstraint()
    {
        Vector2 intersection;
        Vector2 prevLoc = particle.previous;
        Vector2 nextLoc = particle.position;

        // Calculate the cross product to determine the side of the edge
        Vector2 edgeDirection = edge.p2.position - edge.p1.position;
        Vector2 toPrev = prevLoc - edge.p1.position;
        Vector2 toNext = nextLoc - edge.p1.position;

        float crossPrev = LineUtil.CrossProduct2D(edgeDirection, toPrev);
        float crossNext = LineUtil.CrossProduct2D(edgeDirection, toNext);

        // Check if the particle is crossing the edge from the specified side
        if (crossPrev >= 0 && crossNext < 0)
        {
            if (LineUtil.IntersectLineSegments2D(edge.p1.position, edge.p2.position, prevLoc, nextLoc, out intersection))
            {
                // Calculate the edge normal vector
                Vector2 edgeNormal = new Vector2(edgeDirection.y, -edgeDirection.x).normalized;
                // Push the particle outwards perpendicular to the edge
                float offsetDistance = 0.01f; // Small offset distance
                particle.position = intersection - edgeNormal * offsetDistance;
                particle.previous = particle.position; // Update previous position to prevent re-crossing
            }
        }
    }
}
