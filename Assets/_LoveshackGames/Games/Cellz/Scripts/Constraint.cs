using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Constraint
{
    abstract void SatisfyConstraint();
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

    public void SatisfyConstraint()
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

    public FixedAngleConstraint(particle parent, particle child)
    {
        this.parent = parent;
        this.child = child;
        Vector2 toChild = child.position - parent.position;
        direction = toChild.normalized;
    }

    public void SatisfyConstraint()
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
    //if this is true, only p2 will move. Otherwise they share
    bool parentMode;

    public MinDistanceConstraint(particle p1, particle p2, float minDistance, bool parentMode = false)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.minDistance = minDistance;
    }

    public void SatisfyConstraint()
    {
        Vector2 delta = p2.position - p1.position;
        float currentDistance = delta.magnitude;
        
        if (currentDistance < minDistance)
        {
            float difference = (minDistance - currentDistance) / currentDistance;
            Vector2 offset = delta * 0.5f * difference;
            if(parentMode)
                p2.position += offset * 2;
            else{
                p1.position -= offset;
                p2.position += offset;
            }
        }
    }
}

public class MaxDistanceConstraint : Constraint
{
    public particle p1;
    public particle p2;
    private float maxDistance;
    // if this is true, only p2 will move. Otherwise they share
    bool parentMode;

    public MaxDistanceConstraint(particle p1, particle p2, float maxDistance, bool parentMode = false)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.maxDistance = maxDistance;
        this.parentMode = parentMode;
    }

    public void SatisfyConstraint()
    {
        Vector2 delta = p2.position - p1.position;
        float currentDistance = delta.magnitude;

        if (currentDistance > maxDistance)
        {
            float difference = (currentDistance - maxDistance) / currentDistance;
            Vector2 offset = delta * 0.5f * difference;
            if (parentMode)
                p2.position -= offset * 2;
            else
            {
                p1.position += offset;
                p2.position -= offset;
            }
        }
    }
}