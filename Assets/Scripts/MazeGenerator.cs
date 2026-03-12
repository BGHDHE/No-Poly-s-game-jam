using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int mazeWidth;
    public int mazeHeight;
    private float cellSize = 6f;

    public List<GameObject> CellPrefabs;
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    private MazeCell[,] cells;

    public MazeCell[,] GetCells() => cells;

    private void Start()
    {
        if (CellPrefabs == null || CellPrefabs.Count == 0)
        {
            return;
        }

        GenerateMaze();
        SpawnPlayersInCorners();
    }

    void GenerateMaze()
    {
        CreateGrid();
        GenerateDFS();
        GenerateDFS();
    }

    public void ClearMaze()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        PlayerMarker[] existingPlayers = FindObjectsOfType<PlayerMarker>();
        foreach (var p in existingPlayers)
        {
            Destroy(p.gameObject);
        }

        EnemyAI existingEnemy = FindObjectOfType<EnemyAI>();
        if (existingEnemy != null)
        {
            Destroy(existingEnemy.gameObject);
        }

        cells = null;
    }

    public void GenerateNewLevel()
    {
        ClearMaze();
        GenerateMaze();
        SpawnPlayersInCorners();
    }

    void CreateGrid()
    {
        cells = new MazeCell[mazeWidth, mazeHeight];
        Vector3 offset = new Vector3(3f, 0f, 0.5f);

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize) + offset;

                int randomIndex = Random.Range(0, CellPrefabs.Count);
                GameObject randomPrefab = CellPrefabs[randomIndex];

                GameObject cellObj = Instantiate(randomPrefab, pos, Quaternion.identity, transform);
                cellObj.name = $"Cell_{x}_{y}";

                cells[x, y] = cellObj.GetComponent<MazeCell>();
            }
        }
    }

    void SpawnPlayersInCorners()
    {
        if (playerPrefab == null || enemyPrefab == null) return;

        List<Vector2Int> corners = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(mazeWidth - 1, 0),
            new Vector2Int(0, mazeHeight - 1),
            new Vector2Int(mazeWidth - 1, mazeHeight - 1)
        };

        corners.RemoveAt(Random.Range(0, corners.Count));

        Vector2Int enemyGridPos = new Vector2Int(mazeWidth / 2, mazeHeight / 2);
        Vector3 enemySpawnPos = CalculateWorldPositionFromGrid(enemyGridPos);

        GameObject enemyObj = Instantiate(enemyPrefab, enemySpawnPos, Quaternion.identity);
        EnemyAI enemyAI = enemyObj.GetComponent<EnemyAI>();

        if (enemyAI != null)
        {
            enemyAI.Setup(enemyGridPos);
        }

        enemyObj.name = "EnemyAI";

        foreach (Vector2Int playerGridPos in corners)
        {
            Vector3 playerSpawnPos = CalculateWorldPositionFromGrid(playerGridPos);
            GameObject playerObj = Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity);

            PlayerMarker marker = playerObj.GetComponent<PlayerMarker>();

            if (marker != null)
            {
                marker.SetGridPosition(playerGridPos.x, playerGridPos.y);
            }

            playerObj.name = $"Player_{playerGridPos.x}_{playerGridPos.y}";
        }
    }

    private Vector3 CalculateWorldPositionFromGrid(Vector2Int gridPos)
    {
        float posX = (gridPos.x * cellSize) + (cellSize / 2f);
        float posZ = (gridPos.y * cellSize) + (cellSize / 2f);

        Vector3 offset = new Vector3(0f, 0f, 0f);

        return new Vector3(posX, 1.0f, posZ) + offset;
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

        Vector2Int[] dirs =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (var d in dirs)
        {
            int nx = cell.x + d.x;
            int ny = cell.y + d.y;

            if (nx >= 0 && nx < mazeWidth &&
                ny >= 0 && ny < mazeHeight &&
                !visited[nx, ny])
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