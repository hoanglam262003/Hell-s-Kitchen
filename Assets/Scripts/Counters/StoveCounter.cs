using System;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<StateChangedEventArgs> OnStateChanged;
    public class StateChangedEventArgs : EventArgs
    {
        public State state;
    }
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }
    [SerializeField]
    private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField]
    private BurningRecipeSO[] burningRecipeSOArray;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0f);
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;

    private State singleState = State.Idle;
    private float singleFryingTimer;
    private float singleBurningTimer;

    private bool IsMultiplayer => KitchenGameMultiplayer.playMultiplayer;

    private State CurrentState
    {
        get => IsMultiplayer ? state.Value : singleState;
        set
        {
            if (IsMultiplayer)
                state.Value = value;
            else
                singleState = value;

            OnStateChanged?.Invoke(this, new StateChangedEventArgs { state = value });
        }
    }

    private float FryingTimer
    {
        get => IsMultiplayer ? fryingTimer.Value : singleFryingTimer;
        set
        {
            if (IsMultiplayer)
                fryingTimer.Value = value;
            else
                singleFryingTimer = value;
        }
    }

    private float BurningTimer
    {
        get => IsMultiplayer ? burningTimer.Value : singleBurningTimer;
        set
        {
            if (IsMultiplayer)
                burningTimer.Value = value;
            else
                singleBurningTimer = value;
        }
    }

    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, new StateChangedEventArgs
        {
            state = state.Value
        });
    }

    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue)
    {
        float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }
    private void Update()
    {
        if (IsMultiplayer && !IsServer) return;
        if (HasKitchenObject())
        {
            switch (CurrentState)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    if (fryingRecipeSO == null) break;
                    FryingTimer += Time.deltaTime;
                    if (!KitchenGameMultiplayer.playMultiplayer)
                    {
                        float fryingTimerMax = fryingRecipeSO.fryingTimerMax;
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = FryingTimer / fryingTimerMax
                        });
                    }

                    if (FryingTimer > fryingRecipeSO.fryingTimerMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);
                        CurrentState = State.Fried;
                        BurningTimer = 0f;
                        if (IsMultiplayer)
                        {
                            SetBurningRecipeSOClientRpc(
                                KitchenGameMultiplayer.Instance
                                    .GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO())
                            );
                        }
                        else
                        {
                            burningRecipeSO = GetBurningRecipeSOWithInput(fryingRecipeSO.output);
                        }
                    }
                    break;
                case State.Fried:
                    if (burningRecipeSO == null) break;
                    BurningTimer += Time.deltaTime;
                    if (!KitchenGameMultiplayer.playMultiplayer)
                    {
                        float burningTimerMax = burningRecipeSO.burningTimerMax;
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = BurningTimer / burningTimerMax
                        });
                    }

                    if (BurningTimer > burningRecipeSO.burningTimerMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);
                        CurrentState = State.Burned;
                    }
                    break;
                case State.Burned:
                    if (KitchenGameMultiplayer.playMultiplayer)
                    {
                        SetStateIdleServerRpc();
                    }
                    else
                    {
                        CurrentState = State.Idle;
                        FryingTimer = 0f;
                        BurningTimer = 0f;

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
                    }

                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);
                    if (KitchenGameMultiplayer.playMultiplayer)
                    {
                        InteractLogicPlaceObjectOnCounterServerRpc(
                            KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(
                                kitchenObject.GetKitchenObjectSO())
                        );
                    }
                    else
                    {
                        FryingTimer = 0f;
                        CurrentState = State.Frying;

                        fryingRecipeSO =
                            GetFryingRecipeSOWithInput(kitchenObject.GetKitchenObjectSO());
                    }

                }
            }
        }
        else
        {
            if (player.HasKitchenObject())
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        if (KitchenGameMultiplayer.playMultiplayer)
                        {
                            SetStateIdleServerRpc();
                        }
                        else
                        {
                            CurrentState = State.Idle;
                            FryingTimer = 0f;
                            BurningTimer = 0f;

                            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                            {
                                progressNormalized = 0f
                            });
                        }

                    }
                }

            }
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                if (KitchenGameMultiplayer.playMultiplayer)
                {
                    SetStateIdleServerRpc();
                }
                else
                {
                    SetStateIdleLocal();
                }

            }
        }
    }

    private void SetStateIdleLocal()
    {
        CurrentState = State.Idle;
        FryingTimer = 0f;
        BurningTimer = 0f;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetStateIdleServerRpc()
    {
        CurrentState = State.Idle;
        FryingTimer = 0f;
        BurningTimer = 0f;
        ResetProgressClientRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        FryingTimer = 0f;
        CurrentState = State.Frying;
        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void ResetProgressClientRpc()
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
    }

    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);
    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
    }

    private bool HasRecipeWithInput(KitchenObjectSO input)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(input);
        return fryingRecipeSO != null;
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO input)
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == input)
            {
                return fryingRecipeSO;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO input)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == input)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }

    public bool IsFried()
    {
        return CurrentState == State.Fried;
    }
}
