using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int mazeWidth;
    public int mazeHeight;
    private float cellSize = 6f;

    public GameObject CellPrefab;

    private MazeCell[,] cells;
    
    private void Start()
    {
        GenerateMaze();
    }

    void GenerateMaze()
    {
        CreateGrid();
        GenerateDFS();
        GenerateDFS();
    }

void CreateGrid()
{
    cells = new MazeCell[mazeWidth, mazeHeight];
    Vector3 offset = new Vector3(0f, 0f, -2f); 

    for (int x = 0; x < mazeWidth; x++)
    {
        for (int y = 0; y < mazeHeight; y++)
        {
            Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize) + offset;
            
            GameObject cellObj = Instantiate(CellPrefab, pos, Quaternion.identity, transform);
            cellObj.name = $"Cell_{x}_{y}";
            cells[x, y] = cellObj.GetComponent<MazeCell>();
        }
    }
}

    void GenerateDFS()
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int current = new Vector2Int(0, 0);
        bool[,] visited = new bool[mazeWidth, mazeHeight];

        visited[current.x, current.y] = true;
        stack.Push(current);

        while (stack.Count > 0)
        {
            current = stack.Pop();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current, visited);

            if (neighbors.Count > 0)
            {
                stack.Push(current);
                Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
                RemoveWall(current, chosen);

                visited[chosen.x, chosen.y] = true;
                stack.Push(chosen);
            }
        }
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell, bool[,] visited)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        foreach (var d in dirs)
        {
            int nx = cell.x + d.x;
            int ny = cell.y + d.y;
            if (nx >= 0 && nx < mazeWidth && ny >= 0 && ny < mazeHeight && !visited[nx, ny])
            {
                neighbors.Add(new Vector2Int(nx, ny));
            }
        }
        return neighbors;
    }

    void RemoveWall(Vector2Int a, Vector2Int b)
    {
        int dx = b.x - a.x;
        int dy = b.y - a.y;

        MazeCell cellA = cells[a.x, a.y];
        MazeCell cellB = cells[b.x, b.y];

        if (dx == 1)
        {
            cellA.RemoveEast();
            cellB.RemoveWest();
        }
        else if (dx == -1)
        {
            cellA.RemoveWest();
            cellB.RemoveEast();
        }
        
        if (dy == 1)
        {
            cellA.RemoveNorth();
            cellB.RemoveSouth();
        }
        else if (dy == -1)
        {
            cellA.RemoveSouth();
            cellB.RemoveNorth();
        }

    }
} 