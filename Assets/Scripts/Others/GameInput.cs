using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private const string PLAYER_PREFS_BINDINGS = "InputBindings";
    public static GameInput Instance { get; private set; }
    public enum Binding
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Interact,
        Cut,
        Pause,
    }
    private PlayerInputActions playerInputActions;
    private InputActionRebindingExtensions.RebindingOperation currentRebind;

    private void Awake()
    {
        Instance = this;
        playerInputActions = new PlayerInputActions();

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }
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
    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            default:
            case Binding.Move_Up: return playerInputActions.Player.Move.bindings[1].ToDisplayString();
            case Binding.Move_Down: return playerInputActions.Player.Move.bindings[2].ToDisplayString();
            case Binding.Move_Left: return playerInputActions.Player.Move.bindings[3].ToDisplayString();
            case Binding.Move_Right: return playerInputActions.Player.Move.bindings[4].ToDisplayString();
            case Binding.Interact: return playerInputActions.Player.Interact.bindings[0].ToDisplayString();
            case Binding.Cut: return playerInputActions.Player.Cut.bindings[0].ToDisplayString();
            case Binding.Pause: return playerInputActions.Player.Pause.bindings[0].ToDisplayString();
        }
    }
    public void RebindBinding(Binding binding, Action onSuccess, Action onDuplicate)
    {
        if (currentRebind != null)
        {
            currentRebind.Cancel();
            currentRebind.Dispose();
            currentRebind = null;
        }

        playerInputActions.Player.Disable();

        InputAction action;
        int bindingIndex;

        switch (binding)
        {
            default:
            case Binding.Move_Up: action = playerInputActions.Player.Move; bindingIndex = 1; break;
            case Binding.Move_Down: action = playerInputActions.Player.Move; bindingIndex = 2; break;
            case Binding.Move_Left: action = playerInputActions.Player.Move; bindingIndex = 3; break;
            case Binding.Move_Right: action = playerInputActions.Player.Move; bindingIndex = 4; break;
            case Binding.Interact: action = playerInputActions.Player.Interact; bindingIndex = 0; break;
            case Binding.Cut: action = playerInputActions.Player.Cut; bindingIndex = 0; break;
            case Binding.Pause: action = playerInputActions.Player.Pause; bindingIndex = 0; break;
        }

        void StartRebind()
        {
            currentRebind = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnPotentialMatch(operation =>
                {
                    var newControl = operation.selectedControl;

                    if (IsDuplicateBinding(newControl, action))
                    {
                        operation.Cancel();
                        operation.Dispose();
                        currentRebind = null;

                        onDuplicate();

                        StartRebind();

                        return;
                    }
                })
                .OnComplete(operation =>
                {
                    operation.Dispose();
                    currentRebind = null;

                    playerInputActions.Player.Enable();

                    PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, playerInputActions.SaveBindingOverridesAsJson());
                    PlayerPrefs.Save();

                    onSuccess();
                });

            currentRebind.Start();
        }

        StartRebind();
    }

    private bool IsDuplicateBinding(InputControl newControl, InputAction rebindingAction)
    {
        string newPath = newControl.path;

        foreach (var action in playerInputActions.asset)
        {
            if (action == rebindingAction)
                continue;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                string path = binding.effectivePath;

                if (!string.IsNullOrEmpty(path) &&
                    InputControlPath.Matches(path, newControl))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void CancelRebindIfRunning()
    {
        if (currentRebind != null)
        {
            currentRebind.Cancel();
            currentRebind.Dispose();
            currentRebind = null;
        }
    }
}
