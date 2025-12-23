using System;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;
    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    private float plateSpawnTimer;
    private float plateSpawnTimeMax = 4f;
    private int plateSpawnAmount;
    private int platesSpawnAmountMax = 4;

    private void Update()
    {
        if (KitchenGameMultiplayer.playMultiplayer && !IsServer)
        {
            return;
        }

        plateSpawnTimer += Time.deltaTime;
        if (plateSpawnTimer > plateSpawnTimeMax)
        {
            plateSpawnTimer = 0f;
            if (GameManager.Instance.IsGamePlaying() && plateSpawnAmount < platesSpawnAmountMax)
            {
                if (KitchenGameMultiplayer.playMultiplayer)
                {
                    SpawnPlatesServerRpc();
                }
                else
                {
                    SpawnPlateLocal();
                }
            }
        }
    }

    private void SpawnPlateLocal()
    {
        plateSpawnAmount++;
        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }


    [ServerRpc]
    private void SpawnPlatesServerRpc()
    {
        SpawnPlatesClientRpc();
    }

    [ClientRpc]
    private void SpawnPlatesClientRpc()
    {
        plateSpawnAmount++;
        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            if (plateSpawnAmount > 0)
            {
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                if (KitchenGameMultiplayer.playMultiplayer)
                {
                    InteractLogicServerRpc();
                }
                else
                {
                    plateSpawnAmount--;
                    OnPlateRemoved?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        plateSpawnAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
