using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour 
{
    private GridSystem gridSystem;
    private GridPosition currentGridPosition;
    public bool HasMovedThisTurn { get; set; } = false;

    public void Setup(GridSystem grid, GridPosition startPos) 
    {
        this.gridSystem = grid;
        this.currentGridPosition = startPos;
        UpdateVisual();
    }

    public void TakeTurn(List<Player> pursuers) 
    {
        for (int i = 0; i < 2; i++) 
        {
            currentGridPosition = GetBestMove(pursuers);
        }
    
        UpdateVisual();
        HasMovedThisTurn = true;
    }

    private GridPosition GetBestMove(List<Player> pursuers) 
    {
        GridPosition bestPos = currentGridPosition;
        float maxMinDistance = -1f;

        GridPosition[] directions = 
        {
            new GridPosition(0, 1), new GridPosition(0, -1),
            new GridPosition(1, 0), new GridPosition(-1, 0),
            new GridPosition(0, 0)
        };

        foreach (var dir in directions) 
        {
            GridPosition target = new GridPosition(currentGridPosition.x + dir.x, currentGridPosition.z + dir.z);

            if (gridSystem.IsValidGridPosition(target)) 
            {
                float minDistToPursuer = float.MaxValue;
                foreach (var p in pursuers) 
                {
                    float dist = Vector3.Distance
                    (
                        new Vector3(target.x, 0, target.z), 
                        new Vector3(p.GetGridPosition().x, 0, p.GetGridPosition().z)
                    );

                    if (dist < minDistToPursuer) 
                        minDistToPursuer = dist;
                }

                if (minDistToPursuer > maxMinDistance) 
                {
                    maxMinDistance = minDistToPursuer;
                    bestPos = target;
                }
            }
        }
        return bestPos;
    }

    private void UpdateVisual() 
    {
        transform.position = gridSystem.GetWorldPosition(currentGridPosition.x, currentGridPosition.z);
    }

    public GridPosition GetGridPosition() => currentGridPosition;
}