using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlledCellHandler : MonoBehaviour
{

    public static ControlledCellHandler instance;
    public void Start() => instance = this;
    public Cell controlledCell;

    public static void SetControlledCell(Cell cell){
        if(instance.controlledCell != null)
            instance.controlledCell.isControlled = false;
        if(cell != null)
            cell.isControlled = true;
        instance.controlledCell = cell;
    }

    private void Update()
    {
        // Check if Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetControlledCell(null);
        }
    }
}
