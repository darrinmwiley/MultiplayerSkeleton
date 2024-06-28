using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Force
{
    void Apply();
}
public class Spring : Force
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
        //todo single linerenderer in parent
        lineRenderer = new GameObject("SpringLine").AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(0, 0, 1, 0.5f); // Blue translucent
        lineRenderer.endColor = new Color(0, 0, 1, 0.5f); // Blue translucent
        lineRenderer.positionCount = 2;
    }

    public void Apply()
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