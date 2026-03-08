using UnityEngine;

public class Player : MonoBehaviour
{
    private GridSystem gridSystem;
    private GridPosition currentGridPosition;
    public int playerId; // id: 1,2,3...
    public bool HasMovedThisTurn { get; set; } = false;
    public GameObject selectionVisual;

    public void SetSelected(bool isSelected)
    {
        if (selectionVisual != null)
        {
            selectionVisual.SetActive(isSelected);
        }
    }

    public void Setup(GridSystem grid, GridPosition startPos, int id) 
    {
        this.gridSystem = grid;
        this.currentGridPosition = startPos;
        this.playerId = id;
        UpdateVisual();
    }

    public bool TryMove(GridPosition offset) 
    {
        GridPosition target = new GridPosition(currentGridPosition.x + offset.x, currentGridPosition.z + offset.z);
        
        if (gridSystem.IsValidGridPosition(target)) 
        {
            currentGridPosition = target;
            UpdateVisual();
            return true;
        }
        return false;
    }

    private void UpdateVisual() 
    {
        transform.position = gridSystem.GetWorldPosition(currentGridPosition.x, currentGridPosition.z);
    }

    public GridPosition GetGridPosition()
    {
        return currentGridPosition;
    }
}