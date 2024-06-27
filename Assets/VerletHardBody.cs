using UnityEngine;
using System.Collections.Generic;

public class VerletSquare
{
    public List<VerletPoint> points = new List<VerletPoint>();
    
    public class EdgeConstraint
    {
        public int ptId1, ptId2;
        public float restLength;
    }
    
    public List<EdgeConstraint> edgeConstraints = new List<EdgeConstraint>();

    public VerletSquare()
    {
        points.Add(new VerletPoint(new Vector2(-1, -1), VerletSimulator.CreateCircle()));
        points.Add(new VerletPoint(new Vector2(-1, 1), VerletSimulator.CreateCircle()));
        points.Add(new VerletPoint(new Vector2(1, 1), VerletSimulator.CreateCircle()));
        points.Add(new VerletPoint(new Vector2(1, -1), VerletSimulator.CreateCircle()));
        
        AddEdgeConstraint(0, 1);
        AddEdgeConstraint(1, 2);
        AddEdgeConstraint(2, 3);
        AddEdgeConstraint(3, 0);
        AddEdgeConstraint(0, 2);
        AddEdgeConstraint(1, 3);
    }

    public void AddEdgeConstraint(int a, int b)
    {
        EdgeConstraint c = new EdgeConstraint()
        {
            ptId1 = a,
            ptId2 = b,
            restLength = Vector2.Distance(points[a].position, points[b].position)
        };
        edgeConstraints.Add(c);
    }

    public void UpdatePosition(float dt)
    {
        ApplySpringForces();
        foreach (VerletPoint pt in points)
        {
            pt.UpdatePosition(dt);
        }
    }

    private void ApplySpringForces()
    {
        foreach (EdgeConstraint c in edgeConstraints)
        {
            VerletPoint a = points[c.ptId1];
            VerletPoint b = points[c.ptId2];
            Vector2 delta = b.position - a.position;
            float distance = delta.magnitude;
            float difference = (distance - c.restLength) / distance;
            Vector2 force = delta * difference * 0.5f; // 0.5f to distribute the force equally

            // Apply forces to the points
            a.acceleration += force;
            b.acceleration -= force;
        }
    }

    public void Accelerate(Vector2 acc)
    {
        foreach (VerletPoint point in points)
            point.Accelerate(acc);
    }
}