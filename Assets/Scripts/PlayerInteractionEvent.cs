using System;

public static class PlayerInteractionEvent
{
    public static event Action<ClearCounter, Player> OnInteract;

    public static void RaiseInteract(ClearCounter counter, Player player)
    {
        OnInteract?.Invoke(counter, player);
    }
}