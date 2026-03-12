using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private MazeGenerator mazeGenerator;
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0f, 2f, 0f);

    private List<PlayerMarker> players = new List<PlayerMarker>();
    private int currentPlayerIndex = 0;
    private bool isMoving = false;
    private EnemyAI enemy;
    private GameObject currentIndicator;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Invoke(nameof(InitializePlayers), 0.2f);
    }

    private bool CheckGameOver()
    {
        if (enemy == null) return false;

        foreach (var player in players)
        {
            if (player.GridPos == enemy.GridPos)
            {
                StartCoroutine(ResetSequence());
                return true;
            }
        }

        return false;
    }

    private IEnumerator ResetSequence()
    {
        isMoving = true;

        yield return new WaitForSeconds(1f);

        mazeGenerator.GenerateNewLevel();

        yield return new WaitForEndOfFrame();

        players.Clear();
        InitializePlayers();

        currentPlayerIndex = 0;
        isMoving = false;
    }

    public void HandleMoveInput(Vector2 input)
    {
        if (isMoving || players.Count == 0) return;

        Vector2Int moveDir = Vector2Int.zero;

        if (input.y > 0.5f) moveDir = new Vector2Int(0, 1);
        else if (input.y < -0.5f) moveDir = new Vector2Int(0, -1);
        else if (input.x > 0.5f) moveDir = new Vector2Int(1, 0);
        else if (input.x < -0.5f) moveDir = new Vector2Int(-1, 0);

        if (moveDir != Vector2Int.zero)
        {
            TryMoveCurrentPlayer(moveDir);
        }
    }

    public void HandleSkipInput()
    {
        if (isMoving) return;
        NextTurn();
    }

    private void TryMoveCurrentPlayer(Vector2Int direction)
    {
        PlayerMarker player = players[currentPlayerIndex];
        Vector2Int currentGridPos = player.GridPos;
        Vector2Int targetGridPos = currentGridPos + direction;

        if (targetGridPos.x < 0 || targetGridPos.x >= mazeGenerator.mazeWidth ||
            targetGridPos.y < 0 || targetGridPos.y >= mazeGenerator.mazeHeight)
        {
            return;
        }

        MazeCell currentCell = mazeGenerator.GetCells()[currentGridPos.x, currentGridPos.y];
        bool canMove = false;

        if (direction == Vector2Int.up && currentCell.IsNorthOpen) canMove = true;
        if (direction == Vector2Int.down && currentCell.IsSouthOpen) canMove = true;
        if (direction == Vector2Int.right && currentCell.IsEastOpen) canMove = true;
        if (direction == Vector2Int.left && currentCell.IsWestOpen) canMove = true;

        if (canMove)
        {
            StartCoroutine(MoveRoutine(player, targetGridPos));
        }
    }

    private IEnumerator MoveRoutine(PlayerMarker player, Vector2Int targetGridPos)
    {
        isMoving = true;

        player.SetGridPosition(targetGridPos.x, targetGridPos.y);

        float cellSize = 6f;
        Vector3 offset = new Vector3(0f, 1f, 0f);

        Vector3 targetWorldPos =
            new Vector3(
                targetGridPos.x * cellSize + (cellSize / 2f),
                offset.y,
                targetGridPos.y * cellSize + (cellSize / 2f)
            ) + new Vector3(0, 0, offset.z);

        while (Vector3.Distance(player.transform.position, targetWorldPos) > 0.01f)
        {
            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                targetWorldPos,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        player.transform.position = targetWorldPos;

        isMoving = false;

        if (CheckGameOver()) yield break;

        NextTurn();
    }

    private void InitializePlayers()
    {
        players.Clear();

        PlayerMarker[] foundPlayers = FindObjectsOfType<PlayerMarker>();
        players.AddRange(foundPlayers);

        enemy = FindObjectOfType<EnemyAI>();

        if (players.Count > 0)
        {
            UpdateTurnIndicator();
        }
    }

    private void NextTurn()
    {
        currentPlayerIndex++;

        if (currentPlayerIndex >= players.Count)
        {
            StartCoroutine(EnemyTurnRoutine());
        }
        else
        {
            UpdateTurnIndicator();
        }
    }

    private IEnumerator EnemyTurnRoutine()
    {
        isMoving = true;

        yield return StartCoroutine(
            enemy.TakeTurnCoroutine(players, mazeGenerator.GetCells())
        );

        currentPlayerIndex = 0;
        isMoving = false;

        if (CheckGameOver()) yield break;

        UpdateTurnIndicator();
    }

    private void UpdateTurnIndicator()
    {
        if (indicatorPrefab == null) return;

        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
        }

        if (players.Count > 0 && currentPlayerIndex < players.Count)
        {
            PlayerMarker currentPlayer = players[currentPlayerIndex];

            Vector3 spawnPos = currentPlayer.transform.position + indicatorOffset;

            currentIndicator = Instantiate(
                indicatorPrefab,
                spawnPos,
                Quaternion.identity
            );

            currentIndicator.transform.SetParent(currentPlayer.transform);
        }
    }
}