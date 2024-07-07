using System.Collections.Generic;
using UnityEngine;

public class Splitter : Cell
{
    public override void OnMouseDown(int mouseButton)
    {
        if (mouseButton == 0)
        {
            ControlledCellHandler.SetControlledCell(this);
        }
        if (mouseButton == 1)
        {
            Split();
        }
        if (mouseButton == 2)
        {
        }
    }

    public void Split()
    {
        float newRadius = radius / 2;

        // Pick a random direction
        Vector2 direction = Random.insideUnitCircle.normalized;

        // Create the first split cell
        GameObject split1 = CreateSplitCell(gameObject.transform.position + (Vector3)direction * newRadius / 2, newRadius, direction);

        // Create the second split cell
        GameObject split2 = CreateSplitCell(gameObject.transform.position - (Vector3)direction * newRadius / 2, newRadius, -direction);

        Destroy();
    }

    GameObject CreateSplitCell(Vector3 position, float newRadius, Vector2 direction)
    {
        // Create a new GameObject
        GameObject cellGo = new GameObject("Splitter");
        Splitter cell = new Splitter();
        cell.radius = newRadius;
        cell.Init(cellGo, renderer.material);
        // "CellController" is a line of communication from Unity to our cell class.
        // It will call appropriate methods for us such as Start, Update, FixedUpdate, onMouseDown
        // cells without this line of communication will not tick
        cellGo.AddComponent<CellController>().cell = cell;
        cell.SetPosition(new Vector2(position.x, position.y));
        cell.rb.AddForce(direction, ForceMode2D.Impulse);
        return cellGo;
    }
}