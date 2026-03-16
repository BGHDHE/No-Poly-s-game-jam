using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public Vector2Int GridPos { get; set; }

    private Vector2Int lastGridPos = new Vector2Int(-1, -1);
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
        {
            anim.SetBool(paramName, state);
        }
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

    private bool CanMoveInDirection(MazeCell cell, Vector2Int dir)
    {
        if (dir == Vector2Int.up) return cell.IsNorthOpen;
        if (dir == Vector2Int.down) return cell.IsSouthOpen;
        if (dir == Vector2Int.right) return cell.IsEastOpen;
        if (dir == Vector2Int.left) return cell.IsWestOpen;
        return false;
    }

    private Vector2Int GetAggressiveEscapeMove(List<PlayerMarker> players, MazeCell[,] mazeCells)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left, Vector2Int.zero };
        Vector2Int bestMove = GridPos;
        float bestScore = float.MinValue;

        MazeCell currentCell = mazeCells[GridPos.x, GridPos.y];

        foreach (var dir in directions)
        {
            if (dir != Vector2Int.zero && !CanMoveInDirection(currentCell, dir)) continue;

            Vector2Int target = GridPos + dir;

            if (target.x < 0 || target.x >= mazeCells.GetLength(0) ||
                target.y < 0 || target.y >= mazeCells.GetLength(1))
                continue;

            bool isPlayerOnTarget = false;
            foreach (var p in players) { if (p.GridPos == target) isPlayerOnTarget = true; }
            if (isPlayerOnTarget) continue;

            float score = EvaluatePosition(target, players, mazeCells);
            if (target == lastGridPos) score -= 20f;
            if (target == GridPos) score -= 10f;

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = target;
            }
        }
        return bestMove;
    }

    private float EvaluatePosition(Vector2Int pos, List<PlayerMarker> players, MazeCell[,] mazeCells)
    {
        float score = 0;
        foreach (var p in players)
        {
            float dist = Vector2Int.Distance(pos, p.GridPos);
            if (dist < 2) score -= 50f;
            score += dist * 5f;
        }

        int exits = 0;
        MazeCell targetCell = mazeCells[pos.x, pos.y];
        if (targetCell.IsNorthOpen) exits++;
        if (targetCell.IsSouthOpen) exits++;
        if (targetCell.IsEastOpen) exits++;
        if (targetCell.IsWestOpen) exits++;

        if (exits <= 1) score -= 30f;
        score += exits * 3f;

        return score;
    }
}