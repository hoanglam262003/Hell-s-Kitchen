using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickSomething;
    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }
    public static Player LocalInstance { get; private set; }
    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField]
    private float moveSpeed = 7f;
    [SerializeField]
    private float rotationSpeed = 10f;
    [SerializeField]
    private LayerMask countersLayerMask;
    [SerializeField]
    private LayerMask collisionsLayerMask;
    [SerializeField]
    private Transform kitchenObjectHoldPoint;
    [SerializeField]
    private List<Vector3> spawnPositionList;
    private float playerRadius = 0.7f;
    //private float playerHeight = 2f;
    private float moveDistance;
    private bool isWalking;
    private float interactionDistance = 2f;
    private BaseCounter baseCounter;
    private KitchenObject kitchenObject;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }
        transform.position = spawnPositionList[(int)OwnerClientId];
        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        HandleMovement();
        HandleInteractions();
        HandleInteractionInput();
        HandlePauseInput();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandlePauseInput()
    {
        if (GameInput.Instance.IsPausePressed())
        {
            GameManager.Instance.TogglePauseGame();
        }
    }

    private void HandleInteractionInput()
    {
        if (baseCounter == null) return;
        if (!GameManager.Instance.IsGamePlaying()) return;

        if (GameInput.Instance.IsInteractPressed())
        {
            PlayerInteractionEvent.RaiseInteraction(baseCounter, this, InteractionType.Interact);
        }

        if (GameInput.Instance.IsCutPressed())
        {
            PlayerInteractionEvent.RaiseInteraction(baseCounter, this, InteractionType.Cut);
        }
    }

    private void HandleInteractions()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, interactionDistance, countersLayerMask))
        {
            if (hit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                if (this.baseCounter != baseCounter)
                {
                    SetSelectedCounter(baseCounter);
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
        Vector2 input = GameInput.Instance.GetMovementVectorNormalized();
        moveDistance = moveSpeed * Time.deltaTime;
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);

        bool canMove = !Physics.BoxCast(
            transform.position,
            Vector3.one * playerRadius,
            moveDirection,
            Quaternion.identity,
            moveDistance,
            collisionsLayerMask
        );

        if (!canMove)
        {
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0f, 0f).normalized;
            canMove = (moveDirection.x < -0.5f || moveDirection.x > 0.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirectionX, Quaternion.identity, moveDistance, collisionsLayerMask);
            if (canMove)
            {
                moveDirection = moveDirectionX;
            }
            else
            {
                Vector3 moveDirectionZ = new Vector3(0f, 0f, moveDirection.z).normalized;
                canMove = (moveDirection.z < -0.5f || moveDirection.z > 0.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirectionZ, Quaternion.identity, moveDistance, collisionsLayerMask);
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

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.baseCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
