using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public static CameraHandler instance;

    public float zoomSpeed = 10.0f; // Speed of zooming
    public float minZoom = 5.0f; // Minimum zoom level
    public float maxZoom = 20.0f; // Maximum zoom level
    public float dragSpeed = 2.0f; // Speed of camera drag

    private Vector3 dragOrigin;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // Move the main camera to center on the controlled cell
        Cell controlledCell = ControlledCellHandler.instance.controlledCell;
        if (controlledCell != null)
        {
            Camera.main.transform.position = new Vector3(
                controlledCell.gameObject.transform.position.x, 
                controlledCell.gameObject.transform.position.y, 
                Camera.main.transform.position.z
            );
        }

        // Handle zooming with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            Camera.main.orthographicSize -= scroll * zoomSpeed;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
        }

        // Handle camera drag on click and drag
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += difference;
        }
    }
}