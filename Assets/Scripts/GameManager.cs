using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private MazeGenerator mazeGenerator;
    [SerializeField] private float moveSpeed;

    [SerializeField] private GameObject ghostAuraPersistentPrefab;
    [SerializeField] private GameObject ghostBurstActionPrefab;
    [SerializeField] private GameObject rangeParticlePrefab;
    [SerializeField] private float gameOverAnimationDelay = 2.0f;

    private List<PlayerMarker> players = new();
    private List<PowerUp> spawnedPowerUps = new();

    private int currentPlayerIndex = 0;
    private int movesRemaining = 0;
    private bool isMoving = false;

    private EnemyAI enemy;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Invoke(nameof(InitializePlayers), 0.2f);
    }

    private IEnumerator GhostEffectRoutine(PlayerMarker player)
    {
        if (ghostBurstActionPrefab != null)
        {
            GameObject burst = Instantiate(ghostBurstActionPrefab, player.transform.position, Quaternion.identity);
            burst.transform.SetParent(player.transform);
            Destroy(burst, 2f);
        }

        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Color originalColor = renderers[0].material.color;
            Color ghostColor = new(originalColor.r, originalColor.g, originalColor.b, 0.4f);

            foreach (var r in renderers)
                r.material.color = ghostColor;

            yield return new WaitForSeconds(0.8f);

            foreach (var r in renderers)
                r.material.color = originalColor;
        }
    }

    private void TryMoveCurrentPlayer(Vector2Int direction)
    {
        PlayerMarker player = players[currentPlayerIndex];
        Vector2Int targetGridPos = player.GridPos + direction;

        if (targetGridPos.x < 0 || targetGridPos.x >= mazeGenerator.mazeWidth ||
            targetGridPos.y < 0 || targetGridPos.y >= mazeGenerator.mazeHeight)
            return;

        MazeCell currentCell = mazeGenerator.GetCells()[player.GridPos.x, player.GridPos.y];

        bool canMoveNormal =
            (direction == Vector2Int.up && currentCell.IsNorthOpen) ||
            (direction == Vector2Int.down && currentCell.IsSouthOpen) ||
            (direction == Vector2Int.right && currentCell.IsEastOpen) ||
            (direction == Vector2Int.left && currentCell.IsWestOpen);

        if (canMoveNormal)
        {
            movesRemaining--;
            StartCoroutine(MoveRoutine(player, targetGridPos));
        }
        else if (player.GhostCharges > 0)
        {
            player.GhostCharges--;
            movesRemaining--;

            StartCoroutine(GhostEffectRoutine(player));

            if (player.GhostCharges <= 0 && player.activePersistentGhostEffect != null)
            {
                var ps = player.activePersistentGhostEffect.GetComponent<ParticleSystem>();
                if (ps != null) ps.Stop();

                Destroy(player.activePersistentGhostEffect, 2f);
                player.activePersistentGhostEffect = null;
            }

            StartCoroutine(MoveRoutine(player, targetGridPos));
        }
    }

    private void CheckForPowerUp(PlayerMarker player)
    {
        PowerUp found = spawnedPowerUps.Find(p => p.GridPos == player.GridPos);
        if (found == null) return;

        if (found.type == PowerUpType.Ghost)
        {
            player.GhostCharges += 2;

            if (player.activePersistentGhostEffect == null && ghostAuraPersistentPrefab != null)
            {
                player.activePersistentGhostEffect = Instantiate(
                    ghostAuraPersistentPrefab,
                    player.transform.position,
                    Quaternion.identity);

                player.activePersistentGhostEffect.transform.SetParent(player.transform);
            }
        }
        else if (found.type == PowerUpType.Range)
        {
            player.StepRange *= 2;

            if (rangeParticlePrefab != null)
            {
                GameObject effect = Instantiate(
                    rangeParticlePrefab,
                    player.transform.position,
                    player.transform.rotation);

                effect.transform.SetParent(player.transform);
            }
        }

        spawnedPowerUps.Remove(found);
        found.Collect();
    }

    private IEnumerator MoveRoutine(PlayerMarker player, Vector2Int targetGridPos)
    {
        isMoving = true;
        player.GridPos = targetGridPos;

        float cellSize = 18f;

        Vector3 targetWorldPos = new(
            targetGridPos.x * cellSize + cellSize / 2f,
            player.transform.position.y,
            targetGridPos.y * cellSize + cellSize / 2f
        );

        SetPlayerBool(player, "isRunning", true);

        while (Vector3.Distance(player.transform.position, targetWorldPos) > 0.05f)
        {
            Vector3 dir = (targetWorldPos - player.transform.position).normalized;

            if (dir != Vector3.zero)
            {
                player.transform.rotation = Quaternion.Slerp(
                    player.transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 15f);
            }

            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                targetWorldPos,
                moveSpeed * Time.deltaTime);

            yield return null;
        }

        player.transform.position = targetWorldPos;

        SetPlayerBool(player, "isRunning", false);
        CheckForPowerUp(player);

        isMoving = false;

        if (CheckGameOver())
            yield break;

        if (movesRemaining > 0)
            UpdateTurnIndicator();
        else
            NextTurn();
    }

    private bool CheckGameOver()
    {
        if (enemy == null) return false;

        foreach (var p in players)
        {
            if (p.GridPos == enemy.GridPos)
            {
                StartCoroutine(GameOverAnimationRoutine(p, enemy));
                return true;
            }
        }

        return false;
    }

    private IEnumerator GameOverAnimationRoutine(PlayerMarker winnerPlayer, EnemyAI loserEnemy)
    {
        isMoving = true;

        Vector3 playerPos = winnerPlayer.transform.position;
        Vector3 enemyPos = loserEnemy.transform.position;

        winnerPlayer.transform.LookAt(new Vector3(enemyPos.x, playerPos.y, enemyPos.z));
        loserEnemy.transform.LookAt(new Vector3(playerPos.x, enemyPos.y, playerPos.z));

        Animator playerAnim = winnerPlayer.GetComponentInChildren<Animator>();
        Animator enemyAnim = loserEnemy.GetComponentInChildren<Animator>();

        if (playerAnim != null)
            playerAnim.SetTrigger("attack");

        yield return new WaitForSeconds(0.2f);

        if (enemyAnim != null)
            enemyAnim.SetTrigger("die");

        yield return new WaitForSeconds(gameOverAnimationDelay);

        StartCoroutine(ResetSequence());
    }

    public void HandleMoveInput(Vector2 input)
    {
        if (isMoving || players.Count == 0) return;

        Vector2Int moveDir = Vector2Int.zero;

        if (input.y > 0.5f) moveDir = Vector2Int.up;
        else if (input.y < -0.5f) moveDir = Vector2Int.down;
        else if (input.x > 0.5f) moveDir = Vector2Int.right;
        else if (input.x < -0.5f) moveDir = Vector2Int.left;

        if (moveDir != Vector2Int.zero)
            TryMoveCurrentPlayer(moveDir);
    }

    public void HandleSkipInput()
    {
        if (!isMoving)
        {
            movesRemaining = 0;
            NextTurn();
        }
    }

    public void RegisterPowerUp(PowerUp pu)
    {
        if (!spawnedPowerUps.Contains(pu))
            spawnedPowerUps.Add(pu);
    }

    private IEnumerator ResetSequence()
    {
        isMoving = true;

        yield return new WaitForSeconds(1f);

        foreach (var p in players)
        {
            if (p != null)
                p.ResetProperties();
        }

        spawnedPowerUps.Clear();

        mazeGenerator.GenerateNewLevel();

        yield return new WaitForEndOfFrame();

        players.Clear();
        InitializePlayers();

        currentPlayerIndex = 0;
        movesRemaining = 0;
        isMoving = false;
    }

    private void SetPlayerBool(PlayerMarker player, string param, bool state)
    {
        Animator anim = player.GetComponentInChildren<Animator>();

        if (anim != null)
            anim.SetBool(param, state);
    }

    private void InitializePlayers()
    {
        players.Clear();
        players.AddRange(FindObjectsOfType<PlayerMarker>());

        enemy = FindObjectOfType<EnemyAI>();

        if (players.Count > 0)
            UpdateTurnIndicator();
    }

    private void NextTurn()
    {
        currentPlayerIndex++;

        if (currentPlayerIndex >= players.Count)
            StartCoroutine(EnemyTurnRoutine());
        else
            UpdateTurnIndicator();
    }

