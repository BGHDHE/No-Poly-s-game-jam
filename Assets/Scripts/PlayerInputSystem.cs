using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    public GameManager gameManager;

    private void Awake() 
    {
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Player.Move.performed += OnMovePerformed;
        playerInputActions.Player.Skip.performed += OnSkipPerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext context) 
    {
        Vector2 input = context.ReadValue<Vector2>();
        gameManager.HandleMoveInput(input);
    }

    private void OnSkipPerformed(InputAction.CallbackContext context)
    {
        gameManager.HandleSkipInput();
    }

    private void OnDisable() 
    {
        playerInputActions.Player.Move.performed -= OnMovePerformed;
        playerInputActions.Disable();
    }
}