using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle
{
    public Vector2 position;
    public Vector2 previous;
    public Vector2 acceleration;
    public int id;
    public int parentId = 0;
    public float size;

    public particle(Vector2 pos, float size = 0, int parentId = -1, int id = -1)
    {
        this.parentId = parentId;
        this.id = id;
        position = pos;
        previous = pos;
        acceleration = Vector2.zero;
        this.size = size;
    }

    public void SetRadius(float r)
    {
        this.size = r;
    }

    public void SetPosition(Vector2 location)
    {
        position = location;
    }
}
