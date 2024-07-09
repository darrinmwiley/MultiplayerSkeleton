using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBehaviour : Behaviour
{
    float movementStartTime;
    float movementLength;
    bool isMoving;
    float lastMovementTime;
    Vector2 movementDirection;

    public override void FixedUpdate()
    {
        if (!isMoving && Time.time >= lastMovementTime + Random.Range(3f, 7f))
        {
            StartMoving();
        }

        if (isMoving)
        {
            Move();
        }

        if (isMoving && Time.time >= movementStartTime + movementLength)
        {
            StopMoving();
        }

        if (cell is Splitter && cell.maxRadius - cell.radius < .01f)
        {
            ((Splitter)cell).Split();
        }
    }

    void StartMoving()
    {
        isMoving = true;
        movementStartTime = Time.time;
        movementLength = Random.Range(0f, 1f);
        movementDirection = Random.insideUnitCircle.normalized;
    }

    void Move()
    {
        cell.Move(movementDirection);
    }

    void StopMoving()
    {
        isMoving = false;
        lastMovementTime = Time.time;
    }
}