private IEnumerator EnemyTurnRoutine()
{
    isMoving = true;

    // 1. Játékos highlight lekapcsolása, ellenségé bekapcsolása (pl. pirosas)
    HighlightCell(enemy.GridPos, new Color(1f, 0.4f, 0.4f), true);

    // Várjunk egy kicsit, hogy látszódjon, az ellenség jön
    yield return new WaitForSeconds(0.5f);

    yield return StartCoroutine(
        enemy.TakeTurnCoroutine(players, mazeGenerator.GetCells())
    );

    // Frissítjük a highlightot az ellenség ÚJ pozíciójára a mozgás után
    HighlightCell(enemy.GridPos, new Color(1f, 0.4f, 0.4f), true);
    yield return new WaitForSeconds(0.3f);

    currentPlayerIndex = 0;
    isMoving = false;

    if (!CheckGameOver())
        UpdateTurnIndicator(); // Ez majd visszaugrik az 1. játékosra
}

    private void HighlightCell(Vector2Int gridPos, Color color, bool state)
{
    // Előző kiemelés törlése, ha létezik
    if (!state && lastHighlightedCell.x != -1)
    {
        var oldCell = mazeGenerator.GetCells()[lastHighlightedCell.x, lastHighlightedCell.y];
        if (oldCell != null) oldCell.SetHighlight(false, Color.white);
        return;
    }

    // Új kiemelés bekapcsolása
    if (state)
    {
        // Ha volt előző, azt előbb lekapcsoljuk
        HighlightCell(Vector2Int.zero, Color.white, false); 

        lastHighlightedCell = gridPos;
        var currentCell = mazeGenerator.GetCells()[gridPos.x, gridPos.y];
        if (currentCell != null)
        {
            currentCell.SetHighlight(true, color);
        }
    }
}

