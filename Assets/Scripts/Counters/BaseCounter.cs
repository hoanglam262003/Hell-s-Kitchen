using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField]
    private Transform spawnPoint;

    private KitchenObject kitchenObject;
    private void OnEnable()
    {
        PlayerInteractionEvent.OnInteraction += TryInteract;
    }

    private void OnDisable()
    {
        PlayerInteractionEvent.OnInteraction -= TryInteract;
    }

    private void TryInteract(BaseCounter counter, Player player, InteractionType type)
    {
        if (counter != this) return;

        switch (type)
        {
            case InteractionType.Interact:
                Interact(player);
                break;
            case InteractionType.Cut:
                TryCut(player);
                break;
        }
    }
    public virtual void Interact(Player player)
    {
        
    }
    public virtual void TryCut(Player player)
    {

    }
    public Transform GetKitchenObjectFollowTransform()
    {
        return spawnPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
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
}
