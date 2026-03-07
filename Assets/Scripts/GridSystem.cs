using UnityEngine;

public class GridSystem
{
    private int width;
    private int height;
    private float cellSize;
    private int[,] matrix;
    public GridSystem(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        matrix = new int[width, height];

        for (int x = 0; x < width; x++) {
            for (int z = 0; z < height; z++) {
                Vector3 start = GetWorldPosition(x, z);
                Debug.DrawLine(start, start + Vector3.right * cellSize, Color.white, 1000f);
                Debug.DrawLine(start, start + Vector3.forward * cellSize, Color.white, 1000f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 1000f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 1000f);
    }
    public Vector3 GetWorldPosition(int x, int z) {
        return new Vector3(x, 0, z) * cellSize;
    }
    public bool IsValidGridPosition(GridPosition gridPosition) {
        return gridPosition.x >= 0 && gridPosition.z >= 0 && gridPosition.x < width && gridPosition.z < height;
    }
    public void SetValue(GridPosition gridPosition, int value) {
        if (IsValidGridPosition(gridPosition)) {
            matrix[gridPosition.x, gridPosition.z] = value;
        }
    }
}
