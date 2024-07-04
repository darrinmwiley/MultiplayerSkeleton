using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class line
{
    public Vector2 p1, p2;
    public int id;

    public line(){}

    public line(Vector2 p1, Vector2 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }

    public float InterpolateY(float x)
    {
        float slope = (p2.y - p1.y) / (p2.x - p1.x);
        float y = p1.y + slope * (x - p1.x);
        return y;
    }

    public int CompareToAtX(line l, float x)
    {
        float thisY = InterpolateY(x);
        float otherY = l.InterpolateY(x);

        if (thisY < otherY)
            return -1;
        if (thisY > otherY)
            return 1;
        return 0;
    }

    public override string ToString()
    {
        return $"Line ID: {id}, Point 1: {p1}, Point 2: {p2}, Slope: {(p2.y - p1.y) / (p2.x - p1.x)}";
    }
}