private Vector2Int lastHighlightedCell = new Vector2Int(-1, -1);
    private void UpdateTurnIndicator()
    {
ResetAllPlayersAnimation();

    if (players.Count > 0 && currentPlayerIndex < players.Count)
    {
        PlayerMarker p = players[currentPlayerIndex];

        // 1. ELŐZŐ CELLA VISSZAÁLLÍTÁSA
        if (lastHighlightedCell.x != -1)
        {
            var oldCell = mazeGenerator.GetCells()[lastHighlightedCell.x, lastHighlightedCell.y];
            if (oldCell != null)
            {
                // Itt a második paraméternek nincs hatása a MazeCell-ben lévő if-else miatt
                oldCell.SetHighlight(false, Color.white); 
            }
        }
        // 2. ÚJ CELLA KIEMELÉSE
        lastHighlightedCell = p.GridPos;
        var currentCell = mazeGenerator.GetCells()[p.GridPos.x, p.GridPos.y];
        if (currentCell != null)
        {
            currentCell.SetHighlight(true, new Color(1f, 0.9f, 0.5f)); // Pl. halványsárga kiemelés
        }
        HighlightCell(p.GridPos, new Color(1f, 0.9f, 0.5f), true);

            if (movesRemaining <= 0)
                movesRemaining = p.StepRange;

            SetPlayerBool(p, "isAlerted", true);
        }
    }

    private void ResetAllPlayersAnimation()
    {
        foreach (var p in players)
        {
            SetPlayerBool(p, "isAlerted", false);
            SetPlayerBool(p, "isRunning", false);
        }
    }
}