using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 input = playerInputActions.Player.Move.ReadValue<Vector2>();
        input = input.normalized;
        return input;
    }
    public bool IsPausePressed()
    {
        return playerInputActions.Player.Pause.triggered;
    }
    public bool IsInteractPressed()
    {
        return playerInputActions.Player.Interact.triggered;
    }
    public bool IsCutPressed()
    {
        return playerInputActions.Player.Cut.triggered;
    }
}
