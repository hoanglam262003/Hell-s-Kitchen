using System;

public static class PlayerInteractionEvent
{
    public static event Action<BaseCounter, Player> OnInteract;

    public static void RaiseInteract(BaseCounter counter, Player player)
    {
        OnInteract?.Invoke(counter, player);
    }
}