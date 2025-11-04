using UnityEngine;

public class ClearCounter : MonoBehaviour
{
    [SerializeField]
    private KitchenObjectSO kitchenObjectSO;
    [SerializeField]
    private Transform spawmPoint;

    private void OnEnable()
    {
        PlayerInteractionEvent.OnInteract += TryInteract;
    }

    private void OnDisable()
    {
        PlayerInteractionEvent.OnInteract -= TryInteract;
    }

    private void TryInteract(ClearCounter counter, Player player)
    {
        if (counter == this)
        {
            Interact(player);
        }
    }

    public void Interact(Player player)
    {
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab, spawmPoint);
        kitchenObjectTransform.localPosition = Vector3.zero;
    }
}