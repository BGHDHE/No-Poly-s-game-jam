using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GridSystem gridSystem;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject aiPrefab;

    private List<Player> players = new List<Player>();
    private EnemyAI enemyAI;

    private MazeCell[,] mazeCells;

    private int width = 10;
    private int height = 10;
    private float cellSize = 6f;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        gridSystem = new GridSystem(width, height, cellSize);
        InitializeMazeReference();

        SpawnPlayers();
        UpdateSelectionVisuals();
    }

    private void InitializeMazeReference()
    {
        mazeCells = new MazeCell[width, height];

        MazeCell[] allCells = FindObjectsOfType<MazeCell>();

        foreach (MazeCell cell in allCells)
        {
            int x = Mathf.RoundToInt(cell.transform.position.x / cellSize);
            int z = Mathf.RoundToInt(cell.transform.position.z / cellSize);

            if (x >= 0 && x < width && z >= 0 && z < height)
            {
                mazeCells[x, z] = cell;
            }
        }
    }

    private void SpawnPlayers()
    {
        GridPosition[] startPositions =
        {
            new GridPosition(0, 0),
            new GridPosition(0, 9),
            new GridPosition(9, 9)
        };

        for (int i = 0; i < 3; i++)
        {
            GameObject go = Instantiate(playerPrefab);

            Player p = go.GetComponent<Player>();
            p.Setup(gridSystem, startPositions[i], i + 1);

            players.Add(p);
        }

        GameObject aiGo = Instantiate(aiPrefab);
        enemyAI = aiGo.GetComponent<EnemyAI>();
        enemyAI.Setup(gridSystem, new GridPosition(5, 5));
    }

    public void HandleMoveInput(Vector2 input)
    {
        Player activePlayer = players.Find(p => !p.HasMovedThisTurn);

        if (activePlayer == null)
            return;

        int dx = Mathf.RoundToInt(input.x);
        int dy = Mathf.RoundToInt(input.y);

        GridPosition currentPos = activePlayer.GetGridPosition();
        MazeCell currentCell = mazeCells[currentPos.x, currentPos.z];

        bool canMove = false;

        if (dy == 1 && currentCell.IsNorthOpen) canMove = true;
        else if (dy == -1 && currentCell.IsSouthOpen) canMove = true;
        else if (dx == 1 && currentCell.IsEastOpen) canMove = true;
        else if (dx == -1 && currentCell.IsWestOpen) canMove = true;

        if (canMove)
        {
            if (activePlayer.TryMove(new GridPosition(dx, dy)))
            {
                activePlayer.HasMovedThisTurn = true;

                UpdateSelectionVisuals();

                if (players.All(p => p.HasMovedThisTurn))
                {
                    ExecuteAITurn();
                }
            }
        }
        else
        {
            Debug.Log("nem jooo");
        }
    }

    public void HandleSkipInput()
    {
        Player activePlayer = players.Find(p => !p.HasMovedThisTurn);

        if (activePlayer == null)
            return;

        activePlayer.HasMovedThisTurn = true;

        UpdateSelectionVisuals();
        Debug.Log($"{activePlayer.name} skippelt.");

        if (players.All(p => p.HasMovedThisTurn))
        {
            ExecuteAITurn();
        }
    }

    private void ExecuteAITurn()
    {
        if (enemyAI != null)
        {
            enemyAI.TakeTurn(players, mazeCells);
        }

        ResetPlayersForNewTurn();
    }

    private void ResetPlayersForNewTurn()
    {
        foreach (Player p in players)
        {
            p.HasMovedThisTurn = false;
        }

        UpdateSelectionVisuals();
        Debug.Log("uj kor kezdodik");
    }

    private void UpdateSelectionVisuals()
    {
        Player activePlayer = players.Find(p => !p.HasMovedThisTurn);

        foreach (Player p in players)
        {
            p.SetSelected(p == activePlayer);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var p in players)
        {
            if (p == null) continue;

            Gizmos.color = p.HasMovedThisTurn ? Color.gray : Color.green;

            Gizmos.DrawWireSphere(
                p.transform.position + Vector3.up * 4f,
                0.3f
            );
        }
    }
}