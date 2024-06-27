using UnityEngine;
using System.Collections.Generic;

public class VerletPoint{

    public Vector2 position;
    public Vector2 previousPosition;
    public Vector2 acceleration;

    public GameObject gameObject;

    public VerletPoint(Vector2 position, GameObject go)
    {
        this.position = this.previousPosition = position;
        gameObject = go;
        gameObject.transform.position = new Vector3(position.x, position.y, 0);
    }

    public void UpdatePosition(float dt)
    {
        Vector2 velocity = position - previousPosition;
        previousPosition = position;
        position += velocity + acceleration * dt * dt;
        gameObject.transform.position = new Vector3(position.x, position.y, 0);
        acceleration = new Vector2(0,0);
    }

    public void Accelerate(Vector2 acc)
    {
        acceleration += acc;
    }

}