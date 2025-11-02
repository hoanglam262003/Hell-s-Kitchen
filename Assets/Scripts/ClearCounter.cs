using UnityEngine;

public class ClearCounter : MonoBehaviour
{
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
        Debug.Log($"Interacted with {name}");
    }
}