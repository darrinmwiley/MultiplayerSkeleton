using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle
{
    public Vector2 position;
    public Vector2 previous;
    public Vector2 acceleration;
    public GameObject gameObject;

    public particle(Vector2 pos, GameObject obj)
    {
        position = pos;
        previous = pos;
        acceleration = Vector2.zero;
        gameObject = obj;
        gameObject.transform.position = new Vector3(pos.x, pos.y, 0);
    }

    public void SetPosition(Vector2 location)
    {
        position = location;
        gameObject.transform.position = new Vector3(location.x, location.y, 0);
    }
}
