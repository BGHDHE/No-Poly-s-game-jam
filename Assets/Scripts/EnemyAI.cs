using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public Vector2Int GridPos { get; set; }

    private Vector2Int lastGridPos = new(-1, -1);
    private float cellSize = 18f;
    private float moveSpeed = 9f;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void Setup(Vector2Int startPos)
    {
        GridPos = startPos;
        lastGridPos = startPos;
        UpdateVisualImmediate();
    }

    public IEnumerator TakeTurnCoroutine(List<PlayerMarker> players, MazeCell[,] mazeCells)
    {
        for (int i = 0; i < 2; i++)
        {
            Vector2Int nextMove = GetAggressiveEscapeMove(players, mazeCells);

            if (nextMove != GridPos)
            {
                lastGridPos = GridPos;
                GridPos = nextMove;
                yield return StartCoroutine(MoveVisualRoutine());
            }
        }
    }

    private IEnumerator MoveVisualRoutine()
    {
        Vector3 targetWorldPos = CalculateWorldPos(GridPos);

        SetAnimBool("isRunning", true);

        while (Vector3.Distance(transform.position, targetWorldPos) > 0.05f)
        {
            Vector3 direction = (targetWorldPos - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    lookRotation,
                    Time.deltaTime * 15f
                );
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWorldPos,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetWorldPos;

        SetAnimBool("isRunning", false);
    }

    private void SetAnimBool(string paramName, bool state)
    {
        if (anim != null)
            anim.SetBool(paramName, state);
    }

    private void UpdateVisualImmediate()
    {
        transform.position = CalculateWorldPos(GridPos);
    }

    private Vector3 CalculateWorldPos(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize + (cellSize / 2f),
            0f,
            gridPos.y * cellSize + (cellSize / 2f)
        );
    }

    private bool CanMoveInDirection(Vector2Int currentPos, Vector2Int dir, MazeCell[,] maze)
    {
        int w = maze.GetLength(0);
        int h = maze.GetLength(1);

        Vector2Int next = currentPos + dir;

        if (next.x < 0 || next.y < 0 || next.x >= w || next.y >= h)
            return false;

        MazeCell cell = maze[currentPos.x, currentPos.y];

        if (dir == Vector2Int.up) return cell.IsNorthOpen;
        if (dir == Vector2Int.down) return cell.IsSouthOpen;
        if (dir == Vector2Int.right) return cell.IsEastOpen;
        if (dir == Vector2Int.left) return cell.IsWestOpen;

        return false;
    }

    private Vector2Int GetAggressiveEscapeMove(List<PlayerMarker> players, MazeCell[,] mazeCells)
    {
        if (players == null || players.Count == 0)
            return GridPos;

        PlayerMarker closestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var p in players)
        {
            float dist = Vector2.Distance(GridPos, p.GridPos);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestPlayer = p;
            }
        }

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.zero
        };

        Vector2Int bestMove = GridPos;
        float bestScore = float.MinValue;

        foreach (var dir in directions)
        {
            if (dir != Vector2Int.zero && !CanMoveInDirection(GridPos, dir, mazeCells))
                continue;

            Vector2Int target = GridPos + dir;

            bool isPlayerOnTarget = false;

            foreach (var p in players)
            {
                if (p.GridPos == target)
                    isPlayerOnTarget = true;
            }

            if (isPlayerOnTarget)
                continue;

            float score = Vector2.Distance(target, closestPlayer.GridPos) * 10f;

            int exits = GetExitCount(target, mazeCells);
            score += exits * 5f;

            if (target == lastGridPos) score -= 15f;
            if (target == GridPos) score -= 5f;

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = target;
            }
        }

        return bestMove;
    }

    private int GetExitCount(Vector2Int pos, MazeCell[,] maze)
    {
        int count = 0;

        MazeCell cell = maze[pos.x, pos.y];

        if (cell.IsNorthOpen) count++;
        if (cell.IsSouthOpen) count++;
        if (cell.IsEastOpen) count++;
        if (cell.IsWestOpen) count++;

        return count;
    }
}