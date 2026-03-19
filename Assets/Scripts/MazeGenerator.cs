using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int mazeWidth;
    public int mazeHeight;

    private float cellSize = 18f;

    public List<GameObject> CellPrefabs;
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    public GameObject rangePowerUpPrefab;
    public GameObject ghostPowerUpPrefab;

    private MazeCell[,] cells;

    public MazeCell[,] GetCells() => cells;

    private void Start()
    {
        if (CellPrefabs == null || CellPrefabs.Count == 0)
            return;

        GenerateMaze();
        SpawnPlayersInCorners();
        CenterCamera();
        SpawnPowerUps(5);
    }

    private void GenerateMaze()
    {
        CreateGrid();
        GenerateDFS();
        GenerateDFS();
    }

    public void ClearMaze()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        PlayerMarker[] existingPlayers = FindObjectsOfType<PlayerMarker>();
        foreach (var p in existingPlayers)
            Destroy(p.gameObject);

        EnemyAI existingEnemy = FindObjectOfType<EnemyAI>();
        if (existingEnemy != null)
            Destroy(existingEnemy.gameObject);

        cells = null;
    }

    public void GenerateNewLevel()
    {
        ClearMaze();
        GenerateMaze();
        SpawnPlayersInCorners();
        CenterCamera();
        SpawnPowerUps(5);
    }

    private void CenterCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
            return;

        float centerX = (mazeWidth * cellSize) / 2f - (cellSize / 2f) + 6f;
        float centerZ = 0f;

        float height = Mathf.Max(mazeWidth, mazeHeight) * (cellSize / 1.6f);

        mainCam.transform.position = new Vector3(centerX, height, centerZ);
        mainCam.transform.rotation = Quaternion.Euler(63f, 0f, 0f);
    }

    public void SpawnPowerUps(int count)
    {
        List<Vector2Int> blockedPositions = new();

        foreach (var player in FindObjectsOfType<PlayerMarker>())
            blockedPositions.Add(player.GridPos);

        EnemyAI enemy = FindObjectOfType<EnemyAI>();
        if (enemy != null)
            blockedPositions.Add(enemy.GridPos);

        int spawned = 0;
        int safety = 0;

        while (spawned < count && safety < 500)
        {
            safety++;

            Vector2Int randomPos = new(
                Random.Range(0, mazeWidth),
                Random.Range(0, mazeHeight)
            );

            if (blockedPositions.Contains(randomPos))
                continue;

            Vector3 worldPos = CalculateWorldPositionFromGrid(randomPos);

            bool spawnGhost = Random.value > 0.5f;
            GameObject prefabToSpawn = spawnGhost ? ghostPowerUpPrefab : rangePowerUpPrefab;

            GameObject go = Instantiate(prefabToSpawn, new Vector3(worldPos.x, 5f, worldPos.z), Quaternion.identity);
            PowerUp pu = go.GetComponent<PowerUp>();

            pu.type = spawnGhost ? PowerUpType.Ghost : PowerUpType.Range;
            pu.Setup(randomPos);

            GameManager.Instance.RegisterPowerUp(pu);

            spawned++;
        }
    }

    private void CreateGrid()
    {
        cells = new MazeCell[mazeWidth, mazeHeight];
        Vector3 offset = new(8.8f, 0f, 1f);

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize) + offset;

                int randomIndex = Random.Range(0, CellPrefabs.Count);
                GameObject cellObj = Instantiate(CellPrefabs[randomIndex], pos, Quaternion.identity, transform);

                MazeCell cell = cellObj.GetComponent<MazeCell>();
                cells[x, y] = cell;

                bool isDark = (x + y) % 2 == 0;
                Color gridColor = isDark ? Color.rosyBrown : Color.white;

                cell.SetBaseColor(gridColor);
            }
        }
    }

    private void SpawnPlayersInCorners()
    {
        if (playerPrefab == null || enemyPrefab == null)
            return;

        List<Vector2Int> corners = new()
        {
            new(0, 0),
            new(mazeWidth - 1, 0),
            new(0, mazeHeight - 1),
            new(mazeWidth - 1, mazeHeight - 1)
        };

        corners.RemoveAt(Random.Range(0, corners.Count));

        Vector2Int enemyGridPos = new(mazeWidth / 2, mazeHeight / 2);
        Vector3 enemySpawnPos = CalculateWorldPositionFromGrid(enemyGridPos);

        GameObject enemyObj = Instantiate(enemyPrefab, enemySpawnPos, Quaternion.identity);
        EnemyAI enemyAI = enemyObj.GetComponent<EnemyAI>();

        if (enemyAI != null)
            enemyAI.Setup(enemyGridPos);

        enemyObj.name = "EnemyAI";

        foreach (Vector2Int playerGridPos in corners)
        {
            Vector3 playerSpawnPos = CalculateWorldPositionFromGrid(playerGridPos);

            GameObject playerObj = Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity);
            PlayerMarker marker = playerObj.GetComponent<PlayerMarker>();

            if (marker != null)
                marker.SetGridPosition(playerGridPos.x, playerGridPos.y);

            playerObj.name = $"Player_{playerGridPos.x}_{playerGridPos.y}";
        }
    }

    private Vector3 CalculateWorldPositionFromGrid(Vector2Int gridPos)
    {
        float posX = (gridPos.x * cellSize) + (cellSize / 2f);
        float posZ = (gridPos.y * cellSize) + (cellSize / 2f);

        Vector3 offset = new(0f, -0.125f, 0f);

        return new Vector3(posX, 0f, posZ) + offset;
    }

    private void GenerateDFS()
    {
        Stack<Vector2Int> stack = new();

        Vector2Int current = new(0, 0);
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

    private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell, bool[,] visited)
    {
        List<Vector2Int> neighbors = new();

        Vector2Int[] dirs =
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1)
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

    private void RemoveWall(Vector2Int a, Vector2Int b)
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