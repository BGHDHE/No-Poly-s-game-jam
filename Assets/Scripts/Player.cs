using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private GridSystem gridSystem;
    public Transform cubeTransform;
    private GridPosition currentGridPosition;

    private void Start() {
        gridSystem = new GridSystem(10, 10, 2f);
        currentGridPosition = new GridPosition(0, 0);
        UpdateCubeVisualPosition();
    }

    public void Move(InputAction.CallbackContext context) {
        Vector2 inputVector = context.ReadValue<Vector2>();
        GridPosition moveOffset = new GridPosition(Mathf.RoundToInt(inputVector.x),Mathf.RoundToInt(inputVector.y));
        MoveCube(moveOffset);
    }
    private void MoveCube(GridPosition offset) {
        GridPosition targetPosition = new GridPosition(currentGridPosition.x + offset.x,currentGridPosition.z + offset.z);

        if (gridSystem.IsValidGridPosition(targetPosition)) {
            gridSystem.SetValue(currentGridPosition, 0); 
            currentGridPosition = targetPosition;
            gridSystem.SetValue(currentGridPosition, 1);
            UpdateCubeVisualPosition();
            Debug.Log($"Mozgás ide: {currentGridPosition}");
        }
    }
    private void UpdateCubeVisualPosition() {
        cubeTransform.position = gridSystem.GetWorldPosition(currentGridPosition.x, currentGridPosition.z);
    }
}
