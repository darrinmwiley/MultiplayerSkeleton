using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletSimulator : MonoBehaviour
{

    static VerletSimulator instance;
    void Awake() => instance = this;

    public GameObject verletPrefab;

    Vector2 gravity = new Vector2(0,-2f);
    List<VerletPoint> objects = new List<VerletPoint>();
    VerletSquare square;
    public int maxDistance = 3;

    public static GameObject CreateCircle(){
        return VerletSimulator.Instantiate(instance.verletPrefab);
    }

    void Start()
    {
        square = new VerletSquare();
        objects = square.points;
    }

    // Start is called before the first frame update
    void Update()
    {
        ApplyGravity();
        UpdatePositions(Time.deltaTime);
        ApplyConstraints();
    }

    void UpdatePositions(float dt)
    {
        square.UpdatePosition(dt);
        /*foreach(VerletPoint obj in objects)
        {
            obj.UpdatePosition(dt);
        }*/
    }

    void ApplyGravity()
    {
        square.Accelerate(gravity);
        /*foreach(VerletPoint obj in objects)
        {
            obj.Accelerate(gravity);
        }*/
    }

    void ApplyConstraints()
    {
        foreach(VerletPoint obj in objects)
        {
            if(obj.position.magnitude > maxDistance)
            {
                obj.position *= maxDistance / obj.position.magnitude;
                obj.gameObject.transform.position = new Vector3(obj.position.x, obj.position.y, 0);
            }
        }
    }
}
