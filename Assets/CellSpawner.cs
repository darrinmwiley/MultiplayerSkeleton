using UnityEngine;

public class CellSpawner : MonoBehaviour
{
    public GameObject cellPrefab; // Prefab to spawn
    public int numberOfCells = 16; // Total number of cells to spawn
    public float spacing = 3f; // Spacing between cells
    public float xmin = -5f; // Minimum x position
    public float xmax = 5f; // Maximum x position
    public float ymin = -5f; // Minimum y position

    void Start()
    {
        SpawnCells();
    }

    void SpawnCells()
    {
        float y = ymin;
        float x = xmin;
        for(int i = 0;i<numberOfCells;i++)
        {
            float nextx = x;
            float nexty = y;
            if(nextx > xmax)
            {
                x = xmin;
                y += spacing;
                nexty = y;
                nextx = x;
            }
            Vector3 spawnPosition = new Vector3(nextx, nexty, 0);

            GameObject newCell = Instantiate(cellPrefab, spawnPosition, Quaternion.identity);
            newCell.transform.SetParent(transform);
            x += spacing;
        }
    }
}
