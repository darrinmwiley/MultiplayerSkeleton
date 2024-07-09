using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour
{
    public Cell cell;
    public Material material;
    public bool initOnStart = false;

    void Start()
    {
        if(initOnStart){
            cell = new Splitter();
            cell.Init(gameObject, material);   
        }
    }

    void Update()
    {
        cell.Update();
    }

    void FixedUpdate(){
        cell.FixedUpdate();
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            cell.OnMouseDown(0);
        }
        if (Input.GetMouseButtonDown(1))
        {
            cell.OnMouseDown(1);
        }
        if (Input.GetMouseButtonDown(2))
        {
            cell.OnMouseDown(2);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CellController otherCellController = other.GetComponent<CellController>();
        if(otherCellController == null)
            return;
        VerletSoftBody softBody = otherCellController.cell.softBody;
        cell.overlappingSoftBodyIds.Add(softBody.ID);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        CellController otherCellController = other.GetComponent<CellController>();
        if(otherCellController == null)
            return;
        VerletSoftBody softBody = otherCellController.cell.softBody;
        cell.overlappingSoftBodyIds.Remove(softBody.ID);
    }

}
