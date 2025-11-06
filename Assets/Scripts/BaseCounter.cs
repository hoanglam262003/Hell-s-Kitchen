using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField]
    private Transform spawmPoint;

    private KitchenObject kitchenObject;
    private void OnEnable()
    {
        PlayerInteractionEvent.OnInteract += TryInteract;
    }

    private void OnDisable()
    {
        PlayerInteractionEvent.OnInteract -= TryInteract;
    }

    private void TryInteract(BaseCounter counter, Player player)
    {
        if (counter == this)
        {
            Interact(player);
        }
    }
    public virtual void Interact(Player player)
    {
        
    }
    public Transform GetKitchenObjectFollowTransform()
    {
        return spawmPoint;
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
