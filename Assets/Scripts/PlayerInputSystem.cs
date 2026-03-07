using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    public Player player;

    private void Awake() {
        playerInputActions = new PlayerInputActions();
    }
    private void OnEnable() {
        playerInputActions.Enable();
        playerInputActions.Player.Move.performed += OnMovePerformed;
    }
    private void OnMovePerformed(InputAction.CallbackContext context) {
        player.Move(context);
    }

    private void OnDisable() {
        playerInputActions.Player.Move.performed -= OnMovePerformed;
        playerInputActions.Disable();
    }
}
