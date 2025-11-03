using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public ClearCounter selectedCounter;
    }

    [SerializeField]
    private float moveSpeed = 7f;
    [SerializeField]
    private float rotationSpeed = 10f;
    [SerializeField]
    private LayerMask countersLayerMask;
    private float playerRadius = 0.7f;
    private float playerHeight = 2f;
    private float moveDistance;
    private bool isWalking;
    private float interactionDistance = 2f;
    private ClearCounter selectedCounter;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one Player instance!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
        HandleInteractionInput();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteractionInput()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && selectedCounter != null)
        {
            PlayerInteractionEvent.RaiseInteract(selectedCounter, this);
        }
    }

    private void HandleInteractions()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, interactionDistance, countersLayerMask))
        {
            if (hit.transform.TryGetComponent(out ClearCounter clearCounter))
            {
                if (selectedCounter != clearCounter)
                {
                    SetSelectedCounter(clearCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement()
    {
        Vector2 input = new Vector2(0, 0);

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            input.y = 1;
        }
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            input.y = -1;
        }
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            input.x = -1;
        }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            input.x = 1;
        }

        input = input.normalized;
        moveDistance = moveSpeed * Time.deltaTime;
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);

        bool canMove = !Physics.CapsuleCast(
            transform.position,
            transform.position + Vector3.up * playerHeight,
            playerRadius,
            moveDirection,
            moveDistance
        );

        if (!canMove)
        {
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0f, 0f).normalized;
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirectionX, moveDistance);
            if (canMove)
            {
                moveDirection = moveDirectionX;
            }
            else
            {
                Vector3 moveDirectionZ = new Vector3(0f, 0f, moveDirection.z).normalized;
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirectionZ, moveDistance);
                if (canMove)
                {
                    moveDirection = moveDirectionZ;
                }
                else
                {
                    moveDirection = Vector3.zero;
                }
            }
        }

        isWalking = moveDirection != Vector3.zero;

        if (canMove)
        {
            transform.position += moveDirection * moveDistance;
        }

        if (moveDirection != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotationSpeed);
        }
    }

    private void SetSelectedCounter(ClearCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }
}
